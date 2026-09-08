import { expect } from '@playwright/test';
export class AdminDemoPage {
  constructor(page) { this.page = page; }
  async showField(label) {
    await this.page.getByLabel(label, { exact: true }).evaluate(el => el.scrollIntoView({ block: 'center' }));
  }
  async showStages() {
    await expect(this.page.getByRole('button', { name: 'Edit Arrival and welcome information', exact: true })).toBeVisible();
    await this.page.getByRole('heading', { name: 'Stages and content', exact: true }).evaluate(el => el.scrollIntoView({ block: 'start' }));
    await expect(this.page.getByRole('button', { name: /^Edit / })).toHaveCount(21);
  }
}
