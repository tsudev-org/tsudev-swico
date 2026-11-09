# demo-report

Sinh **báo cáo mẫu** từ dữ liệu giả lập bằng chính mã dựng báo cáo của `Tsudev.Audit.Core` - dùng cho website
`swico.tsudev.com/bao-cao-mau/` và ảnh chụp trong README. Không đọc máy thật: tên máy, serial, MAC đều bịa, IP thuộc
dải tài liệu `192.0.2.0/24` (RFC 5737).

```bash
dotnet run --project tools/demo-report -- <thư-mục-đầu-ra>
# hoặc sinh thẳng vào website kèm ảnh chụp:
cd website && npm run bao-cao-mau
```
