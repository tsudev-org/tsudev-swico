namespace Tsudev.Audit.Tests;

/// <summary>Muc 14 cua bo test cu: cong kiem tra cap nhat.</summary>
public class UpdateCheckerTests
{
    private static ReleaseInfo Rel(string tag, string? installer = "https://x/tsudev-swico_x64-setup.exe",
                                   string? portable = "https://x/tsudev-swico_x64-portable.zip")
        => H.Rel(tag, installer, portable);

    [Fact]
    public void Co_ban_moi_hon_thi_BUOC_cap_nhat()
    {
        var newer = new UpdateChecker(new FakeFeed(Rel("v26.8.20"))).Check("26.8.18");

        Assert.True(newer.Status == UpdateStatus.UpdateRequired && newer.MustUpdate, "co ban moi hon -> BUOC cap nhat");
        Assert.True(newer.Latest!.Version == H.V("26.8.20"), "giu thong tin ban moi de hien trong hop thoai");
    }

    [Fact]
    public void Cung_phien_ban_hoac_ban_tren_GitHub_cu_hon()
    {
        Assert.True(new UpdateChecker(new FakeFeed(Rel("v26.8.18"))).Check("26.8.18").Status == UpdateStatus.UpToDate,
            "cung phien ban -> da moi nhat");
        Assert.True(new UpdateChecker(new FakeFeed(Rel("v26.8.10"))).Check("26.8.18").Status == UpdateStatus.UpToDate,
            "ban tren GitHub CU hon (vi du bi rollback) -> khong bat ha cap");
    }

    /// <summary>
    /// QUYET DINH THIET KE QUAN TRONG NHAT: kiem tra that bai thi KHONG chan.
    /// Cong cu duoc dung o may cach ly mang, may bi tuong lua chan - chan lai se
    /// lam no vo dung chinh o noi can nhat.
    /// </summary>
    [Fact]
    public void Kiem_tra_that_bai_thi_KHONG_chan_quet()
    {
        var noNetwork = new UpdateChecker(new FakeFeed(err: "Không kết nối được")).Check("26.8.18");
        Assert.True(noNetwork.Status == UpdateStatus.CheckFailed && !noNetwork.MustUpdate, "mat mang -> KHONG chan quet");

        var threw = new UpdateChecker(new FakeFeed(ex: new InvalidOperationException("sap"))).Check("26.8.18");
        Assert.True(threw.Status == UpdateStatus.CheckFailed && !threw.MustUpdate,
            "nguon nem exception -> KHONG chan quet, khong lam sap cong cu");

        Assert.True(!new UpdateChecker(new FakeFeed(Rel("khong-phai-phien-ban"))).Check("26.8.18").MustUpdate,
            "tag GitHub khong doc duoc -> KHONG chan quet");
        Assert.True(!new UpdateChecker(new FakeFeed(Rel("v26.8.20"))).Check("khong-doc-duoc").MustUpdate,
            "khong doc duoc phien ban CUA CHINH MINH -> KHONG chan quet");
    }

    [Fact]
    public void Ban_moi_khong_kem_file_cai_dat_thi_huong_dan_thu_cong()
    {
        var noInstaller = new UpdateChecker(new FakeFeed(Rel("v26.8.20", installer: null))).Check("26.8.18");

        Assert.True(noInstaller.Status == UpdateStatus.CheckFailed && !noInstaller.MustUpdate
                 && noInstaller.Message!.Contains("thủ công", StringComparison.Ordinal),
            "ban moi khong kem file cai dat -> huong dan cap nhat thu cong, khong chan");
    }

    /// <summary>
    /// Chay file setup cho ban portable la mot CAI BAY: no cai mot ban thu hai vao
    /// Program Files, con file .exe dang chay (thuong tren USB) VAN CU. Lan sau
    /// chay lai chinh file do thi lai bi chan - mot vong lap khong loi thoat ma
    /// nguoi dung khong hieu vi sao.
    /// </summary>
    [Fact]
    public void Ban_portable_khong_tu_cai_duoc()
    {
        var portable = new UpdateChecker(new FakeFeed(Rel("v26.8.20"))).Check("26.8.19", InstallKind.Portable);

        Assert.True(portable.MustUpdate, "ban portable co ban moi -> VAN chan quet (bo luat cu van la bo luat cu)");
        Assert.True(!portable.CanSelfInstall && portable.MustUpdateManually,
            "ban portable KHONG tu cai duoc -> phai cap nhat thu cong");
        Assert.True(portable.Message!.Contains("tsudev-swico_x64-portable.zip", StringComparison.Ordinal),
            "chi ro cho tai ban portable moi, khong phai file setup");
    }

    [Fact]
    public void Ban_da_cai_thi_tu_cai_duoc()
    {
        var installed = new UpdateChecker(new FakeFeed(Rel("v26.8.20"))).Check("26.8.19", InstallKind.Installed);

        Assert.True(installed.MustUpdate && installed.CanSelfInstall && !installed.MustUpdateManually,
            "ban da cai thi tu cai duoc -> van hien hop thoai mot nut nhu cu");
        Assert.True(new UpdateChecker(new FakeFeed(Rel("v26.8.20"))).Check("26.8.19").CanSelfInstall,
            "mac dinh khi khong noi ro la ban DA CAI - giu nguyen hanh vi cu");
    }

    [Fact]
    public void Khong_co_zip_portable_thi_chi_ve_trang_phat_hanh()
    {
        var noZip = new UpdateChecker(new FakeFeed(Rel("v26.8.20", portable: null)))
            .Check("26.8.19", InstallKind.Portable);

        Assert.True(noZip.MustUpdate && noZip.MustUpdateManually, "khong co ban portable moi -> van chan");
        Assert.True(noZip.Message!.Contains("releases/v26.8.20", StringComparison.Ordinal),
            "khong co .zip thi chi ve trang phat hanh");
    }

    [Fact]
    public void Ban_portable_dang_o_ban_moi_nhat_thi_khong_chan()
        => Assert.True(new UpdateChecker(new FakeFeed(Rel("v26.8.19"))).Check("26.8.19", InstallKind.Portable).Status
                == UpdateStatus.UpToDate,
            "ban portable dang o ban moi nhat -> khong chan");

    [Fact]
    public void Can_cap_nhat_thang_moi_ket_luan_danh_gia()
        => Assert.True(ExitCodes.Combine(ExitCodes.UpdateRequired, ExitCodes.VerdictCritical) == ExitCodes.UpdateRequired,
            "can cap nhat thang moi ket luan danh gia (chua quet thi chua co ket luan)");
}
