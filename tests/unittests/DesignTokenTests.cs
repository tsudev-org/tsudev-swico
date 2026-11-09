using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Tsudev.Audit.Tests;

/// <summary>
/// Muc 19 cua bo test cu: token giao dien trong bao cao HTML (AGENTS.md muc 6).
///
/// AGENTS.md muc 6: cam hard-code mau / co chu / radius, chi duoc dung token.
/// Rang buoc rieng cua bao cao: KHONG duoc nap tai nguyen ngoai, nen token phai
/// duoc NHUNG thang vao the &lt;style&gt;. Lop nay canh ca hai dieu do.
/// </summary>
public class DesignTokenTests
{
    private static readonly string StyleBlock = ExtractStyle(Infected.Html);

    private static string ExtractStyle(string html)
    {
        var start = html.IndexOf("<style>", StringComparison.Ordinal);
        var end = html.IndexOf("</style>", StringComparison.Ordinal);
        return start > 0 && end > start ? html[(start + "<style>".Length)..end] : "";
    }

    [Fact]
    public void Trang_co_dung_mot_the_style_nhung_san()
    {
        var start = Infected.Html.IndexOf("<style>", StringComparison.Ordinal);
        var end = Infected.Html.IndexOf("</style>", StringComparison.Ordinal);

        Assert.True(start > 0 && end > start, "trang co dung mot the <style> nhung san");
    }

    // --- Token that su den tu file token, khong phai chep tay vao ma
    [Fact]
    public void Token_den_tu_file_token()
    {
        Assert.True(StyleBlock.Contains(DesignTokens.Color("light", "primary"), StringComparison.OrdinalIgnoreCase),
            "mau primary trong CSS lay tu .standards/tokens/design-tokens.json");
        Assert.True(StyleBlock.Contains("'Inter'", StringComparison.Ordinal),
            "font Inter lay tu token (DESIGN_SYSTEM.md muc 4)");
        Assert.True(StyleBlock.Contains("--radius-lg:8px", StringComparison.Ordinal),
            "thang radius lay tu token");
    }

    // --- Phan CSS VIET TAY khong duoc chua mot ma mau / co chu / radius nao
    [Fact]
    public void CSS_viet_tay_khong_hard_code_gi_ca()
    {
        var handWritten = StyleBlock.Replace(DesignTokens.RootCss, "", StringComparison.Ordinal);
        Assert.True(handWritten.Length > 0 && handWritten.Length < StyleBlock.Length,
            "tach duoc phan CSS viet tay ra khoi khoi bien do token sinh");

        var hex = Regex.Matches(handWritten, "#[0-9A-Fa-f]{3,8}\\b");
        Assert.True(hex.Count == 0,
            hex.Count == 0 ? "CSS viet tay: 0 ma mau viet cung"
                           : $"CSS viet tay con {hex.Count} ma mau viet cung ({hex[0].Value})");

        Assert.True(!Regex.IsMatch(handWritten, @"font-size:\s*(?!var\()"),
            "CSS viet tay: moi co chu deu qua var(--fs-...)");
        Assert.True(!Regex.IsMatch(handWritten, @"border-radius:\s*(?!var\()"),
            "CSS viet tay: moi radius deu qua var(--radius-...)");
    }

    /// <summary>
    /// Moi bien duoc DUNG deu phai duoc DINH NGHIA. Thieu mot cai la ca thuoc
    /// tinh do bien mat am tham - trinh duyet khong bao loi.
    /// </summary>
    [Fact]
    public void Moi_bien_CSS_duoc_dung_deu_co_dinh_nghia()
    {
        var dinhNghia = Regex.Matches(StyleBlock, @"(--[a-z0-9-]+)\s*:")
            .Select(m => m.Groups[1].Value).ToHashSet(StringComparer.Ordinal);
        var suDung = Regex.Matches(StyleBlock, @"var\((--[a-z0-9-]+)")
            .Select(m => m.Groups[1].Value).ToHashSet(StringComparer.Ordinal);
        var thieu = suDung.Except(dinhNghia, StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList();

        Assert.True(thieu.Count == 0,
            thieu.Count == 0 ? "moi bien CSS duoc dung deu co dinh nghia"
                             : $"thieu dinh nghia cho: {string.Join(", ", thieu)}");
    }

    // --- Khong nap tai nguyen ngoai: bao cao phai xem duoc khi khong co mang
    [Fact]
    public void Khong_nap_tai_nguyen_ngoai()
    {
        Assert.True(!StyleBlock.Contains("@import", StringComparison.Ordinal), "CSS khong co @import");
        Assert.True(!StyleBlock.Contains("http", StringComparison.Ordinal),
            "CSS khong tro toi dia chi mang nao - bao cao xem duoc khi khong co mang");

        foreach (Match m in Regex.Matches(StyleBlock, @"url\(\s*""?([^""')]+)"))
        {
            var v = m.Groups[1].Value;
            Assert.True(v.StartsWith("data:", StringComparison.Ordinal),
                $"url() trong CSS la du lieu nhung san, khong phai file ngoai ({v[..Math.Min(24, v.Length)]})");
        }
    }

    // --- Ba che do: sang mac dinh, toi theo he dieu hanh, IN LUON dung bang sang
    [Fact]
    public void Co_che_do_toi_theo_he_dieu_hanh()
        => Assert.True(StyleBlock.Contains("@media (prefers-color-scheme:dark)", StringComparison.Ordinal),
            "co khoi che do toi theo he dieu hanh (DESIGN_SYSTEM.md muc 1)");

    /// <summary>
    /// Neu thieu khoi nay, may dat che do toi se in ra to giay den kin muc.
    /// </summary>
    [Fact]
    public void Khi_in_thi_bang_mau_bi_ep_ve_che_do_sang()
    {
        var printIdx = StyleBlock.IndexOf("@media print{\n  :root{", StringComparison.Ordinal);
        Assert.True(printIdx > 0, "co khoi ep bang mau khi in");

        var printBlock = StyleBlock[printIdx..(printIdx + 900)];
        Assert.True(printBlock.Contains("--c-bg-base:" + DesignTokens.Color("light", "bg-base"), StringComparison.Ordinal),
            "khi IN thi bang mau bi ep ve che do SANG du may dang o che do toi");
        Assert.True(!printBlock.Contains(DesignTokens.Color("dark", "bg-base"), StringComparison.Ordinal),
            "khoi in khong lot mau cua che do toi");
    }

    /// <summary>
    /// Chu ky thuong hieu doi cho giua hai che do. O che do toi, dau trang tro
    /// thanh nen SANG nen sac dung cho dau trang phai la sac danh cho nen sang.
    /// </summary>
    [Fact]
    public void Chu_ky_thuong_hieu_doi_cho_giua_hai_che_do()
    {
        var darkIdx = StyleBlock.IndexOf("@media (prefers-color-scheme:dark)", StringComparison.Ordinal);
        var darkBlock = StyleBlock[darkIdx..(darkIdx + 900)];

        // Dau trang KHONG doi mau theo che do (mang thuong hieu), chan trang thi CO.
        Assert.True(!darkBlock.Contains("--brand-tsu-hero", StringComparison.Ordinal)
                 && !darkBlock.Contains("--c-hero-from", StringComparison.Ordinal),
            "dau trang giu nguyen dien mao o che do toi");
        Assert.True(Regex.Match(StyleBlock, @"--brand-tsu-foot:(#[0-9A-Fa-f]{6})").Groups[1].Value
                 != Regex.Match(darkBlock, @"--brand-tsu-foot:(#[0-9A-Fa-f]{6})").Groups[1].Value,
            "sac chu ky o CHAN trang DOI khi sang che do toi (chan trang nam trong nen trang)");
    }

    // --- Mau thanh dia chi cua trinh duyet cung phai lay tu token
    [Fact]
    public void Theme_color_lay_tu_token()
        => Assert.True(Infected.Html.Contains($"content=\"{DesignTokens.Color("light", "primary-active")}\"", StringComparison.Ordinal),
            "theme-color lay tu token, khong viet cung");

    // --- Trang tong hop dung CHUNG bo CSS nay, khong co ban sao thu hai
    [Fact]
    public void Doc_duoc_phien_ban_bo_token()
        => Assert.True(DesignTokens.Version.Length > 0, $"doc duoc phien ban bo token ({DesignTokens.Version})");

    // --- File .xlsx cung phai theo token, khong duoc lech mau voi ban HTML
    [Fact]
    public void File_xlsx_cung_theo_token()
    {
        var stylePath = H.TempFile("tsudev-token", ".xlsx");
        try
        {
            new XlsxWriter().Write(stylePath, Infected.NamedTables(Infected.FreshLicenseReport()));

            using var zip = ZipFile.OpenRead(stylePath);
            var stylesXml = new StreamReader(zip.GetEntry("xl/styles.xml")!.Open()).ReadToEnd();

            // Excel doi ARGB 8 ky tu; token ghi #RRGGBB.
            var fill = "FF" + DesignTokens.Color("light", "bg-subtle").TrimStart('#').ToUpperInvariant();
            Assert.True(stylesXml.Contains(fill, StringComparison.Ordinal),
                "nen dong tieu de trong .xlsx lay tu token bg-subtle");
            Assert.True(!stylesXml.Contains("FFEAF2FC", StringComparison.Ordinal),
                "khong con mau cua bang mau CU sot lai trong .xlsx");
        }
        finally { File.Delete(stylePath); }
    }
}
