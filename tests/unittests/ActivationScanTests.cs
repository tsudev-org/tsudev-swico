namespace Tsudev.Audit.Tests;

/// <summary>Muc 4 cua bo test cu: quet dau hieu crack tren may GIA LAP BI NHIEM.</summary>
public class ActivationScanTests
{
    private static List<RiskFinding> Found => Infected.Findings;

    [Theory]
    [InlineData("Tên file/thư mục nghi vấn", "phat hien thu muc KMSAuto")]
    [InlineData("Hosts file bị chỉnh sửa", "phat hien hosts tro toi KMS cong cong")]
    [InlineData("Windows Service nghi vấn", "phat hien service KMSAuto")]
    [InlineData("KMS server tùy chỉnh trong registry", "phat hien KMS server trong registry")]
    public void Phat_hien_dung_tung_nhom_dau_hieu(string category, string vi)
        => Assert.True(Found.Any(f => f.Category == category), vi);

    [Fact]
    public void File_hook_he_thong_ban_quyen_xep_muc_Rat_cao()
        => Assert.True(Found.Any(f => f.Category == "File hook hệ thống bản quyền" && f.Level == RiskLevel.Critical),
            "phat hien file hook SppExtComObjHook.dll (muc Rat cao)");

    [Fact]
    public void Khong_bao_dong_nham_service_hop_le()
        => Assert.True(!Found.Any(f => f.Detection.Contains("Print Spooler")),
            "KHONG bao dong nham service hop le");

    [Fact]
    public void Bang_pham_vi_quet_luon_du_sau_hang_muc()
    {
        var scope = ActivationRiskScanner.BuildScope(Found);

        Assert.True(scope.Count == 6, "bang pham vi quet luon co du 6 hang muc");
        Assert.True(scope.Sum(s => s.FoundCount) == Found.Count,
            "tong so dem trong bang pham vi khop so phat hien");
    }
}
