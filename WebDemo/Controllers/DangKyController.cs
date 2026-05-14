using System;
using System.Linq;
using System.Web.Mvc;
using WebDemo.Models;

namespace WebDemo.Controllers
{
    public class DangKyController : Controller
    {
        QuanLyHocThemEntities db = new QuanLyHocThemEntities();

        // GET: Hiển thị trang Landing Page
        public ActionResult Index()
        {
            // Lấy danh sách khóa học để đổ vào Dropdown
            ViewBag.DanhSachKhoaHoc = new SelectList(db.KhoaHocs, "MaKhoaHoc", "TenKhoaHoc");
            return View();
        }

        // POST: Xử lý khi bấm nút "Thanh Toán"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XuLyThanhToan(DangKyViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Tạo hoặc tìm Học Sinh (Dựa theo Email/SĐT)
                    // Ở đây mình làm đơn giản: Tạo mới luôn
                    var nguoiDung = new NguoiDung
                    {
                        HoTen = model.HoTen,
                        Email = model.Email,
                        SoDienThoai = model.SoDienThoai,
                        DiaChi = model.DiaChi,

                        MatKhau = model.MatKhau,

                        VaiTro = "HocSinh",
                        NgayTao = DateTime.Now
                    };
                    db.NguoiDungs.Add(nguoiDung);
                    db.SaveChanges(); // Lưu để lấy ID

                    var hocSinh = new HocSinh
                    {
                        MaHocSinh = nguoiDung.MaNguoiDung,
                        NgaySinh = DateTime.Now // Tạm để ngày hiện tại
                    };
                    db.HocSinhs.Add(hocSinh);

                    // 2. Tìm lớp học phù hợp với Khóa học (Lấy lớp đầu tiên còn trống)
                    var lopHoc = db.LopHocs.FirstOrDefault(l => l.MaKhoaHoc == model.MaKhoaHoc);
                    int maLopGan = 0;

                    if (lopHoc != null)
                    {
                        maLopGan = lopHoc.MaLop;
                        // 3. Tạo Đăng Ký Lớp
                        var dangKy = new DangKyLop
                        {
                            MaHocSinh = hocSinh.MaHocSinh,
                            MaLop = maLopGan,
                            NgayDangKy = DateTime.Now,
                            TrangThai = "DaXacNhan"
                        };
                        db.DangKyLops.Add(dangKy);
                    }

                    // 4. Tạo Hóa Đơn (Học Phí) -> Coi như đã thanh toán
                    var khoaHoc = db.KhoaHocs.Find(model.MaKhoaHoc);
                    var hocPhi = new HocPhi
                    {
                        MaHocSinh = hocSinh.MaHocSinh,
                        MaKhoaHoc = model.MaKhoaHoc,
                        SoTien = khoaHoc.HocPhi, // Lấy giá từ DB
                        NgayDong = DateTime.Now,
                        TrangThai = "DaDong" // Set luôn là đã đóng tiền
                    };
                    db.HocPhis.Add(hocPhi);

                    db.SaveChanges();

                    // Thông báo thành công
                    TempData["Success"] = "Đăng ký và thanh toán thành công! Tài khoản của bạn đã được tạo.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Có lỗi xảy ra: " + ex.Message;
                }
            }

            // Nếu lỗi thì load lại dropdown để không bị null
            ViewBag.DanhSachKhoaHoc = new SelectList(db.KhoaHocs, "MaKhoaHoc", "TenKhoaHoc");
            return View("Index", model);
        }
    }
}