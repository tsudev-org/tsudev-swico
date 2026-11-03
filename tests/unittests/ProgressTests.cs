namespace Tsudev.Audit.Tests;

/// <summary>Muc 17 cua bo test cu: tien trinh quet - thu tu buoc, huy, ma thoat.</summary>
public class ProgressTests
{
    [Fact]
    public void Bao_cao_ban_quyen_bao_du_cac_buoc()
    {
        var recorder = new RecordingProgressSink();
        _ = LicenseReportBuilder.Build(H.ProgressCtx(recorder), new AuditOptions());

        Assert.True(recorder.StepLabels.Count >= 6,
            $"bao cao ban quyen bao it nhat 6 buoc (duoc {recorder.StepLabels.Count})");
        Assert.True(recorder.StepLabels[0] == "Hệ điều hành",
            $"buoc dau tien la 'He dieu hanh' (duoc '{recorder.StepLabels.FirstOrDefault()}')");
        Assert.True(recorder.StepLabels.Contains("Bản quyền Windows")
                 && recorder.StepLabels.Contains("Dấu hiệu kích hoạt trái phép"),
            "co buoc ban quyen Windows va buoc quet dau hieu kich hoat trai phep");
        Assert.True(recorder.Completed == recorder.StepLabels.Count,
            $"moi buoc da bat dau deu ket thuc ({recorder.Completed}/{recorder.StepLabels.Count})");
        Assert.True(!recorder.Events.Any(e => e.StartsWith("FAIL", StringComparison.Ordinal)),
            "khong buoc nao bao that bai khi du lieu rong");
    }

    /// <summary>
    /// Thu tu la mot RANG BUOC NGHIEP VU, khong phai ngau nhien: phan mem phai
    /// duoc thu thap TRUOC khi quet dau hieu, vi diem rui ro tinh tu ca hai.
    /// </summary>
    [Fact]
    public void Thu_thap_phan_mem_chay_truoc_khi_quet_dau_hieu()
    {
        var recorder = new RecordingProgressSink();
        _ = LicenseReportBuilder.Build(H.ProgressCtx(recorder), new AuditOptions());

        var labels = recorder.StepLabels.ToList();
        var iSoftware = labels.IndexOf("Danh sách phần mềm đã cài");
        var iRisk = labels.IndexOf("Dấu hiệu kích hoạt trái phép");

        Assert.True(iSoftware >= 0 && iRisk >= 0 && iSoftware < iRisk,
            "thu thap phan mem chay TRUOC khi quet dau hieu (diem rui ro can ca hai)");
    }

    [Fact]
    public void Bao_cao_phan_cung_bao_du_cac_buoc()
    {
        var hwRecorder = new RecordingProgressSink();
        _ = HardwareReportBuilder.Build(H.ProgressCtx(hwRecorder), new AuditOptions { RunDism = false, RunSfc = false });

        Assert.True(hwRecorder.StepLabels.Contains("CPU") && hwRecorder.StepLabels.Contains("RAM")
                 && hwRecorder.StepLabels.Contains("Windows Defender"),
            "bao cao phan cung bao cac buoc CPU / RAM / Defender");
        Assert.True(hwRecorder.Completed == hwRecorder.StepLabels.Count,
            "bao cao phan cung: moi buoc deu ket thuc");
    }

    /// <summary>Huy: phai dung LAI, khong duoc chay not cho het roi mai bao.</summary>
    [Fact]
    public void Huy_truoc_khi_chay_thi_khong_buoc_nao_bat_dau()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var cancelledSink = new RecordingProgressSink();

        var cancelThrew = false;
        try { _ = LicenseReportBuilder.Build(H.ProgressCtx(cancelledSink, cts.Token), new AuditOptions()); }
        catch (OperationCanceledException) { cancelThrew = true; }

        Assert.True(cancelThrew, "token da huy -> Build nem OperationCanceledException");
        Assert.True(cancelledSink.StepLabels.Count == 0,
            $"huy truoc khi chay -> khong buoc nao duoc bat dau (duoc {cancelledSink.StepLabels.Count})");
    }

    /// <summary>Huy GIUA CHUNG: cac buoc da xong phai duoc giu, phan con lai dung han.</summary>
    [Fact]
    public void Huy_giua_chung_giu_lai_cac_buoc_da_xong()
    {
        using var midCts = new CancellationTokenSource();
        var midSink = new RecordingProgressSink();
        var midCtx = new SystemContext
        {
            Wmi = new FakeWmiQuery(),
            Registry = new FakeRegistryReader(),
            Process = new FakeProcessRunner(),
            Files = new FakeFileProbe(),
            ComputerName = "MAY-TEST",
            ScanTime = DateTimeOffset.Now,
            IsElevated = true,
            Progress = new CancelAfterSink(midSink, midCts, afterSteps: 2),
            Cancellation = midCts.Token
        };

        var midThrew = false;
        try { _ = LicenseReportBuilder.Build(midCtx, new AuditOptions()); }
        catch (OperationCanceledException) { midThrew = true; }

        Assert.True(midThrew, "huy giua chung -> Build dung lai bang OperationCanceledException");
        Assert.True(midSink.Completed == 2,
            $"huy sau 2 buoc -> dung 2 buoc hoan tat duoc giu lai (duoc {midSink.Completed})");
        Assert.True(midSink.StepLabels.Count == 2,
            $"khong buoc nao khac duoc bat dau sau khi huy (duoc {midSink.StepLabels.Count})");
    }

    /// <summary>ScanStep phai bao FAIL khi cong viec nem loi, va van de loi noi len tren.</summary>
    [Fact]
    public void ScanStep_bao_FAIL_va_khong_nuot_ngoai_le()
    {
        var failSink = new RecordingProgressSink();
        var failThrew = false;
        try
        {
            ScanStep.Run(H.ProgressCtx(failSink), "Buoc hong",
                () => throw new InvalidOperationException("hong that"));
        }
        catch (InvalidOperationException) { failThrew = true; }

        Assert.True(failThrew, "ScanStep KHONG nuot ngoai le cua cong viec");
        Assert.True(failSink.Events.Any(e => e == "FAIL hong that"),
            "ScanStep bao FAIL kem ly do khi cong viec nem loi");
    }

    /// <summary>Note: dung cho viec chay lau, phai den duoc sink.</summary>
    [Fact]
    public void Ghi_chu_thoi_gian_du_kien_den_duoc_sink()
    {
        var noteSink = new RecordingProgressSink();
        _ = SystemIntegrityCollector.Collect(H.ProgressCtx(noteSink), runDism: true, runSfc: true);

        Assert.True(noteSink.Events.Count(e => e.StartsWith("NOTE", StringComparison.Ordinal)) == 2,
            "DISM va sfc moi cai bao mot ghi chu thoi gian du kien");
        Assert.True(noteSink.Events.Any(e => e.Contains("5-15 phút", StringComparison.Ordinal)),
            "ghi chu cua sfc noi ro no mat 5-15 phut");
    }

    /// <summary>Sink mac dinh khong duoc lam gi ca - day la thu bao ve cac test cu.</summary>
    [Fact]
    public void SystemContext_khong_khai_bao_thi_dung_mac_dinh_im_lang()
    {
        var quiet = new SystemContext
        {
            Wmi = new FakeWmiQuery(), Registry = new FakeRegistryReader(),
            Process = new FakeProcessRunner(), Files = new FakeFileProbe(),
            ComputerName = "MAY-TEST", ScanTime = DateTimeOffset.Now
        };

        Assert.True(ReferenceEquals(quiet.Progress, NullProgressSink.Instance),
            "SystemContext khong khai bao Progress -> dung NullProgressSink");
        Assert.True(!quiet.Cancellation.CanBeCanceled,
            "SystemContext khong khai bao Cancellation -> khong bao gio huy");
    }

    [Fact]
    public void Ma_thoat_130_khi_nguoi_dung_huy()
    {
        Assert.True(ExitCodes.Cancelled == 130, "ma thoat huy = 130 (quy uoc POSIX 128+SIGINT)");
        Assert.True(ExitCodes.Describe(130).Contains("huỷ", StringComparison.Ordinal),
            "Describe(130) noi ro la nguoi dung huy");
        Assert.True(ExitCodes.Combine(ExitCodes.Cancelled, ExitCodes.VerdictCritical) == ExitCodes.Cancelled,
            "huy giua chung THANG ket luan danh gia - ket luan do dua tren du lieu chua day du");
        Assert.True(ExitCodes.Combine(ExitCodes.Partial, ExitCodes.VerdictCritical) == ExitCodes.VerdictCritical,
            "quet xong nhung thieu du lieu thi ket luan van thang (hanh vi cu giu nguyen)");
        Assert.True(CliOptions.UsageText.Contains("130", StringComparison.Ordinal),
            "phan tro giup co liet ke ma thoat 130");
    }
}
