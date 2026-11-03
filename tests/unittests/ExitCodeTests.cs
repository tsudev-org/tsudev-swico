namespace Tsudev.Audit.Tests;

/// <summary>Muc 12 cua bo test cu: ma thoat.</summary>
public class ExitCodeTests
{
    [Fact]
    public void Ma_thoat_suy_ra_tu_cac_ket_luan()
    {
        Assert.True(ExitCodes.FromVerdicts(new[]{ VerdictLevel.Ok, VerdictLevel.Unknown }) == ExitCodes.Ok,
            "khong co ket luan dang bao dong -> 0");
        Assert.True(ExitCodes.FromVerdicts(new[]{ VerdictLevel.Ok, VerdictLevel.Warning }) == ExitCodes.VerdictWarning,
            "co canh bao -> 10");
        Assert.True(ExitCodes.FromVerdicts(new[]{ VerdictLevel.Warning, VerdictLevel.Bad }) == ExitCodes.VerdictCritical,
            "co ket luan nghiem trong -> 20 (thang canh bao)");
        Assert.True(ExitCodes.FromVerdicts(Array.Empty<VerdictLevel>()) == ExitCodes.Ok, "khong co bao cao nao -> 0");
    }

    /// <summary>
    /// CA HOI QUY DUNG THEO MAY THAT: Windows hop le + Office chua kich hoat,
    /// truoc day tra ve 0 nen script khong bat duoc.
    /// </summary>
    [Fact]
    public void Office_chua_kich_hoat_khong_con_tra_ve_0()
        => Assert.True(ExitCodes.Combine(ExitCodes.Ok, ExitCodes.VerdictWarning) == ExitCodes.VerdictWarning,
            "Office chua kich hoat -> ma thoat 10, KHONG con la 0");

    [Fact]
    public void Thu_tu_uu_tien_khi_gop_ma_thoat()
    {
        Assert.True(ExitCodes.Combine(ExitCodes.Partial, ExitCodes.VerdictCritical) == ExitCodes.VerdictCritical,
            "ket luan danh gia thang 'thieu du lieu'");
        Assert.True(ExitCodes.Combine(ExitCodes.Fatal, ExitCodes.VerdictCritical) == ExitCodes.Fatal,
            "loi nghiem trong thang tat ca");
        Assert.True(ExitCodes.Combine(ExitCodes.BadArgs, ExitCodes.VerdictWarning) == ExitCodes.BadArgs,
            "tham so sai thang ket luan danh gia");
        Assert.True(ExitCodes.Combine(ExitCodes.Partial, ExitCodes.Ok) == ExitCodes.Partial,
            "thieu du lieu ma khong co ket luan -> giu ma 1");
        Assert.True(ExitCodes.Combine(ExitCodes.Ok, ExitCodes.Ok) == ExitCodes.Ok, "moi thu binh thuong -> 0");
    }

    [Fact]
    public void Mo_ta_ma_thoat_bang_tieng_Viet()
        => Assert.True(ExitCodes.Describe(ExitCodes.VerdictWarning).Contains("CẢNH BÁO", StringComparison.Ordinal),
            "mo ta ma thoat bang tieng Viet");
}
