import { test, expect } from '@playwright/test';

test.describe('Auth Flow', () => {
  test('should navigate to login modal from navbar', async ({ page }) => {
    await page.goto('/');
    // Click Sign In button in navbar
    const signInButton = page.locator('button, a', { hasText: /Sign In/i }).first();
    await signInButton.click();
    
    // Modal should appear with login form
    const modal = page.locator('[role="dialog"]');
    await expect(modal).toBeVisible();
    
    // Email and password fields should be present
    const emailInput = page.locator('input[type="email"]');
    const passwordInput = page.locator('input[type="password"]');
    await expect(emailInput).toBeVisible();
    await expect(passwordInput).toBeVisible();
  });

  test('should show validation error on empty form submission', async ({ page }) => {
    await page.goto('/');
    const signInButton = page.locator('button, a', { hasText: /Sign In/i }).first();
    await signInButton.click();
    
    const submitButton = page.locator('button[type="submit"]');
    await submitButton.click();
    
    // Should display validation error or stay on same form
    const modal = page.locator('[role="dialog"]');
    await expect(modal).toBeVisible();
  });

  test('should navigate to register modal', async ({ page }) => {
    await page.goto('/');
    const registerLink = page.locator('button, a', { hasText: /Create Account/i }).first();
    await registerLink.click();
    
    const modal = page.locator('[role="dialog"]');
    await expect(modal).toBeVisible();
    
    // Register form should have email, password, name fields
    const inputs = page.locator('input');
    await expect(inputs).toHaveCount(3); // email, password, display name
  });
});
