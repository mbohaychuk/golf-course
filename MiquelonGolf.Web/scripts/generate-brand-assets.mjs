#!/usr/bin/env node
// Regenerates raster brand assets in public/ from the SVG monogram and
// course photography. Run with `node scripts/generate-brand-assets.mjs`.
//
// Outputs:
//   public/apple-touch-icon.png    — 180×180 PNG for iOS home-screen
//   public/icon-32.png             — 32×32 PNG fallback (rel="icon")
//   public/icon-192.png            — 192×192 PNG (Android maskable)
//   public/og-default.jpg          — 1200×630 JPG social card
//
// The SVG source of truth is public/icon.svg. The OG card composes
// fairway-corridor.avif as the photographic base, layered with a forest-green
// gradient and the wordmark.

import { readFile, writeFile } from 'node:fs/promises'
import { fileURLToPath } from 'node:url'
import { dirname, resolve } from 'node:path'
import sharp from 'sharp'

const here = dirname(fileURLToPath(import.meta.url))
const publicDir = resolve(here, '..', 'public')

const monogramSvg = await readFile(resolve(publicDir, 'icon.svg'))

// — Favicons / touch icons ──────────────────────────────────────────────
const sizes = [
  { name: 'apple-touch-icon.png', size: 180 },
  { name: 'icon-192.png', size: 192 },
  { name: 'icon-32.png', size: 32 },
]
for (const { name, size } of sizes) {
  await sharp(monogramSvg)
    .resize(size, size)
    .png({ compressionLevel: 9 })
    .toFile(resolve(publicDir, name))
  console.log(`wrote public/${name} (${size}×${size})`)
}

// — Social card (1200×630 JPG) ─────────────────────────────────────────
// Compose the course photo, a forest-green gradient overlay (so the wordmark
// is legible regardless of which crop of the photo lands behind it), and the
// brand wordmark + tagline.
const photoBuf = await readFile(resolve(publicDir, 'images', 'fairway-corridor.avif'))

const gradientSvg = Buffer.from(`
<svg xmlns="http://www.w3.org/2000/svg" width="1200" height="630">
  <defs>
    <linearGradient id="g" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0%" stop-color="#1C4209" stop-opacity="0.40"/>
      <stop offset="55%" stop-color="#1C4209" stop-opacity="0.55"/>
      <stop offset="100%" stop-color="#0F2603" stop-opacity="0.85"/>
    </linearGradient>
  </defs>
  <rect width="1200" height="630" fill="url(#g)"/>
</svg>
`)

const wordmarkSvg = Buffer.from(`
<svg xmlns="http://www.w3.org/2000/svg" width="1200" height="630">
  <g fill="#EDE8D4" font-family="'Playfair Display', 'Georgia', serif">
    <text x="600" y="290" text-anchor="middle" font-size="78" font-weight="800" letter-spacing="-1">
      Miquelon Hills
    </text>
    <text x="600" y="372" text-anchor="middle" font-size="42" font-weight="500" letter-spacing="6" fill="#A07828">
      GOLF COURSE
    </text>
    <text x="600" y="470" text-anchor="middle" font-size="28" font-weight="400" font-style="italic" fill="#EDE8D4" opacity="0.85">
      An hour from Edmonton. A world away.
    </text>
  </g>
</svg>
`)

const photoResized = await sharp(photoBuf)
  .resize(1200, 630, { fit: 'cover', position: 'centre' })
  .toBuffer()

await sharp(photoResized)
  .composite([
    { input: gradientSvg, top: 0, left: 0 },
    { input: wordmarkSvg, top: 0, left: 0 },
  ])
  .jpeg({ quality: 88, progressive: true, mozjpeg: true })
  .toFile(resolve(publicDir, 'og-default.jpg'))
console.log('wrote public/og-default.jpg (1200×630)')
