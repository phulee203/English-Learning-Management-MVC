using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebDemo.Models;

namespace WebDemo.Controllers
{
    public class GiaoVienController : Controller
    {
        QuanLyHocThemEntities db = new QuanLyHocThemEntities();

        // 1. Xem danh sách lớp mình được phân công dạy
        public ActionResult LopCuaToi()
        {
            // Kiểm tra đăng nhập
            if (Session["UserID"] == null || Session["Role"].ToString() != "GiaoVien")
                return RedirectToAction("Login", "Account");

            int maGV = (int)Session["UserID"];

            // Lấy danh sách lớp mà giáo viên này được phân công [cite: 290]
            var dsLop = (from pc in db.PhanCongGiaoViens
                         join lop in db.LopHocs on pc.MaLop equals lop.MaLop
                         join kh in db.KhoaHocs on lop.MaKhoaHoc equals kh.MaKhoaHoc
                         where pc.MaGiaoVien == maGV
                         select new LopHocViewModel
                         {
                             MaLop = lop.MaLop,
                             TenLop = lop.TenLop,
                             TenKhoaHoc = kh.TenKhoaHoc,
                             LichHoc = lop.PhongHoc // Hoặc LichHoc nếu có cột riêng
                         }).ToList();

            return View(dsLop);
        }

        // 2. Giao diện Nhập điểm cho 1 lớp
        [HttpGet]
        public ActionResult NhapDiem(int id) // id là MaLop
        {
            var lop = db.LopHocs.Find(id);
            if (lop == null) return HttpNotFound();

            ViewBag.TenLop = lop.TenLop;
            ViewBag.MaLop = id;

            // Lấy danh sách học sinh trong lớp + Điểm hiện tại (nếu có)
            var dsHocSinh = (from dk in db.DangKyLops
                             join hs in db.HocSinhs on dk.MaHocSinh equals hs.MaHocSinh
                             join nd in db.NguoiDungs on hs.MaHocSinh equals nd.MaNguoiDung
                             // Left join với bảng Diem để lấy điểm cũ nếu đã nhập
                             // Ép kiểu (int) để đảm bảo 2 bên giống nhau hoàn toàn
                             join d in db.Diems on new { a = dk.MaHocSinh, b = (int)lop.MaKhoaHoc } equals new { a = (int)d.MaHocSinh, b = (int)d.MaKhoaHoc } into diems
                             from diem in diems.DefaultIfEmpty()
                             where dk.MaLop == id && dk.TrangThai == "DaXacNhan"
                             select new DiemViewModel
                             {
                                 MaHocSinh = hs.MaHocSinh,
                                 HoTen = nd.HoTen,
                                 DiemGiuaKy = diem != null ? diem.DiemGiuaKy : null,
                                 DiemCuoiKy = diem != null ? diem.DiemCuoiKy : null,
                                 NhanXet = diem != null ? diem.GhiChu : ""
                             }).ToList();

            return View(dsHocSinh);
        }

        public ActionResult XemThongBao()
        {
            if (Session["UserID"] == null || Session["Role"].ToString() != "GiaoVien")
                return RedirectToAction("Login", "Account");

            int userId = (int)Session["UserID"];

            var dsThongBao = (from gtb in db.GuiThongBaos
                              join tb in db.ThongBaos on gtb.MaThongBao equals tb.MaThongBao
                              join admin in db.NguoiDungs on tb.MaAdmin equals admin.MaNguoiDung
                              where gtb.MaNguoiDung == userId
                              orderby tb.NgayGui descending
                              select new ThongBaoViewModel
                              {
                                  TieuDe = tb.TieuDe,
                                  NoiDung = tb.NoiDung,
                                  NgayGui = tb.NgayGui,
                                  NguoiGui = admin.HoTen,
                                  DaDoc = (bool)gtb.DaDoc
                              }).ToList();

            return View(dsThongBao);
        }

        // 3. Xử lý lưu điểm (Nhận list học sinh từ form)
        [HttpPost]
        public ActionResult LuuDiem(int maLop, List<DiemViewModel> listDiem)
        {
            var lop = db.LopHocs.Find(maLop);
            int maKhoaHoc = (int)lop.MaKhoaHoc;

            foreach (var item in listDiem)
            {
                // Tìm xem đã có bản ghi điểm chưa
                var diemDB = db.Diems.FirstOrDefault(d => d.MaHocSinh == item.MaHocSinh && d.MaKhoaHoc == maKhoaHoc);

                if (diemDB != null)
                {
                    // Cập nhật điểm cũ [cite: 418]
                    diemDB.DiemGiuaKy = item.DiemGiuaKy;
                    diemDB.DiemCuoiKy = item.DiemCuoiKy;
                    diemDB.GhiChu = item.NhanXet;
                    diemDB.NgayCapNhat = DateTime.Now;
                }
                else
                {
                    // Tạo mới nếu chưa có
                    if (item.DiemGiuaKy != null || item.DiemCuoiKy != null) // Chỉ lưu nếu có nhập điểm
                    {
                        Diem diemMoi = new Diem();
                        diemMoi.MaHocSinh = item.MaHocSinh;
                        diemMoi.MaKhoaHoc = maKhoaHoc;
                        diemMoi.DiemGiuaKy = item.DiemGiuaKy;
                        diemMoi.DiemCuoiKy = item.DiemCuoiKy;
                        diemMoi.GhiChu = item.NhanXet;
                        diemMoi.NgayCapNhat = DateTime.Now;
                        db.Diems.Add(diemMoi);
                    }
                }
            }
            db.SaveChanges();
            TempData["Success"] = "Đã lưu bảng điểm thành công!";
            return RedirectToAction("NhapDiem", new { id = maLop });
        }
    }
}