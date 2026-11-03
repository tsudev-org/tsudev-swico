namespace Tsudev.Audit.Tests;

/// <summary>Muc 16 cua bo test cu: doi chieu ma bam truoc khi chay file tai ve.</summary>
public class ChecksumTests
{
    private const string Sums = """
06a284ef82dc8b78c278e8053ffb028914e896b48d1b36505ecb13a24caa9c82  tsudev-swico_26.8.2001_x64-portable.zip
997bd2318999d0fd7fe82238cecfe51ab7ee7c08c0bcd4b9fe1910aa5e884abb  tsudev-swico_26.8.2001_x64-setup.exe
c23bb0ae0b7d02fe90736f762a7ecdd6a13a199c85f8243c344095f7488d80d7  swico.exe
""";

    [Fact]
    public void Tim_dung_ma_bam_theo_ten_tep()
    {
        Assert.True(ChecksumFile.Find(Sums, "tsudev-swico_26.8.2001_x64-setup.exe")
                == "997bd2318999d0fd7fe82238cecfe51ab7ee7c08c0bcd4b9fe1910aa5e884abb",
            "tim dung ma bam theo ten tep");
        Assert.True(ChecksumFile.Find(Sums, "swico.exe")
                == "c23bb0ae0b7d02fe90736f762a7ecdd6a13a199c85f8243c344095f7488d80d7",
            "khong khop nham dong khac co ten la tien to");
    }

    [Fact]
    public void Khong_tim_thay_hoac_du_lieu_hong_thi_null()
    {
        Assert.True(ChecksumFile.Find(Sums, "khong-co-tep-nay.exe") is null,
            "khong tim thay -> null (KHONG duoc coi la hop le)");
        Assert.True(ChecksumFile.Find("", "x.exe") is null && ChecksumFile.Find(null, "x.exe") is null,
            "noi dung rong -> null");
        Assert.True(ChecksumFile.Find("khong-phai-ma-bam  swico.exe", "swico.exe") is null,
            "ma bam sai dinh dang -> null, khong chay file khong xac minh duoc");
    }

    [Fact]
    public void Chap_nhan_cac_bien_the_dinh_dang()
    {
        Assert.True(ChecksumFile.Find("997BD2318999D0FD7FE82238CECFE51AB7EE7C08C0BCD4B9FE1910AA5E884ABB  a.exe", "a.exe")
                == "997bd2318999d0fd7fe82238cecfe51ab7ee7c08c0bcd4b9fe1910aa5e884abb",
            "chap nhan ma bam viet hoa, chuan hoa ve chu thuong");
        Assert.True(ChecksumFile.Find("997bd2318999d0fd7fe82238cecfe51ab7ee7c08c0bcd4b9fe1910aa5e884abb *a.exe", "a.exe") is not null,
            "chap nhan tien to '*' cua che do nhi phan");
    }
}
