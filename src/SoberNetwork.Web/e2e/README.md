# Playwright E2E Testing Guide

End-to-end tests for Sober Network's Angular frontend using [Playwright](https://playwright.dev/).

## Quick Start

### Prerequisites
- Node 22+
- Playwright installed (`npm install --save-dev @playwright/test` — already done)
- Angular dev server running on `http://localhost:4200`

### Running Tests

```bash
# Run all E2E tests (requires dev server running)
npm run e2e

# Run tests with interactive UI mode (recommended for development)
npm run e2e:ui

# Debug mode (step through tests)
npm run e2e:debug

# View the HTML test report
npm run e2e:report

# Run tests in a specific browser
npx playwright test --project=chromium
npx playwright test --project="Mobile Chrome"

# Run a single test file
npx playwright test e2e/landing.spec.ts

# Run tests matching a pattern
npx playwright test -g "Auth Flow"
```

## Test Structure

Tests are organized by feature in `e2e/` directory:

- **`landing.spec.ts`** — Landing page load, hero section, header visibility
- **`auth.spec.ts`** — Login/register modals, form validation, navigation

## Writing Tests

### Basic Test Template

```typescript
import { test, expect } from '@playwright/test';

test.describe('Feature Name', () => {
  test('should do something specific', async ({ page }) => {
    // Arrange
    await page.goto('/');
    
    // Act
    const button = page.locator('button', { hasText: /Click me/i });
    await button.click();
    
    // Assert
    const modal = page.locator('[role="dialog"]');
    await expect(modal).toBeVisible();
  });
});
```

### Key Patterns

**Find elements by text (case-insensitive):**
```typescript
const button = page.locator('button', { hasText: /Sign In/i });
```

**Wait for element and interact:**
```typescript
await page.locator('input[type="email"]').fill('user@example.com');
await page.locator('button[type="submit"]').click();
```

**Check visibility:**
```typescript
await expect(page.locator('h1')).toBeVisible();
```

**Check text content:**
```typescript
await expect(page.locator('h1')).toHaveText('Welcome');
```

**Take screenshots (automatic on failure):**
- Playwright captures screenshots on test failure automatically
- View in `playwright-report/`

## Configuration

### `playwright.config.ts`

Key settings:
- **`baseURL`** — `http://localhost:4200`
- **`trace`** — `on-first-retry` (captures trace on first retry)
- **`screenshot`** — `only-on-failure` (captures on error)
- **`webServer`** — auto-starts `npm start` before tests if not already running
- **Projects** — tests run on Chromium, Firefox, WebKit, and mobile viewports

### Mobile Testing

Playwright automatically tests on:
- Desktop Chrome, Firefox, Safari
- Mobile Chrome (Pixel 5)
- Mobile Safari (iPhone 12)

To run on mobile only:
```bash
npx playwright test --project="Mobile Chrome" --project="Mobile Safari"
```

## Best Practices

1. **Use semantic locators** — prefer `[role="dialog"]`, `button`, `[aria-label]` over CSS selectors
2. **Use `hasText` for accessibility** — `{ hasText: /Sign In/i }` is more robust than class names
3. **Wait implicitly** — Playwright waits up to 30s for elements by default
4. **Test user flows** — not implementation details (avoid testing internal state or private methods)
5. **One assertion per test section** — use `test.describe` blocks for related tests
6. **Keep tests DRY** — use fixtures for common setup (auth, page state)

## CI/CD Integration

In GitHub Actions (`.github/workflows/deploy.yml`):

```yaml
- name: Run E2E tests
  run: cd src/SoberNetwork.Web && npm run e2e
```

Tests require the backend API to be running on `:5000` and forwarding requests to Angular on `:4200`.

## Debugging

### Use Debug Mode

```bash
npm run e2e:debug
```

- Opens Playwright Inspector
- Step through tests, inspect DOM
- View console logs

### Inspect Network Traffic

```typescript
page.on('request', request => console.log(request.url()));
page.on('response', response => console.log(response.url(), response.status()));
```

### Enable Verbose Logging

```bash
npx playwright test --project=chromium --verbose
```

## Common Issues

**Test timeout (30s)**
- Increase timeout: `test.setTimeout(60 * 1000);` (60 seconds)
- Add explicit waits: `await page.waitForTimeout(1000);`

**Element not found**
- Use `page.locator()` inspector: `await page.locator('...').inspect();`
- Check network tab in test report for failed requests

**Flaky tests (intermittent failures)**
- Avoid hardcoded `waitForTimeout()` — use `waitForSelector()` or `toBeVisible()`
- Ensure tests don't depend on execution order

**Tests pass locally but fail in CI**
- Run tests without `--headed` (headless mode, used in CI)
- Check viewport size (CI uses different defaults)
- Verify env vars or test data differences

## Next Steps

1. Add authentication helper to auto-login in tests (avoid repeated login flows)
2. Add test data fixtures (create groups, meetings, users for consistent test state)
3. Add visual regression tests (screenshot diffs)
4. Expand coverage to core workflows (creating groups, meetings, joining)

## Resources

- [Playwright Docs](https://playwright.dev/docs/intro)
- [Best Practices](https://playwright.dev/docs/best-practices)
- [Debugging Guide](https://playwright.dev/docs/debug)
- [CI Configuration](https://playwright.dev/docs/ci)
