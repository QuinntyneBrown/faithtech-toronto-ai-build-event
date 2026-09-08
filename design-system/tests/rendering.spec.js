import { test,expect } from '@playwright/test';
import { ArtifactPage } from './page-objects/artifact-page.js';
import { readFile } from 'node:fs/promises';
// Read the artifact inventory as review cases; tests assert visible browser behavior, not repository structure.
const catalog=await readFile(new URL('../../docs/mocks/catalog.js',import.meta.url),'utf8');
const inventory=await import('data:text/javascript,'+encodeURIComponent(catalog.slice(catalog.indexOf('export const screens='),catalog.indexOf('export function catalog'))));
for(const width of [320,768,1024,1440]){
  test(`Every screen and state renders at ${width}px`,async({page})=>{
    test.setTimeout(240000);await page.setViewportSize({width,height:900});const artifact=new ArtifactPage(page);
    for(const [screen,,,[...states]] of inventory.screens){for(const state of states){await test.step(`${screen} / ${state}`,async()=>{await artifact.open(screen,{state});if(screen!=='not-found')await artifact.rendered();await artifact.noOverflow();});}}
    expect(artifact.failures).toEqual([]);
  });
}
test('Every dialog opens in context and dismisses with Escape',async({page})=>{
  test.setTimeout(180000);await page.setViewportSize({width:390,height:844});const artifact=new ArtifactPage(page);
  for(const [dialog,,screen,item] of inventory.dialogs){await test.step(`${dialog} / ${item}`,async()=>{await artifact.open(screen,{dialog,item});await artifact.dialog();await artifact.noOverflow();await artifact.dismiss();});}
  expect(artifact.failures).toEqual([]);
});
test('All surfaces remain light when the operating system requests dark',async({page})=>{
  await page.emulateMedia({colorScheme:'dark'});const artifact=new ArtifactPage(page);await artifact.open('admin-overview');
  expect(await page.evaluate(()=>getComputedStyle(document.documentElement).colorScheme)).toBe('light');await artifact.rendered();
});
