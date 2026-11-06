using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace QuanLyKhachSan.Repositories
{
    /// <summary>Các thao tác truy cập SQL Server có tham số và hỗ trợ transaction.</summary>
    public static class Database
    {
        private static string _chuoiKetNoi;

        /// <summary>Chuỗi kết nối, đọc từ appsettings.json (khóa ConnectionStrings:KhachSan).</summary>
        public static string ChuoiKetNoi
        {
            get
            {
                if (_chuoiKetNoi == null)
                {
                    _chuoiKetNoi = DocChuoiKetNoi();
                }
                return _chuoiKetNoi;
            }
        }

        /// <summary>Tạo một kết nối mới (chưa mở). Người gọi chịu trách nhiệm Dispose.</summary>
        public static SqlConnection TaoKetNoi()
        {
            return new SqlConnection(ChuoiKetNoi);
        }

        public static int ThucThi(string sql, IEnumerable<SqlParameter> thamSo = null,
            SqlTransaction transaction = null, CommandType loaiLenh = CommandType.Text)
        {
            using (SqlCommand command = TaoLenh(sql, thamSo, transaction, loaiLenh))
                return command.ExecuteNonQuery();
        }

        public static object ThucThiGiaTri(string sql, IEnumerable<SqlParameter> thamSo = null,
            SqlTransaction transaction = null, CommandType loaiLenh = CommandType.Text)
        {
            using (SqlCommand command = TaoLenh(sql, thamSo, transaction, loaiLenh))
                return command.ExecuteScalar();
        }

        public static DataTable TruyVan(string sql, IEnumerable<SqlParameter> thamSo = null,
            SqlTransaction transaction = null, CommandType loaiLenh = CommandType.Text)
        {
            using (SqlCommand command = TaoLenh(sql, thamSo, transaction, loaiLenh))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                DataTable ketQua = new DataTable();
                adapter.Fill(ketQua);
                return ketQua;
            }
        }

        public static T ThucHienTrongTransaction<T>(Func<SqlConnection, SqlTransaction, T> thaoTac,
            IsolationLevel mucCachLy = IsolationLevel.ReadCommitted)
        {
            if (thaoTac == null) throw new ArgumentNullException(nameof(thaoTac));
            using (SqlConnection connection = TaoKetNoi())
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction(mucCachLy))
                {
                    try
                    {
                        T ketQua = thaoTac(connection, transaction);
                        transaction.Commit();
                        return ketQua;
                    }
                    catch
                    {
                        try { transaction.Rollback(); } catch (InvalidOperationException) { }
                        throw;
                    }
                }
            }
        }

        private static SqlCommand TaoLenh(string sql, IEnumerable<SqlParameter> thamSo,
            SqlTransaction transaction, CommandType loaiLenh)
        {
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentException("Câu lệnh không được để trống.", nameof(sql));
            SqlConnection connection = transaction == null ? TaoKetNoi() : transaction.Connection;
            if (connection == null) throw new InvalidOperationException("Transaction không còn kết nối hợp lệ.");
            if (connection.State != ConnectionState.Open) connection.Open();
            SqlCommand command = connection.CreateCommand();
            if (transaction == null) command.Disposed += delegate { connection.Dispose(); };
            command.CommandText = sql;
            command.CommandType = loaiLenh;
            command.Transaction = transaction;
            if (thamSo != null)
            {
                foreach (SqlParameter parameter in thamSo)
                    command.Parameters.Add(parameter);
            }
            return command;
        }

        /// <summary>Thử mở kết nối. Trả về false và thông báo lỗi nếu không được.</summary>
        public static bool KiemTraKetNoi(out string loi)
        {
            try
            {
                using (SqlConnection conn = TaoKetNoi())
                {
                    conn.Open();
                }
                loi = null;
                return true;
            }
            catch (Exception ex)
            {
                loi = ex.Message;
                return false;
            }
        }

        private static string DocChuoiKetNoi()
        {
            string duongDan = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(duongDan))
            {
                throw new FileNotFoundException("Không tìm thấy appsettings.json", duongDan);
            }

            using (JsonDocument doc = JsonDocument.Parse(File.ReadAllText(duongDan)))
            {
                return doc.RootElement
                    .GetProperty("ConnectionStrings")
                    .GetProperty("KhachSan")
                    .GetString();
            }
        }
    }
}
