namespace Tsudev.Audit.Tests;

/// <summary>
/// Du lieu dung chung cho nhieu lop test: mot MAY GIA LAP BI NHIEM, hai bao
/// cao dung tu no, va ban HTML render ra.
///
/// Vi sao dung tinh (static) thay vi IClassFixture: cung mot bo du lieu duoc
/// nhieu lop test dung toi, va dung lai ket qua render mot lan la khac biet
/// dang ke ve thoi gian chay.
///
/// LUAT: moi thu tra ve tu lop nay phai duoc coi la CHI DOC. Test nao lam thay
/// doi bao cao (vi du <c>XlsxWriter.Write</c> dat lai <c>Title</c> cho bang
/// khong ten) phai goi <see cref="FreshLicenseReport"/> de lay ban rieng.
/// </summary>
public static class Infected
{
    public static readonly AuditOptions Options = new() { RunDism = true, RunSfc = false };

    public static FakeWmiQuery Wmi() => new FakeWmiQuery()
        .Add("Win32_OperatingSystem", new Dictionary<string, object?> { ["Caption"] = "Microsoft Windows 11 Pro", ["Version"] = "10.0.26200",
                                              ["BuildNumber"] = "26200", ["OSArchitecture"] = "64-bit",
                                              ["InstallDate"] = "20260626120000.000000+420" })
        .Add("Win32_ComputerSystem", new Dictionary<string, object?> { ["Manufacturer"] = "Dell Inc.", ["Model"] = "Latitude E5570",
                                             ["TotalPhysicalMemory"] = 17179869184L })
        .Add("Win32_BIOS", new Dictionary<string, object?> { ["SerialNumber"] = "FVRFVD2", ["SMBIOSBIOSVersion"] = "1.34.3" })
        .Add("Win32_Processor", new Dictionary<string, object?> { ["Name"] = "Intel(R) Core(TM) i7-6600U", ["NumberOfCores"] = 2,
                                        ["NumberOfLogicalProcessors"] = 4, ["MaxClockSpeed"] = 2801 })
        .Add("Win32_Service", new Dictionary<string, object?> { ["Name"] = "KMSAutoSvc", ["DisplayName"] = "KMSAuto Service" },
                              new Dictionary<string, object?> { ["Name"] = "Spooler", ["DisplayName"] = "Print Spooler" })
        .Add("SoftwareLicensingProduct", new Dictionary<string, object?> { ["Name"] = "Windows(R), Professional edition",
                                                 ["Description"] = "Windows(R) Operating System, OEM_DM channel",
                                                 ["LicenseStatus"] = 1, ["PartialProductKey"] = "KHJ6C",
                                                 ["ProductKeyChannel"] = "OEM:DM" });

    public static FakeFileProbe Files() => new FakeFileProbe()
        .WithEnv("SystemRoot", @"C:\Windows")
        .WithEnv("ProgramData", @"C:\ProgramData")
        .WithEnv("SystemDrive", @"C:")
        .AddDirectory(@"C:\ProgramData", @"C:\ProgramData\KMSAuto", @"C:\ProgramData\Microsoft")
        .AddFile(@"C:\Windows\System32\SppExtComObjHook.dll")
        .AddFile(@"C:\Windows\System32\drivers\etc\hosts",
            "# comment\n127.0.0.1 kms.digiboy.ir\n127.0.0.1 localhost\n");

    public static FakeRegistryReader Registry() => new FakeRegistryReader()
        .AddSubKeys(RegistryRoot.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            "{111}", "{222}", "{333}")
        .AddValue(RegistryRoot.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{111}", "DisplayName", "Visual Studio Code")
        .AddValue(RegistryRoot.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{111}", "DisplayVersion", "1.90.0")
        .AddValue(RegistryRoot.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{111}", "Publisher", "Microsoft Corporation")
        .AddValue(RegistryRoot.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{111}", "InstallDate", "20240105")
        .AddValue(RegistryRoot.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{222}", "DisplayName", "Infatica Proxy Tool")
        .AddValue(RegistryRoot.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{333}", "DisplayName", "7-Zip 22.01")
        .AddValue(RegistryRoot.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{333}", "DisplayVersion", "22.01")
        .AddValue(RegistryRoot.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{333}", "Publisher", "Igor Pavlov")
        .AddValue(RegistryRoot.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SoftwareProtectionPlatform",
            "KeyManagementServiceName", "kms.fake-server.local");

    public static FakeProcessRunner Process() => new FakeProcessRunner()
        .When("slmgr.vbs", new ProcessResult(0, "Name: Windows(R), Professional edition\nLicense Status: Licensed", ""))
        .When("DISM.exe", new ProcessResult(0, "The component store is repairable.", ""));

    /// <summary>May gia lap bi nhiem, moc thoi gian co dinh de ket qua lap lai duoc.</summary>
    public static SystemContext NewCtx(string scanTime = "2026-08-17T10:00:00+07:00") => new()
    {
        Wmi = Wmi(), Registry = Registry(), Process = Process(), Files = Files(),
        ComputerName = "TSUITSTMY", ScanTime = DateTimeOffset.Parse(scanTime),
        IsElevated = true
    };

    private static readonly Lazy<SystemContext> _ctx = new(() => NewCtx());
    private static readonly Lazy<List<RiskFinding>> _findings = new(() => ActivationRiskScanner.Scan(Ctx));
    private static readonly Lazy<AuditReport> _lic = new(() => LicenseReportBuilder.Build(Ctx, Options));
    private static readonly Lazy<AuditReport> _hw =
        new(() => HardwareReportBuilder.Build(NewCtx("2026-08-17T10:05:00+07:00"), Options));
    private static readonly Lazy<string> _html =
        new(() => new HtmlReportRenderer().Render(Lic, "../tsudev-tong-hop.html"));

    public static SystemContext Ctx => _ctx.Value;
    public static List<RiskFinding> Findings => _findings.Value;
    public static AuditReport Lic => _lic.Value;
    public static AuditReport Hw => _hw.Value;
    public static string Html => _html.Value;

    /// <summary>Ban bao cao RIENG cho test nao se lam thay doi no (xem doan LUAT o tren).</summary>
    public static AuditReport FreshLicenseReport() => LicenseReportBuilder.Build(NewCtx(), Options);

    /// <summary>Danh sach bang da co ten - dang ma <c>XlsxWriter.Write</c> can.</summary>
    public static IEnumerable<DataTable> NamedTables(AuditReport report) =>
        report.Sections.SelectMany(s => s.Tables)
              .Select((t, i) => { if (string.IsNullOrWhiteSpace(t.Title)) t.Title = $"Bang {i + 1}"; return t; });
}

/// <summary>Cac ham tro giup dung o nhieu lop test.</summary>
public static class H
{
    /// <summary>Doc so hieu phien ban, tra ve gia tri khong hop le neu chuoi la.</summary>
    public static VersionNumber V(string s) { VersionNumber.TryParse(s, out var v); return v; }

    public static bool NameOk(string? s) => ReleaseName.Validate(s, out _, out _);
    public static string? WhyBad(string s) { ReleaseName.Validate(s, out _, out var p); return p; }

    public static ReleaseInfo Rel(string tag, string? installer = "https://x/tsudev-swico_x64-setup.exe",
                                  string? portable = "https://x/tsudev-swico_x64-portable.zip")
        => new(V(tag), tag, "https://x/releases/" + tag, installer, "https://x/SHA256SUMS.txt",
               DateTimeOffset.Now, portable);

    /// <summary>
    /// SystemContext toi thieu: cac collector deu chiu duoc du lieu rong (chung
    /// khong bao gio nem), nen bao cao van dung duoc va ta do dung thu ban muon
    /// do - THU TU CAC BUOC, khong phai noi dung bao cao.
    /// </summary>
    public static SystemContext ProgressCtx(IProgressSink sink, CancellationToken cancel = default) => new()
    {
        Wmi = new FakeWmiQuery(),
        Registry = new FakeRegistryReader(),
        Process = new FakeProcessRunner(),
        Files = new FakeFileProbe(),
        ComputerName = "MAY-TEST",
        ScanTime = DateTimeOffset.Now,
        IsElevated = true,
        Progress = sink,
        Cancellation = cancel
    };

    /// <summary>SystemContext rong, khong khai bao Progress/Cancellation.</summary>
    public static SystemContext EmptyCtx(string computerName = "MAY-TEST", DateTimeOffset? scanTime = null) => new()
    {
        Wmi = new FakeWmiQuery(), Registry = new FakeRegistryReader(),
        Process = new FakeProcessRunner(), Files = new FakeFileProbe(),
        ComputerName = computerName, ScanTime = scanTime ?? DateTimeOffset.Now, IsElevated = true
    };

    public static string TempFile(string prefix, string ext) =>
        Path.Combine(Path.GetTempPath(), $"{prefix}-{Guid.NewGuid():N}{ext}");
}

/// <summary>Adapter gia lap cho nguon ban phat hanh.</summary>
public sealed class FakeFeed : IUpdateFeed
{
    private readonly ReleaseInfo? _r; private readonly string? _err; private readonly Exception? _ex;
    public FakeFeed(ReleaseInfo? r = null, string? err = null, Exception? ex = null)
        { _r = r; _err = err; _ex = ex; }
    public ReleaseInfo? GetLatest(out string? failureReason)
    {
        if (_ex is not null) throw _ex;
        failureReason = _err; return _r;
    }
}

/// <summary>
/// Sink chuyen tiep moi su kien sang mot sink khac, va bam nut huy sau dung
/// <c>afterSteps</c> buoc hoan tat.
///
/// Dung de mo phong nguoi dung bam Ctrl+C GIUA lan quet - tinh huong ma test
/// "huy truoc khi chay" khong cham toi duoc, nhung lai la tinh huong that su
/// hay xay ra.
/// </summary>
public sealed class CancelAfterSink : IProgressSink
{
    private readonly IProgressSink _inner;
    private readonly CancellationTokenSource _cts;
    private readonly int _afterSteps;
    private int _done;

    public CancelAfterSink(IProgressSink inner, CancellationTokenSource cts, int afterSteps)
        { _inner = inner; _cts = cts; _afterSteps = afterSteps; }

    public void BeginStep(string label) => _inner.BeginStep(label);
    public void FailStep(string reason) => _inner.FailStep(reason);
    public void Note(string message) => _inner.Note(message);

    public void EndStep()
    {
        _inner.EndStep();
        if (++_done >= _afterSteps) _cts.Cancel();
    }
}
