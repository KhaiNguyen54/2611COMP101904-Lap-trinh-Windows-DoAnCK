using System;
using System.Security.Cryptography;

namespace QuanLyKhachSan.Services
{
    /// <summary>Tạo và xác minh hash PBKDF2-SHA256 có salt ngẫu nhiên riêng cho từng mật khẩu.</summary>
    public static class PasswordHasher
    {
        public const int SoVongLapMacDinh = 100000;
        private const int DoDaiSalt = 16;
        private const int DoDaiHash = 32;

        public static void TaoHash(string matKhau, out string hash, out string salt, out int soVongLap)
        {
            if (string.IsNullOrEmpty(matKhau)) throw new ArgumentException("Mật khẩu không được để trống.", nameof(matKhau));
            byte[] saltBytes = RandomNumberGenerator.GetBytes(DoDaiSalt);
            soVongLap = SoVongLapMacDinh;
            salt = Convert.ToBase64String(saltBytes);
            hash = Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(matKhau, saltBytes, soVongLap,
                HashAlgorithmName.SHA256, DoDaiHash));
        }

        public static bool KiemTra(string matKhau, string hash, string salt, int soVongLap)
        {
            if (string.IsNullOrEmpty(matKhau) || string.IsNullOrWhiteSpace(hash) ||
                string.IsNullOrWhiteSpace(salt) || soVongLap < 10000) return false;
            try
            {
                byte[] expected = Convert.FromBase64String(hash);
                byte[] actual = Rfc2898DeriveBytes.Pbkdf2(matKhau, Convert.FromBase64String(salt),
                    soVongLap, HashAlgorithmName.SHA256, expected.Length);
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch (FormatException) { return false; }
        }
    }
}
