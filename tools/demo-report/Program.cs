using System.Globalization;
using System.Text;
using Tsudev.Audit.Core.Abstractions;
using Tsudev.Audit.Core.Models;
using Tsudev.Audit.Core.Rendering;
using Tsudev.Audit.Core.Reports;
using Tsudev.Audit.Core.Serialization;
using Tsudev.Audit.Core.Testing;

// Bao cao mau: HAI may bia, mot may sach va mot may co dau hieu kich hoat trai
// phep, de trang tong hop co du ca hai mau ket luan. Thoi diem quet co dinh de
// moi lan sinh lai cho ra cung mot ket qua (anh chup khong doi vo co).
var outRoot = args.Length > 0 ? args[0] : "demo-output";
var scanDay = DateTimeOffset.Parse("2026-11-09T09:30:00+07:00", CultureInfo.InvariantCulture);
var level2 = FileNaming.Level2(outRoot, "PHONG-KE-TOAN", scanDay);
if (Directory.Exists(level2)) Directory.Delete(level2, recursive: true);

var renderer = new HtmlReportRenderer();
var options = new AuditOptions { RunDism = true, RunSfc = false };
var utf8 = new UTF8Encoding(false);

foreach (var machine in new[] { Demo.Clean(scanDay), Demo.Suspicious(scanDay.AddMinutes(14)) })
{
    // CO Y khong dung FileNaming.Level3: ten "tsudev-ket-qua-ra-quet-*" bi .gitignore
    // chan de bao cao may THAT khong bao gio lot vao repo. Mau gia lap dat ten khac
    // khuon; trang tong hop doc de quy nen khong phu thuoc ten thu muc.
    var dir = Path.Combine(level2, "mau-" + machine.ComputerName);
    Directory.CreateDirectory(dir);
    var lic = LicenseReportBuilder.Build(machine, options);
    var hw = HardwareReportBuilder.Build(Demo.WithTime(machine, machine.ScanTime.AddMinutes(1)), options);
    foreach (var report in new[] { lic, hw })
    {
        File.WriteAllText(Path.Combine(dir, report.HtmlReportFile),
            renderer.Render(report, dashboardRelativeLink: "../tsudev-tong-hop.html"), utf8);
        File.WriteAllText(Path.Combine(dir, Path.ChangeExtension(report.HtmlReportFile, ".json")),
            AuditJson.WriteReport(report), utf8);
    }
    Console.WriteLine($"{machine.ComputerName}: {lic.HtmlReportFile}, {hw.HtmlReportFile}");
}

Console.WriteLine(new DashboardBuilder(renderer).BuildAndSave(level2));

static class Demo
{
    static readonly string[] Ip = { "192.0.2.24" };
    static readonly string[] Gateway = { "192.0.2.1" };

    static Dictionary<string, object?> R(params (string k, object? v)[] kv)
        => kv.ToDictionary(p => p.k, p => p.v);

    public static SystemContext WithTime(SystemContext c, DateTimeOffset t) => new()
    {
        Wmi = c.Wmi, Registry = c.Registry, Process = c.Process, Files = c.Files,
        ComputerName = c.ComputerName, ScanTime = t, IsElevated = c.IsElevated
    };

    static FakeWmiQuery Hardware(string model, string cpu, int cores, int threads, long ram, string serial) => new FakeWmiQuery()
        .Add("Win32_OperatingSystem", R(("Caption", "Microsoft Windows 11 Pro"), ("Version", "10.0.26200"),
            ("BuildNumber", "26200"), ("OSArchitecture", "64-bit"), ("InstallDate", "20260312083000.000000+420")))
        .Add("Win32_ComputerSystem", R(("Manufacturer", "Dell Inc."), ("Model", model), ("TotalPhysicalMemory", ram)))
        .Add("Win32_BIOS", R(("SerialNumber", serial), ("SMBIOSBIOSVersion", "1.21.0"), ("Manufacturer", "Dell Inc.")))
        .Add("Win32_BaseBoard", R(("Manufacturer", "Dell Inc."), ("Product", "0DEMO1"), ("SerialNumber", "/" + serial + "/DEMO/")))
        .Add("Win32_SystemEnclosure", R(("ChassisTypes", new ushort[] { 3 }), ("SerialNumber", serial)))
        .Add("Win32_Processor", R(("Name", cpu), ("NumberOfCores", cores), ("NumberOfLogicalProcessors", threads), ("MaxClockSpeed", 2500)))
        .Add("Win32_PhysicalMemoryArray", R(("MemoryDevices", 2)))
        .Add("Win32_PhysicalMemory",
            R(("DeviceLocator", "DIMM1"), ("Capacity", ram / 2), ("Speed", 3200), ("Manufacturer", "Samsung"), ("SMBIOSMemoryType", 26)),
            R(("DeviceLocator", "DIMM2"), ("Capacity", ram / 2), ("Speed", 3200), ("Manufacturer", "Samsung"), ("SMBIOSMemoryType", 26)))
        .Add("Win32_DiskDrive", R(("Model", "NVMe DEMO SSD 512GB"), ("Size", 512110190592L), ("InterfaceType", "SCSI"),
            ("SerialNumber", "DEMO-0000-0001"), ("FirmwareRevision", "2B2QEXM7")))
        .Add("Win32_LogicalDisk",
            R(("DeviceID", "C:"), ("FileSystem", "NTFS"), ("Size", 254000000000L), ("FreeSpace", 96500000000L), ("VolumeName", "Windows")),
            R(("DeviceID", "D:"), ("FileSystem", "NTFS"), ("Size", 257000000000L), ("FreeSpace", 201000000000L), ("VolumeName", "Du lieu")))
        .Add("Win32_VideoController", R(("Name", "Intel(R) UHD Graphics 770"), ("AdapterRAM", 1073741824L),
            ("DriverVersion", "31.0.101.5186"), ("CurrentHorizontalResolution", 1920), ("CurrentVerticalResolution", 1080)))
        .Add("Win32_NetworkAdapterConfiguration", R(("Description", "Intel(R) Ethernet Connection (17) I219-LM"),
            ("MACAddress", "00:00:5E:00:53:01"), ("IPAddress", Ip), ("DefaultIPGateway", Gateway),
            ("DHCPEnabled", true), ("IPEnabled", true)));

    static FakeFileProbe BaseFiles() => new FakeFileProbe()
        .WithEnv("SystemRoot", @"C:\Windows").WithEnv("ProgramData", @"C:\ProgramData").WithEnv("SystemDrive", @"C:")
        .WithEnv("ProgramFiles", @"C:\Program Files").WithEnv("ProgramFiles(x86)", @"C:\Program Files (x86)")
        .AddDirectory(@"C:\ProgramData", @"C:\ProgramData\Microsoft")
        .AddFile(@"C:\Windows\System32\drivers\etc\hosts", "# Copyright (c) Microsoft Corp.\n127.0.0.1 localhost\n");

    static FakeRegistryReader Software(params (string id, string name, string ver, string pub)[] apps)
    {
        var reg = new FakeRegistryReader();
        const string U = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        reg.AddSubKeys(RegistryRoot.LocalMachine, U, apps.Select(a => a.id).ToArray());
        foreach (var a in apps)
        {
            reg.AddValue(RegistryRoot.LocalMachine, $@"{U}\{a.id}", "DisplayName", a.name);
            reg.AddValue(RegistryRoot.LocalMachine, $@"{U}\{a.id}", "DisplayVersion", a.ver);
            reg.AddValue(RegistryRoot.LocalMachine, $@"{U}\{a.id}", "Publisher", a.pub);
            reg.AddValue(RegistryRoot.LocalMachine, $@"{U}\{a.id}", "InstallDate", "20260315");
        }
        return reg;
    }

    static readonly (string, string, string, string)[] CommonApps =
    {
        ("{A1}", "Microsoft Edge", "142.0.3595.53", "Microsoft Corporation"),
        ("{A2}", "Mozilla Firefox (x64 vi)", "144.0", "Mozilla"),
        ("{A3}", "7-Zip 24.09 (x64)", "24.09", "Igor Pavlov"),
        ("{A4}", "Zalo", "25.10.1", "VNG Corp."),
        ("{A5}", "UniKey 4.6 RC2", "4.6", "Pham Kim Long"),
    };

    public static SystemContext Clean(DateTimeOffset t)
    {
        var wmi = Hardware("OptiPlex 7010", "13th Gen Intel(R) Core(TM) i5-13500", 14, 20, 17179869184L, "DEMO001")
            .Add("Win32_Service", R(("Name", "Spooler"), ("DisplayName", "Print Spooler")))
            .Add("SoftwareLicensingProduct", R(("Name", "Windows(R), Professional edition"),
                ("Description", "Windows(R) Operating System, OEM_DM channel"), ("LicenseStatus", 1),
                ("PartialProductKey", "3V66T"), ("ProductKeyChannel", "OEM:DM"),
                ("ApplicationID", "55c92734-d682-4d71-983e-d6ec3f16059f")));
        return new SystemContext
        {
            Wmi = wmi, Registry = Software(CommonApps), Files = BaseFiles(),
            Process = new FakeProcessRunner()
                .When("slmgr.vbs", new ProcessResult(0, "Name: Windows(R), Professional edition\nLicense Status: Licensed", ""))
                .When("DISM.exe", new ProcessResult(0, "No component store corruption detected.", "")),
            ComputerName = "DEMO-KETOAN-01", ScanTime = t, IsElevated = true
        };
    }

    public static SystemContext Suspicious(DateTimeOffset t)
    {
        var wmi = Hardware("Latitude 5440", "13th Gen Intel(R) Core(TM) i5-1335U", 10, 12, 8589934592L, "DEMO002")
            .Add("Win32_Service", R(("Name", "KMSAutoSvc"), ("DisplayName", "KMSAuto Service")),
                                  R(("Name", "Spooler"), ("DisplayName", "Print Spooler")))
            .Add("SoftwareLicensingProduct", R(("Name", "Windows(R), Professional edition"),
                ("Description", "Windows(R) Operating System, VOLUME_KMSCLIENT channel"), ("LicenseStatus", 1),
                ("PartialProductKey", "T83GX"), ("ProductKeyChannel", "Volume:GVLK"),
                ("ApplicationID", "55c92734-d682-4d71-983e-d6ec3f16059f")));
        var reg = Software(CommonApps.Append(("{B1}", "KMSAuto Net", "1.5.4", "")).ToArray());
        reg.AddValue(RegistryRoot.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SoftwareProtectionPlatform",
            "KeyManagementServiceName", "kms.example.invalid");
        var files = BaseFiles()
            .AddDirectory(@"C:\ProgramData", @"C:\ProgramData\KMSAuto", @"C:\ProgramData\Microsoft")
            .AddFile(@"C:\Windows\System32\SppExtComObjHook.dll")
            .AddFile(@"C:\Windows\System32\drivers\etc\hosts", "127.0.0.1 localhost\n127.0.0.1 kms.example.invalid\n");
        return new SystemContext
        {
            Wmi = wmi, Registry = reg, Files = files,
            Process = new FakeProcessRunner()
                .When("slmgr.vbs", new ProcessResult(0, "Name: Windows(R), Professional edition\nLicense Status: Licensed", ""))
                .When("DISM.exe", new ProcessResult(0, "The component store is repairable.", "")),
            ComputerName = "DEMO-KHO-02", ScanTime = t, IsElevated = true
        };
    }
}
