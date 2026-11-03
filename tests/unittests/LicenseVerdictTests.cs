namespace Tsudev.Audit.Tests;

/// <summary>
/// Muc 8b cua bo test cu: ket luan license khong duoc bao NHAM la hop le.
///
/// LOI HOI QUY THAT (phat hien khi doi chieu voi bo PowerShell cu tren may that):
/// truoc day ket luan duoc suy ra bang licensedCount > 0, tuc chi can MOT SKU
/// bat ky o trang thai Licensed. Windows khai bao NHIEU SKU duoi cung mot
/// ApplicationID, nen may co SKU chinh dang Notification nhung co SKU phu
/// Licensed van bi cham diem "hop le".
/// </summary>
public class LicenseVerdictTests
{
    private static SystemContext LicenseCtx(params (long Status, string Key)[] skus)
    {
        var wmi = new FakeWmiQuery();
        foreach (var (status, key) in skus)
            wmi.Add("SoftwareLicensingProduct", new Dictionary<string, object?>
            {
                ["Name"] = "Windows(R), Professional edition",
                ["Description"] = "Windows(R) Operating System",
                // ApplicationID la BAT BUOC: collector loc theo truong nay de tach
                // SKU Windows khoi SKU Office. Adapter gia lap nay tuan thu
                // whereClause, nen thieu truong la khong dong nao lot qua bo loc.
                ["ApplicationID"] = "55c92734-d682-4d71-983e-d6ec3f16059f",
                ["LicenseStatus"] = status,
                ["PartialProductKey"] = key,
                ["ProductKeyChannel"] = "Retail"
            });
        return new SystemContext
        {
            Wmi = wmi, Registry = new FakeRegistryReader(),
            Process = new FakeProcessRunner(), Files = new FakeFileProbe(),
            ComputerName = "MAY-TEST", ScanTime = DateTimeOffset.Now, IsElevated = true
        };
    }

    private static Verdict VerdictOf(LicenseHealth windows)
        => RiskScoring.BuildVerdict(Array.Empty<RiskFinding>(), windows, LicenseHealth.Unknown);

    // 1 = Licensed, 5 = Notification (chua kich hoat)
    [Fact]
    public void SKU_tron_lan_KHONG_duoc_ket_luan_hop_le()
    {
        var mixed = WindowsLicenseCollector.Collect(LicenseCtx((1, "AAAAA"), (5, "BBBBB"))).Summary;

        Assert.True(!mixed.IsGenuine, "SKU vua Licensed vua Notification -> KHONG duoc ket luan hop le");
        Assert.True(mixed.Overall == LicenseHealth.Problem, "tron lan co SKU hong -> xep loai Problem");
        Assert.True(VerdictOf(mixed.Overall).Level == VerdictLevel.Bad,
            "ket luan cuoi cung la Bad khi co SKU chua kich hoat");
    }

    [Fact]
    public void Moi_SKU_Licensed_thi_hop_le()
    {
        var allOk = WindowsLicenseCollector.Collect(LicenseCtx((1, "AAAAA"), (1, "BBBBB"))).Summary;

        Assert.True(allOk.IsGenuine && allOk.Overall == LicenseHealth.Ok, "moi SKU Licensed -> hop le");
        Assert.True(VerdictOf(allOk.Overall).Level == VerdictLevel.Ok, "ket luan Ok khi moi SKU deu hop le");
    }

    // 2 = OOB Grace: chua sai nhung cung CHUA hop le -> canh bao, khong phai Ok
    [Fact]
    public void Con_han_dung_thu_la_canh_bao_chu_khong_phai_hop_le()
    {
        var graceOnly = WindowsLicenseCollector.Collect(LicenseCtx((1, "AAAAA"), (2, "BBBBB"))).Summary;

        Assert.True(!graceOnly.IsGenuine && graceOnly.Overall == LicenseHealth.Grace,
            "con han dung thu -> KHONG phai hop le");
        Assert.True(VerdictOf(graceOnly.Overall).Level == VerdictLevel.Warning,
            "con han dung thu -> canh bao, khong phai Ok cung khong phai Bad");
    }

    /// <summary>
    /// Bo loc ApplicationID phai tach dung SKU Windows khoi SKU Office: neu khong,
    /// mot Office chua kich hoat se bi cham diem nham thanh "Windows co van de".
    /// </summary>
    [Fact]
    public void Bo_loc_ApplicationID_khong_lay_nham_SKU_Office()
    {
        var mixedApps = new FakeWmiQuery()
            .Add("SoftwareLicensingProduct", new Dictionary<string, object?>
            {
                ["Name"] = "Windows(R), Professional edition",
                ["ApplicationID"] = "55c92734-d682-4d71-983e-d6ec3f16059f",
                ["LicenseStatus"] = 1L, ["PartialProductKey"] = "AAAAA"
            })
            .Add("SoftwareLicensingProduct", new Dictionary<string, object?>
            {
                ["Name"] = "Office 16, Office16HomePremR_Retail edition",
                ["ApplicationID"] = "0ff1ce15-a989-479d-af46-f275c6370663",
                ["LicenseStatus"] = 5L, ["PartialProductKey"] = "BBBBB"
            });
        var winOnly = WindowsLicenseCollector.Collect(new SystemContext
        {
            Wmi = mixedApps, Registry = new FakeRegistryReader(),
            Process = new FakeProcessRunner(), Files = new FakeFileProbe(),
            ComputerName = "MAY-TEST", ScanTime = DateTimeOffset.Now, IsElevated = true
        }).Summary;

        Assert.True(winOnly.Total == 1 && winOnly.IsGenuine,
            "bo loc ApplicationID tach dung SKU Windows, KHONG lay nham SKU Office");
    }

    [Fact]
    public void Khong_doc_duoc_SKU_nao_thi_KHONG_mac_dinh_la_hop_le()
    {
        var none = WindowsLicenseCollector.Collect(LicenseCtx()).Summary;

        Assert.True(!none.IsKnown && none.Overall == LicenseHealth.Unknown, "khong doc duoc SKU nao -> Unknown");
        Assert.True(VerdictOf(none.Overall).Level == VerdictLevel.Unknown,
            "khong co du lieu -> chua du co so ket luan, KHONG mac dinh la hop le");
    }

    [Fact]
    public void Dau_hieu_crack_muc_cao_thang_moi_trang_thai_license()
        => Assert.True(RiskScoring.BuildVerdict(
                new[] { new RiskFinding { Category = "x", Detection = "y", Level = RiskLevel.Critical } },
                LicenseHealth.Ok, LicenseHealth.Ok).Level == VerdictLevel.Bad,
            "phat hien muc Rat cao -> Bad du license bao hop le");
}
