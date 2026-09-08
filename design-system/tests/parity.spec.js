import { test,expect } from '@playwright/test';
import { readFile } from 'node:fs/promises';
import { ComponentGalleryPage } from './page-objects/component-gallery-page.js';
const read=name=>readFile(new URL(name,import.meta.url),'utf8');
test('Local primitives match the captured Cornerstone light reference',async({page})=>{
  const reference=await read('./reference/cornerstone-light.css');
  const local=(await Promise.all(['../tokens.css','../theme.css','../patterns.css'].map(read))).join('\n');
  const context='body{line-height:1.6} main{width:500px;padding:24px} h2,p{margin:0 0 16px} h2{font-size:28px;line-height:1.12;letter-spacing:-.045em}';
  const gallery=new ComponentGalleryPage(page);await gallery.specimen(reference+context);const expected=await gallery.styles();
  await page.screenshot({path:'../docs/mocks/screenshots/cornerstone-reference.png',fullPage:true});
  await gallery.specimen(local+context);const actual=await gallery.styles();
  await page.screenshot({path:'../docs/mocks/screenshots/local-components.png',fullPage:true});
  expect(actual).toEqual(expected);
});
test('Standalone gallery opens an accessible dialog',async({page})=>{const gallery=new ComponentGalleryPage(page);await gallery.open();await gallery.openDialog();});
