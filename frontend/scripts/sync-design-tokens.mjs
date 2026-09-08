import { copyFile, mkdir } from 'node:fs/promises';

const destination = new URL('../projects/styles/', import.meta.url);
await mkdir(destination, { recursive: true });
await copyFile(new URL('../../design-system/tokens.css', import.meta.url), new URL('tokens.css', destination));
