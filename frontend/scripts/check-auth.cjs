// Run with Playwright installed: node scripts/check-auth.cjs
// Uses mocked auth responses to cover roles without creating privileged accounts.
const { chromium } = require("playwright");
const assert = require("node:assert/strict");

(async () => {
  const browser = await chromium.launch({ channel: "msedge", headless: true });
  const baseURL = process.env.TEST_BASE_URL || "http://localhost:3000";
  try {
    for (const role of ["Guest", "Author", "Admin", "Expired", "Offline"]) {
      const context = await browser.newContext({ baseURL });
      const page = await context.newPage();
      if (role !== "Guest") {
        await context.addInitScript(() => {
          localStorage.setItem("accessToken", "test-token");
          // A forged local role must not grant admin access.
          localStorage.setItem("authUser", JSON.stringify({ role: "Admin" }));
        });
      }
      await page.route("**/api/v1/auth/me", async (route) => {
        assert.equal(route.request().headers().authorization, "Bearer test-token");
        if (role === "Offline") return route.abort();
        return route.fulfill({
          status: role === "Expired" ? 401 : 200,
          json: { id: "test-user", email: "test@example.com", displayName: "Test User", role },
        });
      });
      let loggedOut = false;
      await page.route("**/api/v1/auth/logout", (route) => {
        loggedOut = true;
        return route.fulfill({ status: 200, json: "OK" });
      });
      await page.goto("/dashboard/categories", { waitUntil: "domcontentloaded" });
      if (role === "Guest" || role === "Expired") {
        await page.waitForURL("**/auth/login?next=**");
        assert.equal(await page.getByRole("heading", { name: "Quan ly danh muc" }).count(), 0);
      } else if (role === "Author") {
        await page.getByRole("heading", { name: "Bạn không có quyền truy cập trang này" }).waitFor();
        await page.goto("/dashboard", { waitUntil: "domcontentloaded" });
        await page.getByRole("heading", { name: /Xin chào, Test User/ }).waitFor();
        assert.equal(await page.getByRole("link", { name: "Danh mục quản lý" }).count(), 0);
      } else if (role === "Admin") {
        await page.getByRole("heading", { name: "Quan ly danh muc" }).waitFor();
        await page.getByRole("link", { name: "Danh mục quản lý" }).waitFor();
        await page.getByRole("button", { name: "Đăng xuất", exact: true }).click();
        await page.waitForURL("**/auth/login?next=**");
        assert.equal(await page.evaluate(() => localStorage.getItem("accessToken")), null);
        assert.equal(loggedOut, true);
      } else {
        await page.getByRole("button", { name: "Thử lại", exact: true }).waitFor();
        assert.equal(await page.getByRole("heading", { name: "Quan ly danh muc" }).count(), 0);
      }
      console.log(`PASS ${role}`);
      await context.close();
    }
  } finally {
    await browser.close();
  }
})().catch((error) => { console.error(error); process.exitCode = 1; });
