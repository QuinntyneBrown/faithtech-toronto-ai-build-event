import { test } from '@playwright/test';
import { AdminAccessPage } from '../page-objects/admin-access-page';

test('L2-038: provisioned administrator signs in and signs out', async ({ page }) => {
  const access = new AdminAccessPage(page);
  await access.open();
  await access.signIn('host@example.com', 'host-demo');
  await access.expectSignedIn();
  await access.signOut();
  await access.expectSignIn();
});

test('L2-038: invalid credentials retain the access screen', async ({ page }) => {
  const access = new AdminAccessPage(page);
  await access.open();
  await access.signIn('unknown', 'incorrect');
  await access.expectDenied();
});
