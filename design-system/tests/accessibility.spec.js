import { test,expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { ArtifactPage } from './page-objects/artifact-page.js';
import { screens,dialogs } from '../../docs/mocks/inventory.js';
test('Every main screen passes the WCAG A and AA browser audit',async({page})=>{
  test.setTimeout(180000);const artifact=new ArtifactPage(page);
  for(const [screen] of screens){await test.step(screen,async()=>{await artifact.open(screen);const result=await new AxeBuilder({page}).withTags(['wcag2a','wcag2aa','wcag21aa']).analyze();expect(result.violations.map(v=>({id:v.id,nodes:v.nodes.map(n=>n.failureSummary)}))).toEqual([]);});}
});
test('Form and confirmation dialogs pass the WCAG browser audit',async({page})=>{
  test.setTimeout(180000);const artifact=new ArtifactPage(page);
  for(const [dialog,,screen,item] of dialogs){await test.step(dialog,async()=>{await artifact.open(screen,{dialog,item});const result=await new AxeBuilder({page}).withTags(['wcag2a','wcag2aa','wcag21aa']).analyze();expect(result.violations.map(v=>({id:v.id,nodes:v.nodes.map(n=>n.failureSummary)}))).toEqual([]);});}
});
