namespace Tsudev.Audit.Tests;

/// <summary>Muc 6 cua bo test cu: dien giai ket qua DISM/SFC.</summary>
public class SystemIntegrityTests
{
    [Theory]
    [InlineData(0, "No component store corruption detected.", "Không phát hiện lỗi", "DISM sach")]
    [InlineData(0, "The component store is repairable.", "CÓ THỂ SỬA ĐƯỢC", "DISM co the sua")]
    [InlineData(-1, "", "Không chạy được", "DISM khong chay duoc -> thong bao ro rang")]
    public void Dien_giai_DISM(int exitCode, string stdout, string expected, string vi)
        => Assert.True(SystemIntegrityCollector.InterpretDism(new ProcessResult(exitCode, stdout, "")).Contains(expected), vi);

    [Fact]
    public void Dien_giai_SFC_sach()
        => Assert.True(SystemIntegrityCollector
                .InterpretSfc(new ProcessResult(0, "Windows Resource Protection did not find any integrity violations.", ""))
                .Contains("Không phát hiện"),
            "SFC sach");
}
