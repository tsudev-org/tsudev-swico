namespace Tsudev.Audit.Tests;

/// <summary>
/// Muc 13b cua bo test cu: quy uoc dat ten phien ban (docs/VERSIONING.md).
///
/// Day la bo test dung sau cong `--validate-release-name` ma release.yml goi
/// truoc khi build (xem Program.cs). Luat chi ton tai o MOT cho.
/// </summary>
public class ReleaseNameTests
{
    private static VersionNumber V(string s) => H.V(s);
    private static bool Ok(string? s) => H.NameOk(s);
    private static string? Why(string s) => H.WhyBad(s);

    // --- Dung quy uoc ---
    [Theory]
    [InlineData("26.8.1901", "dang so hieu tran")]
    [InlineData("v26.8.1901", "dang tag git")]
    [InlineData("tsudev-swico_26.8.1901", "dang ten phat hanh day du")]
    [InlineData("26.8.1902", "ban thu hai trong ngay")]
    [InlineData("tsudev-swico_26.8.1903", "ban thu ba trong ngay")]
    [InlineData("26.9.0101", "thang mot chu so")]
    [InlineData("26.12.3101", "ngay cuoi thang")]
    [InlineData("27.1.0101", "ranh gioi cuoi nam")]
    [InlineData("26.9.0901", "NHAN so 0 dung dau o thanh phan thu ba - o do no BAT BUOC")]
    public void Nhan_ten_dung_quy_uoc(string name, string vi) => Assert.True(Ok(name), vi);

    // --- Sai quy uoc: dang CU ---
    // Day la loi hay gap nhat trong giai doan chuyen doi: nguoi go dang chep tu
    // mot ban phat hanh cu. Bao loi phai chi ra cach viet moi TUONG DUONG.
    [Fact]
    public void Tu_choi_dang_cu_va_chi_ra_cach_viet_moi()
    {
        Assert.True(!Ok("26.8.19") && !Ok("26.8.19.2"), "TU CHOI dang CU (YY.M.D va YY.M.D.N)");
        Assert.True(Why("26.8.19.2")!.Contains("26.8.1902", StringComparison.Ordinal),
            "loi cua dang cu chi thang ra cach viet moi tuong duong");
    }

    /// <summary>
    /// '901' va '0901' cung doc ra ngay 9 ban 1, nhung o khau phat hanh thi mot so
    /// hieu phai co DUNG MOT cach viet - neu khong se thanh hai ten file cai dat.
    /// </summary>
    [Fact]
    public void Tu_choi_thanh_phan_thu_ba_khong_du_bon_chu_so()
    {
        Assert.True(!Ok("26.9.901"), "TU CHOI thanh phan thu ba chi co 3 chu so");
        Assert.True(Why("26.9.901")!.Contains("4 chữ số", StringComparison.Ordinal),
            "loi noi ro phai du 4 chu so");
        Assert.True(!Ok("26.8.1900"), "TU CHOI so thu tu '00' - ban thu nhat la '01'");
    }

    /// <summary>
    /// 26.08.1901 va 26.8.1901 la CUNG mot so hieu nhung KHAC chuoi -> hai ten file
    /// cai dat cho cung mot ban, trong khi cong cu tim file cai dat THEO TEN.
    /// </summary>
    [Fact]
    public void Tu_choi_so_0_dung_dau_o_nam_va_thang()
    {
        Assert.True(!Ok("26.08.1901"), "TU CHOI so 0 dung dau o thang");
        Assert.True(Why("26.08.1901")!.Contains("0 đứng đầu", StringComparison.Ordinal),
            "loi so 0 dung dau noi dung ban chat");
    }

    /// <summary>
    /// Luu y su khac biet co chu dich voi VersionNumber.TryParse: no CAT hau to vi
    /// no doc du lieu tu mang; con Validate gac cong khau phat hanh nen KHAT KHE.
    /// </summary>
    [Fact]
    public void TryParse_de_dai_hon_Validate()
        => Assert.True(V("26.8.1901-rc1").IsValid && !Ok("26.8.1901-rc1"),
            "TryParse de dai voi '-rc1', Validate thi khong");

    [Fact]
    public void Tu_choi_cac_dang_con_lai()
    {
        Assert.True(!Ok("26.8") && !Ok("26.8.1901.2.1"), "TU CHOI thieu hoac thua thanh phan");
        Assert.True(!Ok("") && !Ok(null) && !Ok("   "), "TU CHOI chuoi rong");
        Assert.True(!Ok("26.13.0101") && !Ok("26.8.3201"), "TU CHOI thang/ngay ngoai pham vi");
        Assert.True(!Ok("2026.8.1901"), "TU CHOI nam bon chu so - CalVer dung nam hai chu so");
    }

    // --- Dung ten ---
    [Fact]
    public void Dung_ten_phat_hanh_va_tag()
    {
        Assert.True(ReleaseName.For(new VersionNumber(26, 8, 19)) == "tsudev-swico_26.8.1901",
            "dung ten phat hanh cua ban thu nhat");
        Assert.True(ReleaseName.For(new VersionNumber(26, 8, 19, 2)) == "tsudev-swico_26.8.1902",
            "dung ten phat hanh cua ban thu hai");
        Assert.True(ReleaseName.TagFor(new VersionNumber(26, 8, 19, 2)) == "v26.8.1902",
            "tag git dung dang ngan de release.yml bat duoc mau 'v*'");
    }

    [Fact]
    public void Dung_ten_file_cai_dat_va_ban_portable()
    {
        Assert.True(ReleaseName.InstallerFileName(new VersionNumber(26, 8, 19)) == "tsudev-swico_26.8.1901_x64-setup.exe",
            "ten file cai dat dung dinh dang cua DESIGN_SYSTEM muc 6");
        Assert.True(ReleaseName.PortableFileName(new VersionNumber(26, 8, 19, 2)) == "tsudev-swico_26.8.1902_x64-portable.zip",
            "ten ban portable dung dinh dang");
        Assert.True(ReleaseName.InstallerFileName(new VersionNumber(26, 9, 9)) == "tsudev-swico_26.9.0901_x64-setup.exe",
            "ngay mot chu so van ra ten file du hai chu so");
    }

    // --- Dem thu tu trong ngay ---
    [Fact]
    public void Dem_thu_tu_trong_ngay()
    {
        Assert.True(ReleaseName.OrdinalOfDay(new VersionNumber(26, 8, 19)) == 1,
            "ban khong mang so dem la ban thu NHAT trong ngay");
        Assert.True(ReleaseName.OrdinalOfDay(new VersionNumber(26, 8, 19, 2)) == 2,
            "so dem CHINH LA thu tu cua ban trong ngay");
        Assert.True(ReleaseName.NextSameDay(new VersionNumber(26, 8, 19)) == new VersionNumber(26, 8, 19, 2),
            "phat hanh lai lan dau trong ngay -> ban 02");
        Assert.True(ReleaseName.NextSameDay(new VersionNumber(26, 8, 19, 2)) == new VersionNumber(26, 8, 19, 3),
            "phat hanh lai tiep -> ban 03");
        Assert.True(ReleaseName.ForDate(new DateOnly(2026, 8, 19)) == new VersionNumber(26, 8, 19),
            "sinh so hieu tu ngay thang");
        Assert.True(ReleaseName.ForDate(new DateOnly(2027, 1, 1)) == new VersionNumber(27, 1, 1),
            "sinh so hieu qua nam moi");
    }

    /// <summary>
    /// CA QUAN TRONG NHAT CUA CA MUC NAY.
    ///
    /// Quet CA THANG: moi ngay, moi so thu tu deu phai (a) viet ra roi doc lai duoc
    /// nguyen ven, (b) qua duoc cong Validate, va (c) giu dung thu tu tang dan.
    ///
    /// Quet toan bo thay vi kiem vai ca le chinh la cach bat duoc loi cua ngay 1-9,
    /// noi ma so 0 dung dau quyet dinh dung/sai. Neu mot phien sau doi lai cach danh
    /// so, ca kiem tra nay se do.
    /// </summary>
    [Fact]
    public void Quet_ca_thang()
    {
        var everyRelease = new List<VersionNumber>();
        for (var day = 1; day <= 31; day++)
            for (var ord = 1; ord <= 4; ord++)
                everyRelease.Add(new VersionNumber(26, 9, day, ord == 1 ? 0 : ord));

        Assert.True(everyRelease.All(v => V(v.ToString()) == v),
            "CA THANG: viet ra roi doc lai duoc nguyen ven (124 so hieu)");
        Assert.True(everyRelease.All(v => Ok(v.ToString())),
            "CA THANG: moi so hieu sinh ra deu qua duoc cong Validate");
        Assert.True(everyRelease.Zip(everyRelease.Skip(1)).All(pair => pair.First < pair.Second),
            "CA THANG: thu tu tang dan lien tuc, khong dut o ranh gioi ngay nao");
    }

    [Fact]
    public void Ranh_gioi_ngay_9_sang_ngay_10()
        => Assert.True(V("26.9.0901") < V("26.9.1001") && V("26.9.0904") < V("26.9.1001"),
            "ranh gioi ngay 9 -> 10: cho de sai nhat khi dem so 0");

    [Fact]
    public void Thu_tu_trong_chuoi_ban_phat_hanh()
        => Assert.True(V("26.8.1901") < V("26.8.1902") && V("26.8.1902") < V("26.8.1903")
                    && V("26.8.1903") < V("26.8.2001"),
            "THU TU TRONG CHUOI: 26.8.1901 < 26.8.1902 < 26.8.1903 < 26.8.2001");

    [Fact]
    public void Duong_cap_nhat_khong_dut_qua_ranh_gioi_ngay()
    {
        Assert.True(new UpdateChecker(new FakeFeed(H.Rel("v26.8.2001"))).Check("26.8.1903").MustUpdate,
            "may chay ban thu 3 ngay 19/8 VAN nhan duoc ban ngay 20/8");
        Assert.True(!new UpdateChecker(new FakeFeed(H.Rel("v26.8.1902"))).Check("26.8.2001").MustUpdate,
            "may chay ban ngay 20/8 KHONG bi moi ha cap ve ban 19/8");
    }

    /// <summary>Ban DA PHAT HANH theo dang cu van phai nhan duoc ban moi theo dang moi.</summary>
    [Fact]
    public void Ban_dang_cu_van_nhan_duoc_ban_moi()
    {
        Assert.True(new UpdateChecker(new FakeFeed(H.Rel("v26.8.2001"))).Check("26.8.18.2").MustUpdate,
            "may dang chay 26.8.18.2 (dang cu) VAN nhan duoc ban 26.8.2001 (dang moi)");
        Assert.True(new UpdateChecker(new FakeFeed(H.Rel("v26.8.1801"))).Check("26.8.18").Status == UpdateStatus.UpToDate,
            "26.8.1801 KHONG bi moi cap nhat khi dang chay 26.8.18 - cung mot ban");
    }
}
