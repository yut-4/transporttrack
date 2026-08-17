import { test, expect } from '@playwright/test';

test.describe('Tema Light/Dark/System', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => window.localStorage.removeItem('transporttrack-theme'));
    await page.reload();
  });

  test('aplica tema oscuro y lo persiste', async ({ page }) => {
    await page.locator('.theme-select').selectOption('dark');

    await expect(page.locator('html')).toHaveClass(/dark/);
    expect(await page.evaluate(() => window.localStorage.getItem('transporttrack-theme'))).toBe('dark');

    await page.reload();
    await expect(page.locator('html')).toHaveClass(/dark/);
  });

  test('aplica tema claro', async ({ page }) => {
    await page.locator('.theme-select').selectOption('light');

    await expect(page.locator('html')).not.toHaveClass(/dark/);
    expect(await page.evaluate(() => window.localStorage.getItem('transporttrack-theme'))).toBe('light');
  });

  test('muestra la vista principal con tabs y GPS', async ({ page }) => {
    await expect(page.locator('.header h1')).toContainText('TransportTrack');
    const tabs = page.locator('.tabs button');
    await expect(tabs).toHaveCount(5);
    await expect(tabs.nth(4)).toContainText('GPS');
  });

  test('el tab GPS muestra la página de GPS', async ({ page }) => {
    await page.locator('.tabs button', { hasText: 'GPS' }).click();

    await expect(page.locator('.section-header h2')).toContainText('GPS de conductores');
  });
});