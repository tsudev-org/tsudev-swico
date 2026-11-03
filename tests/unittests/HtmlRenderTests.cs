using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tsudev.Audit.Tests;

/// <summary>Muc 8 cua bo test cu: render HTML va JSON round-trip.</summary>
public class HtmlRenderTests
{
    [Fact]
    public void Trang_HTML_hoan_chinh()
    {
        var html = Infected.Html;

        Assert.True(html.Contains("<!DOCTYPE html>") && html.TrimEnd().EndsWith("</html>"), "HTML hoan chinh");
        Assert.True(html.Contains("https://tsudev.com"), "co link tsudev.com");
        Assert.True(html.Contains("KMSAuto"), "noi dung phat hien xuat hien trong HTML");
    }

    /// <summary>
    /// Hai cau nay TRUOC DAY khoa "theme #eaf2fc, khong dark-mode". Ca hai deu bi
    /// thay o phien S005 (QU-4): mau nay lay tu tokens/design-tokens.json, va
    /// DESIGN_SYSTEM.md muc 1 BAT BUOC chon che do mac dinh theo he dieu hanh.
    /// Chi tiet + ly do: docs/STATE.md muc QU-4.
    /// </summary>
    [Fact]
    public void Bang_mau_va_che_do_toi_theo_quy_uoc()
    {
        var html = Infected.Html;

        Assert.True(html.Contains(DesignTokens.Color("light", "bg-base"), StringComparison.OrdinalIgnoreCase),
            "nen trang lay tu token color.light.bg-base");
        Assert.True(html.Contains("prefers-color-scheme:dark", StringComparison.Ordinal),
            "co che do toi tu dong theo he dieu hanh");
    }

    [Fact]
    public void Du_lieu_doc_hai_duoc_escape()
    {
        var evilCtx = new SystemContext
        {
            Wmi = new FakeWmiQuery(), Registry = new FakeRegistryReader(),
            Process = new FakeProcessRunner(), Files = new FakeFileProbe(),
            ComputerName = "<script>alert(1)</script>", ScanTime = DateTimeOffset.Now, IsElevated = false
        };
        var evilHtml = new HtmlReportRenderer().Render(LicenseReportBuilder.Build(evilCtx, Infected.Options));

        Assert.True(!evilHtml.Contains("<script>alert(1)</script>"),
            "ten may doc hai DA duoc escape (chong HTML injection)");
    }

    [Fact]
    public void JSON_round_trip_giu_nguyen_bao_cao()
    {
        var lic = Infected.Lic;
        var jsonOpts = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        var json = JsonSerializer.Serialize(lic, jsonOpts);
        var restored = JsonSerializer.Deserialize<AuditReport>(
            json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        Assert.True(restored.ComputerName == lic.ComputerName, "JSON round-trip giu ten may");
        Assert.True(restored.VerdictLevel == lic.VerdictLevel, "JSON round-trip giu ket luan (enum)");
        Assert.True(restored.RiskScore?.Value == lic.RiskScore?.Value, "JSON round-trip giu diem rui ro");
        Assert.True(restored.SchemaVersion == AuditReport.CurrentSchemaVersion, "JSON co SchemaVersion");
    }
}
