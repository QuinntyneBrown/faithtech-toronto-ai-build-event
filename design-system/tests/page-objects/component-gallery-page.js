import { expect } from '@playwright/test';
const specimen=`<main><button class="cs-button">Continue</button><button class="cs-button cs-button--secondary">Cancel</button><button class="cs-button cs-button--small">Small action</button><button class="cs-button" disabled>Unavailable</button><label class="cs-field"><span class="cs-label">Full name</span><input class="cs-input" value="Alex Morgan"><span class="cs-hint">As your team will see it.</span></label><article class="cs-card"><h2>Build with purpose.</h2><p>A little curiosity goes a long way.</p><span class="cs-pill">LIVE</span></article><div class="cs-alert cs-tone--success">Your changes are saved.</div><span class="avatar">AM</span><label class="choice choice-card"><input type="radio" checked><span>Listen to the community</span></label></main>`;
const properties=['color','backgroundColor','fontFamily','fontSize','fontWeight','lineHeight','paddingTop','paddingRight','paddingBottom','paddingLeft','marginTop','marginBottom','borderTopWidth','borderTopColor','borderRadius','boxShadow','minBlockSize','height','opacity'];
export class ComponentGalleryPage {
  constructor(page){this.page=page;}
  async specimen(css){await this.page.setContent(`<style>${css}</style>${specimen}`);}
  async styles(){return this.page.locator('.cs-button,.cs-input,.cs-card,.cs-pill,.cs-alert,.avatar,.choice-card').evaluateAll((els,props)=>els.map(el=>Object.fromEntries(props.map(p=>[p,getComputedStyle(el)[p]]))),properties);}
  async open(){await this.page.goto('/design-system/');await expect(this.page.getByRole('heading',{name:/A common language/})).toBeVisible();}
  async openDialog(){await this.page.getByRole('button',{name:'Open dialog',exact:true}).click();await expect(this.page.getByRole('dialog')).toBeVisible();}
}
