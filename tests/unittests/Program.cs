using Tsudev.Audit.Core.Updates;

namespace Tsudev.Audit.Tests;

// ---------------------------------------------------------------------------
// CONG KIEM TRA QUY UOC DAT TEN - duoc release.yml goi TRUOC khi build:
//
//     dotnet run --project tests/unittests -c Release -- --validate-release-name v26.8.1901
//
// Vi sao goi vao day thay vi chep lai luat sang PowerShell: hai ban cua cung
// mot luat se lech nhau theo thoi gian, va ban lech se dung la ban khong ai
// test. O day dung CHINH ReleaseName.Validate cua ma san xuat - luat chi ton
// tai o MOT cho, va cho do co 25 ca kiem tra (ReleaseNameTests.cs).
//
// Vi sao entry point nay nam trong CHINH bo test thay vi mot project rieng:
// no phai dung cung mot ban ReleaseName voi cac test khoa luat do. Tach ra
// project khac la them mot cho de hai ban lech nhau.
//
// `dotnet test` khong goi Main - VSTest nap assembly va tu tim [Fact]. Hai
// duong chay nay khong dam nhau.
// ---------------------------------------------------------------------------
public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 2 && args[0] == "--validate-release-name")
        {
            if (ReleaseName.Validate(args[1], out var checkedVersion, out var problem))
            {
                Console.WriteLine($"OK: {ReleaseName.For(checkedVersion)} " +
                                  $"(ban thu {ReleaseName.OrdinalOfDay(checkedVersion)} trong ngay " +
                                  $"{checkedVersion.Day}/{checkedVersion.Month}/20{checkedVersion.Year:00})");
                return 0;
            }
            Console.Error.WriteLine($"SAI QUY UOC DAT TEN: {problem}");
            Console.Error.WriteLine("Quy uoc day du: docs/VERSIONING.md");
            return 1;
        }

        Console.Error.WriteLine("Day la bo test xUnit. Chay bang:  dotnet test tests/unittests");
        Console.Error.WriteLine("Hoac dung cong kiem tra ten:      -- --validate-release-name <so hieu>");
        return 2;
    }
}
