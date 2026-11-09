// Kiểm tra khả năng truy cập (ACCESSIBILITY.md của tsudev) bằng axe-core: mọi trang trong dist/,
// cả chế độ sáng và tối (prefers-color-scheme), quy tắc WCAG 2.0/2.1/2.2 mức A và AA.
// Chạy sau `npm run build`: `npm run check:a11y` (CI: website.yml). Cần Chromium của Playwright:
// `npx playwright install --with-deps chromium`.
import { createServer } from 'node:http';
import { readdirSync, readFileSync } from 'node:fs';
import { dirname, extname, join, normalize, relative, sep } from 'node:path';
import { fileURLToPath } from 'node:url';
import { chromium } from 'playwright';
import { AxeBuilder } from '@axe-core/playwright';

const dist = join(dirname(fileURLToPath(import.meta.url)), '../dist');
const MIME = {
  '.html': 'text/html; charset=utf-8', '.css': 'text/css', '.js': 'text/javascript', '.svg': 'image/svg+xml',
  '.png': 'image/png', '.ico': 'image/x-icon', '.webp': 'image/webp', '.woff2': 'font/woff2', '.json': 'application/json',
  '.xml': 'application/xml', '.txt': 'text/plain', '.webmanifest': 'application/manifest+json',
};

function pages(dir) {
  const out = [];
  for (const e of readdirSync(dir, { withFileTypes: true })) {
    const p = join(dir, e.name);
    if (e.isDirectory()) out.push(...pages(p));
    else if (e.isFile() && e.name.endsWith('.html')) out.push('/' + relative(dist, p).split(sep).join('/').replace(/(^|\/)index\.html$/, '$1'));
  }
  return out.sort();
}

// Máy chủ tĩnh tối thiểu cho dist/ (chỉ 127.0.0.1, chặn vượt thư mục)
const server = createServer((req, res) => {
  let path = decodeURIComponent(new URL(req.url, 'http://127.0.0.1').pathname);
  if (path.endsWith('/')) path += 'index.html';
  const file = normalize(join(dist, path));
  // Đọc thẳng rồi bắt lỗi (không kiểm tồn tại trước: tránh TOCTOU); chặn vượt ra ngoài dist/
  let body = null;
  if (file.startsWith(dist + sep)) {
    try { body = readFileSync(file); } catch { body = null; }   // không có, hoặc là thư mục (EISDIR)
  }
  if (body === null) {
    res.writeHead(404, { 'content-type': MIME['.html'] });
    res.end(readFileSync(join(dist, '404.html')));
    return;
  }
  res.writeHead(200, { 'content-type': MIME[extname(file)] ?? 'application/octet-stream' });
  res.end(body);
});
await new Promise((r) => server.listen(0, '127.0.0.1', r));
const base = `http://127.0.0.1:${server.address().port}`;

const browser = await chromium.launch();
const tags = ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa', 'wcag22aa'];
let total = 0;
const list = pages(dist);
for (const scheme of ['light', 'dark']) {
  const context = await browser.newContext({ colorScheme: scheme });
  for (const path of list) {
    const page = await context.newPage();
    await page.goto(base + path, { waitUntil: 'load' });
    const { violations } = await new AxeBuilder({ page }).withTags(tags).analyze();
    for (const v of violations) {
      total++;
      console.error(`[a11y] LỖI ${scheme} ${path}: ${v.id} (${v.impact}) - ${v.help}`);
      for (const n of v.nodes.slice(0, 5)) console.error(`         ${n.target.join(' ')}: ${n.failureSummary?.split('\n').slice(1).join(' ').trim()}`);
    }
    await page.close();
  }
  await context.close();
}
await browser.close();
server.close();
console.log(`[a11y] ${list.length} trang x 2 chế độ màu, ${total} lỗi`);
process.exit(total ? 1 : 0);
