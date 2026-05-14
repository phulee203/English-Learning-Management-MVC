using System;
using System.Linq;
using System.Web.Mvc;
using WebDemo.Models;
using System.Collections.Generic;

namespace WebDemo.Controllers
{
    public class HocSinhController : Controller
    {
        QuanLyHocThemEntities db = new QuanLyHocThemEntities();

        // Hàm tiện ích: Lấy ID học sinh cần xem
        // Nếu là HS -> Trả về ID của chính nó
        // Nếu là PH -> Trả về ID của con
        private int? LayMaHocSinhCanXem()
        {
            if (Session["UserID"] == null) return null;

            int userId = (int)Session["UserID"];
            string role = Session["Role"] as string;

            if (role == "HocSinh")
            {
                return userId;
            }
            else if (role == "PhuHuynh")
            {
                // Tìm đứa con (Học sinh) có MaPhuHuynh trùng với UserID này
                var con = db.HocSinhs.FirstOrDefault(hs => hs.MaPhuHuynh == userId);
                return con != null ? con.MaHocSinh : (int?)null;
            }
            return null;
        }

        // 1. XEM KẾT QUẢ HỌC TẬP
        public ActionResult KetQuaHocTap()
        {
            int? maHS = LayMaHocSinhCanXem();
            if (maHS == null) return RedirectToAction("Login", "Account");

            // Lấy điểm của mã HS đã xác định được
            var bangDiem = (from d in db.Diems
                            join kh in db.KhoaHocs on d.MaKhoaHoc equals kh.MaKhoaHoc
                            where d.MaHocSinh == maHS
                            select new KetQuaHocTapViewModel
                            {
                                TenKhoaHoc = kh.TenKhoaHoc,
                                DiemGiuaKy = d.DiemGiuaKy,
                                DiemCuoiKy = d.DiemCuoiKy,
                                NhanXet = d.GhiChu
                            }).ToList();

            // Hiển thị tên của học sinh (để phụ huynh biết đang xem của ai)
            ViewBag.TenHocSinh = db.NguoiDungs.Find(maHS).HoTen;

            return View(bangDiem);
        }

        // 2. XEM THÔNG BÁO
        public ActionResult XemThongBao()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");
            int userId = (int)Session["UserID"];

            // Thông báo thì gửi riêng cho từng tài khoản (PH nhận tin của PH, HS nhận tin của HS)
            // Nên không cần tìm ID con, cứ lấy theo UserID đang đăng nhập
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

        #region CHỨC NĂNG ĐĂNG KÝ MÔN MỚI

        // 1. Hiển thị form đăng ký
        public ActionResult DangKyMon()
        {
            if (Session["UserID"] == null) return RedirectToAction("Login", "Account");

            // Lấy danh sách khóa học đang mở
            ViewBag.DanhSachKhoaHoc = new SelectList(db.KhoaHocs, "MaKhoaHoc", "TenKhoaHoc");
            return View();
        }

        // 2. Xử lý lưu đăng ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LuuDangKy(int maKhoaHoc, string caHoc)
        {
            // Xác định ai là người học (Học sinh tự đăng ký hay Phụ huynh đăng ký cho con)
            int? maHocSinh = LayMaHocSinhCanXem(); // Hàm này đã viết ở bước trước

            if (maHocSinh == null) return RedirectToAction("Login", "Account");

            // 1. Tìm lớp học phù hợp (Lấy lớp đầu tiên của khóa này)
            // Trong thực tế có thể cho chọn lớp cụ thể, ở đây làm đơn giản chọn Khóa -> Tự gán Lớp
            var lopHoc = db.LopHocs.FirstOrDefault(l => l.MaKhoaHoc == maKhoaHoc);

            if (lopHoc != null)
            {
                // Kiểm tra đã đăng ký chưa
                bool daDangKy = db.DangKyLops.Any(d => d.MaHocSinh == maHocSinh && d.MaLop == lopHoc.MaLop);
                if (daDangKy)
                {
                    TempData["Error"] = "Bạn đã đăng ký môn học này rồi!";
                    return RedirectToAction("DangKyMon");
                }

                // 2. Tạo Đăng Ký Lớp
                var dangKy = new DangKyLop
                {
                    MaHocSinh = (int)maHocSinh,
                    MaLop = lopHoc.MaLop,
                    NgayDangKy = DateTime.Now,
                    TrangThai = "DaXacNhan" // Hoặc "DangCho" tùy quy trình
                };
                db.DangKyLops.Add(dangKy);

                // 3. Tạo khoản nợ Học Phí (Quan trọng: Để Admin thu tiền)
                var khoaHoc = db.KhoaHocs.Find(maKhoaHoc);
                var hocPhi = new HocPhi
                {
                    MaHocSinh = (int)maHocSinh,
                    MaKhoaHoc = maKhoaHoc,
                    SoTien = khoaHoc.HocPhi,
                    NgayDong = null,       // Chưa đóng
                    TrangThai = "ChuaDong" // Trạng thái chờ đóng
                };
                db.HocPhis.Add(hocPhi);

                db.SaveChanges();
                TempData["Success"] = "Đăng ký thành công! Vui lòng liên hệ văn phòng để đóng học phí.";
            }
            else
            {
                TempData["Error"] = "Hiện chưa có lớp nào mở cho môn này.";
            }

            return RedirectToAction("DangKyMon");
        }

        #endregion

        public ActionResult XemLichHoc()
        {
            int? maHS = LayMaHocSinhCanXem();
            if (maHS == null) return RedirectToAction("Login", "Account");

            var dsLich = (from dk in db.DangKyLops
                          join lop in db.LopHocs on dk.MaLop equals lop.MaLop
                          join kh in db.KhoaHocs on lop.MaKhoaHoc equals kh.MaKhoaHoc
                          // Join với bảng LichHoc để lấy chi tiết giờ học
                          join lh in db.LichHocs on lop.MaLop equals lh.MaLop
                          // Join để lấy tên giáo viên (Left Join vì có thể chưa phân công)
                          join pc in db.PhanCongGiaoViens on lop.MaLop equals pc.MaLop into pcGroup
                          from pc in pcGroup.DefaultIfEmpty()
                          join gv in db.NguoiDungs on pc.MaGiaoVien equals gv.MaNguoiDung into gvGroup
                          from gv in gvGroup.DefaultIfEmpty()

                          where dk.MaHocSinh == maHS && dk.TrangThai == "DaXacNhan"
                          orderby lh.ThuTrongTuan // Sắp xếp theo thứ
                          select new LichHocViewModel
                          {
                              TenLop = lop.TenLop,
                              TenMon = kh.TenKhoaHoc,
                              PhongHoc = lop.PhongHoc, // Ví dụ: P101
                              Thu = lh.ThuTrongTuan,   // Ví dụ: 2, 3, 4, CN
                              GioBatDau = lh.GioBatDau,
                              GioKetThuc = lh.GioKetThuc,
                              GiaoVien = gv != null ? gv.HoTen : "Chưa có GV"
                          }).ToList();

            return View(dsLich);
        }
    }
}