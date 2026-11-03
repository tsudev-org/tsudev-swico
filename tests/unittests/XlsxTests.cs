using System.IO.Compression;

namespace Tsudev.Audit.Tests;

/// <summary>Muc 9 + 9b cua bo test cu: XlsxWriter va toan ven goi OPC.</summary>
public class XlsxTests
{
    [Fact]
    public void Tao_duoc_file_xlsx_co_noi_dung()
    {
        var tmp = H.TempFile("tsudev-test", ".xlsx");
        try
        {
            new XlsxWriter().Write(tmp, Infected.NamedTables(Infected.FreshLicenseReport()));
            Assert.True(File.Exists(tmp) && new FileInfo(tmp).Length > 1000, "tao duoc file .xlsx co noi dung");
        }
        finally { File.Delete(tmp); }
    }

    [Fact]
    public void Quy_doi_ten_cot_Excel()
        => Assert.True(XlsxWriter.ColumnName(1) == "A" && XlsxWriter.ColumnName(26) == "Z" && XlsxWriter.ColumnName(27) == "AA",
            "quy doi ten cot Excel dung");

    [Fact]
    public void Nhan_dien_so()
        => Assert.True(XlsxWriter.IsNumeric("42", out _) && !XlsxWriter.IsNumeric("007", out _) && !XlsxWriter.IsNumeric("-", out _),
            "nhan dien so: '42' la so, '007' va '-' KHONG phai so");

    [Fact]
    public void Ten_sheet_bi_cat_31_ky_tu_va_khong_trung()
    {
        var used = new HashSet<string>();
        var n1 = XlsxWriter.MakeSafeSheetName("Phạm vi quét (luôn hiển thị dù có phát hiện hay không)", used);
        var n2 = XlsxWriter.MakeSafeSheetName("Phạm vi quét (luôn hiển thị dù có phát hiện hay không)", used);

        Assert.True(n1.Length <= 31 && n2.Length <= 31 && n1 != n2, "ten sheet bi cat 31 ky tu va khong trung nhau");
    }

    /// <summary>
    /// Cat ten sheet KHONG duoc lam vo cap the thay the (surrogate pair): mot nua
    /// the thay the khong phai ky tu XML hop le va se lam Excel bao file hong.
    /// </summary>
    [Fact]
    public void Cat_ten_sheet_khong_lam_vo_cap_the_thay_the()
    {
        var emojiTitle = new string('A', 30) + "\U0001F600" + "duoi";
        var emojiName = XlsxWriter.MakeSafeSheetName(emojiTitle, new HashSet<string>());

        Assert.True(emojiName.Length <= 31 && !char.IsSurrogate(emojiName[^1]),
            "cat ten sheet khong lam vo cap the thay the");
    }

    /// <summary>
    /// Bo test cu chi kiem "file co ton tai va lon hon 1000 byte". Do la ly do mot
    /// file .xlsx THIEU QUAN HE toi styles.xml van di qua duoc toan bo 54 test, roi
    /// Excel that bao "We found a problem with some content" khi nguoi dung mo len.
    /// Cac kiem tra duoi day soi vao CAU TRUC goi, khong chi soi kich thuoc.
    /// </summary>
    [Fact]
    public void Toan_ven_goi_OPC()
    {
        var opcPath = H.TempFile("tsudev-opc", ".xlsx");
        try
        {
            new XlsxWriter().Write(opcPath, Infected.NamedTables(Infected.FreshLicenseReport()));

            using var zip = ZipFile.OpenRead(opcPath);
            string Part(string name) => new StreamReader(zip.GetEntry(name)!.Open()).ReadToEnd();

            var names = zip.Entries.Select(e => e.FullName).ToHashSet(StringComparer.Ordinal);
            var wbRels = Part("xl/_rels/workbook.xml.rels");
            var contentTypes = Part("[Content_Types].xml");
            var workbook = Part("xl/workbook.xml");

            Assert.True(names.Contains("xl/styles.xml"), "goi co phan styles.xml");

            // Day la ca kiem thu bat duoc dung loi that: phan nam trong goi nhung
            // KHONG co quan he nao tro toi thi theo chuan OPC coi nhu khong ton tai.
            Assert.True(wbRels.Contains("Target=\"styles.xml\"", StringComparison.Ordinal),
                "workbook.xml.rels CO quan he tro toi styles.xml");

            // Moi phan trong goi (tru chinh cac file .rels) phai duoc mot quan he tro toi.
            var allRels = Part("_rels/.rels") + wbRels;
            var unreferenced = names
                .Where(n => !n.EndsWith(".rels", StringComparison.Ordinal) && n != "[Content_Types].xml")
                .Where(n => !allRels.Contains(Path.GetFileName(n), StringComparison.Ordinal))
                .ToList();
            Assert.True(unreferenced.Count == 0,
                $"moi phan trong goi deu duoc quan he tro toi (mo coi: {string.Join(", ", unreferenced)})");

            // Moi phan phai duoc khai bao kieu noi dung, neu khong Excel khong biet doc.
            var undeclared = names
                .Where(n => n.EndsWith(".xml", StringComparison.Ordinal) && n != "[Content_Types].xml")
                .Where(n => !contentTypes.Contains("/" + n, StringComparison.Ordinal))
                .ToList();
            Assert.True(undeclared.Count == 0,
                $"moi phan .xml deu khai bao trong [Content_Types] (thieu: {string.Join(", ", undeclared)})");

            // sheetView khai bao workbookViewId="0" nen bookViews phai ton tai.
            Assert.True(workbook.Contains("<bookViews>", StringComparison.Ordinal),
                "workbook.xml co <bookViews> (vi sheetView tro toi workbookViewId=0)");

            // Moi phan phai la XML doc duoc.
            bool allParse = true;
            foreach (var e in zip.Entries)
            {
                try { System.Xml.Linq.XDocument.Parse(new StreamReader(e.Open()).ReadToEnd()); }
                catch (System.Xml.XmlException) { allParse = false; }
            }
            Assert.True(allParse, "moi phan trong goi deu la XML hop le");
        }
        finally { File.Delete(opcPath); }
    }
}
