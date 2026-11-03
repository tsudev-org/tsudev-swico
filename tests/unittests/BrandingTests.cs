namespace Tsudev.Audit.Tests;

/// <summary>Muc 9c cua bo test cu: thuong hieu trong bao cao HTML.</summary>
public class BrandingTests
{
    private static string Html => Infected.Html;

    [Fact]
    public void Logo_va_favicon_duoc_nhung_thang_vao_trang()
    {
        Assert.True(Html.Contains("data:image/png;base64,", StringComparison.Ordinal),
            "logo duoc NHUNG thang vao trang (khong phu thuoc file ngoai)");
        Assert.True(Html.Contains("<link rel=\"icon\"", StringComparison.Ordinal), "trang co favicon");
        Assert.True(Html.Contains("theme-color", StringComparison.Ordinal), "co mau chu de cho trinh duyet");
        Assert.True(!Html.Contains("src=\"assets/", StringComparison.Ordinal)
                 && !Html.Contains(".png\"", StringComparison.Ordinal),
            "khong tro toi file anh ben ngoai - bao cao copy di van hien duoc logo");
    }

    [Fact]
    public void Chu_ky_thuong_hieu_la_van_ban_that()
    {
        Assert.True(Html.Contains("bw-tsu", StringComparison.Ordinal) && Html.Contains("bw-dev", StringComparison.Ordinal),
            "chu ky thuong hieu tach 'tsu' va 'dev' thanh hai the rieng de to hai mau");
        Assert.True(Html.Contains(">tsu<", StringComparison.Ordinal) && Html.Contains(">dev<", StringComparison.Ordinal),
            "chu ky la VAN BAN that (sac net khi in, doc duoc bang trinh doc man hinh)");
    }

    [Fact]
    public void Logo_la_lien_ket_bam_duoc_toi_tsudev_com()
    {
        var brandIdx = Html.IndexOf("class=\"brand\"", StringComparison.Ordinal);

        Assert.True(brandIdx > 0 && Html.LastIndexOf("https://tsudev.com", brandIdx + 200, StringComparison.Ordinal) > 0,
            "logo nam trong lien ket toi tsudev.com");
        Assert.True(Html.Contains("role=\"img\"", StringComparison.Ordinal)
                 && Html.Contains("aria-label=\"tsudev\"", StringComparison.Ordinal),
            "logo co van ban thay the cho trinh doc man hinh");
        Assert.True(Html.Contains("rel=\"noopener noreferrer\"", StringComparison.Ordinal),
            "lien ket ra ngoai co rel=noopener noreferrer");
    }

    /// <summary>
    /// Logo hien o nhieu cho nhung du lieu base64 chi duoc nhung MOT lan (nam trong
    /// CSS). Nhung lap se cong them ~13 KB cho moi lan xuat hien. Dung 2: mot cho
    /// favicon, mot cho logo.
    /// </summary>
    [Fact]
    public void Anh_nhung_khong_bi_lap()
        => Assert.True(Html.Split("data:image/png;base64,").Length - 1 == 2,
            "chi co dung 2 anh nhung: favicon + logo (logo khong bi nhung lap)");

    [Fact]
    public void Thuong_hieu_hien_o_ca_dau_trang_va_chan_trang()
        => Assert.True(Html.Split("class=\"brand").Length - 1 >= 2,
            "thuong hieu hien o ca dau trang va chan trang");
}
