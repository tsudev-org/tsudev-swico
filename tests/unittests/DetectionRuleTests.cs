using System.Text.Json;

namespace Tsudev.Audit.Tests;

/// <summary>
/// Muc 10 cua bo test cu: bo luat phat hien tach roi.
///
/// Bo luat dong kem phai nap duoc va hop le - neu hong thi moi may deu "sach",
/// tao cam giac an toan gia, nen day la ca kiem thu quan trong.
/// </summary>
public class DetectionRuleTests
{
    private const string CustomJson = """
{
  "version": "test-9.9",
  "scanRoots": ["%ProgramFiles%"],
  "suspiciousNames": ["CongCuBiaDatRaDeTest"],
  "legitimateTaskNames": ["SoftwareProtectionPlatform"],
  "hookDirectories": ["%SystemRoot%\\System32"],
  "hookFiles": ["KhongCoThat.dll"],
  "knownKmsHosts": [],
  "hostsInterferenceKeywords": []
}
""";

    [Fact]
    public void Bo_luat_dong_kem_hop_le()
    {
        var embedded = DetectionRuleSet.Embedded;

        Assert.True(embedded.Validate().Count == 0, "bo luat dong kem hop le");
        Assert.True(!string.IsNullOrWhiteSpace(embedded.Version), "bo luat dong kem co so hieu phien ban");
        Assert.True(embedded.SuspiciousNames.Contains("KMSAuto"), "bo luat dong kem giu du ten nghi van");
    }

    /// <summary>
    /// Scan(ctx) va Scan(ctx, Embedded) phai cho KET QUA Y HET - chung minh viec
    /// tach luat khong lam doi hanh vi.
    /// </summary>
    [Fact]
    public void Tach_luat_khong_lam_doi_hanh_vi_quet()
    {
        var viaDefault = ActivationRiskScanner.Scan(Infected.Ctx);
        var viaExplicit = ActivationRiskScanner.Scan(Infected.Ctx, DetectionRuleSet.Embedded);

        Assert.True(viaDefault.Count == viaExplicit.Count, "tach luat KHONG lam doi hanh vi quet");
    }

    [Fact]
    public void Bo_luat_tu_file_ngoai_thuc_su_duoc_dung()
    {
        var custom = DetectionRuleSet.Parse(CustomJson);

        Assert.True(custom.Version == "test-9.9" && custom.Validate().Count == 0, "nap duoc bo luat tu JSON ngoai");
        Assert.True(ActivationRiskScanner.Scan(Infected.Ctx, custom).Count < ActivationRiskScanner.Scan(Infected.Ctx).Count,
            "bo luat khac cho ket qua khac (luat that su duoc dung)");
    }

    [Fact]
    public void File_luat_thieu_quay_ve_ban_dong_kem()
    {
        var missing = DetectionRuleSet.LoadOrEmbedded("/khong/ton/tai/rules.json", out var w1);

        Assert.True(ReferenceEquals(missing, DetectionRuleSet.Embedded) && w1 is not null,
            "file luat thieu -> quay ve bo luat dong kem kem canh bao");
    }

    [Fact]
    public void File_luat_sai_dinh_dang_quay_ve_ban_dong_kem()
    {
        var badPath = H.TempFile("bad-rules", ".json");
        try
        {
            File.WriteAllText(badPath, "{ day khong phai JSON hop le ");
            var broken = DetectionRuleSet.LoadOrEmbedded(badPath, out var w2);

            Assert.True(ReferenceEquals(broken, DetectionRuleSet.Embedded) && w2 is not null,
                "file luat sai dinh dang -> quay ve bo luat dong kem kem canh bao");
        }
        finally { File.Delete(badPath); }
    }

    /// <summary>Bo luat RONG nguy hiem hon bo luat sai: no lam moi may deu "sach".</summary>
    [Fact]
    public void Bo_luat_rong_bi_tu_choi()
    {
        var emptyPath = H.TempFile("empty-rules", ".json");
        try
        {
            File.WriteAllText(emptyPath, """{"version":"rong","suspiciousNames":[],"scanRoots":[],"hookFiles":[],"hookDirectories":[]}""");
            var empty = DetectionRuleSet.LoadOrEmbedded(emptyPath, out var w3);

            Assert.True(ReferenceEquals(empty, DetectionRuleSet.Embedded) && w3 is not null,
                "bo luat RONG bi tu choi (khong duoc tao cam giac an toan gia)");
        }
        finally { File.Delete(emptyPath); }
    }

    [Fact]
    public void Bao_cao_ghi_lai_phien_ban_bo_luat_da_dung()
        => Assert.True(Infected.Lic.DetectionRulesVersion == DetectionRuleSet.Embedded.Version,
            "bao cao ghi lai phien ban bo luat da dung");

    /// <summary>
    /// CAM BAY: file luat ben ngoai LUON thang bo luat dong kem. Sau khi nang cap
    /// exe, mot file .json CU nam canh exe se am tham vo hieu hoa bo luat moi.
    /// Phai co canh bao ro rang, neu khong khong ai biet dieu do dang xay ra.
    /// </summary>
    [Fact]
    public void Lech_phien_ban_giua_file_ngoai_va_ban_dong_kem_thi_canh_bao()
    {
        var stalePath = H.TempFile("stale-rules", ".json");
        try
        {
            File.WriteAllText(stalePath, CustomJson);          // phien ban "test-9.9", khac ban dong kem
            var stale = DetectionRuleSet.LoadOrEmbedded(stalePath, out var staleWarn);

            Assert.True(stale.Version == "test-9.9", "file luat ngoai duoc uu tien hon ban dong kem");
            Assert.True(staleWarn is not null
                     && staleWarn.Contains("test-9.9", StringComparison.Ordinal)
                     && staleWarn.Contains(DetectionRuleSet.Embedded.Version, StringComparison.Ordinal),
                "lech phien ban giua file ngoai va ban dong kem -> canh bao neu RO ca hai phien ban");
        }
        finally { File.Delete(stalePath); }
    }

    /// <summary>
    /// Cung phien ban thi KHONG canh bao - tranh lam nhieu bao cao moi lan quet.
    /// Dung lai chinh bo luat dong kem thay vi doc file theo duong dan tuong doi:
    /// duong dan tuong doi phu thuoc thu muc chay va se hong khi doi cach chay test.
    /// </summary>
    [Fact]
    public void File_luat_ngoai_cung_phien_ban_thi_khong_canh_bao_thua()
    {
        var samePath = H.TempFile("same-rules", ".json");
        try
        {
            File.WriteAllText(samePath, JsonSerializer.Serialize(DetectionRuleSet.Embedded));
            var same = DetectionRuleSet.LoadOrEmbedded(samePath, out var sameWarn);

            Assert.True(same.Version == DetectionRuleSet.Embedded.Version && sameWarn is null,
                "file luat ngoai CUNG phien ban -> khong canh bao thua");
        }
        finally { File.Delete(samePath); }
    }
}
