using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebDemo.Models;

namespace WebDemo.Controllers
{
    public class AccountController : Controller
    {
        QuanLyHocThemEntities db = new QuanLyHocThemEntities();

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            if (ModelState.IsValid)
            {
                // Tìm user trong CSDL
                var user = db.NguoiDungs.FirstOrDefault(u => (u.Email == email || u.SoDienThoai == email) && u.MatKhau == password);

                if (user != null)
                {
                    // Lưu thông tin vào Session
                    Session["UserID"] = user.MaNguoiDung;
                    Session["UserName"] = user.HoTen;
                    Session["Role"] = user.VaiTro;

                    // Điều hướng dựa trên Vai Trò 
                    if (user.VaiTro == "Admin")
                    {
                        return RedirectToAction("Index", "Admin"); // Vào trang quản lý lớp
                    }
                    else if (user.VaiTro == "GiaoVien")
                    {
                        return RedirectToAction("LopCuaToi", "GiaoVien"); // Vào danh sách lớp dạy
                    }
                    else if (user.VaiTro == "HocSinh" || user.VaiTro == "PhuHuynh")
                    {
                        return RedirectToAction("KetQuaHocTap", "HocSinh"); // Vào xem điểm
                    }
                }
                else
                {
                    ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
                }
            }
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Kiểm tra Email đã tồn tại chưa
                var checkEmail = db.NguoiDungs.FirstOrDefault(u => u.Email == model.Email);
                if (checkEmail != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    return View(model);
                }

                // 2. Tạo User mới (Mặc định là HocSinh)
                var user = new NguoiDung
                {
                    HoTen = model.HoTen,
                    Email = model.Email,
                    SoDienThoai = model.SoDienThoai,
                    MatKhau = model.MatKhau, // Lưu ý: Thực tế nên mã hóa MD5/BCrypt
                    VaiTro = "HocSinh",
                    NgayTao = DateTime.Now
                };

                db.NguoiDungs.Add(user);
                db.SaveChanges(); // Lưu để lấy ID tự tăng

                // 3. Tạo dữ liệu bảng HocSinh tương ứng
                var hocSinh = new HocSinh
                {
                    MaHocSinh = user.MaNguoiDung,
                    NgaySinh = null // Để null hoặc update sau
                };
                db.HocSinhs.Add(hocSinh);
                db.SaveChanges();

                // 4. Thông báo và chuyển về trang đăng nhập
                TempData["Success"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        public ActionResult Logout()
        {
            Session.Clear(); // Xóa session
            return RedirectToAction("Login");
        }
    }
}