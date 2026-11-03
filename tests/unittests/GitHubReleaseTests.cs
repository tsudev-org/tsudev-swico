namespace Tsudev.Audit.Tests;

/// <summary>Muc 15 cua bo test cu: doc du lieu ban phat hanh tu GitHub.</summary>
public class GitHubReleaseTests
{
    private const string ReleaseJson = """
{
  "tag_name": "v26.8.2001",
  "html_url": "https://github.com/tsudev-tsudev/swico/releases/tag/v26.8.2001",
  "published_at": "2026-08-20T10:00:00Z",
  "assets": [
    { "name": "tsudev-swico_26.8.2001_x64-setup.exe", "browser_download_url": "https://x/tsudev-swico_26.8.2001_x64-setup.exe" },
    { "name": "tsudev-swico_26.8.2001_x64-portable.zip", "browser_download_url": "https://x/portable.zip" },
    { "name": "SHA256SUMS.txt", "browser_download_url": "https://x/SHA256SUMS.txt" },
    { "name": "swico.exe", "browser_download_url": "https://x/swico.exe" }
  ]
}
""";

    /// <summary>
    /// NHAN CA HAI DANG TEN - bu tru bat buoc cua viec doi quy uoc dat ten.
    /// Mot ban phat hanh CU (hoac mot ban moi vi ly do nao do con kem tep ten cu)
    /// van phai duoc nhan ra. Bo ve nay thi cap nhat hong IM LANG: nguoi dung nhan
    /// duoc thong bao "phai tu cap nhat" thay vi duoc cap nhat.
    /// </summary>
    private const string OldNamesJson = """
{
  "tag_name": "v26.8.18.2",
  "assets": [
    { "name": "swico-setup-26.8.18.2.exe", "browser_download_url": "https://x/old-setup.exe" },
    { "name": "swico-portable-26.8.18.2.zip", "browser_download_url": "https://x/old-portable.zip" }
  ]
}
""";

    [Fact]
    public void Doc_duoc_JSON_ban_phat_hanh()
    {
        var parsed = GitHubReleaseParser.Parse(ReleaseJson, out var perr);

        Assert.True(parsed is not null && perr is null, "doc duoc JSON ban phat hanh");
        Assert.True(parsed!.Version == H.V("26.8.2001"), "lay dung phien ban tu tag_name");
        Assert.True(parsed.InstallerUrl == "https://x/tsudev-swico_26.8.2001_x64-setup.exe",
            "chon dung file cai dat, KHONG nham sang swico.exe hay ban portable");
        Assert.True(parsed.ChecksumsUrl == "https://x/SHA256SUMS.txt", "tim thay file ma bam");
        Assert.True(parsed.PublishedAt is not null, "doc duoc thoi diem phat hanh");
        Assert.True(parsed.PortableUrl == "https://x/portable.zip",
            "nhan ra ban portable .zip trong danh sach tep dinh kem");
    }

    [Fact]
    public void JSON_hong_tra_ve_null_kem_ly_do()
    {
        Assert.True(GitHubReleaseParser.Parse("{ khong phai json", out var badErr) is null && badErr is not null,
            "JSON hong -> tra ve null kem ly do, khong nem exception");
        Assert.True(GitHubReleaseParser.Parse("", out _) is null, "chuoi rong -> null");
        Assert.True(GitHubReleaseParser.Parse("[]", out _) is null, "JSON khong phai doi tuong -> null");
    }

    [Fact]
    public void Khong_co_tep_dinh_kem()
    {
        Assert.True(GitHubReleaseParser.Parse("""{"tag_name":"v26.8.20"}""", out _)!.PortableUrl is null,
            "khong co tep dinh kem -> khong co ban portable");
        Assert.True(GitHubReleaseParser.Parse("""{"tag_name":"v26.8.2001"}""", out _)!.InstallerUrl is null,
            "khong co danh sach tep dinh kem -> khong co file cai dat");
    }

    [Fact]
    public void Van_nhan_ra_ten_tep_dang_CU()
    {
        var oldNames = GitHubReleaseParser.Parse(OldNamesJson, out _);

        Assert.True(oldNames!.InstallerUrl == "https://x/old-setup.exe",
            "VAN nhan ra file cai dat ten DANG CU (swico-setup-*.exe)");
        Assert.True(oldNames.PortableUrl == "https://x/old-portable.zip",
            "VAN nhan ra ban portable ten DANG CU (swico-portable-*.zip)");
        Assert.True(oldNames.Version == H.V("26.8.1802"), "doc duoc tag dang cu, quy ve dang moi");
    }

    [Fact]
    public void Nhan_dien_tep_dinh_kem_theo_ten()
    {
        Assert.True(GitHubReleaseParser.IsInstallerAsset("tsudev-swico_26.8.2001_x64-setup.exe")
                 && GitHubReleaseParser.IsInstallerAsset("swico-setup-26.8.18.exe"),
            "IsInstallerAsset nhan ca hai dang ten");
        Assert.True(!GitHubReleaseParser.IsInstallerAsset("swico.exe")
                 && !GitHubReleaseParser.IsInstallerAsset("tsudev-swico_26.8.2001_x64-portable.zip"),
            "IsInstallerAsset KHONG nham sang swico.exe hay ban portable");
        Assert.True(GitHubReleaseParser.IsPortableAsset("tsudev-swico_26.8.2001_x64-portable.zip")
                 && GitHubReleaseParser.IsPortableAsset("swico-portable-26.8.18.zip"),
            "IsPortableAsset nhan ca hai dang ten");
    }
}
