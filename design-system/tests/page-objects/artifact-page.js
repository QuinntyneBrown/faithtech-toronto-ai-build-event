import { expect } from '@playwright/test';
export class ArtifactPage {
  constructor(page){this.page=page;this.failures=[];page.on('pageerror',e=>this.failures.push(e.message));page.on('response',r=>{if(r.status()>=400)this.failures.push(`${r.status()} ${r.url()}`);});}
  async open(screen,params={}){await this.page.goto('/docs/mocks/?'+new URLSearchParams({screen,...params}));await expect(this.page.locator('#main h1')).toBeVisible();}
  async rendered(){await expect(this.page.locator('#main')).not.toContainText('This page has wandered off.');expect(this.failures).toEqual([]);}
  async noOverflow(){expect(await this.page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth+1)).toBe(true);}
  async dialog(){await expect(this.page.getByRole('dialog')).toBeVisible();await expect(this.page.getByRole('dialog').getByRole('heading')).toBeVisible();}
  async dismiss(){await this.page.keyboard.press('Escape');await expect(this.page.getByRole('dialog')).not.toBeVisible();}
  async screenshot(name){await this.page.screenshot({path:`../docs/mocks/screenshots/${name}.png`,fullPage:true});}
}
