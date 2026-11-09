// Danh mục báo cáo mẫu trong public/bao-cao-mau/xem/ (sinh bằng scripts/sinh-bao-cao-mau.mjs). Đọc lúc build: thêm
// hay đổi máy giả lập ở tools/demo-report thì trang tự đổi theo, không có danh sách viết tay để lệch.
import { readdirSync, readFileSync, statSync } from 'node:fs';
import { join, relative } from 'node:path';

const GOC = join(process.cwd(), 'public/bao-cao-mau/xem');

function duyet(d: string): string[] {
  return readdirSync(d).flatMap((f) => (statSync(join(d, f)).isDirectory() ? duyet(join(d, f)) : [join(d, f)]));
}

export interface MauBaoCao { may: string; loai: string; ketLuan: string; html: string; json: string }

export function danhSachMau(): { tongHop: string; baoCao: MauBaoCao[] } {
  const tep = duyet(GOC);
  const url = (p: string) => '/bao-cao-mau/xem/' + relative(GOC, p).split('\\').join('/');
  const tongHop = tep.find((p) => p.endsWith('tsudev-tong-hop.html'));
  if (!tongHop) throw new Error('Chưa sinh báo cáo mẫu: chạy npm run bao-cao-mau');
  const baoCao = tep.filter((p) => p.endsWith('.json')).map((p) => {
    const r = JSON.parse(readFileSync(p, 'utf8')) as { ComputerName: string; ReportKind: string; VerdictText?: string; HtmlReportFile: string };
    return {
      may: r.ComputerName,
      loai: r.ReportKind === 'LicenseAudit' ? 'Bản quyền' : 'Phần cứng',
      ketLuan: (r.VerdictText ?? '-').replace(/^\P{L}+/u, ''),
      html: url(p.replace(/\.json$/, '.html')),
      json: url(p),
    };
  }).sort((a, b) => a.may.localeCompare(b.may) || a.loai.localeCompare(b.loai));
  return { tongHop: url(tongHop), baoCao };
}
