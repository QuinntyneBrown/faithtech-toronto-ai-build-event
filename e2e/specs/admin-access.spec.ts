import { test } from '../fixtures/session-fixture';
import { AdminAccessPage } from '../page-objects/admin-access-page';
import { AdminSessionPage } from '../page-objects/admin-session-page';

test('L2-038: provisioned administrator signs in and signs out', async ({ page }) => {
  const access = new AdminAccessPage(page);
  await access.open();
  await access.signIn('host@example.com', 'host-demo');
  const session = new AdminSessionPage(page);
  await session.expectSignedIn();
  await session.signOut();
  await access.expectSignIn();
});

test('L2-036: keyboard-only administrator access', async ({ page }) => {
  const access = new AdminAccessPage(page);
  await access.open();
  await access.keyboardSignIn();
  await new AdminSessionPage(page).expectSignedIn();
});

for (const width of [320, 575, 576, 767, 768, 991, 992, 1199, 1200, 1440]) {
  test(`L2-035/036: administrator access is readable and accessible at ${width}px`, async ({ page }, testInfo) => {
    const access = new AdminAccessPage(page);
    await access.useViewport(width);
    await access.open();
    await access.expectAccessible();
    await access.capture(testInfo.outputPath(`admin-access-${width}.png`));
  });
}

test('L2-038: invalid credentials retain the access screen', async ({ page }) => {
  const access = new AdminAccessPage(page);
  await access.open();
  await access.signIn('unknown', 'incorrect');
  await access.expectDenied();
});
