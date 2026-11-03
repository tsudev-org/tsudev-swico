using Xunit;

// Bo test nay chay TUAN TU, co chu dich - dung bat song song lai.
//
// Hai ly do cu the, ca hai deu la loi "chay thi xanh, chay lai thi do":
//   1. DateDisplayTests doi CultureInfo.CurrentCulture sang th-TH / de-DE roi
//      tra lai. Doi tren mot luong trong khi luong khac dang doc ngay thang la
//      ket qua bap benh.
//   2. Infected.Lic / Infected.Html la du lieu dung chung. XlsxWriter.Write dat
//      lai Title cho bang chua co ten - hai test cung ghi mot luc se dam nhau.
//      (Cac test do da dung Infected.FreshLicenseReport(), day la lop bao ve
//      thu hai.)
//
// Ca bo chay duoi 5 giay nen chay song song khong doi lai duoc gi dang ke.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
