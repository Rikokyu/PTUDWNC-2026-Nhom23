import { test, expect } from "@playwright/test";

// UI contract test with intercepted API; does not claim PostgreSQL integration.
test("recipe pagination requests each page, syncs URL and restores browser history", async ({ page }) => {
  const requests: number[] = [];
  await page.route("**/api/v1/recipes?*", async route => {
    const url = new URL(route.request().url());
    const current = Number(url.searchParams.get("page") ?? 1);
    requests.push(current);
    await route.fulfill({ json: {
      items: [{ id: `recipe-${current}`, title: `Recipe page ${current}`, slug: `recipe-${current}`, thumbnailUrl: null,
        categoryName: "Test", categoryId: "test", prepTimeMinutes: 5, cookTimeMinutes: 10,
        servings: 2, difficulty: 1, status: 1, createdAt: "2026-10-01T00:00:00Z" }],
      totalCount: 19, page: current, pageSize: 9, totalPages: 3, hasNextPage: current < 3, hasPreviousPage: current > 1,
    } });
  });
  await page.goto("/recipes");
  await expect(page.getByRole("heading", { name: "Recipe page 1" })).toBeVisible();
  await expect(page.getByRole("button", { name: "Trước", exact: true })).toBeDisabled();
  await page.getByRole("button", { name: "Sau", exact: true }).click();
  await expect(page.getByRole("heading", { name: "Recipe page 2" })).toBeVisible();
  await expect(page).toHaveURL(/page=2/);
  await page.getByRole("button", { name: "Sau", exact: true }).click();
  await expect(page.getByRole("heading", { name: "Recipe page 3" })).toBeVisible();
  await expect(page.getByRole("button", { name: "Sau", exact: true })).toBeDisabled();
  await page.goBack();
  await expect(page.getByRole("heading", { name: "Recipe page 2" })).toBeVisible();
  expect(requests).toEqual(expect.arrayContaining([1, 2, 3]));
});
