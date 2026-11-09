import { docTep, TEP_LUAT } from '../../lib/noi-dung';

// Nguyên văn tệp luật đóng kèm, để người dùng tải về đặt cạnh swico.exe.
export function GET() {
  return new Response(docTep(TEP_LUAT), { headers: { 'Content-Type': 'application/json; charset=utf-8' } });
}
