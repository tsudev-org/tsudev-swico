using System.Text.RegularExpressions;
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
    /// thay o phien S005 (QU-4): mau nay lay tu .standards/tokens/design-tokens.json, va
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

    /// <summary>
    /// Mo ta muc duoc phep mang dung nam the trinh bay (b, code, br). Truoc
    /// 09/11/2026 chung bi escape nen bao cao in nguyen van "&lt;b&gt;hop le&lt;/b&gt;".
    /// </summary>
    [Fact]
    public void Mo_ta_hien_the_trinh_bay_khong_in_nguyen_van()
    {
        var html = Infected.Html;

        Assert.Contains("<b>hợp lệ</b>", html, StringComparison.Ordinal);
        Assert.DoesNotContain("&lt;b&gt;", html, StringComparison.Ordinal);
        Assert.DoesNotContain("&lt;code&gt;", html, StringComparison.Ordinal);
        Assert.DoesNotContain("&lt;br&gt;", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("<script>alert(1)</script>", "<script>alert")]
    [InlineData("<b onclick=\"alert(1)\">x</b>", "<b onclick")]
    [InlineData("<img src=x onerror=alert(1)>", "<img")]
    [InlineData("<B>hoa</B>", "<B>")]
    public void Mo_ta_chi_mo_lai_dung_the_trong_danh_sach(string evil, string mustNotAppear)
    {
        var report = new AuditReport { ComputerName = "PC", Title = "T" };
        report.Sections.Add(new ReportSection
        {
            Id = "s", NavLabel = "s", Heading = "s",
            Description = "truoc " + evil + " sau",
            MethodNote = evil
        });

        var html = new HtmlReportRenderer().Render(report);

        Assert.DoesNotContain(mustNotAppear, html, StringComparison.Ordinal);
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

    /// <summary>
    /// QU-STD-1 (09/11/2026): token doc tu .standards/tokens/, khong con ban sao
    /// tokens/ cuc bo. Ban sao cu giu text-muted #5F7891 - tuong phan 4,13:1 tren
    /// nen bg-base, truot WCAG AA; bo quy uoc v2.0.0 da doi sang #52627A.
    /// </summary>
    [Fact]
    public void Token_lay_tu_bo_quy_uoc_chung()
    {
        Assert.Equal("#52627A", DesignTokens.Color("light", "text-muted"), ignoreCase: true);
        Assert.DoesNotContain("#5F7891", Infected.Html, StringComparison.OrdinalIgnoreCase);

        // Wordmark o chan trang: tsu = text-primary, dev = text-link (BRAND_ASSETS.md).
        // Cam #D2690A cu chi dat 3,29:1 tren nen sang.
        Assert.Contains("--brand-dev-foot:" + DesignTokens.Color("light", "text-link"), Infected.Html, StringComparison.Ordinal);
        Assert.Contains("--brand-tsu-foot:" + DesignTokens.Color("light", "text-primary"), Infected.Html, StringComparison.Ordinal);
        Assert.DoesNotContain("#D2690A", Infected.Html, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Bang rong cuon ngang phai cuon duoc bang ban phim (WCAG 2.1.1).</summary>
    [Fact]
    public void Vung_cuon_cua_bang_nhan_focus()
    {
        var html = Infected.Html;
        var vung = Regex.Matches(html, "<div class=\"table-scroll\"[^>]*>");

        Assert.NotEmpty(vung);
        Assert.All(vung, m =>
        {
            Assert.Contains("tabindex=\"0\"", m.Value, StringComparison.Ordinal);
            Assert.Contains("role=\"region\"", m.Value, StringComparison.Ordinal);
            Assert.Contains("aria-label=\"", m.Value, StringComparison.Ordinal);
        });
    }
}
