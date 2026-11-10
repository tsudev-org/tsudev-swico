# PHIẾU YÊU CẦU - Nút đổi giao diện Sáng/Tối trong header website swico.tsudev.com

- **Mã phiếu**: 20261110-02
- **Từ**: phiên tsudev-c8 (repo `tsudev`) - **Đến**: phiên Claude của repo `tsudev-swico`
- **Thời điểm**: 10/11/2026
- **Trạng thái**: ĐÓNG - THAY bởi phiếu `20261110-05` (bộ chọn ở chân trang, có "Hệ thống"); làm thẳng theo phiếu đó
- **Ủy quyền**: chủ dự án yêu cầu bổ sung nút đổi màu sáng/tối ở header website của mọi dự án hệ tsudev
  (chat 10/11/2026, phiên tsudev-c8).

## Yêu cầu

Thêm **nút đổi giao diện Sáng/Tối** vào header của website `swico.tsudev.com` (thư mục `website/`), cùng hành vi với nút
trên tsudev.com để người dùng thấy một hệ sinh thái nhất quán.

1. **Nút trong header**, cạnh điều hướng chính, cả desktop lẫn di động. Biểu tượng mặt trời/mặt trăng đổi theo
   chế độ đang hiện. Bấm mở menu nhỏ 4 lựa chọn: **Sáng · Ấm · Tối · Theo hệ thống** (giống
   `packages/ui/src/components/ThemeToggle.tsx` của repo tsudev). Nếu muốn gọn: nút bật/tắt Sáng ↔ Tối cũng đạt,
   nhưng phải giữ được lựa chọn "Theo hệ thống".
2. **Ghi chế độ vào `data-theme` trên `<html>`** - `tokens.css` của site đã có đủ khối `light`/`warm`/`dark`.
   Lưu lựa chọn ở `localStorage` khoá **`tsudev-theme`**, giá trị `light|warm|dark|system` (cùng khoá với
   tsudev.com). `system` được GIẢI bằng `matchMedia('(prefers-color-scheme: dark)')` trong JavaScript - đừng
   treo bảng màu vào `@media (prefers-color-scheme)` trong CSS.
3. **Chống nháy màu**: áp chế độ đã lưu bằng script ĐỒNG BỘ trong `<head>`, TRƯỚC stylesheet - không dùng
   `defer`/`type=module`/`DOMContentLoaded` (cho ra một khung hình sai màu ở mọi lần tải). Bọc `try/catch`
   quanh `localStorage` (ném lỗi khi chặn cookie/iframe sandbox) - hỏng thì về chế độ mặc định, không trang trắng.
   Đồng thời cập nhật `<meta name="theme-color">` theo `--bg-base` của chế độ đang hiện.
   **Thứ tự trong `<head>`**: đặt script SAU các thẻ `<meta name="theme-color">` và TRƯỚC stylesheet - script
   đặt trước meta thì chạy lúc meta chưa tồn tại nên không sửa được (bài học từ cwico, PR tsudev-cwico#58).
4. **CSP - đọc kỹ, đây là chỗ dễ hỏng im lặng.** `website/public/_headers` khai `script-src 'unsafe-inline'` - script nội tuyến chạy được, nhưng nên nhân dịp này chuyển sang tệp tĩnh `public/theme.js` (nạp chặn, không `defer`) và thay `'unsafe-inline'` bằng `'self'` để siết CSP. Nếu giữ nội tuyến thì đừng để mất `'unsafe-inline'` khi sửa CSP.
5. **Khả năng truy cập**: `<button type="button">` có `aria-label="Đổi giao diện"`, `aria-haspopup`/`aria-expanded`
   cho menu, Esc đóng menu, vòng focus nhìn thấy được; mục đang chọn có `aria-checked`/`aria-current`.
6. **Tương phản**: duyệt MỌI trang ở cả 3 chế độ - `site.css` có thể còn mã hex cắm cứng chỉ đúng ở chế độ Sáng.
   Màu phải đi qua token để đổi theo `data-theme`.
7. **Chế độ mặc định giữ nguyên** như hiện tại của site (`data-theme` đang đặt cứng trong `Trang.astro`), trừ khi
   chủ dự án quyết khác. Ghi chú: từ 10/11/2026 tsudev.com mặc định Tối (nền gần-đen `#05070B`, xanh dương
   `#3D9BFF` + xanh lá `#2EE6A6`) - nếu chủ dự án muốn đồng bộ thì đó là một quyết định riêng, đừng tự đổi.

## Nghiệm thu

- Build sạch; nếu repo có test quét nguồn thì thêm test: script chống nháy nằm trong `<head>`, giải đủ 4 lựa chọn,
  và CSP cho phép đúng cách nạp script đó.
- Sau deploy: `curl -sI https://swico.tsudev.com/ | grep -i content-security-policy` khớp cách nạp script; mở trang bằng
  trình duyệt thật, đổi qua 4 lựa chọn, tải lại trang - chế độ được giữ và **không có cú nháy**; console không có
  lỗi CSP (`Refused to execute inline script`).
- Ghi kết quả vào mục "Kết quả xử lý" của phiếu này, rồi đổi Trạng thái thành ĐÓNG.

## Tham chiếu

- Repo tsudev (`~/projects/tsudev` hoặc `github.com/tsudev-org/tsudev`): `apps/frontend-main/pages/_document.tsx`
  (`THEME_SCRIPT`), `packages/ui/src/components/ThemeToggle.tsx`, `apps/frontend-main/test/themeTokens.test.ts`.
- Bài học đã trả giá ở tsudev: đổi nội dung script nội tuyến mà quên cập nhật băm CSP ⇒ production chặn script,
  site vẫn 200, không gì báo lỗi. Dev/E2E chạy `http://localhost` không có CSP nên không bắt được.

## Kết quả xử lý

-
