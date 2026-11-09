# PHIẾU YÊU CẦU - README theo nhận diện tsudev (ECOSYSTEM_IDENTITY mục 2.1)

- **Mã phiếu**: 20261109-01
- **Từ**: phiên tsudev-files-c2 (repo `tsudev-files`) - **Đến**: phiên sau của repo này
- **Thời điểm**: 21:26 09/11/2026
- **Trạng thái**: ĐÃ XONG (phiên S007, 10/11/2026)
- **Ủy quyền**: chủ dự án đồng ý chuẩn hóa nhận diện mọi repo hệ tsudev (chat 09/11/2026, phiên tsudev-files).

## Đã làm hộ (trên GitHub, không đụng mã repo này)

- Mô tả repo kết thúc bằng ` | tsudev.com`, `homepage` = `https://tsudev.com` (chưa có trang sản phẩm riêng),
  thêm nhãn `tsudev`. Kiểm: `gh api repos/<owner>/<repo> --jq '.description,.homepage,.topics'`.

## Việc cần làm ở repo này

1. `README.md` mở đầu: dấu hiệu thu gọn, tên sản phẩm, một câu mô tả, rồi dòng
   `Một sản phẩm của [tsudev](https://tsudev.com).` (`.standards/docs/ECOSYSTEM_IDENTITY.md` mục 2.1).
2. Ghi **hạng sản phẩm** A/B/C vào README (`.standards/docs/AUTH_AND_ACCOUNT.md` mục 1). Bảng ví dụ ở mục 1 lấy `tsudev-swico` làm ví dụ hạng C.
3. Khi repo `tsudev` làm xong trang sản phẩm (phiếu `tsudev/logs/handover/20261109-06_*`), đổi `homepage`
   sang trang đó.

## Kết quả xử lý (S007)

1. ✅ `README.md` mở đầu: dấu hiệu thu gọn, `# tsudev SWICO`, một câu mô tả, `Một sản phẩm của [tsudev](https://tsudev.com).`
2. ✅ Ghi **Hạng sản phẩm: C - Ngoại tuyến** ngay dưới dòng đó.
3. ✅ Trang sản phẩm riêng đã có: **https://swico.tsudev.com** (dựng trong repo này, `website/`). `homepage` GitHub đã đổi
   sang trang đó; mô tả repo sửa `tsuowlit SWICO` (tên cũ đã bỏ ở S001) thành `tsudev SWICO`.
