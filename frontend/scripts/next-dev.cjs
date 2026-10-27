const { startServer } = require("next/dist/server/lib/start-server");

const port = Number.parseInt(process.env.PORT ?? "3000", 10);

startServer({
  dir: process.cwd(),
  isDev: true,
  hostname: process.env.HOST ?? "localhost",
  port,
  allowRetry: true,
}).catch((error) => {
  console.error(error);
  process.exit(1);
});
