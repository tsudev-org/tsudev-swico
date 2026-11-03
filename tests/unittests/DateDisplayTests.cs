using System.Globalization;

namespace Tsudev.Audit.Tests;

/// <summary>
/// Muc 18 cua bo test cu: dinh dang ngay gio hien thi (docs/DESIGN_SYSTEM.md).
///
/// Quy uoc bat buoc toan he thong:
///     Ngay     : DD/MM/YYYY        Ngay gio : HH:mm DD/MM/YYYY
/// Truoc phien S005 bao cao in ra BA dinh dang khac nhau cho cung mot khai niem
/// (dd/MM/yyyy HH:mm, dd/MM/yyyy HH:mm:ss, yyyy-MM-dd HH:mm:ss) va KHONG co test
/// nao chan lai. Lop nay la cai chan do.
/// </summary>
public class DateDisplayTests
{
    private static readonly DateTimeOffset Moc = new(2026, 8, 19, 14, 30, 5, TimeSpan.FromHours(7));

    [Fact]
    public void Dung_vi_du_trong_quy_uoc()
    {
        Assert.True(DateDisplay.Date(new DateTime(2027, 2, 1)) == "01/02/2027",
            "Date -> DD/MM/YYYY dung vi du trong quy uoc");
        Assert.True(DateDisplay.DateTimeText(new DateTime(2026, 8, 19, 14, 30, 0)) == "14:30 19/08/2026",
            "DateTimeText -> HH:mm DD/MM/YYYY dung vi du trong quy uoc");
        Assert.True(DateDisplay.Date(new DateTime(2026, 3, 7)) == "07/03/2026",
            "ngay va thang MOT chu so van du hai chu so");
    }

    [Fact]
    public void Ngay_gio_khong_kem_giay()
    {
        var mocLocal = Moc.LocalDateTime;

        Assert.True(!DateDisplay.DateTimeText(mocLocal).Contains(":05", StringComparison.Ordinal)
                 && DateDisplay.DateTimeText(mocLocal).Length == "HH:mm DD/MM/YYYY".Length,
            "ngay gio KHONG kem giay - quy uoc chi den phut");
        Assert.True(DateDisplay.DateTimeText(Moc) == DateDisplay.DateTimeText(mocLocal),
            "ban DateTimeOffset quy ve gio may dang chay, khong lech mot tieng");
    }

    /// <summary>
    /// Vi sao khoa culture: tren may ngon ngu Thai, lich mac dinh la Phat lich va
    /// "yyyy" cho ra 2569 thay vi 2026; mot so ngon ngu doi luon dau "/" thanh "."
    /// hoac "-". Bao cao gui cho ke toan doc phai giong nhau tren moi may.
    ///
    /// (Day la mot trong hai ly do bo test nay chay TUAN TU - xem AssemblyInfo.cs.)
    /// </summary>
    [Fact]
    public void Khong_phu_thuoc_ngon_ngu_cua_may()
    {
        var vanHoaCu = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("th-TH");
            Assert.True(DateDisplay.Date(new DateTime(2026, 8, 19)) == "19/08/2026",
                "may dat ngon ngu Thai van ra nam Duong lich 2026, khong phai 2569");

            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            Assert.True(DateDisplay.DateTimeText(new DateTime(2026, 8, 19, 14, 30, 0)) == "14:30 19/08/2026",
                "may dat ngon ngu Duc van dung dau '/' chu khong doi thanh '.'");
        }
        finally { CultureInfo.CurrentCulture = vanHoaCu; }
    }

    /// <summary>
    /// Ten FILE thi NGUOC LAI: phai sap xep duoc theo thu tu chu cai nen giu
    /// yyyyMMdd. Hai luat nay khac nhau CO CHU DICH - dung "thong nhat" chung lai.
    /// </summary>
    [Fact]
    public void Ten_file_giu_dang_sap_xep_duoc()
    {
        var ctxTen = H.EmptyCtx("MAY-KE-TOAN", Moc);

        Assert.True(FileNaming.Stamp(ctxTen) == Moc.LocalDateTime.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture),
            "ten file GIU yyyyMMdd_HHmmss - sap xep duoc theo thu tu chu cai");
        Assert.True(!FileNaming.Stamp(ctxTen).Contains('/', StringComparison.Ordinal),
            "ten file khong duoc chua '/' - se thanh duong dan thu muc");
        Assert.True(FileNaming.DateOnly(Moc) == Moc.LocalDateTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
            "ten thu muc theo ngay GIU yyyyMMdd");
    }

    /// <summary>Ngay cai dat doc tu registry cung phai theo quy uoc hien thi.</summary>
    [Fact]
    public void InstallDate_registry_theo_quy_uoc()
    {
        Assert.True(SoftwareCollector.FormatInstallDate("20260819") == "19/08/2026",
            "InstallDate registry -> DD/MM/YYYY");
        Assert.True(SoftwareCollector.FormatInstallDate("khong-phai-ngay") == "khong-phai-ngay",
            "chuoi khong phai ngay thi giu nguyen de nguoi doc thay registry ghi gi");
    }
}
