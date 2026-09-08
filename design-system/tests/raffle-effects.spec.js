import {test,expect} from '@playwright/test';
import { ArtifactPage } from './page-objects/artifact-page.js';
test('Raffle celebration renders through WebGPU or the canvas fallback',async({page})=>{const r=new ArtifactPage(page);await r.open('raffle',{state:'winner'});await r.renderer(/webgpu|canvas/);await r.rendered();});
test('Forced canvas fallback still renders the winner celebration',async({page})=>{const r=new ArtifactPage(page);await r.open('raffle',{state:'winner',renderer:'canvas'});await r.renderer('canvas');await r.rendered();});
test('Reduced-motion raffle remains readable without animated particles',async({page})=>{await page.emulateMedia({reducedMotion:'reduce'});const r=new ArtifactPage(page);await r.open('raffle',{state:'winner'});await r.renderer(null);await r.rendered();});
