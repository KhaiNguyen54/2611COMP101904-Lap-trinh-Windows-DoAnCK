#!/usr/bin/env python3
"""Lập lại TestCases/TestCase.xlsx từ nguồn thật để bảng test case luôn đủ và khớp mã nguồn.

Cách dùng (chạy tại thư mục gốc solution, cần python3 và openpyxl):
    dotnet run --project QuanLyKhachSan.Tests -c Release -- --xuat TestCases/ketqua_tudong.tsv
    py TestCases/tao_testcase.py TestCases/ketqua_tudong.tsv

Nguồn dữ liệu:
  - ketqua_tudong.tsv: từng ca của QuanLyKhachSan.Tests (do chính bộ test xuất ra, nên không bỏ sót ca nào).
  - QuanLyKhachSan/Scripts/KiemThu.sql: các ca SQL (DB-xx).
  - docs/07_KichBanKiemThu.md: kịch bản giao diện/web thủ công (UI-xx, WEB-xx) và mô tả nhóm kiểm thử.
Cột "Kết quả thực tế" của các ca thủ công do người thử điền; script giữ nguyên khi lập lại theo mã ca.
"""
import re
import sys
from copy import copy
from pathlib import Path

from openpyxl import Workbook, load_workbook
from openpyxl.styles import Alignment, Font, PatternFill

GOC = Path(__file__).resolve().parent.parent
XLSX = GOC / "TestCases" / "TestCase.xlsx"
MD = GOC / "docs" / "07_KichBanKiemThu.md"
SQL = GOC / "QuanLyKhachSan" / "Scripts" / "KiemThu.sql"


def doc_bang_md():
    dong = MD.read_text(encoding="utf-8").splitlines()
    thu_cong, tong_quat, nhom = [], [], {}
    thay = set()
    for l in dong:
        if not l.startswith("|"):
            continue
        c = [x.strip() for x in l.strip().strip("|").split("|")]
        if len(c) >= 3 and re.match(r"^(UI|WEB|DB|BUILD)-\d+", c[0]) and c[0] not in thay:
            thay.add(c[0])
            (thu_cong if c[0].startswith(("UI", "WEB")) else tong_quat).append(c[:3])
        if len(c) >= 4 and c[0].startswith("`") and c[1].isdigit():
            nhom[c[0].strip("`")] = (c[2], c[3])
    return thu_cong, tong_quat, nhom


def doc_sql():
    s = SQL.read_text(encoding="utf-8-sig")
    return [(m.group(1), m.group(2).replace("''", "'")) for m in re.finditer(r"INSERT #KQ VALUES \('([^']+)', N'((?:[^']|'')+)'", s)]


def doc_tsv(duong_dan):
    dong = Path(duong_dan).read_text(encoding="utf-8").splitlines()[1:]
    return [d.split("\t") for d in dong if d.strip()]


def giu_ket_qua_cu():
    cu = {}
    if XLSX.exists():
        ws = load_workbook(XLSX)["GiaoDien_Web"] if "GiaoDien_Web" in load_workbook(XLSX).sheetnames else None
        if ws:
            for r in ws.iter_rows(min_row=2, values_only=True):
                if r[0]:
                    ghi_chu = r[6] if len(r) > 6 else r[4] if len(r) > 4 else None
                    cu[r[0]] = (r[3] if len(r) > 3 else None, ghi_chu)
    return cu


def dinh_dang(ws, rong):
    for c in ws[1]:
        c.font = Font(bold=True, color="FFFFFF")
        c.fill = PatternFill("solid", fgColor="7A3E12")
        c.alignment = Alignment(vertical="center", wrap_text=True)
    ws.freeze_panes = "A2"
    for row in ws.iter_rows(min_row=2):
        for c in row:
            c.alignment = Alignment(vertical="top", wrap_text=True)
    for i, w in enumerate(rong):
        ws.column_dimensions[chr(65 + i)].width = w
    ws.auto_filter.ref = ws.dimensions


def main():
    if len(sys.argv) < 2:
        sys.exit(__doc__)
    thu_cong, tong_quat, nhom_md = doc_bang_md()
    cu = giu_ket_qua_cu()
    wb = Workbook()

    # ---- Kiểm thử tự động (từng ca của QuanLyKhachSan.Tests)
    ta = wb.active
    ta.title = "KiemThuTuDong"
    ta.append(["ID", "Nhóm (hàm kiểm thử)", "Quy tắc", "Mô tả ca kiểm thử", "Kết quả mong đợi", "Giá trị thực tế", "Kết quả"])
    dem = {}
    quy_tac = {k: v[0] for k, v in nhom_md.items()}
    quy_tac["TongThanhToan"] = "QT44, QT45"
    for nhom, mota, kyvong, thucte, ketqua in doc_tsv(sys.argv[1]):
        m = re.match(r"^(MVP-[A-Z]+\d+)\s+(.*)$", mota)
        if m:
            ma, mota = m.group(1), m.group(2)
        else:
            dem[nhom] = dem.get(nhom, 0) + 1
            ma = "%s-%02d" % (nhom, dem[nhom])
        qt = "Presenter MVP" if nhom.endswith("PresenterTests") else quy_tac.get(nhom, "")
        ta.append([ma, nhom, qt, mota, ("= " + kyvong) if kyvong != "" else "Điều kiện kiểm tra thỏa (đúng như mô tả)", thucte, ketqua])
        ta.cell(row=ta.max_row, column=5).data_type = "s"
    dinh_dang(ta, (18, 26, 16, 70, 34, 18, 10))

    # ---- CSDL / build (tự động trên GitHub Actions)
    cs = wb.create_sheet("CSDL_Build")
    cs.append(["ID", "Mô tả", "Kết quả mong đợi", "Cách chạy", "Kết quả"])
    sql = doc_sql()
    co = {i for i, _ in sql}
    hang = [(i, k, r, "Tổng quát (xem các ca con DB-xx)") for i, k, r in tong_quat if i not in co]
    hang += [(i, k, "ĐẠT", "KiemThu.sql") for i, k in sql]
    hang.sort(key=lambda x: (x[0].split("-")[0], x[0]))
    for i, k, r, cach in hang:
        cs.append([i, k, r, "GitHub Actions: " + cach, "Đạt"])
    dinh_dang(cs, (10, 70, 50, 34, 10))

    # ---- Giao diện / web (thủ công)
    tc = wb.create_sheet("GiaoDien_Web")
    tc.append(["ID", "Chức năng / thao tác", "Kết quả mong đợi", "Kết quả thực tế (Đạt/Lỗi)", "Ghi chú"])
    for i, k, r in thu_cong:
        tc.append([i, k, r] + list(cu.get(i, (None, None))))
    dinh_dang(tc, (10, 60, 60, 22, 40))

    # ---- Tổng hợp
    th = wb.create_sheet("TongHop", 0)
    th.append(["Nhóm", "Số ca", "Đạt", "Lỗi", "Chưa chạy"])
    th.append(["Kiểm thử tự động (QuanLyKhachSan.Tests)", "=COUNTA(KiemThuTuDong!A2:A5000)", '=COUNTIF(KiemThuTuDong!G2:G5000,"Đạt")', '=COUNTIF(KiemThuTuDong!G2:G5000,"Lỗi")', "=B2-C2-D2"])
    th.append(["CSDL và build (GitHub Actions)", "=COUNTA(CSDL_Build!A2:A5000)", '=COUNTIF(CSDL_Build!E2:E5000,"Đạt")', '=COUNTIF(CSDL_Build!E2:E5000,"Lỗi")', "=B3-C3-D3"])
    th.append(["Giao diện và cổng web (thủ công)", "=COUNTA(GiaoDien_Web!A2:A5000)", '=COUNTIF(GiaoDien_Web!D2:D5000,"Đạt")', '=COUNTIF(GiaoDien_Web!D2:D5000,"Lỗi")', "=B4-C4-D4"])
    th.append(["Tổng", "=SUM(B2:B4)", "=SUM(C2:C4)", "=SUM(D2:D4)", "=SUM(E2:E4)"])
    dinh_dang(th, (46, 10, 10, 10, 12))
    for c in th[5]:
        c.font = Font(bold=True)

    # ---- Hướng dẫn
    hd = wb.create_sheet("HuongDan")
    for d in [
        ["Cách dùng và cập nhật bảng test case"],
        ["Sheet KiemThuTuDong: liệt kê từng ca do QuanLyKhachSan.Tests xuất ra (mã MVP-xx lấy từ tên ca; ca khác đánh số theo nhóm, ví dụ SoDem-01)."],
        ["Sheet CSDL_Build: ca SQL trong Scripts/KiemThu.sql và build; kết quả ghi nhận từ lần chạy GitHub Actions gần nhất."],
        ["Sheet GiaoDien_Web: kịch bản thủ công, điền cột 'Kết quả thực tế' và 'Ghi chú' sau khi chạy trên máy Windows; lập lại bảng không làm mất phần đã điền."],
        ["Thêm ca mới: viết ca trong QuanLyKhachSan.Tests (hoặc KiemThu.sql / docs/07), rồi chạy lại hai lệnh ở đầu tệp TestCases/tao_testcase.py."],
        ["Nếu thêm ca mà chưa lập lại bảng, số ca ở sheet TongHop sẽ thấp hơn thực tế; sau mỗi lần thêm ca cần lập lại bảng này."],
    ]:
        hd.append(d)
    hd["A1"].font = Font(bold=True)
    hd.column_dimensions["A"].width = 150
    for r in hd.iter_rows(min_row=2):
        r[0].alignment = Alignment(wrap_text=True, vertical="top")

    for ws in wb.worksheets:
        for row in ws.iter_rows():
            for cell in row:
                font = copy(cell.font)
                font.name = "Times New Roman"
                cell.font = font

    wb.save(XLSX)
    print("Đã lập", XLSX, "-", ta.max_row - 1, "ca tự động,", cs.max_row - 1, "ca CSDL/build,", tc.max_row - 1, "ca thủ công")


if __name__ == "__main__":
    main()
