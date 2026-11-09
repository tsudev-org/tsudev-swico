// Sinh báo cáo mẫu (public/bao-cao-mau/xem/) và ảnh chụp (public/anh/) từ tools/demo-report - chính mã dựng báo cáo
// của chương trình, chạy trên hai máy GIẢ LẬP. Chạy tay khi giao diện báo cáo đổi, kết quả được commit:
//   npm run bao-cao-mau          (cần .NET SDK 8 và: npx playwright install chromium)
// Ký hiệu ✓ ⚠ trong báo cáo cần font emoji; trên Linux cài Noto Color Emoji để ảnh giống Windows.
import { execFileSync } from 'node:child_process';
import { cpSync, mkdtempSync, readdirSync, rmSync, statSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { chromium } from 'playwright';

const web = join(dirname(fileURLToPath(import.meta.url)), '..');
const repo = join(web, '..');
const xem = join(web, 'public/bao-cao-mau/xem');
const anh = join(web, 'public/anh');

const tam = mkdtempSync(join(tmpdir(), 'swico-mau-'));
execFileSync('dotnet', ['run', '--project', join(repo, 'tools/demo-report'), '-c', 'Release', '--', tam], { stdio: 'inherit' });
const cap2 = readdirSync(tam).map((d) => join(tam, d)).find((d) => statSync(d).isDirectory());
rmSync(xem, { recursive: true, force: true });
cpSync(cap2, xem, { recursive: true });
rmSync(tam, { recursive: true, force: true });

const duyet = (d) => readdirSync(d).flatMap((f) => (statSync(join(d, f)).isDirectory() ? duyet(join(d, f)) : [join(d, f)]));
const tim = (mau) => {
  const p = duyet(xem).find((f) => mau.test(f));
  if (!p) throw new Error(`Không thấy báo cáo khớp ${mau}`);
  return p;
};
const CHUP = [
  { ten: 'bao-cao-ban-quyen', tep: tim(/Windows_License_Audit_DEMO-KHO-02_.*\.html$/) },
  { ten: 'phan-cung', tep: tim(/Windows_Hardware_Inventory_DEMO-KETOAN-01_.*\.html$/) },
  { ten: 'tong-hop', tep: tim(/tsudev-tong-hop\.html$/) },
];

const trinhDuyet = await chromium.launch();
for (const [che, hau] of [['light', 'sang'], ['dark', 'toi']]) {
  const ctx = await trinhDuyet.newContext({ viewport: { width: 1280, height: 800 }, colorScheme: che });
  const trang = await ctx.newPage();
  for (const c of CHUP) {
    await trang.goto('file://' + c.tep);
    await trang.evaluate(() => document.fonts.ready);
    await trang.screenshot({ path: join(anh, `${c.ten}-${hau}.png`) });
  }
  await ctx.close();
}
await trinhDuyet.close();
console.log('[bao-cao-mau] xong:', xem, anh);
