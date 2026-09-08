import { test,expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
import { ComponentGalleryPage } from './page-objects/component-gallery-page.js';
test('Standalone gallery has no WCAG A or AA violations',async({page})=>{const gallery=new ComponentGalleryPage(page);await gallery.open();const results=await new AxeBuilder({page}).withTags(['wcag2a','wcag2aa','wcag21aa']).analyze();expect(results.violations).toEqual([]);});
