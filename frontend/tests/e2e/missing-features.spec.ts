import { test, expect } from "@playwright/test";
import { randomBytes } from "node:crypto";

test.skip(process.env.CULINARYBLOG_E2E !== "1", "Requires running API, PostgreSQL, Mailpit and production frontend; see report.");

test("homepage, responsive category, search + filters + sort, profile, password and SMTP reset", async ({ page, request }) => {
  const email = `e2e-${Date.now()}@example.com`;
  const original = `Aa1!${randomBytes(16).toString("hex")}`;
  const changed = `Bb2!${randomBytes(16).toString("hex")}`;
  const reset = `Cc3!${randomBytes(16).toString("hex")}`;
  const registration = await request.post("/api/v1/auth/register", { data: { email, password: original, displayName: "Browser Test" } });
  expect(registration.status()).toBe(201);
  const session = await registration.json();
  await page.addInitScript(s => {
    localStorage.setItem("accessToken", s.accessToken);
    localStorage.setItem("authUser", JSON.stringify(s.user));
    localStorage.setItem("accessTokenExpiresAt", s.accessTokenExpiresAt);
  }, session);
  await page.goto("/");
  await expect(page.getByRole("heading", { name: "Công thức mới nhất" })).toBeVisible();
  const cards = page.locator(".recipe-card");
  await expect(cards.first()).toBeVisible();
  const latest = await (await request.get("/api/v1/recipes?sort=-createdAt&pageSize=9")).json();
  await expect(cards.first().getByRole("heading")).toHaveText(latest.items[0].title);
  const categories = await (await request.get("/api/v1/categories")).json();
  const allCategories = Array.isArray(categories) ? categories : categories.data;
  const category = allCategories.find((c: { id: string }) => c.id === latest.items[0].categoryId);
  const listResponse = page.waitForResponse(r => r.url().includes("/api/v1/recipes?") && r.url().includes("categoryId="));
  await page.getByRole("button", { name: category.name, exact: true }).click();
  expect((await listResponse).status()).toBe(200);
  await page.setViewportSize({ width: 390, height: 844 });
  await expect(page.getByRole("complementary", { name: "Danh mục công thức" })).toBeVisible();
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBeTruthy();
  await page.setViewportSize({ width: 1280, height: 900 });
  await page.screenshot({ path: "test-results/homepage.png", fullPage: true });

  const keyword = latest.items[0].title.split(/\s+/).find((word: string) => word.length >= 2)!;
  await page.goto(`/recipes/search?q=${encodeURIComponent(keyword)}`);
  await expect(page.locator(".recipe-card").first()).toBeVisible();
  await page.getByLabel("Danh mục", { exact: true }).selectOption(category.id);
  await page.getByLabel("Độ khó", { exact: true }).selectOption(["", "Easy", "Medium", "Hard", "Expert"][latest.items[0].difficulty]);
  await page.getByLabel("Thời gian nấu tối đa", { exact: true }).fill(String(latest.items[0].cookTimeMinutes));
  const searchResponse = page.waitForResponse(r => r.url().includes("/api/v1/recipes/search") && r.url().includes("sort=-title"));
  await page.getByRole("combobox", { name: "Sắp xếp", exact: true }).selectOption("-title");
  const searchResult = await searchResponse;
  expect(searchResult.status()).toBe(200);
  const matches = await searchResult.json();
  expect(matches.items.length).toBeGreaterThan(0);
  expect(matches.items.every((r: { categoryId: string; difficulty: number; cookTimeMinutes: number }) => r.categoryId === category.id && r.difficulty === latest.items[0].difficulty && r.cookTimeMinutes <= latest.items[0].cookTimeMinutes)).toBeTruthy();

  await page.goto("/profile");
  const avatarUploaded = page.waitForResponse(r => r.url().endsWith("/api/v1/auth/me/avatar"));
  await page.getByLabel("Tải ảnh đại diện", { exact: true }).setInputFiles({
    name: "avatar.png", mimeType: "image/png",
    buffer: Buffer.from("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aA1sAAAAASUVORK5CYII=", "base64"),
  });
  expect((await avatarUploaded).status()).toBe(200);
  await expect(page.getByRole("img", { name: "Ảnh đại diện", exact: true })).toBeVisible();
  await page.getByLabel("Họ tên", { exact: true }).fill("Browser Updated");
  await page.getByRole("textbox", { name: "Giới thiệu", exact: true }).fill("Saved through browser and PostgreSQL");
  const savedProfile = page.waitForResponse(r => r.url().endsWith("/api/v1/auth/me") && r.request().method() === "PATCH");
  await page.getByRole("button", { name: "Lưu hồ sơ", exact: true }).click();
  expect((await savedProfile).status()).toBe(200);
  await expect(page.getByRole("status")).toHaveText("Đã lưu hồ sơ.");
  await expect(page.getByLabel("Họ tên", { exact: true })).toHaveValue("Browser Updated");
  await page.reload();
  await expect(page.getByRole("textbox", { name: "Giới thiệu", exact: true })).toHaveValue("Saved through browser and PostgreSQL");
  await page.getByRole("link", { name: "Đổi mật khẩu", exact: true }).click();
  await page.getByLabel("Mật khẩu hiện tại", { exact: true }).fill(original);
  await page.getByLabel("Mật khẩu mới", { exact: true }).fill(changed);
  await page.getByLabel("Xác nhận mật khẩu mới", { exact: true }).fill(changed);
  await page.getByRole("button", { name: "Cập nhật mật khẩu", exact: true }).click();
  await expect(page).toHaveURL(/login\?passwordChanged=1/);
  expect((await request.get("/api/v1/auth/me", { headers: { Authorization: `Bearer ${session.accessToken}` } })).status()).toBe(401);
  expect((await request.post("/api/v1/auth/login", { data: { email, password: changed } })).status()).toBe(200);

  await page.goto("/forgot-password");
  await page.getByLabel("Email", { exact: true }).fill(email);
  await page.getByRole("button", { name: "Gửi liên kết đặt lại" }).click();
  await expect(page.getByRole("status").filter({ hasText: "Nếu tài khoản tồn tại" })).toBeVisible();
  const mailbox = process.env.E2E_MAILPIT_URL ?? "http://localhost:8025";
  let link = "";
  await expect.poll(async () => {
    const messages = await (await request.get(`${mailbox}/api/v1/messages`)).json();
    const message = messages.messages.find((m: { To: { Address: string }[] }) => m.To.some(t => t.Address === email));
    if (!message) return false;
    const detail = await (await request.get(`${mailbox}/api/v1/message/${message.ID}`)).json();
    link = detail.Text.match(/https?:\/\/[^\s]+/)?.[0] ?? "";
    return !!link;
  }).toBeTruthy();
  await page.goto(link);
  await page.getByLabel("Mật khẩu mới", { exact: true }).fill(reset);
  await page.getByLabel("Xác nhận mật khẩu mới", { exact: true }).fill(reset);
  await page.getByRole("button", { name: "Cập nhật mật khẩu", exact: true }).click();
  await expect(page.getByRole("status")).toContainText("Mật khẩu đã cập nhật");
  expect((await request.post("/api/v1/auth/login", { data: { email, password: reset } })).status()).toBe(200);
});
