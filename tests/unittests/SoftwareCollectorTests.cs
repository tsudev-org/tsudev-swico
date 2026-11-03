namespace Tsudev.Audit.Tests;

/// <summary>Muc 5 cua bo test cu: thu thap phan mem tu registry.</summary>
public class SoftwareCollectorTests
{
    private static readonly IReadOnlyList<InstalledSoftware> Software = SoftwareCollector.Collect(Infected.Ctx);

    [Fact]
    public void Doc_du_so_phan_mem()
        => Assert.True(Software.Count == 3, $"doc duoc 3 phan mem (duoc {Software.Count})");

    [Fact]
    public void Phan_loai_dung()
        => Assert.True(Software.Any(s => s.Name == "Visual Studio Code" && s.Category == "Công cụ lập trình"),
            "phan loai VS Code dung");

    [Fact]
    public void Danh_dau_can_kiem_tra_thu_cong_dung_cho()
    {
        Assert.True(Software.Any(s => s.Name == "Infatica Proxy Tool" && s.NeedsManualReview),
            "Infatica bi danh dau can kiem tra thu cong");
        Assert.True(!Software.First(s => s.Name == "7-Zip 22.01").NeedsManualReview,
            "7-Zip du thong tin -> khong can kiem tra");
    }

    [Fact]
    public void Ngay_cai_dat_duoc_chuan_hoa()
        => Assert.True(Software.First(s => s.Name == "Visual Studio Code").InstallDate == "05/01/2024",
            "ngay cai dat duoc chuan hoa ve DD/MM/YYYY");
}
