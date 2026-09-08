import { expect } from '@playwright/test';
import { ArtifactPage } from './artifact-page.js';
export class ReviewPage extends ArtifactPage {
  main(){return this.page.locator('#main');}
  overlay(){return this.page.getByRole('dialog');}
  async fill(label,value,dialog=false){await (dialog?this.overlay():this.main()).getByRole('textbox',{name:label,exact:true}).fill(value);}
  async field(label,value,dialog=false){await (dialog?this.overlay():this.main()).getByLabel(label,{exact:false}).fill(value);}
  async press(name,dialog=false){await (dialog?this.overlay():this.main()).getByRole('button',{name,exact:true}).first().click();}
  async follow(name){await this.main().getByRole('link',{name,exact:true}).first().click();}
  async choose(label,value,dialog=false){await (dialog?this.overlay():this.main()).getByLabel(label,{exact:false}).selectOption(value);}
  async toggle(label,value){await this.main().getByRole('checkbox',{name:label}).setChecked(value);}
  async sees(text){await expect(this.main().getByText(text,{exact:true}).first()).toBeVisible();}
  async error(text){await expect(this.page.getByRole('alert').filter({hasText:text}).first()).toBeVisible();}
  async hasLink(name,visible){const l=this.main().getByRole('link',{name,exact:true});if(visible)await expect(l).toBeVisible();else await expect(l).toHaveCount(0);}
  async answer(index){await this.main().getByRole('radio').nth(index).check();await this.press('Submit answer');}
  async reviewClock(time,speed='60'){await this.page.getByText(/Design review controls/).click();await this.page.getByLabel('Simulated time').fill(time);await this.page.getByLabel('Clock speed').selectOption(speed);await this.page.getByRole('button',{name:'Play scheduled flow'}).click();}
  async drawn(){await expect(this.page).toHaveURL(/state=winner/);await expect(this.main().getByRole('heading',{name:/Alex Morgan|Sarah Chen/})).toBeVisible();}
  async draft(label,value){await expect(this.main().getByRole('textbox',{name:label,exact:true})).toHaveValue(value);}
  async dialogValue(label,value){await expect(this.overlay().getByLabel(label)).toHaveValue(value);}
}
