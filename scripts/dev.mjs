import { spawn, spawnSync } from "node:child_process";
import { existsSync } from "node:fs";
import net from "node:net";
import path from "node:path";
import { fileURLToPath } from "node:url";

const rootDirectory = path.resolve(
  path.dirname(fileURLToPath(import.meta.url)),
  "..",
);
const apiPort = 5080;
const frontendPortStart = 3000;
const frontendPortEnd = 3010;
const composeServices = ["postgres", "redis"];
const runningChildren = [];

let servicesToStop = [];
let isShuttingDown = false;
let shutdownTask;

function getSmtpEnvironment() {
  const values = {
    Smtp__Host: process.env.SMTP_HOST,
    Smtp__Port: process.env.SMTP_PORT,
    Smtp__UseSsl: process.env.SMTP_USE_SSL,
    Smtp__UserName: process.env.SMTP_USERNAME,
    Smtp__Password: process.env.SMTP_PASSWORD,
    Smtp__FromAddress: process.env.SMTP_FROM_ADDRESS,
    Smtp__FromName: process.env.SMTP_FROM_NAME,
    WelcomeEmail__ApplicationUrl: process.env.WELCOME_EMAIL_APPLICATION_URL,
  };

  return Object.fromEntries(
    Object.entries(values).filter(([, value]) => value?.trim()),
  );
}

function getGoogleEnvironment() {
  const clientId = process.env.Google__ClientId || process.env.GOOGLE_CLIENT_ID;
  return clientId?.trim() ? { Google__ClientId: clientId } : {};
}

function delay(milliseconds) {
  return new Promise((resolve) => setTimeout(resolve, milliseconds));
}

function isDockerReady() {
  const result = spawnSync("docker", ["info", "--format", "{{.ServerVersion}}"], {
    cwd: rootDirectory,
    encoding: "utf8",
    windowsHide: true,
  });

  return !result.error && result.status === 0;
}

async function ensureDockerReady() {
  if (isDockerReady()) {
    return;
  }

  if (process.platform === "win32") {
    const dockerDesktopPaths = [
      path.join(
        process.env.LOCALAPPDATA ?? "",
        "Programs",
        "DockerDesktop",
        "Docker Desktop.exe",
      ),
      path.join(
        process.env.ProgramFiles ?? "C:\\Program Files",
        "Docker",
        "Docker",
        "Docker Desktop.exe",
      ),
    ];
    const dockerDesktopPath = dockerDesktopPaths.find((candidate) =>
      existsSync(candidate),
    );

    if (dockerDesktopPath) {
      const dockerDesktop = spawn(dockerDesktopPath, [], {
        detached: true,
        stdio: "ignore",
        windowsHide: true,
      });
      dockerDesktop.unref();
      console.log("Starting Docker Desktop...");
    }
  }

  const timeoutMilliseconds = 120_000;
  const startedAt = Date.now();
  while (Date.now() - startedAt < timeoutMilliseconds) {
    if (isDockerReady()) {
      return;
    }
    await delay(2000);
  }

  throw new Error(
    "Docker Engine did not become ready. Start Docker Desktop and retry `npm run dev`.",
  );
}

function getRunningComposeServices() {
  const result = spawnSync(
    "docker",
    ["compose", "ps", "--services", "--status", "running"],
    {
      cwd: rootDirectory,
      encoding: "utf8",
      windowsHide: true,
    },
  );

  if (result.error || result.status !== 0) {
    throw new Error(
      "Docker is not ready. Start Docker Desktop, then run `npm run dev` again.",
    );
  }

  return new Set(
    result.stdout
      .split(/\r?\n/)
      .map((service) => service.trim())
      .filter(Boolean),
  );
}

function runOnce(command, args, options = {}) {
  return new Promise((resolve, reject) => {
    const child = spawn(command, args, {
      cwd: rootDirectory,
      stdio: "inherit",
      windowsHide: true,
      ...options,
    });

    child.once("error", reject);
    child.once("exit", (code, signal) => {
      if (code === 0) {
        resolve();
      } else {
        reject(
          new Error(
            `${command} ${args.join(" ")} failed${signal ? ` (${signal})` : ` with exit code ${code}`}.`,
          ),
        );
      }
    });
  });
}

function getNpmCommand(args) {
  const npmCliPath = process.env.npm_execpath;
  if (!npmCliPath) {
    throw new Error("Run this launcher with `npm run dev` from the repository root.");
  }

  return {
    command: process.execPath,
    args: [npmCliPath, ...args],
  };
}

async function ensureFrontendDependencies() {
  const nextExecutable = path.join(
    rootDirectory,
    "frontend",
    "node_modules",
    "next",
    "dist",
    "bin",
    "next",
  );

  if (existsSync(nextExecutable)) {
    return;
  }

  console.log("Frontend packages are missing; installing from package-lock.json...");
  const npm = getNpmCommand(["ci", "--prefix", "frontend"]);
  await runOnce(npm.command, npm.args);
}

function canConnect(port) {
  return new Promise((resolve) => {
    const socket = net.createConnection({ host: "127.0.0.1", port });
    socket.setTimeout(500);
    socket.once("connect", () => {
      socket.destroy();
      resolve(true);
    });
    socket.once("error", () => resolve(false));
    socket.once("timeout", () => {
      socket.destroy();
      resolve(false);
    });
  });
}

async function isApiReady() {
  try {
    const response = await fetch(`http://127.0.0.1:${apiPort}/swagger/v1/swagger.json`, {
      signal: AbortSignal.timeout(1000),
    });
    return response.ok;
  } catch {
    return false;
  }
}

function startManagedProcess(name, command, args, options = {}) {
  const child = spawn(command, args, {
    cwd: rootDirectory,
    env: process.env,
    stdio: "inherit",
    // Next.js/npm crash on Windows if the console window is hidden.
    windowsHide: false,
    detached: process.platform !== "win32",
    ...options,
  });

  runningChildren.push({ name, child });
  child.once("error", (error) => {
    console.error(`[dev] Could not start ${name}: ${error.message}`);
    void shutdown(1);
  });
  child.once("exit", (code, signal) => {
    if (!isShuttingDown) {
      console.error(
        `[dev] ${name} stopped unexpectedly${signal ? ` (${signal})` : ` with exit code ${code}`}.`,
      );
      void shutdown(1);
    }
  });

  return child;
}

async function waitForHttp(url, child, name, timeoutMilliseconds = 90_000) {
  const startedAt = Date.now();

  while (Date.now() - startedAt < timeoutMilliseconds) {
    if (child.exitCode !== null || child.signalCode !== null) {
      throw new Error(`${name} stopped before it became ready.`);
    }

    try {
      const response = await fetch(url, {
        signal: AbortSignal.timeout(1500),
      });
      if (response.ok) {
        return;
      }
    } catch {
      // The server may still be compiling or applying migrations.
    }

    await delay(500);
  }

  throw new Error(`${name} did not become ready within ${timeoutMilliseconds / 1000} seconds.`);
}

async function findAvailableFrontendPort() {
  for (let port = frontendPortStart; port <= frontendPortEnd; port += 1) {
    if (!(await canConnect(port))) {
      return port;
    }
  }

  throw new Error(`Ports ${frontendPortStart}-${frontendPortEnd} are already in use.`);
}

function openBrowser(url) {
  let command;
  let args;

  if (process.platform === "win32") {
    command = "cmd.exe";
    args = ["/c", "start", "", url];
  } else if (process.platform === "darwin") {
    command = "open";
    args = [url];
  } else {
    command = "xdg-open";
    args = [url];
  }

  const browser = spawn(command, args, {
    cwd: rootDirectory,
    detached: true,
    stdio: "ignore",
    windowsHide: true,
  });
  browser.once("error", () => {
    console.log(`[dev] Open this URL in your browser: ${url}`);
  });
  browser.unref();
}

function stopChild(child) {
  if (child.exitCode !== null || child.signalCode !== null || child.pid === undefined) {
    return;
  }

  if (process.platform === "win32") {
    spawnSync("taskkill", ["/pid", String(child.pid), "/t", "/f"], {
      stdio: "ignore",
      windowsHide: true,
    });
    return;
  }

  try {
    process.kill(-child.pid, "SIGTERM");
  } catch {
    child.kill("SIGTERM");
  }
}

async function shutdown(exitCode) {
  if (shutdownTask) {
    return shutdownTask;
  }

  isShuttingDown = true;
  shutdownTask = (async () => {
    for (const { child } of runningChildren) {
      stopChild(child);
    }

    const childExitTasks = runningChildren.map(
      ({ child }) =>
        new Promise((resolve) => {
          if (child.exitCode !== null || child.signalCode !== null) {
            resolve();
          } else {
            child.once("exit", resolve);
          }
        }),
    );
    await Promise.race([Promise.all(childExitTasks), delay(5000)]);

    if (servicesToStop.length > 0) {
      try {
        await runOnce("docker", ["compose", "stop", ...servicesToStop]);
      } catch (error) {
        console.error(`[dev] Could not stop its Docker services: ${error.message}`);
      }
    }

    process.exitCode = exitCode;
  })();

  return shutdownTask;
}

process.once("SIGINT", () => void shutdown(0));
process.once("SIGTERM", () => void shutdown(0));

async function main() {
  const envFile = path.join(rootDirectory, ".env");
  if (existsSync(envFile)) {
    process.loadEnvFile(envFile);
  }

  await ensureFrontendDependencies();
  await ensureDockerReady();

  const alreadyRunningServices = getRunningComposeServices();
  await runOnce("docker", [
    "compose",
    "up",
    "-d",
    "--wait",
    ...composeServices,
  ]);
  servicesToStop = composeServices.filter(
    (service) => !alreadyRunningServices.has(service),
  );

  if (await canConnect(apiPort)) {
    if (!(await isApiReady())) {
      throw new Error(
        `Port ${apiPort} is already in use by a service that is not the CulinaryBlog API.`,
      );
    }
    console.log(`[dev] Reusing the API already running at http://localhost:${apiPort}.`);
  } else {
    const api = startManagedProcess(
      "API",
      "dotnet",
      [
        "run",
        "--project",
        "backend/src/CulinaryBlog.API",
        "--",
        "Logging:LogLevel:Microsoft.EntityFrameworkCore.Database.Command=Warning",
      ],
      {
        env: {
          ...process.env,
          ...getSmtpEnvironment(),
          ...getGoogleEnvironment(),
          FileStorage__Provider: "Local",
        },
      },
    );
    await waitForHttp(
      `http://127.0.0.1:${apiPort}/swagger/v1/swagger.json`,
      api,
      "API",
    );
  }

  const frontendPort = await findAvailableFrontendPort();
  const frontendDirectory = path.join(rootDirectory, "frontend");
  const nextCli = path.join(
    frontendDirectory,
    "node_modules",
    "next",
    "dist",
    "bin",
    "next",
  );
  const websiteUrl = `http://localhost:${frontendPort}`;
  const frontendEnvironment = {
    ...process.env,
    API_BASE_URL: process.env.API_BASE_URL ?? `http://127.0.0.1:${apiPort}`,
    AUTH_URL: websiteUrl,
  };
  if (!frontendEnvironment.GOOGLE_CLIENT_ID?.trim()) {
    delete frontendEnvironment.GOOGLE_CLIENT_ID;
  }
  const frontend = startManagedProcess(
    "Frontend",
    process.execPath,
    [nextCli, "dev", "--hostname", "127.0.0.1", "--port", String(frontendPort)],
    {
      cwd: frontendDirectory,
      env: frontendEnvironment,
    },
  );

  await waitForHttp(websiteUrl, frontend, "Frontend", 240_000);
  console.log(`\nCulinaryBlog is ready: ${websiteUrl}`);
  console.log("Press Ctrl+C to stop the app.\n");
  openBrowser(websiteUrl);
}

main().catch(async (error) => {
  console.error(`\n[dev] ${error.message}`);
  await shutdown(1);
});
