namespace Tsudev.Audit.Tests;

/// <summary>
/// Muc 8c cua bo test cu: Office phai co tieng noi trong ket luan.
///
/// CA HOI QUY DUNG THEO MAY THAT CUA NGUOI DUNG (TSUITSTMY, 18/08/2026):
///   Windows Professional (ApplicationID 55c92734-...) -> LicenseStatus 1 (Licensed)
///   Office 16 HomePremR  (ApplicationID 0ff1ce15-...) -> LicenseStatus 5 (Notification)
/// Truoc day cong cu ket luan "KHONG PHAT HIEN DAU HIEU" vi:
///   (a) WindowsLicenseCollector loc theo ApplicationID cua Windows nen khong
///       thay SKU Office, va
///   (b) trang thai Office khong he tham gia vao ket luan tong the.
/// Bo PowerShell cu bao "khong hop le" - va no dung.
/// </summary>
public class OfficeVerdictTests
{
    private static SystemContext RealCtx()
    {
        var realWorld = new FakeWmiQuery()
            .Add("SoftwareLicensingProduct", new Dictionary<string, object?>
            {
                ["Name"] = "Windows(R), Professional edition",
                ["Description"] = "Windows(R) Operating System, RETAIL channel",
                ["ApplicationID"] = "55c92734-d682-4d71-983e-d6ec3f16059f",
                ["LicenseStatus"] = 1L, ["PartialProductKey"] = "ABCDE", ["ID"] = "sku-win"
            })
            .Add("SoftwareLicensingProduct", new Dictionary<string, object?>
            {
                ["Name"] = "Office 16, Office16HomePremR_Retail edition",
                ["Description"] = "Office 16, RETAIL channel",
                ["ApplicationID"] = "0ff1ce15-a989-479d-af46-f275c6370663",
                ["LicenseStatus"] = 5L, ["PartialProductKey"] = "FGHIJ", ["ID"] = "sku-office"
            });

        return new SystemContext
        {
            Wmi = realWorld, Registry = new FakeRegistryReader(),
            Process = new FakeProcessRunner(), Files = new FakeFileProbe(),
            ComputerName = "TSUITSTMY", ScanTime = DateTimeOffset.Now, IsElevated = true
        };
    }

    [Fact]
    public void Doc_duoc_SKU_Office_tu_WMI()
    {
        var officeSummary = OfficeLicenseCollector.Collect(RealCtx()).Summary;

        Assert.True(officeSummary.IsInstalled && officeSummary.Total == 1,
            "doc duoc SKU Office tu WMI (khong can ospp.vbs)");
        Assert.True(officeSummary.Overall == LicenseHealth.Problem,
            "Office o trang thai Notification -> xep loai Problem");
    }

    [Fact]
    public void Windows_hop_le_nhung_Office_chua_kich_hoat_thi_CANH_BAO()
    {
        var officeSummary = OfficeLicenseCollector.Collect(RealCtx()).Summary;
        var realVerdict = RiskScoring.BuildVerdict(
            Array.Empty<RiskFinding>(), LicenseHealth.Ok, officeSummary.Overall);

        Assert.True(realVerdict.Level == VerdictLevel.Warning,
            "Windows hop le + Office chua kich hoat -> CANH BAO, khong con la 'khong phat hien dau hieu'");
        Assert.True(realVerdict.Title.Contains("OFFICE", StringComparison.Ordinal),
            "ket luan noi RO la van de nam o Office");

        // Office chua kich hoat la van de TUAN THU, khong phai dau hieu crack.
        // Gop chung vao muc Bad se lam mat y nghia cua muc Bad.
        Assert.True(realVerdict.Level != VerdictLevel.Bad,
            "Office chua kich hoat KHONG bi day len muc Bad");
    }

    [Theory]
    [InlineData("Notification (Chưa kích hoạt - đã hết hạn thông báo)", LicenseHealth.Problem)]
    [InlineData("Licensed (Đã kích hoạt)", LicenseHealth.Ok)]
    [InlineData("Grace period (Chưa kích hoạt - còn hạn dùng thử)", LicenseHealth.Grace)]
    public void Xep_loai_trang_thai_ospp(string status, LicenseHealth expected)
        => Assert.True(OfficeLicenseCollector.ClassifyOspp(status) == expected,
            $"xep loai trang thai ospp.vbs dung: '{status}'");
}
