import { defineConfig } from "@playwright/test";
export default defineConfig({
  testDir: "./tests/e2e", workers: 1, retries: 0, timeout: 90000, expect: { timeout: 15000 },
  use: { baseURL: process.env.E2E_BASE_URL ?? "http://localhost:3000", channel: "msedge", headless: true },
  reporter: [["list"]],
});
