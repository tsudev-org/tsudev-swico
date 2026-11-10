# Website swico.tsudev.com

Trang sản phẩm của tsudev SWICO: [Astro](https://astro.build) dựng trang tĩnh; JavaScript phía trình duyệt chỉ có
`public/theme.js` (bộ chọn giao diện Sáng/Tối/Hệ thống ở chân trang, mặc định Tối, khóa `tsudev-theme` như tsudev.com;
nạp chặn trong `<head>`, CSP `script-src 'self'`), host bằng Cloudflare Workers Static Assets (Worker `tsudev-swico`, chỉ phục vụ tệp tĩnh, không chạy mã; tên
miền con `swico.tsudev.com` của hệ tsudev.com, cấu hình ở `wrangler.jsonc`). Khung dùng chung với `files.tsudev.com`.

* **Một nguồn tài liệu.** Nội dung lấy trực tiếp từ tệp của repo lúc build - sửa ở đó, không sửa trong `website/`:

  | Trang | Nguồn |
  | --- | --- |
  | `/huong-dan/` | các mục `Cách dùng`, `Kết quả đầu ra`, `Tự động cập nhật`, `Đánh số phiên bản` của `README.md` |
  | `/luat-phat-hien/` | `src/Tsudev.Audit.Core/Rules/detection-rules.json` + `docs/DETECTION-RULES.md` |
  | `/quyen-rieng-tu/`, `/dieu-khoan/` | `PRIVACY.md`, `EULA.txt` |
  | `/nhat-ky/`, `/ban-quyen/` | `CHANGELOG.md` (6 bản gần nhất), `LICENSE`, `NOTICE`, `THIRD-PARTY-NOTICES.md` |
  | `/tai-ve/` | GitHub Releases (phiên bản, liên kết, SHA-256 từ `SHA256SUMS.txt`) lúc build |

  Đổi tên một mục `##` của README thì sửa danh sách `MUC` trong `src/pages/huong-dan/index.astro` (build báo lỗi nếu lệch).
* **Báo cáo mẫu** (`public/bao-cao-mau/xem/`) và ảnh chụp (`public/anh/`) sinh bằng chính mã dựng báo cáo của chương
  trình, chạy trên hai máy **giả lập** (`tools/demo-report`): không bao giờ có dữ liệu máy thật. Đổi giao diện báo cáo
  thì chạy lại `npm run bao-cao-mau` (cần .NET SDK 8; trên Linux cài font Noto Color Emoji để ký hiệu hiện như Windows).
* Tài sản thương hiệu (`public/favicon*`, `android-chrome-*`, `og-image.png`, `brand/`) sinh bằng
  `scripts/brand_assets.py` từ dây chuyền thương hiệu của repo `tsudev-org/tsudev`; không sửa tay.
* Font Be Vietnam Pro tự host (SIL OFL 1.1); `src/styles/tokens.css` sao nguyên văn `.standards/tokens/tokens.css`.
* Header bảo mật ở `public/_headers`: CSP chặt cho mọi trang; riêng `/bao-cao-mau/xem/*` cho CSS nội tuyến và script
  nội tuyến THEO BĂM (`sha256` + `'unsafe-hashes'` cho đúng trình xử lý `tsudevFilter(this)`) vì đó là tệp báo cáo
  một-tệp do chương trình sinh ra (vẫn không nạp gì từ nguồn ngoài, `noindex`). Sinh lại báo cáo mà đổi script thì
  `check-dist` báo băm lệch: cập nhật băm trong `_headers`.
* `scripts/check-dist.mjs` chạy sau mỗi lần build: title, `og:site_name`, canonical, độ dài mô tả, chặn CSS/JS nội
  tuyến, `rel` của liên kết ngoài, em-dash, liên kết nội bộ gãy. `npm run check:a11y`: axe-core WCAG 2.2 AA mọi trang
  kể cả báo cáo mẫu, Sáng + Tối. `npm run check:theme`: bộ chọn giao diện trong Chromium với CSP production.

```bash
cd website
npm ci
npm run dev          # xem thử ở http://localhost:4321
npm run build        # dựng vào dist/ và kiểm tra
npm run check:a11y   # lần đầu: npx playwright install chromium
npm run deploy       # build rồi `wrangler deploy` (cần `npx wrangler login` với tài khoản tsudev)
```

Sau mỗi lần phát hành bản mới, chạy lại `npm run deploy` để trang Tải về cập nhật phiên bản và mã SHA-256.

## Vì sao Workers mà không phải Pages (09/11/2026)

`wrangler pages project create` bản 4.158 tự chuyển sang Workers rồi lỗi; tạo Pages bằng `--force` thì được, nhưng tên
miền riêng của Pages cần thêm bản ghi CNAME, mà phiên đăng nhập `wrangler login` không có quyền ghi DNS. Custom Domain
của Workers tự tạo bản ghi DNS và chứng chỉ với quyền `workers:write` sẵn có. Cả hai đều miễn phí; `_headers`,
trang 404 và đường dẫn có `/` cuối hoạt động như nhau. Dự án Pages tạm đã xóa.

Lần đầu chỉ cần `npm run deploy` (khối `routes` trong `wrangler.jsonc` tự gắn `swico.tsudev.com`), rồi
`gh repo edit tsudev-org/tsudev-swico --homepage https://swico.tsudev.com`.
