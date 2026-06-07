import { test, expect } from '@playwright/test';

test.describe('Landing Page', () => {
  test('should load the landing page', async ({ page }) => {
    await page.goto('/');
    await expect(page).toHaveTitle(/Sober Network/i);
  });

  test('should display the header', async ({ page }) => {
    await page.goto('/');
    const header = page.locator('header, nav');
    await expect(header).toBeVisible();
  });

  test('should display the hero section', async ({ page }) => {
    await page.goto('/');
    const hero = page.locator('section').first();
    await expect(hero).toBeVisible();
  });
});
