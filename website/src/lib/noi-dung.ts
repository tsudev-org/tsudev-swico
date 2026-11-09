// Nội dung trang lấy trực tiếp từ các tệp của repo (một nguồn duy nhất cho tài liệu) và từ GitHub Releases lúc build.
import { readFileSync } from 'node:fs';
import { dirname, join, normalize } from 'node:path';
import { Marked } from 'marked';

export const SAN_PHAM = 'tsudev SWICO';
export const REPO = 'tsudev-org/tsudev-swico';
export const REPO_URL = `https://github.com/${REPO}`;
export const RELEASES_URL = `${REPO_URL}/releases/latest`;
export const SITE = 'https://swico.tsudev.com';

// Gốc repo: build chạy trong thư mục website/ (npm run build), nên gốc là thư mục cha.
const GOC = normalize(join(process.cwd(), '..'));

/** Tệp .md của repo -> đường dẫn trên website. Tệp khác của repo trỏ về GitHub. */
export const TRANG_MD: Record<string, string> = {
  'README.md': '/huong-dan/',
  'PRIVACY.md': '/quyen-rieng-tu/',
  'EULA.txt': '/dieu-khoan/',
  'CHANGELOG.md': '/nhat-ky/',
  'docs/DETECTION-RULES.md': '/luat-phat-hien/',
  'THIRD-PARTY-NOTICES.md': '/ban-quyen/#thanh-phan-ben-thu-ba',
  'LICENSE': '/ban-quyen/#giay-phep-apache-2-0',
  'NOTICE': '/ban-quyen/#notice',
};

export function docTep(duongDan: string): string {
  return readFileSync(join(GOC, duongDan), 'utf8').replace(/^﻿/, '');
}

/** Slug kiểu GitHub: chữ thường, bỏ dấu câu, khoảng trắng -> "-" (giữ chữ có dấu tiếng Việt). */
export function slug(text: string): string {
  // Bỏ thẻ HTML lặp tới khi hết (một lượt có thể để lại thẻ ghép lồng như "<scr<b>ipt>")
  let s = text.toLowerCase().trim();
  for (let truoc = ''; truoc !== s; ) {
    truoc = s;
    s = s.replace(/<[^<>]*>/g, '');
  }
  return s.replace(/&[a-z#0-9]+;/g, '').replace(/[^\p{L}\p{N}\s_-]/gu, '').replace(/\s/g, '-');
}

function viTriLienKet(href: string, tepMd: string): string {
  if (/^(https?:|mailto:|#)/.test(href)) return href;
  const [duong, neo] = href.split('#', 2);
  const tuyetDoi = normalize(join(dirname(tepMd), decodeURI(duong))).replace(/\\/g, '/').replace(/\/$/, '');
  const trang = TRANG_MD[tuyetDoi];
  if (trang) return neo ? `${trang.split('#')[0]}#${neo}` : trang;
  const loai = /\.[A-Za-z0-9_-]+$/.test(tuyetDoi) ? 'blob' : 'tree';
  return `${REPO_URL}/${loai}/main/${encodeURI(tuyetDoi)}${neo ? '#' + neo : ''}`;
}

/** Liên kết ngoài = http(s) tới host khác trang này (so hostname, không so chuỗi đầu: "swico.tsudev.com.evil.example" là ngoài). */
export function laNgoai(href: string): boolean {
  if (!/^https?:/i.test(href)) return false;
  try {
    return new URL(href).hostname.toLowerCase() !== new URL(SITE).hostname;
  } catch {
    return true;   // URL hỏng: coi là ngoài (thêm rel="noopener noreferrer")
  }
}

const thoat = (s: string) => s.replace(/&/g, '&amp;').replace(/"/g, '&quot;').replace(/</g, '&lt;');

/** Chuyển Markdown của repo sang HTML: sửa liên kết tương đối, thêm id cho tiêu đề, rel cho liên kết ngoài. */
export function renderMd(tepMd: string, opts: { boTieuDeDau?: boolean; md?: string; haCap?: number } = {}): { html: string; tieuDe: string } {
  let md = opts.md ?? docTep(tepMd);
  // Bỏ chú thích HTML (khối <!-- update ... --> của CHANGELOG, ghi chú nội bộ): không hiển thị, không cần gửi đi.
  md = md.replace(/<!--[\s\S]*?-->/g, '');
  const tieuDe = md.match(/^#\s+(.+)$/m)?.[1]?.trim() ?? '';
  if (opts.boTieuDeDau) md = md.replace(/^#\s+.+\n/m, '');
  const haCap = opts.haCap ?? 0;   // hạ cấp tiêu đề khi ghép tài liệu vào giữa trang
  const marked = new Marked({ gfm: true });
  marked.use({
    renderer: {
      heading({ tokens, depth }) {
        const text = this.parser.parseInline(tokens);
        const d = Math.min(depth + haCap, 6);
        return `<h${d} id="${slug(text)}">${text}</h${d}>\n`;
      },
      link({ href, title, tokens }) {
        const text = this.parser.parseInline(tokens);
        const dich = viTriLienKet(href, tepMd);
        const ngoai = laNgoai(dich) ? ' target="_blank" rel="noopener noreferrer"' : '';
        return `<a href="${thoat(dich)}"${title ? ` title="${thoat(title)}"` : ''}${ngoai}>${text}</a>`;
      },
      image({ href, title, text }) {
        return `<img src="${thoat(viTriLienKet(href, tepMd))}" alt="${thoat(text)}"${title ? ` title="${thoat(title)}"` : ''} loading="lazy">`;
      },
    },
  });
  const html = (marked.parse(md, { async: false }) as string)
    .replace(/<table>/g, '<div class="bang" tabindex="0" role="region" aria-label="Bảng, cuộn ngang được"><table>')
    .replace(/<\/table>/g, '</table></div>')
    // Khối mã có thể cuộn ngang: cho nhận focus để cuộn bằng bàn phím (WCAG 2.1.1, axe scrollable-region-focusable)
    .replace(/<pre>/g, '<pre tabindex="0">')
    // Danh sách việc "- [ ]" của GFM ra ô chọn không nhãn (axe: label). Trang chỉ đọc: thay bằng ký hiệu + chữ ẩn.
    .replace(/<input checked="" disabled="" type="checkbox">/g, '<span aria-hidden="true">☑</span><span class="an-di">Đã xong:</span>')
    .replace(/<input disabled="" type="checkbox">/g, '<span aria-hidden="true">☐</span><span class="an-di">Chưa làm:</span>');
  return { html, tieuDe };
}

/** Các mục `## ...` của một tệp Markdown có tiêu đề nằm trong `ten` (giữ thứ tự của tệp). Một nguồn: README.md. */
export function mucMd(tepMd: string, ten: string[]): string {
  const md = docTep(tepMd).replace(/<!--[\s\S]*?-->/g, '');
  const khoi = md.split(/^(?=## )/m).filter((k) => k.startsWith('## '));
  const chon = khoi.filter((k) => ten.includes(k.match(/^## (.+)$/m)![1].trim()));
  if (chon.length !== ten.length) throw new Error(`${tepMd}: thiếu mục ${ten.filter((t) => !chon.some((k) => k.startsWith(`## ${t}`))).join(', ')}`);
  return chon.join('\n');
}

/** `soMuc` phiên bản gần nhất của CHANGELOG (bỏ các khối "Chưa phát hành": việc nội bộ chưa tới tay người dùng). */
export function changelogMoiNhat(soMuc: number): string {
  const md = docTep('CHANGELOG.md');
  return md.split(/^(?=## \[)/m).filter((k) => /^## \[\d/.test(k)).slice(0, soMuc).join('');
}

// ---------------------------------------------------------------------------------------------------------------
// Bản phát hành

export interface GoiTai { ma: string; tieuDe: string; moTa: string; ten: string; url: string; sha256: string; kichThuoc: number }
export interface BanPhatHanh { tag: string; phienBan: string; ngay: string; url: string; goi: GoiTai[]; phu: GoiTai[]; tepSums: { ten: string; url: string }[] }

/**
 * Ba cách giao cùng một chương trình (README mục Cài đặt). Nhận cả tên tệp cũ (`swico-setup-26.8.18.2.exe`) lẫn
 * quy ước mới (`tsudev-swico_26.9.0901_x64-setup.exe`, docs/VERSIONING.md). Thứ tự = thứ tự hiển thị.
 */
export const LOAI_GOI: { ma: string; tieuDe: string; moTa: string; mau: RegExp }[] = [
  { ma: 'setup', tieuDe: 'Bộ cài đặt', moTa: 'Cài cố định, có mục gỡ cài đặt, hỗ trợ /VERYSILENT để triển khai hàng loạt.', mau: /^(swico-setup-.+\.exe|tsudev-swico_.+_x64-setup\.exe)$/ },
  { ma: 'portable', tieuDe: 'Bản di động (ZIP)', moTa: 'Cắm USB đi từng máy: giải nén chạy thẳng, không đụng registry.', mau: /^(swico-portable-.+\.zip|tsudev-swico_.+_x64-portable\.zip)$/ },
  { ma: 'exe', tieuDe: 'Chỉ tệp chạy', moTa: 'Một tệp swico.exe để nhúng vào script, GPO hoặc hệ thống RMM.', mau: /^swico\.exe$/ },
];
const PHU: { tieuDe: string; moTa: string; mau: RegExp }[] = [
  { tieuDe: 'SBOM', moTa: 'Danh mục thành phần phần mềm (CycloneDX) của bản phát hành.', mau: /sbom\.json$/ },
  { tieuDe: 'Manifest winget', moTa: 'Bộ manifest để tự cài qua winget trong mạng nội bộ.', mau: /^winget-manifest-.+\.zip$/ },
];

let boNho: Promise<BanPhatHanh | null> | undefined;

/**
 * Bản phát hành mới nhất, lấy lúc build từ GitHub (SHA-256 từ SHA256SUMS.txt đính kèm).
 * Trả null khi repo chưa có bản phát hành nào (API trả 404). Lỗi mạng/API khác: dừng build, đừng đưa lên trang một
 * danh sách tải sai.
 */
export function banMoiNhat(): Promise<BanPhatHanh | null> {
  boNho ??= (async () => {
    const headers: Record<string, string> = { Accept: 'application/vnd.github+json', 'User-Agent': 'tsudev-swico-website' };
    if (process.env.GITHUB_TOKEN) headers.Authorization = `Bearer ${process.env.GITHUB_TOKEN}`;
    const res = await fetch(`https://api.github.com/repos/${REPO}/releases/latest`, { headers });
    if (res.status === 404) return null;
    if (!res.ok) throw new Error(`GitHub API ${res.status}`);
    const r = await res.json() as { tag_name: string; published_at: string; html_url: string; assets: { name: string; browser_download_url: string; size: number }[] };
    const bang = new Map<string, string>();
    const tepSums = r.assets.filter((a) => a.name.endsWith('SHA256SUMS.txt'));
    for (const s of tepSums) {
      const res2 = await fetch(s.browser_download_url);
      if (!res2.ok) throw new Error(`Tải ${s.name}: ${res2.status}`);
      for (const dong of (await res2.text()).split('\n')) {
        const m = dong.trim().match(/^([0-9a-fA-F]{64})\s+\*?(.+)$/);
        if (m) bang.set(m[2].trim(), m[1].toLowerCase());
      }
    }
    const tao = (a: { name: string; browser_download_url: string; size: number }, ma: string, tieuDe: string, moTa: string): GoiTai =>
      ({ ma, tieuDe, moTa, ten: a.name, url: a.browser_download_url, sha256: bang.get(a.name) ?? '', kichThuoc: a.size });
    const goi = LOAI_GOI.flatMap((l) => r.assets.filter((a) => l.mau.test(a.name)).map((a) => tao(a, l.ma, l.tieuDe, l.moTa)));
    const phu = PHU.flatMap((l) => r.assets.filter((a) => l.mau.test(a.name)).map((a) => tao(a, 'phu', l.tieuDe, l.moTa)));
    return {
      tag: r.tag_name,
      phienBan: r.tag_name.replace(/^v/, ''),
      ngay: r.published_at.slice(0, 10).split('-').reverse().join('/'),
      url: r.html_url,
      goi,
      phu,
      tepSums: tepSums.map((s) => ({ ten: s.name, url: s.browser_download_url })),
    };
  })();
  return boNho;
}

/** Kích thước kiểu Việt Nam: dấu phẩy thập phân. */
export const mb = (b: number) => `${(b / 1048576).toFixed(1).replace('.', ',')} MB`;

// ---------------------------------------------------------------------------------------------------------------
// Bộ luật phát hiện (src/Tsudev.Audit.Core/Rules/detection-rules.json) - đọc lúc build, trang không bao giờ lệch tệp.

export interface LuatPhatHien {
  version: string; updatedUtc: string; notes: string;
  scanRoots: string[]; suspiciousNames: string[]; legitimateTaskNames: string[];
  hookDirectories: string[]; hookFiles: string[]; knownKmsHosts: string[]; hostsInterferenceKeywords: string[];
}
export const TEP_LUAT = 'src/Tsudev.Audit.Core/Rules/detection-rules.json';
export const boLuat = (): LuatPhatHien => JSON.parse(docTep(TEP_LUAT));
