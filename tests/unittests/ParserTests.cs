namespace Tsudev.Audit.Tests;

/// <summary>Muc 1-2 cua bo test cu: doc dau ra cua ospp.vbs va ngay thang WMI.</summary>
public class ParserTests
{
    private const string OsppSample = """
Microsoft (R) Office Software Protection Platform Version: 16.0.14332
---LICENSE STATUS DUMP---
LICENSE NAME: Office 21, Office2021ProPlusVL_KMS_Client edition
LICENSE DESCRIPTION: Office 21, VOLUME_KMSCLIENT channel
LICENSE STATUS:  ---LICENSED---
SKU ID: 85df9b39-5099-46e5-9784-adf0117c9429
---------------------------------------
LICENSE NAME: Office 21, ProjectProVL_KMS_Client edition
LICENSE DESCRIPTION: Office 21, VOLUME_KMSCLIENT channel
LICENSE STATUS:  ---NOTIFICATIONS---
SKU ID: 5b6cb1a4-c1f6-4d97-9f7f-a9c4f5c9b1e0
---------------------------------------
""";

    [Fact]
    public void OsppOutput_tach_dung_tung_san_pham_va_trang_thai()
    {
        var office = OfficeLicenseCollector.ParseOsppOutput(OsppSample);

        Assert.True(office.Count == 2, $"tach dung 2 san pham Office (duoc {office.Count})");
        Assert.True(office[0].Status.Contains("Đã kích hoạt"), "san pham 1 = Licensed");
        Assert.True(office[1].Status.Contains("Chưa kích hoạt"), "san pham 2 = Notifications");
        Assert.True(office[0].SkuId.StartsWith("85df9b39"), "doc dung SKU ID");
    }

    [Fact]
    public void OsppOutput_rong_khong_crash()
        => Assert.True(OfficeLicenseCollector.ParseOsppOutput("").Count == 0,
            "input rong -> khong crash, tra ve rong");

    [Fact]
    public void WmiDate_hop_le_doc_duoc()
        => Assert.True(WmiDateParser.Parse("20260626120000.000000+420")?.ToString("yyyy-MM-dd") == "2026-06-26",
            "parse ngay hop le");

    [Theory]
    [InlineData("", "chuoi rong -> null")]
    [InlineData("rac", "chuoi rac -> null")]
    [InlineData("99999999999999", "so khong hop le -> null")]
    public void WmiDate_du_lieu_hong_tra_ve_null(string input, string vi)
        => Assert.True(WmiDateParser.Parse(input) is null, vi);

    [Fact]
    public void InstallDate_registry_duoc_chuan_hoa()
    {
        Assert.True(SoftwareCollector.FormatInstallDate("20240105") == "05/01/2024",
            "InstallDate registry -> DD/MM/YYYY");
        Assert.True(SoftwareCollector.FormatInstallDate("") == "-", "InstallDate rong -> '-'");
    }
}
