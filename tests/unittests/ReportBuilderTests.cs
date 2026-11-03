namespace Tsudev.Audit.Tests;

/// <summary>Muc 7 cua bo test cu: dung bao cao hoan chinh tu may gia lap bi nhiem.</summary>
public class ReportBuilderTests
{
    [Fact]
    public void Bao_cao_ban_quyen_ket_luan_Bad()
    {
        var lic = Infected.Lic;

        Assert.True(lic.VerdictLevel == VerdictLevel.Bad, "ket luan = Bad (do co phat hien muc cao)");
        Assert.True(lic.RiskFindingsCount == Infected.Findings.Count, "so phat hien khop");
        Assert.True(lic.RiskScore!.Value > 0, $"diem rui ro > 0 (duoc {lic.RiskScore.Value})");
    }

    [Fact]
    public void Bao_cao_ban_quyen_du_muc_va_Id_khong_trung()
    {
        var lic = Infected.Lic;

        Assert.True(lic.Sections.Count == 6, $"bao cao ban quyen co 6 muc (duoc {lic.Sections.Count})");
        Assert.True(lic.Sections.Select(s => s.Id).Distinct().Count() == lic.Sections.Count,
            "cac Id muc khong trung nhau");
    }

    [Fact]
    public void Bao_cao_phan_cung_du_tom_tat_va_cac_muc()
    {
        var hw = Infected.Hw;

        Assert.True(hw.HardwareSummary!.Contains("Latitude E5570"), "tom tat phan cung dung");
        Assert.True(hw.Sections.Any(s => s.Id == "sec-defender"), "co muc Defender");
        Assert.True(hw.Sections.Any(s => s.Id == "sec-integrity"), "co muc toan ven he thong");
    }
}
