namespace Tsudev.Audit.Tests;

/// <summary>Muc 3 cua bo test cu: tinh diem rui ro va bien cua tung nhan.</summary>
public class RiskScoringTests
{
    private static List<RiskFinding> N(RiskLevel level, int count)
        => Enumerable.Range(0, count).Select(_ => new RiskFinding { Level = level }).ToList();

    /// <summary>
    /// Bang bien nay TRUOC DAY nam trong mot vong foreach co `break` ngay sau ca
    /// dau tien, nen bay ca so 0 la ca duy nhat tung chay. Nay moi diem duoc
    /// dung THAT bang cach cong tung nhom trong so:
    ///   genuineFailed=40, Critical 15/cai (tran 30), High 10/cai (tran 30),
    ///   Medium 5/cai (tran 15), kiem tra thu cong 2/cai (tran 15).
    /// </summary>
    [Theory]
    [InlineData(0, 0, 0, 0, false, 0, "Thấp")]
    [InlineData(0, 0, 0, 7, false, 14, "Thấp")]
    [InlineData(0, 0, 0, 8, false, 15, "Trung bình")]   // tran kiem tra thu cong: 8*2 -> 15
    [InlineData(0, 2, 3, 2, false, 39, "Trung bình")]
    [InlineData(0, 0, 0, 0, true, 40, "Cao")]
    [InlineData(1, 0, 2, 2, true, 69, "Cao")]
    [InlineData(2, 0, 0, 0, true, 70, "Nghiêm trọng")]
    [InlineData(2, 3, 0, 0, true, 100, "Nghiêm trọng")]
    public void Bien_cua_tung_nhan_diem(int critical, int high, int medium, int manual,
                                        bool genuineFailed, int expectedScore, string expectedLabel)
    {
        var findings = N(RiskLevel.Critical, critical)
            .Concat(N(RiskLevel.High, high))
            .Concat(N(RiskLevel.Medium, medium)).ToList();

        var rs = RiskScoring.Compute(findings, genuineFailed, manual);

        Assert.True(rs.Value == expectedScore, $"diem mong doi {expectedScore}, duoc {rs.Value}");
        Assert.True(rs.Label == expectedLabel, $"nhan mong doi '{expectedLabel}', duoc '{rs.Label}'");
    }

    [Fact]
    public void Tran_cua_nhom_Critical_hoat_dong()
    {
        var scoreCritical = RiskScoring.Compute(N(RiskLevel.Critical, 3), genuineCheckFailed: true, manualReviewCount: 0);

        Assert.True(scoreCritical.Value == RiskScoring.GenuineFailedWeight + RiskScoring.CriticalCap,
            $"gioi han nhom Critical hoat dong (duoc {scoreCritical.Value})");
        Assert.True(scoreCritical.Label == "Nghiêm trọng", "nhan bang = Nghiem trong");
    }

    [Fact]
    public void Diem_khong_bao_gio_vuot_100()
    {
        var scoreCapped = RiskScoring.Compute(N(RiskLevel.High, 50), true, 100);
        Assert.True(scoreCapped.Value <= 100, $"diem khong bao gio vuot 100 (duoc {scoreCapped.Value})");
    }
}
