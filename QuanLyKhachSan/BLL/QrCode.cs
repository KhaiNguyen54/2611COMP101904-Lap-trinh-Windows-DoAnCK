using System;
using System.Text;

namespace QuanLyKhachSan.BLL
{
    /// <summary>
    /// Bộ tạo mã QR tự cài đặt (chế độ byte UTF-8, mức sửa lỗi M, phiên bản 1–10, tối đa 213 byte)
    /// để không phụ thuộc gói NuGet ngoài. Chỉ trả về ma trận mô-đun; việc vẽ ảnh do tầng GUI đảm nhiệm.
    /// </summary>
    public static class QrCode
    {
        // Mỗi dòng ứng với phiên bản 1..10 (mức M): tổng codeword, EC mỗi khối, số khối nhóm 1, data nhóm 1, số khối nhóm 2, data nhóm 2.
        private static readonly int[][] Bang =
        {
            new[] {  26, 10, 1, 16, 0,  0 },
            new[] {  44, 16, 1, 28, 0,  0 },
            new[] {  70, 26, 1, 44, 0,  0 },
            new[] { 100, 18, 2, 32, 0,  0 },
            new[] { 134, 24, 2, 43, 0,  0 },
            new[] { 172, 16, 4, 27, 0,  0 },
            new[] { 196, 18, 4, 31, 0,  0 },
            new[] { 242, 22, 2, 38, 2, 39 },
            new[] { 292, 22, 3, 36, 2, 37 },
            new[] { 346, 26, 4, 43, 1, 44 },
        };

        private static readonly int[][] ViTriCanChinh =
        {
            new int[0],
            new[] { 6, 18 }, new[] { 6, 22 }, new[] { 6, 26 }, new[] { 6, 30 }, new[] { 6, 34 },
            new[] { 6, 22, 38 }, new[] { 6, 24, 42 }, new[] { 6, 26, 46 }, new[] { 6, 28, 50 },
        };

        public const int ToiDaByte = 213;

        /// <summary>Tạo ma trận QR (true = ô đen). Ném ArgumentException khi nội dung rỗng hoặc quá dài.</summary>
        public static bool[,] Tao(string noiDung)
        {
            if (string.IsNullOrEmpty(noiDung))
                throw new ArgumentException("Nội dung mã QR không được để trống.", nameof(noiDung));
            byte[] data = Encoding.UTF8.GetBytes(noiDung);
            if (data.Length > ToiDaByte)
                throw new ArgumentException("Nội dung mã QR quá dài (tối đa " + ToiDaByte + " byte).", nameof(noiDung));

            int version = ChonPhienBan(data.Length);
            int[] t = Bang[version - 1];
            int soKhoi = t[2] + t[4];
            int dataCw = t[0] - t[1] * soKhoi;

            byte[] codewords = Interleave(TaoDuLieu(data, version, dataCw), version);
            int size = 17 + 4 * version;
            var mo = new bool[size, size];
            var cd = new bool[size, size];
            VeMauChucNang(mo, cd, version, size);
            VeCodeword(mo, cd, codewords, size);

            int tot = 0, diemTot = int.MaxValue;
            for (int mask = 0; mask < 8; mask++)
            {
                AoMask(mo, cd, mask, size);
                VeFormat(mo, cd, mask, size);
                int diem = Phat(mo, size);
                if (diem < diemTot) { diemTot = diem; tot = mask; }
                AoMask(mo, cd, mask, size); // XOR lần hai để hoàn nguyên
            }
            AoMask(mo, cd, tot, size);
            VeFormat(mo, cd, tot, size);
            return mo;
        }

        private static int ChonPhienBan(int soByte)
        {
            for (int v = 1; v <= 10; v++)
            {
                int[] t = Bang[v - 1];
                int dataCw = t[0] - t[1] * (t[2] + t[4]);
                int bitCount = v <= 9 ? 8 : 16;
                if (4 + bitCount + 8 * soByte <= dataCw * 8) return v;
            }
            throw new ArgumentException("Nội dung mã QR quá dài.");
        }

        private static byte[] TaoDuLieu(byte[] data, int version, int dataCw)
        {
            var bits = new System.Collections.Generic.List<bool>();
            Them(bits, 0x4, 4); // chế độ byte
            Them(bits, data.Length, version <= 9 ? 8 : 16);
            foreach (byte b in data) Them(bits, b, 8);
            int toiDa = dataCw * 8;
            for (int i = 0; i < 4 && bits.Count < toiDa; i++) bits.Add(false);
            while (bits.Count % 8 != 0) bits.Add(false);
            var kq = new byte[dataCw];
            for (int i = 0; i < bits.Count; i++)
                if (bits[i]) kq[i >> 3] |= (byte)(0x80 >> (i & 7));
            int n = bits.Count / 8;
            for (int i = n, pad = 0; i < dataCw; i++, pad++) kq[i] = (byte)(pad % 2 == 0 ? 0xEC : 0x11);
            return kq;
        }

        private static void Them(System.Collections.Generic.List<bool> bits, int giaTri, int soBit)
        {
            for (int i = soBit - 1; i >= 0; i--) bits.Add(((giaTri >> i) & 1) != 0);
        }

        private static byte[] Interleave(byte[] data, int version)
        {
            int[] t = Bang[version - 1];
            int ecLen = t[1];
            int soKhoi = t[2] + t[4];
            var khoiData = new byte[soKhoi][];
            var khoiEc = new byte[soKhoi][];
            byte[] gen = TaoDaThucSinh(ecLen);
            int pos = 0;
            for (int k = 0; k < soKhoi; k++)
            {
                int len = k < t[2] ? t[3] : t[5];
                khoiData[k] = new byte[len];
                Array.Copy(data, pos, khoiData[k], 0, len);
                pos += len;
                khoiEc[k] = PhanDu(khoiData[k], gen);
            }
            var kq = new byte[t[0]];
            int w = 0;
            int maxData = Math.Max(t[3], t[5]);
            for (int i = 0; i < maxData; i++)
                for (int k = 0; k < soKhoi; k++)
                    if (i < khoiData[k].Length) kq[w++] = khoiData[k][i];
            for (int i = 0; i < ecLen; i++)
                for (int k = 0; k < soKhoi; k++)
                    kq[w++] = khoiEc[k][i];
            return kq;
        }

        // ---- Reed–Solomon trên GF(256), đa thức nguyên thủy 0x11D ----
        private static int NhanGf(int x, int y)
        {
            int z = 0;
            for (int i = 7; i >= 0; i--)
            {
                z = (z << 1) ^ ((z >> 7) * 0x11D);
                z ^= ((y >> i) & 1) * x;
            }
            return z;
        }

        private static byte[] TaoDaThucSinh(int bac)
        {
            var kq = new byte[bac];
            kq[bac - 1] = 1;
            int goc = 1;
            for (int i = 0; i < bac; i++)
            {
                for (int j = 0; j < bac; j++)
                {
                    kq[j] = (byte)NhanGf(kq[j], goc);
                    if (j + 1 < bac) kq[j] ^= kq[j + 1];
                }
                goc = NhanGf(goc, 0x02);
            }
            return kq;
        }

        private static byte[] PhanDu(byte[] data, byte[] gen)
        {
            var kq = new byte[gen.Length];
            foreach (byte b in data)
            {
                int heSo = b ^ kq[0];
                Array.Copy(kq, 1, kq, 0, kq.Length - 1);
                kq[kq.Length - 1] = 0;
                for (int i = 0; i < kq.Length; i++) kq[i] ^= (byte)NhanGf(gen[i], heSo);
            }
            return kq;
        }

        // ---- Vẽ ma trận ----
        private static void DatChucNang(bool[,] mo, bool[,] cd, int x, int y, bool den)
        {
            mo[y, x] = den;
            cd[y, x] = true;
        }

        private static void VeMauChucNang(bool[,] mo, bool[,] cd, int version, int size)
        {
            for (int i = 0; i < size; i++)
            {
                DatChucNang(mo, cd, 6, i, i % 2 == 0);
                DatChucNang(mo, cd, i, 6, i % 2 == 0);
            }
            VeDinh(mo, cd, 3, 3, size);
            VeDinh(mo, cd, size - 4, 3, size);
            VeDinh(mo, cd, 3, size - 4, size);

            int[] vt = ViTriCanChinh[version - 1];
            int n = vt.Length;
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                {
                    if ((i == 0 && j == 0) || (i == 0 && j == n - 1) || (i == n - 1 && j == 0)) continue;
                    for (int dy = -2; dy <= 2; dy++)
                        for (int dx = -2; dx <= 2; dx++)
                            DatChucNang(mo, cd, vt[i] + dx, vt[j] + dy, Math.Max(Math.Abs(dx), Math.Abs(dy)) != 1);
                }

            VeFormat(mo, cd, 0, size); // giữ chỗ
            if (version >= 7)
            {
                int rem = version;
                for (int i = 0; i < 12; i++) rem = (rem << 1) ^ ((rem >> 11) * 0x1F25);
                int bits = (version << 12) | rem;
                for (int i = 0; i < 18; i++)
                {
                    bool den = ((bits >> i) & 1) != 0;
                    int a = size - 11 + i % 3, b = i / 3;
                    DatChucNang(mo, cd, a, b, den);
                    DatChucNang(mo, cd, b, a, den);
                }
            }
        }

        private static void VeDinh(bool[,] mo, bool[,] cd, int cx, int cy, int size)
        {
            for (int dy = -4; dy <= 4; dy++)
                for (int dx = -4; dx <= 4; dx++)
                {
                    int x = cx + dx, y = cy + dy;
                    if (x < 0 || x >= size || y < 0 || y >= size) continue;
                    int d = Math.Max(Math.Abs(dx), Math.Abs(dy));
                    DatChucNang(mo, cd, x, y, d != 2 && d != 4);
                }
        }

        private static void VeFormat(bool[,] mo, bool[,] cd, int mask, int size)
        {
            int data = mask; // mức sửa lỗi M có mã 00
            int rem = data;
            for (int i = 0; i < 10; i++) rem = (rem << 1) ^ ((rem >> 9) * 0x537);
            int bits = ((data << 10) | rem) ^ 0x5412;
            for (int i = 0; i <= 5; i++) DatChucNang(mo, cd, 8, i, Bit(bits, i));
            DatChucNang(mo, cd, 8, 7, Bit(bits, 6));
            DatChucNang(mo, cd, 8, 8, Bit(bits, 7));
            DatChucNang(mo, cd, 7, 8, Bit(bits, 8));
            for (int i = 9; i < 15; i++) DatChucNang(mo, cd, 14 - i, 8, Bit(bits, i));
            for (int i = 0; i < 8; i++) DatChucNang(mo, cd, size - 1 - i, 8, Bit(bits, i));
            for (int i = 8; i < 15; i++) DatChucNang(mo, cd, 8, size - 15 + i, Bit(bits, i));
            DatChucNang(mo, cd, 8, size - 8, true);
        }

        private static bool Bit(int x, int i) { return ((x >> i) & 1) != 0; }

        private static void VeCodeword(bool[,] mo, bool[,] cd, byte[] data, int size)
        {
            int i = 0;
            for (int phai = size - 1; phai >= 1; phai -= 2)
            {
                if (phai == 6) phai = 5;
                for (int doc = 0; doc < size; doc++)
                    for (int j = 0; j < 2; j++)
                    {
                        int x = phai - j;
                        bool huongLen = ((phai + 1) & 2) == 0;
                        int y = huongLen ? size - 1 - doc : doc;
                        if (!cd[y, x] && i < data.Length * 8)
                        {
                            mo[y, x] = ((data[i >> 3] >> (7 - (i & 7))) & 1) != 0;
                            i++;
                        }
                    }
            }
        }

        private static void AoMask(bool[,] mo, bool[,] cd, int mask, int size)
        {
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    if (cd[y, x]) continue;
                    bool dao;
                    switch (mask)
                    {
                        case 0: dao = (x + y) % 2 == 0; break;
                        case 1: dao = y % 2 == 0; break;
                        case 2: dao = x % 3 == 0; break;
                        case 3: dao = (x + y) % 3 == 0; break;
                        case 4: dao = (x / 3 + y / 2) % 2 == 0; break;
                        case 5: dao = x * y % 2 + x * y % 3 == 0; break;
                        case 6: dao = (x * y % 2 + x * y % 3) % 2 == 0; break;
                        default: dao = ((x + y) % 2 + x * y % 3) % 2 == 0; break;
                    }
                    if (dao) mo[y, x] = !mo[y, x];
                }
        }

        // ---- Chấm điểm phạt để chọn mặt nạ ----
        private static int Phat(bool[,] mo, int size)
        {
            int diem = 0;
            for (int h = 0; h < 2; h++)
                for (int a = 0; a < size; a++)
                {
                    int chay = 1;
                    for (int b = 1; b < size; b++)
                    {
                        bool cur = h == 0 ? mo[a, b] : mo[b, a];
                        bool pre = h == 0 ? mo[a, b - 1] : mo[b - 1, a];
                        if (cur == pre) { chay++; if (chay == 5) diem += 3; else if (chay > 5) diem++; }
                        else chay = 1;
                    }
                    // mẫu giống dấu định vị 1:1:3:1:1
                    for (int b = 0; b + 10 < size; b++)
                    {
                        if (Khop(mo, h, a, b, new[] { true, false, true, true, true, false, true, false, false, false, false })
                            || Khop(mo, h, a, b, new[] { false, false, false, false, true, false, true, true, true, false, true }))
                            diem += 40;
                    }
                }
            for (int y = 0; y + 1 < size; y++)
                for (int x = 0; x + 1 < size; x++)
                    if (mo[y, x] == mo[y, x + 1] && mo[y, x] == mo[y + 1, x] && mo[y, x] == mo[y + 1, x + 1]) diem += 3;
            int den = 0;
            foreach (bool o in mo) if (o) den++;
            int tong = size * size;
            int k = (Math.Abs(den * 20 - tong * 10) + tong - 1) / tong - 1;
            return diem + k * 10;
        }

        private static bool Khop(bool[,] mo, int huong, int a, int b, bool[] mau)
        {
            for (int i = 0; i < mau.Length; i++)
            {
                bool o = huong == 0 ? mo[a, b + i] : mo[b + i, a];
                if (o != mau[i]) return false;
            }
            return true;
        }
    }
}
