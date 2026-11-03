import { test, expect } from "@playwright/test";
import { randomBytes } from "node:crypto";

test.skip(process.env.CULINARYBLOG_E2E !== "1", "Requires isolated runtime API/PostgreSQL/MinIO.");

test("Lab8 real MinIO upload, recipe workflow, ownership and user directory", async ({ page, request }) => {
  const adminPassword = process.env.E2E_ADMIN_PASSWORD;
  if (!adminPassword || adminPassword === "<placeholder>") {
    throw new Error("Missing E2E_ADMIN_PASSWORD. Configure it in frontend/.env.local or the process environment.");
  }
  const password = `Aa1!${randomBytes(16).toString("hex")}`;
  const register = await request.post("/api/v1/auth/register", { data: { email: `lab8-${Date.now()}@example.com`, password, displayName: "Lab8 Runtime Author" } });
  expect(register.status()).toBe(201);
  const session = await register.json();
  const headers = { Authorization: `Bearer ${session.accessToken}` };
  const categoriesResult = await (await request.get("/api/v1/categories")).json();
  const category = (Array.isArray(categoriesResult) ? categoriesResult : categoriesResult.data)[0];
  const title = `Lab8 verified recipe ${Date.now()}`;
  const payload = { title, description: "Recipe used exclusively for runtime verification.", categoryId: category.id,
    prepTimeMinutes: 10, cookTimeMinutes: 20, servings: 2, difficulty: 1,
    ingredients: [{ name: "Rice", quantity: "1", unit: "cup" }], steps: [{ title: "Cook", description: "Cook rice until tender." }] };
  const create = await request.post("/api/v1/recipes", { headers, data: payload });
  expect(create.status()).toBe(201);
  let recipe = (await create.json()).data;
  expect(recipe.status).toBe(0);
  expect((await request.get(`/api/v1/recipes/${recipe.slug}`)).status()).toBe(403);
  const png = Buffer.from("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jRZkAAAAASUVORK5CYII=", "base64");
  expect((await request.post(`/api/v1/recipes/${recipe.id}/images`, { multipart: { file: { name: "test.png", mimeType: "image/png", buffer: png } } })).status()).toBe(401);
  const invalid = await request.post(`/api/v1/recipes/${recipe.id}/images`, { headers, multipart: { file: { name: "fake.png", mimeType: "image/png", buffer: Buffer.from("not an image") } } });
  expect([400, 422]).toContain(invalid.status());
  const upload = await request.post(`/api/v1/recipes/${recipe.id}/images`, { headers, multipart: { file: { name: "test.png", mimeType: "image/png", buffer: png }, altText: "Lab8 real MinIO image" } });
  expect(upload.status()).toBe(201);
  const image = (await upload.json()).data;
  const object = await request.get(image.originalUrl);
  expect(object.status()).toBe(200);
  expect(await object.body()).toEqual(png);
  const detail = await (await request.get(`/api/v1/recipes/${recipe.slug}`, { headers })).json();
  recipe = detail.data ?? detail;
  const update = await request.put(`/api/v1/recipes/${recipe.id}`, { headers, data: { ...payload, title: `${title} updated`, rowVersion: recipe.rowVersion } });
  expect(update.status()).toBe(200);
  const replacement = await request.post(`/api/v1/recipes/${recipe.id}/images`, { headers, multipart: { file: { name: "replacement.png", mimeType: "image/png", buffer: png }, isPrimary: "true", altText: "Replacement MinIO image" } });
  expect(replacement.status()).toBe(201);
  expect((await request.patch(`/api/v1/recipes/${recipe.id}/publish`, { headers })).status()).toBe(200);
  await page.addInitScript(s => {
    localStorage.setItem("accessToken", s.accessToken); localStorage.setItem("authUser", JSON.stringify(s.user));
    localStorage.setItem("accessTokenExpiresAt", s.accessTokenExpiresAt);
  }, session);
  await page.goto(`/recipes/${recipe.slug}`);
  await expect(page.getByRole("heading", { name: `${title} updated`, exact: true })).toBeVisible();
  const visibleImage = page.locator(`img[src*="${new URL(image.originalUrl).pathname.split('/')[1]}"]`).first();
  await expect(visibleImage).toBeVisible();
  expect(await visibleImage.evaluate((img: HTMLImageElement) => img.complete && img.naturalWidth > 0)).toBeTruthy();
  const authors = await request.get("/api/v1/users", { headers });
  expect(authors.status()).toBe(200);
  for (const user of (await authors.json()).items) { expect(user.email).toBeNull(); expect(user.role).toBe("Author"); expect(user.passwordHash).toBeUndefined(); }
  await page.goto("/dashboard/users");
  await expect(page.getByRole("heading", { name: "Tác giả", exact: true })).toBeVisible();
  const adminLogin = await request.post("/api/v1/auth/login", { data: { email: "admin@culinaryblog.com", password: adminPassword } });
  expect(adminLogin.status()).toBe(200);
  const admin = await adminLogin.json();
  const admins = await request.get("/api/v1/users?role=Admin", { headers: { Authorization: `Bearer ${admin.accessToken}` } });
  expect(admins.status()).toBe(200);
  expect((await admins.json()).items[0].email).toBeTruthy();
  const adminPage = await page.context().newPage();
  await adminPage.goto("/");
  await adminPage.evaluate(s => {
    localStorage.setItem("accessToken", s.accessToken); localStorage.setItem("authUser", JSON.stringify(s.user));
    localStorage.setItem("accessTokenExpiresAt", s.accessTokenExpiresAt);
  }, admin);
  await adminPage.goto("/dashboard/users");
  await expect(adminPage.getByRole("heading", { name: "Người dùng", exact: true })).toBeVisible();
  await adminPage.getByLabel("Vai trò", { exact: true }).selectOption("Admin");
  await expect(adminPage.getByText("admin@culinaryblog.com", { exact: true })).toBeVisible();
  await adminPage.close();
  const stranger = await (await request.post("/api/v1/auth/register", { data: { email: `other-${Date.now()}@example.com`, password, displayName: "Other Author" } })).json();
  expect((await request.patch(`/api/v1/recipes/${recipe.id}/archive`, { headers: { Authorization: `Bearer ${stranger.accessToken}` } })).status()).toBe(403);
  expect((await request.patch(`/api/v1/recipes/${recipe.id}/archive`, { headers })).status()).toBe(200);
  expect((await request.get(`/api/v1/recipes/${recipe.slug}`)).status()).toBe(403);
  expect((await request.patch(`/api/v1/recipes/${recipe.id}/publish`, { headers })).status()).toBe(422);
  expect((await request.delete(`/api/v1/recipes/${recipe.id}`, { headers })).status()).toBe(204);
  expect((await request.get(`/api/v1/recipes/${recipe.slug}`, { headers })).status()).toBe(404);
});

test("browser creates recipe with image and opens persisted edit form", async ({ page, request }) => {
  const registration = await request.post("/api/v1/auth/register", { data: { email: `ui-lab8-${Date.now()}@example.com`,
    password: `Aa1!${randomBytes(16).toString("hex")}`, displayName: "Browser Lab8 Author" } });
  expect(registration.status()).toBe(201);
  const session = await registration.json();
  await page.addInitScript(s => {
    localStorage.setItem("accessToken", s.accessToken); localStorage.setItem("authUser", JSON.stringify(s.user));
    localStorage.setItem("accessTokenExpiresAt", s.accessTokenExpiresAt);
  }, session);
  await page.goto("/dashboard/recipes/new");
  const title = `Browser MinIO recipe ${Date.now()}`;
  await page.getByLabel("Tên món ăn *", { exact: true }).fill(title);
  await page.getByPlaceholder("Giới thiệu điểm đặc biệt, hương vị chính hoặc nguồn gốc của món ăn...").fill("Browser creates this recipe using the real API and PostgreSQL.");
  await page.getByPlaceholder("Tên nguyên liệu").first().fill("Rice");
  await page.getByPlaceholder("Số lượng").first().fill("1");
  await page.getByPlaceholder("Mô tả chi tiết các thao tác trong bước 1...").fill("Cook rice until done.");
  await page.locator('input[type="file"]').setInputFiles({ name: "browser.png", mimeType: "image/png",
    buffer: Buffer.from("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jRZkAAAAASUVORK5CYII=", "base64") });
  await expect(page.getByAltText("Ảnh xem trước", { exact: true })).toBeVisible();
  const uploaded = page.waitForResponse(response => response.request().method() === "POST" && /\/images$/.test(new URL(response.url()).pathname));
  await page.getByRole("button", { name: "Đăng công thức bài viết" }).click();
  expect((await uploaded).status()).toBe(201);
  await expect(page).toHaveURL(/\/dashboard\/recipes\/[^/]+\/edit$/);
  await expect(page.getByRole("textbox").first()).toBeVisible();
  // The editor loaded persisted content and MinIO's URL is present in its image grid.
  await expect(page.locator('img[src*="localhost:9000/"]').first()).toBeVisible();
  await page.getByLabel("Tên công thức", { exact: true }).fill(`${title} edited in browser`);
  const updated = page.waitForResponse(response => response.request().method() === "PUT" && /\/recipes\/[^/]+$/.test(new URL(response.url()).pathname));
  await page.getByRole("button", { name: "Lưu thông tin công thức", exact: true }).click();
  expect((await updated).status()).toBe(200);
  await expect(page.getByRole("heading", { name: `Quản lý nội dung: ${title} edited in browser` })).toBeVisible();
  await page.reload();
  await expect(page.getByLabel("Tên công thức", { exact: true })).toHaveValue(`${title} edited in browser`);
});
