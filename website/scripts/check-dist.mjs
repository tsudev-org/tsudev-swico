// Kiểm tra bản build (ECOSYSTEM_IDENTITY mục 2.2 và 4): mọi trang có title đúng khuôn, og:site_name = tsudev, canonical
// không www, description 120-160 ký tự, không <script> hay CSS nội tuyến (CSP), liên kết mở tab mới có rel đủ,
// chỉ dùng gạch ngang ngắn trong chữ hiển thị (AGENTS.md mục 7), liên kết nội bộ không gãy.
import { existsSync, readdirSync, readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const dist = join(dirname(fileURLToPath(import.meta.url)), '../dist');
const loi = [];
const lienKet = [];
function duyet(thuMuc) {
  for (const e of readdirSync(thuMuc, { withFileTypes: true })) {
    const p = join(thuMuc, e.name);
    if (e.isDirectory()) duyet(p);
    // Báo cáo mẫu là tệp chương trình sinh ra (không phải trang của website): có CSS/JS nội tuyến theo thiết kế,
    // được nới CSP riêng trong public/_headers. Chỉ kiểm các trang của website.
    else if (e.isFile() && e.name.endsWith('.html') && !p.includes('/bao-cao-mau/xem/')) kiemTra(p, readFileSync(p, 'utf8'));
  }
}
function kiemTra(tep, html) {
  const ten = tep.slice(dist.length);
  const title = html.match(/<title>([^<]*)<\/title>/)?.[1] ?? '';
  if (!/^(.+ - )?tsudev SWICO \| tsudev$/.test(title)) loi.push(`${ten}: title sai khuôn: ${title}`);
  if (!html.includes('<meta property="og:site_name" content="tsudev">')) loi.push(`${ten}: thiếu og:site_name tsudev`);
  if (!/<link rel="canonical" href="https:\/\/swico\.tsudev\.com\//.test(html)) loi.push(`${ten}: thiếu canonical`);
  const mota = html.match(/<meta name="description" content="([^"]*)"/)?.[1] ?? '';
  const dai = [...mota.replace(/&[a-z#0-9]+;/g, 'x')].length;
  if (dai < 120 || dai > 160) loi.push(`${ten}: description ${dai} ký tự (cần 120-160)`);
  if (/<script(?![^>]*type="application\/ld\+json")[^>]*>/.test(html)) loi.push(`${ten}: có <script>`);
  if (/<style[\s>]/.test(html) || / style="/.test(html)) loi.push(`${ten}: có CSS nội tuyến (CSP chặn)`);
  for (const a of html.matchAll(/<a [^>]*target="_blank"[^>]*>/g)) {
    if (!a[0].includes('rel="noopener noreferrer"')) loi.push(`${ten}: thiếu rel noopener noreferrer: ${a[0]}`);
  }
  // Em-dash/en-dash trong phần chữ do trang này viết. Nhật ký thay đổi và giấy phép bên thứ ba sao nguyên văn
  // từ upstream (ngoại lệ đã ghi trong logs/STATE.md) nên được bỏ qua.
  if (!/^\/(nhat-ky|ban-quyen)\//.test(ten)) {
    const chu = html.replace(/<pre[\s\S]*?<\/pre>/g, '').replace(/<[^>]+>/g, ' ');
    if (/[–—]/.test(chu)) loi.push(`${ten}: có em-dash/en-dash trong chữ hiển thị`);
  }
  for (const m of html.matchAll(/(?:href|src)="(\/[^"#?]*)/g)) lienKet.push([ten, m[1]]);
}
duyet(dist);
for (const [ten, duong] of lienKet) {
  const dich = join(dist, decodeURI(duong));
  if (!existsSync(dich) && !existsSync(join(dich, 'index.html'))) loi.push(`${ten}: liên kết nội bộ gãy: ${duong}`);
}
for (const l of loi) console.error('[check-dist] LỖI', l);
console.log(`[check-dist] ${loi.length} lỗi`);
process.exit(loi.length ? 1 : 0);
