import { expect } from '@playwright/test';
import { test } from '../fixtures/client-fixture';

test('L2-046: the client application boots and shows nothing without an event link', async ({ page }) => {
  const errors: string[] = [];
  page.on('pageerror', error => errors.push(error.message));
  page.on('console', message => { if (message.type() === 'error') errors.push(message.text()); });

  await page.goto('/');

  await expect(page.locator('app-root')).toBeAttached();
  expect(errors).toEqual([]);
});
