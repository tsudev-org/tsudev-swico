namespace Tsudev.Audit.Tests;

/// <summary>Muc 13 cua bo test cu: so hieu phien ban (CalVer YY.M.DDNN).</summary>
public class VersionNumberTests
{
    private static VersionNumber V(string s) => H.V(s);

    // --- Dang MOI (docs/DESIGN_SYSTEM.md muc 6) ---
    [Theory]
    [InlineData("26.8.1901", 26, 8, 19, 0, "doc dung 26.8.1901 = ngay 19 ban 01")]
    [InlineData("26.8.1902", 26, 8, 19, 2, "doc dung 26.8.1902 = ngay 19 ban 02")]
    [InlineData("v26.8.1901", 26, 8, 19, 0, "chap nhan tien to 'v' cua tag GitHub")]
    [InlineData("26.8.1901+9e1f2c", 26, 8, 19, 0, "cat bam commit do SDK them")]
    [InlineData("26.8.2001-rc1", 26, 8, 20, 0, "cat hau to -rc1")]
    [InlineData("tsudev-swico_26.8.1902", 26, 8, 19, 2, "doc duoc ca TEN PHAT HANH day du, khong chi tag")]
    [InlineData("TSUDEV-SWICO_26.8.1901", 26, 8, 19, 0, "tien to ten phat hanh khong phan biet hoa thuong")]
    [InlineData("tsudev-swico_26.8.1901_x64-setup.exe", 26, 8, 19, 0, "doc duoc so hieu tu chinh TEN FILE CAI DAT")]
    [InlineData("26.9.0901", 26, 9, 9, 0, "26.9.0901 = ngay 9 thang 9 ban 01")]
    // --- DOC DUOC DANG CU: hai ban 26.8.18 va 26.8.18.2 DA PHAT HANH ra ngoai ---
    [InlineData("26.8.18", 26, 8, 18, 0, "van doc duoc dang cu 26.8.18")]
    [InlineData("26.8.18.2", 26, 8, 18, 2, "van doc duoc dang cu 26.8.18.2")]
    [InlineData("tsudev-swico-v26.8.19.2", 26, 8, 19, 2, "van doc duoc TEN PHAT HANH dang cu")]
    public void Doc_dung_so_hieu(string text, int y, int m, int d, int ord, string vi)
        => Assert.True(V(text) == (ord == 0 ? new VersionNumber(y, m, d) : new VersionNumber(y, m, d, ord)), vi);

    /// <summary>
    /// Ngay mot chu so: so 0 dung dau la BAT BUOC trong ten file, nhung MSBuild
    /// chuan hoa &lt;VersionPrefix&gt;26.9.0901&lt;/VersionPrefix&gt; thanh
    /// AssemblyVersion 26.9.901. Ca hai cach viet PHAI ra cung mot gia tri, neu
    /// khong thi exe se tu bao no la mot phien ban khac voi ten file cua chinh no.
    /// </summary>
    [Fact]
    public void Ba_chu_so_va_bon_chu_so_doc_ra_cung_mot_so_hieu()
        => Assert.True(V("26.9.901") == V("26.9.0901"), "'901' va '0901' doc ra CUNG mot so hieu");

    [Fact]
    public void Ban_le_cua_ca_cuoc_chuyen_doi()
    {
        Assert.True(V("26.8.18") == V("26.8.1801"),
            "BAN LE CUA CA CUOC CHUYEN DOI: 26.8.18 (da phat hanh) va 26.8.1801 la MOT");
        Assert.True(V("26.8.18.2") == V("26.8.1802"), "26.8.18.2 (da phat hanh) va 26.8.1802 la MOT");
    }

    // --- Thu tu so sanh ---
    [Theory]
    [InlineData("26.8.2001", "26.8.1901", "cung thang: ngay lon hon la moi hon")]
    [InlineData("26.9.0301", "26.8.2001", "sang thang moi")]
    [InlineData("27.1.0101", "26.12.3101", "sang nam moi")]
    public void Thu_tu_so_sanh(string bigger, string smaller, string vi)
        => Assert.True(V(bigger) > V(smaller), vi);

    [Theory]
    [InlineData("linh tinh", "chuoi la -> khong hop le")]
    [InlineData("", "chuoi rong -> khong hop le")]
    [InlineData("26.8", "thieu thanh phan -> khong hop le")]
    [InlineData("26.13.0101", "thang ngoai pham vi -> khong hop le")]
    [InlineData("26.8.3201", "ngay ngoai pham vi (32) -> khong hop le")]
    [InlineData("26.8.0001", "ngay ngoai pham vi (00) -> khong hop le")]
    [InlineData("26.8.1900", "so thu tu 00 -> khong hop le (dem tu 01)")]
    public void Chuoi_khong_hop_le(string text, string vi) => Assert.True(!V(text).IsValid, vi);

    // --- Viet ra chuoi ---
    [Theory]
    [InlineData("26.8.1901", "26.8.1901", "viet ra dung dang YY.M.DDNN")]
    [InlineData("26.8.1902", "26.8.1902", "ban thu hai giu nguyen so thu tu")]
    [InlineData("26.8.18", "26.8.1801", "so hieu dang cu duoc viet ra theo dang MOI")]
    [InlineData("26.9.0901", "26.9.0901", "GIU so 0 dung dau: ngay 9 ban 1 -> '0901'")]
    public void Viet_ra_chuoi(string input, string expected, string vi)
        => Assert.True(V(input).ToString() == expected, vi);

    [Fact]
    public void Dem_du_hai_chu_so()
        => Assert.True(new VersionNumber(26, 9, 9).ToString() == "26.9.0901",
            "dem du hai chu so - '26.9.91' se doc nguoc lai thanh ngay 0 ban 91");
}
