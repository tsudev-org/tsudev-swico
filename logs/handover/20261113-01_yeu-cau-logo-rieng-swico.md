# PHIẾU YÊU CẦU - Logo riêng `tsudev SWICO`: thay icon / logo / favicon, chuyển sang định dạng hiện đại

- **Mã phiếu**: 20261113-01
- **Từ**: phiên `tsudev-06` (repo `tsudev`, phiên 49) - **Đến**: phiên kế tiếp của repo `tsudev-swico`
- **Thời điểm**: 13/11/2026
- **Ủy quyền**: chủ dự án yêu cầu trong chat 13/11/2026 (giao logo `logo-swico.png` cho sản phẩm này)
- **Trạng thái**: ĐÓNG (13/11/2026) - phiên tsudev-06 làm hộ theo ủy quyền của chủ dự án (repo không có phiên nào mở)

## 0. Bối cảnh

- Chủ dự án giao logo riêng cho `tsudev SWICO`. Ảnh gốc (đã chuyển vào repo `tsudev`, KHÔNG sửa ở đó):
  `~/projects/tsudev/packages/brand/source/eco/swico.png` - 554x543 RGBA, nền ĐẶC xanh đen `#2C3942`, sha256 `cb2f58a2f7344a6c065d5b959a9a1d9c300622403a36fb5f2dfe355fb5ee7bb8`.
  Đặc điểm: chữ "S" dạng mạch điện trắng trong khung khiên trắng, trên ô nền xanh đen (đã là kiểu ô biểu tượng ứng dụng).
- Hiện sản phẩm dùng CHUNG con cú thương hiệu tsudev làm favicon / icon, nên trên danh bạ `tsudev.com/apps` và trong
  thanh tác vụ không phân biệt được với app khác.
- tsudev.com đã dùng logo này cho thẻ của sản phẩm ở `/apps` (WebP 128px, `public/eco/swico.webp`, sinh bằng
  `packages/brand/build-eco-icons.js`, chế độ `tile`). Khi website của sản phẩm đã phục vụ logo mới, báo phiên `tsudev`
  đổi dòng `swico` trong `SOURCES` của script đó sang URL website (nguồn sự thật chuyển về repo này).
- Mẫu tham chiếu đã chạy thật: `tsudev-org/tsudev-trafficmonitor` - `assets/brand/brand.json` (cấu hình) +
  `tools/brand/build_brand.py` (một lệnh sinh mọi bản, có `--check`). Đề xuất quy ước cho logo riêng từng sản phẩm:
  `~/projects/tsudev-standards/tsudev-standards/logs/handover/20261112-01_de-xuat-logo-rieng-tung-san-pham.md`.

## 1. Việc cần làm

1. **Đặt ảnh gốc đúng chỗ, một nguồn duy nhất**: chép ảnh gốc vào `assets/brand/source/logo-swico.png` (giữ nguyên
   byte, ghi sha256 vào `assets/brand/README.md`). Mọi bản khác SINH từ đây bằng script - không cắt tay từng cỡ.
   Đổi logo sau này = thay một tệp + chạy một lệnh.
2. **Dây chuyền sinh tài nguyên** (`tools/brand/` + `assets/brand/brand.json`, theo mẫu trafficmonitor), có chế độ
   `--check` chạy trong CI để bắt artifact lệch nguồn.
3. **Xử lý nền**: nguồn là ô biểu tượng NỀN ĐẶC xanh đen với nét TRẮNG - xoá nền thì nét trắng biến mất trên nền Sáng. GIỮ nền làm ô (cắt vuông chính giữa, bo góc ~22% cho web; Windows dùng ô vuông đầy). Nếu cần bản trong suốt (khay hệ thống, bản đơn sắc), tách riêng phần nét trắng thành dấu hiệu một màu, và trên nền Sáng đổi nét sang màu nền `#2C3942`. Ảnh gốc 554px: bản 512 đủ, nhưng icon 256 của .ico và ảnh lớn nên dựng lại SVG (hình học thẳng, vector hoá tay khả thi) để sắc ở mọi cỡ.
4. **Định dạng hiện đại, sắc nét mọi thiết bị**:
   - Web: `<picture>` AVIF → WebP → PNG (PNG chỉ là đường lui), mỗi cỡ hiển thị có bản 1x/2x/3x qua `srcset`;
     `favicon.ico` nhiều lớp THẬT 16/24/32/48/64/128/256 (không PNG đổi đuôi); `favicon-{16,32,96}.png`;
     `apple-touch-icon.png` 180 nền ĐẶC; `android-chrome-{192,512}` + bản `maskable` riêng (vùng an toàn 80%);
     `site.webmanifest` khai tách `any` / `maskable`; `og-image.png` 1200x630.
   - Thử vector hoá (SVG) cho dấu hiệu nếu ra được bản sạch nét: đo dung lượng và soát bằng mắt ở 16/32/48 px trước
     khi dùng - bản vector tự động xấu thì giữ raster đa cỡ.
   - .NET (`Tsudev.SystemAudit`): `assets/swico.ico` dựng lại nhiều lớp 16-256 (PNG nén bên trong cho lớp 256); `ApplicationIcon` trong `.csproj` trỏ đúng tệp; ảnh bộ cài `wizard-large.bmp` / `wizard-small.bmp` (Inno Setup) sinh lại theo logo mới ở đúng cỡ Inno yêu cầu và cả bản HiDPI; `assets/tsudev-logo*.png` dùng trong giao diện đổi sang logo mới.
5. **Dọn**: gỡ các tệp logo/favicon cũ (con cú dùng chung) không còn ai dùng; quét repo tìm mọi chỗ tham chiếu để
   không còn ảnh vỡ (`grep -rn "tsudev-logo\|logo-mark"`).
6. **Ghi lệch chuẩn** ở `AGENTS.md` mục B.4 (logo sản phẩm riêng khác `BRAND_ASSETS.md` mục 2 hiện hành) cho tới khi
   đề xuất quy ước được nhận.
7. **Phát hành + deploy website**, rồi báo phiên `tsudev` (ListAgents → SendMessage, hoặc phiếu vào
   `~/projects/tsudev/logs/handover/`) URL logo mới trên website để đổi nguồn của `/apps`.

## 2. Tiêu chí hoàn thành

- [ ] `assets/brand/source/logo-swico.png` là nguồn duy nhất; script + `--check` trong CI.
- [ ] Favicon tab trình duyệt, icon ứng dụng (exe, bộ cài, lối tắt Start, cửa sổ ứng dụng) đều là logo mới, sắc nét ở 16/32/48 px và màn 2x/3x.
- [ ] Website phục vụ AVIF/WebP qua `<picture>`; cổng kiểm tài sản nhận diện (mục 12.2 BRAND_ASSETS) đạt.
- [ ] Soát bằng mắt trên nền Sáng VÀ Tối, Windows / Android / iOS / macOS (ảnh chụp kèm nhật ký phiên).
- [ ] Đã báo phiên `tsudev` URL logo trên website.

## 3. Cảnh báo

- KHÔNG sửa `.standards/` (chỉ-đọc) và KHÔNG sửa repo `tsudev` từ phiên này - mọi thay đổi bên đó qua phiếu.
- Tên repo cũ dưới `tsudev-tsudev` là bất khả xâm phạm (CLAUDE.md toàn máy).

## 4. Kết quả xử lý (phiên nhận điền)

- 13/11/2026 - phiên tsudev-06 (repo tsudev), chủ dự án ủy quyền làm trực tiếp:
  - Ảnh gốc DUY NHẤT `assets/brand/source/logo-swico.png` (sha256 như mục 0); cấu hình `assets/brand/brand.json`
    (chế độ `tile`); dây chuyền `website/scripts/brand-assets.mjs` (Node + sharp có sẵn trong website, `--check` chạy
    trong CI) sinh: `brand/logo-swico-{48,96,144,192}.{avif,webp}` + PNG 512, favicon.ico 7 lớp, favicon 16/32/96,
    android-chrome 192/512 + maskable riêng, apple-touch nền đặc, og-image 1200x630, `assets/brand/app/` (icon PNG + app.ico).
  - Website: header dùng `<picture>` AVIF → WebP (1x/2x/3x), chân trang dùng logo tsudev mới (dẫn về tsudev.com); gỡ
    `brand/logo-mark.png` và `scripts/brand_assets.py` (script cũ chép `logo-mark.png` từ tsudev.com - tệp đó đã gỡ
    13/11 nên script đã gãy).
  - Ứng dụng: `assets/swico.ico` (exe + bộ cài Inno), `assets/wizard-{large,small}.bmp`, `assets/favicon/*` qua `packaging/tools/make-assets.py` (nay đọc `assets/brand/app/icon-512.png`). `assets/tsudev-logo-{72,144}.png` GIỮ (logo nhà làm nhúng trong báo cáo HTML).
  - Vector SVG: CHƯA - không có công cụ vector hoá trong máy; bản raster đa cỡ đạt ở 16-512 px. Ảnh gốc nhỏ (cạnh
    ngắn < 1024 px) nên bản 512/1024 là phóng to - xin chủ dự án ảnh gốc lớn hơn hoặc SVG khi có.
  - Lệch chuẩn ghi ở `AGENTS.md` B.2. Website deploy sau khi PR merge; tsudev.com `/apps` đã dùng logo này từ PR tsudev#229.
