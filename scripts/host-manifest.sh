#!/usr/bin/env bash
# In ra BỘ SỐ ĐO CHUẨN của một bản sao repo này, để so hai máy bằng `diff`.
#
#   # ở máy nguồn
#   bash scripts/host-manifest.sh > /tmp/nguon.txt
#   # ở máy đích
#   bash scripts/host-manifest.sh > /tmp/dich.txt
#   diff /tmp/nguon.txt /tmp/dich.txt && echo "HAI MAY KHOP"
#
# Ba tầng nghiệm thu độc lập, vì mỗi tầng một mình đều bỏ sót được:
#   1. git      - phủ mọi file được theo dõi. HEAD trùng + 0 file lệch là bằng
#                 chứng MẠNH, vì git so bằng hash NỘI DUNG.
#   2. ngoaigit - nhóm `.gitignore` loại ra nhưng VẪN PHẢI đi theo. Không tầng
#                 nào khác thấy chúng.
#   3. cay      - vân tay SHA-256 của mọi file rsync sẽ chuyển (trừ .git).
#                 Độc lập với sổ sách của git: nó băm nội dung trên đĩa, nên
#                 bắt được cả hỏng hóc mà `git status` không nhìn tới.
#
# Danh tính máy in ra STDERR, không phải stdout - để `diff` của hai máy sạch.
#
# Khuôn lấy từ scripts/host-manifest.sh của ~/projects/tsudev (§10.1, §12.4
# của ~/projects/SO-TAY-DI-CHUYEN-CODEBASE.md). Bốn quyết định thiết kế giữ
# nguyên: LC_ALL=C, danh tính ra stderr, in cả SỐ LƯỢNG lẫn vân tay, và nói
# thẳng khi một phép đo không hợp lệ thay vì in số trông như số đo.
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

# LC_ALL=C ở mọi phép sort: hai máy locale khác nhau (ở đây là vi_VN trên Mint
# và C.UTF-8 trên Ubuntu mới) thì thứ tự sort đổi theo, và tổng vân tay đổi
# theo nó - báo lệch ở nơi không có gì lệch.
export LC_ALL=C

printf 'may      : %s@%s\n' "$(whoami)" "$(hostname)" >&2
printf 'thu muc  : %s\n'    "$ROOT_DIR" >&2

ra() { printf '%-20s %s\n' "$1" "$2"; }

# ── Tầng 1: git ─────────────────────────────────────────────────────────────
ra git.head   "$(git rev-parse --short HEAD 2>/dev/null || echo KHONG-PHAI-REPO)"
ra git.branch "$(git rev-parse --abbrev-ref HEAD 2>/dev/null || echo -)"
ra git.dirty  "$(git status --porcelain 2>/dev/null | wc -l)"
ra git.tracked "$(git ls-files 2>/dev/null | wc -l)"

# Số file lệch KHỚP NHAU chưa chứng minh nội dung lệch giống nhau. rsync mang
# theo cây làm việc, nên repo có việc chưa commit cần thêm vân tay cho CHÍNH
# phần chưa commit đó.
#
# Hạn chế đã biết: `git diff` in "Binary files differ" thay vì nội dung, nên
# thay đổi trong file nhị phân ĐANG THEO DÕI chỉ hiện là "có thay đổi". File
# nhị phân CHƯA theo dõi thì vẫn được băm đầy đủ. Tầng 3 phủ nốt chỗ này.
if [ "$(git status --porcelain 2>/dev/null | wc -l)" -eq 0 ]; then
  ra git.wip.sha256 "(sach)"
else
  ra git.wip.sha256 "$(
    {
      git diff HEAD 2>/dev/null || true
      git ls-files --others --exclude-standard -z 2>/dev/null | sort -z \
        | xargs -0 -r sha256sum 2>/dev/null || true
    } | sha256sum | cut -d' ' -f1
  )"
fi

# ── Nhóm ngoài git của tsudev-swico ─────────────────────────────────────────
# Đã kiểm 03/11/2026: .gitignore loại `*secret*`, `*credential*`, `*.pfx`,
# `secrets.json` - nhưng KHÔNG file nào như thế tồn tại. Toàn bộ nhóm ignore
# là bin/ obj/ dist/ publish/, sinh lại được bằng `./build.ps1`.
#
# ⚠️ REPO NÀY CÓ 42 FILE LỆCH chưa có trên origin (15 sửa + 27 chưa theo dõi).
# Vì vậy `git.wip.sha256` ở tầng 1 là phép đo QUAN TRỌNG NHẤT của file này -
# chỉ so con số 42 thì một file sai trong đó đi qua mà không ai thấy (§12.3).
NGOAI_GIT=()

# ── Tầng 2: nhóm NGOÀI git ──────────────────────────────────────────────────
# Mẫu nào không khớp file nào thì lặng lẽ bỏ qua, nhưng SỐ LƯỢNG được in ra -
# nên một file biến mất vẫn hiện thành lệch, không bị che.
liet_ke_ngoai_git() {
  {
    for pat in "${NGOAI_GIT[@]}"; do
      for f in $pat; do [ -f "$f" ] && printf '%s\n' "$f"; done
    done
  } | sort -u
}
ds="$(liet_ke_ngoai_git)"
if [ -n "$ds" ]; then
  ra ngoaigit.files  "$(printf '%s\n' "$ds" | wc -l)"
  ra ngoaigit.sha256 "$(printf '%s\n' "$ds" | tr '\n' '\0' | xargs -0 sha256sum | sha256sum | cut -d' ' -f1)"
else
  ra ngoaigit.files  0
  ra ngoaigit.sha256 "(khong co file nao)"
fi

# ── Tầng 3: vân tay cây làm việc ────────────────────────────────────────────
# Băm đúng tập file mà rsync sẽ chuyển, nên hai máy phải ra cùng một số.
# Loại .git/ vì nội dung pack file KHÁC NHAU hợp lệ giữa hai máy (git nén và
# đặt lại object theo cách riêng mỗi lần) trong khi HEAD vẫn trùng - đưa nó
# vào là tạo ra một phép đo báo lệch mãi mà không có gì sai.
if [ ! -f .rsync-exclude ]; then
  ra cay.files  "(khong co .rsync-exclude)"
  ra cay.sha256 "(khong co .rsync-exclude)"
else
  ds3="$(rsync -an --exclude-from=.rsync-exclude --exclude='.git/' \
           --out-format='%n' ./ /tmp/host-manifest-probe-$$/ 2>/dev/null \
           | grep -v '/$' | sort -u)"
  rm -rf "/tmp/host-manifest-probe-$$" 2>/dev/null || true
  if [ -n "$ds3" ]; then
    ra cay.files  "$(printf '%s\n' "$ds3" | wc -l)"
    ra cay.sha256 "$(printf '%s\n' "$ds3" | tr '\n' '\0' | xargs -0 sha256sum | sha256sum | cut -d' ' -f1)"
  else
    ra cay.files  0
    ra cay.sha256 "(trong - DANG NGHI)"
  fi
fi
