import { chromium } from '@playwright/test';
import { mkdir,writeFile } from 'node:fs/promises';
import { screens,dialogs } from '../../docs/mocks/inventory.js';
const directory=new URL('../../docs/mocks/screenshots/',import.meta.url);
await mkdir(directory,{recursive:true});const browser=await chromium.launch({channel:'chrome'});
const page=await browser.newPage({viewport:{width:1440,height:1000},reducedMotion:'reduce'});
const entries=[];
for(const [screen,label] of screens){await page.goto('http://127.0.0.1:4317/docs/mocks/?'+new URLSearchParams({screen}));await page.locator('#main h1').waitFor();await page.screenshot({path:new URL(screen+'.png',directory).pathname.replace(/^\/([A-Z]:)/,'$1'),fullPage:true});entries.push(`- [${label}](${screen}.png)`);}
await page.setViewportSize({width:390,height:844});
for(const screen of ['access','countdown','teams','project','messages','admin-overview']){await page.goto('http://127.0.0.1:4317/docs/mocks/?screen='+screen);await page.locator('#main h1').waitFor();await page.screenshot({path:new URL(screen+'-mobile.png',directory).pathname.replace(/^\/([A-Z]:)/,'$1'),fullPage:true});entries.push(`- [${screen} · mobile](${screen}-mobile.png)`);}
await page.setViewportSize({width:1024,height:900});
for(const [dialog,label,screen,item] of dialogs.filter(([d,,s,item])=>item||['assign','draw','unsaved'].includes(d))){await page.goto('http://127.0.0.1:4317/docs/mocks/?'+new URLSearchParams({screen,dialog,item}));await page.getByRole('dialog').waitFor();await page.screenshot({path:new URL('dialog-'+dialog+'.png',directory).pathname.replace(/^\/([A-Z]:)/,'$1')});entries.push(`- [${label}](${`dialog-${dialog}.png`})`);}
await writeFile(new URL('README.md',directory),'# Design review screenshots\n\nGenerated in Chrome. Live HTML artifacts remain authoritative for interactions.\n\n'+entries.join('\n')+'\n');
await browser.close();console.log(`Captured ${entries.length} review images.`);
