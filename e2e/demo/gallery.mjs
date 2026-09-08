import { record } from './capture.mjs';

export const galleryLines = [
  'Faith Tech brings builders together around a shared purpose. This independent design system gives the event experience a consistent visual language.',
  'The light palette pairs paper and raised surfaces with dark ink. Lime highlights actions, while distinct colours communicate information, success, warning, and errors.',
  'Typography creates a clear hierarchy, from a strong invitation to supporting instructions. These foundations are shared design tokens, owned by this independent site.',
  'Primary, secondary, and quieter actions share consistent shapes. The unavailable example is disabled. Opening a real dialog shows how the controls work together.',
  'The dialog keeps keyboard interaction inside the decision. Escape closes it and returns focus to the action that opened it.',
  'Inputs combine visible labels, helpful context, and an explicit error message. Here, the name and event stage can be changed using the real native controls.',
  'Cards group related content. Success, error, and informational feedback remain distinguishable, and skeletons communicate a loading state.',
  'At a narrow viewport, the same foundations reflow into a single readable experience. The gallery stands alone and can be built and deployed independently of the event applications.',
];
export async function gallery(directory, baseURL) {
  return record('design-system', directory, galleryLines, async ({ page, say, expect }) => {
    await page.goto('/design-system/');
    await expect(page.getByRole('heading', { name: 'A common language. A shared purpose.' })).toBeVisible();
    await expect.poll(() => page.locator('img').evaluateAll(images => images.every(i => i.complete && i.naturalWidth > 0))).toBe(true);
    await say(0, 'A shared visual language');
    await page.getByRole('heading', { name: '01 / Colour' }).evaluate(el => el.scrollIntoView({ block: 'start' })); await say(1, 'Colour');
    await page.getByRole('heading', { name: '02 / Typography' }).evaluate(el => el.scrollIntoView({ block: 'start' })); await say(2, 'Typography');
    const primary = page.getByRole('button', { name: 'Primary action', exact: true });
    await primary.evaluate(el => el.scrollIntoView({ block: 'center' })); await expect(page.getByRole('button', { name: 'Unavailable', exact: true })).toBeDisabled();
    await say(3, 'Actions'); await primary.click();
    await expect(page.getByRole('dialog')).toBeVisible(); await say(4, 'Keyboard dialog');
    await page.keyboard.press('Escape'); await expect(primary).toBeFocused();
    await page.getByLabel('Full name', { exact: false }).fill('Jordan Lee');
    await page.getByLabel('Event stage').selectOption('Discover');
    await expect(page.getByLabel('Event stage')).toHaveValue('Discover');
    await page.getByRole('heading', { name: '04 / Inputs' }).evaluate(el => el.scrollIntoView({ block: 'start' })); await say(5, 'Inputs');
    await page.getByRole('heading', { name: '05 / Surfaces & feedback' }).evaluate(el => el.scrollIntoView({ block: 'start' })); await say(6, 'Feedback');
    await page.setViewportSize({ width: 640, height: 720 }); await page.evaluate(() => scrollTo(0, 0));
    await expect.poll(() => page.evaluate(() => document.documentElement.scrollWidth <= innerWidth)).toBe(true);
    await say(7, 'Responsive and independent');
  }, { baseURL });
}
