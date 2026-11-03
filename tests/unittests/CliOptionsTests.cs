namespace Tsudev.Audit.Tests;

/// <summary>
/// Muc 11 cua bo test cu: CLI phan tich tham so.
///
/// README tung tuyen bo "CLI parse tham so: 16/16 test" trong khi KHONG co
/// mot test CLI nao - CliOptions nam trong project net8.0-windows nen bo test
/// chay tren Linux khong voi toi duoc. Da chuyen sang Core de tuyen bo do
/// thanh su that.
/// </summary>
public class CliOptionsTests
{
    private static bool Rejects(params string[] args)
    {
        try { CliOptions.Parse(args); return false; } catch (ArgumentException) { return true; }
    }

    [Fact]
    public void Gia_tri_mac_dinh()
    {
        var d = CliOptions.Parse(Array.Empty<string>());

        Assert.True(d.Scope == AuditScope.All && !d.Silent && d.RunDism && !d.RunSfc && d.ExportCsv,
            "mac dinh: quet ca hai, chay DISM, xuat CSV, khong chay SFC");
        Assert.True(!d.ShowHelp && !d.ShowVersion && !d.NoVerdictExit && d.RulesPath is null,
            "mac dinh: khong bat co nao");
    }

    [Fact]
    public void Ba_dang_tham_so_tro_giup()
        => Assert.True(CliOptions.Parse(new[]{"-h"}).ShowHelp
                    && CliOptions.Parse(new[]{"--help"}).ShowHelp
                    && CliOptions.Parse(new[]{"/?"}).ShowHelp, "ba dang tham so tro giup deu nhan");

    [Fact]
    public void Cac_co_don_le()
    {
        Assert.True(CliOptions.Parse(new[]{"--version"}).ShowVersion, "--version");
        Assert.True(CliOptions.Parse(new[]{"--silent"}).Silent && CliOptions.Parse(new[]{"-s"}).Silent, "--silent / -s");
        Assert.True(CliOptions.Parse(new[]{"--sfc"}).RunSfc, "--sfc bat quet sau");
        Assert.True(!CliOptions.Parse(new[]{"--no-dism"}).RunDism, "--no-dism tat DISM");
        Assert.True(!CliOptions.Parse(new[]{"--no-csv"}).ExportCsv, "--no-csv tat xuat CSV");
        Assert.True(CliOptions.Parse(new[]{"--verbose"}).Verbose && CliOptions.Parse(new[]{"-v"}).Verbose, "--verbose / -v");
        Assert.True(CliOptions.Parse(new[]{"--no-verdict-exit"}).NoVerdictExit, "--no-verdict-exit");
    }

    [Fact]
    public void Scope_nhan_ca_ba_gia_tri_khong_phan_biet_hoa_thuong()
        => Assert.True(CliOptions.Parse(new[]{"--scope","license"}).Scope == AuditScope.License
                    && CliOptions.Parse(new[]{"--scope","HARDWARE"}).Scope == AuditScope.Hardware
                    && CliOptions.Parse(new[]{"--scope","All"}).Scope == AuditScope.All,
            "--scope nhan ca ba gia tri, khong phan biet hoa thuong");

    [Fact]
    public void Duong_dan_duoc_nhan_va_giu_nguyen_hoa_thuong()
    {
        Assert.True(CliOptions.Parse(new[]{"-o",@"D:\BaoCao"}).OutputRoot == @"D:\BaoCao", "-o nhan duong dan");
        Assert.True(CliOptions.Parse(new[]{"--rules",@"C:\luat.json"}).RulesPath == @"C:\luat.json", "--rules nhan duong dan");

        // Duong dan KHONG duoc ha ve chu thuong: he thong tep cua Windows giu nguyen
        // hoa thuong, va tren nen tang khac thi duong dan phan biet hoa thuong.
        Assert.True(CliOptions.Parse(new[]{"-o",@"D:\BaoCao"}).OutputRoot != @"d:\baocao",
            "duong dan giu nguyen hoa thuong");
    }

    [Theory]
    [InlineData(new[]{"--khong-ton-tai"}, "tu choi tham so la")]
    [InlineData(new[]{"--scope"}, "--scope thieu gia tri -> loi")]
    [InlineData(new[]{"--scope","linh-tinh"}, "--scope gia tri sai -> loi")]
    [InlineData(new[]{"--output"}, "--output thieu duong dan -> loi")]
    [InlineData(new[]{"--rules"}, "--rules thieu duong dan -> loi")]
    public void Tu_choi_tham_so_sai(string[] args, string vi) => Assert.True(Rejects(args), vi);

    [Fact]
    public void Ket_hop_nhieu_tham_so()
    {
        var combo = CliOptions.Parse(new[]{"--scope","license","-o","D:\\X","--silent","--sfc","--no-csv"});

        Assert.True(combo.Scope == AuditScope.License && combo.OutputRoot == "D:\\X"
                 && combo.Silent && combo.RunSfc && !combo.ExportCsv, "ket hop nhieu tham so");
    }

    [Fact]
    public void Phan_tro_giup_dung_ten_lenh_moi()
        => Assert.True(CliOptions.UsageText.Contains("swico.exe") && !CliOptions.UsageText.Contains("tsudev-audit.exe"),
            "phan tro giup dung ten lenh moi");
}
