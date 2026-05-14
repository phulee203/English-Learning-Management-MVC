using System;
using System.ComponentModel.DataAnnotations;

namespace WebDemo.Models
{
    public class DangKyViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        // --- THÊM MỚI: Mật khẩu ---
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        public string MatKhau { get; set; }

        public string SoDienThoai { get; set; }
        public string DiaChi { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn khóa học")]
        public int MaKhoaHoc { get; set; }

        public string PhuongThucThanhToan { get; set; }
        public bool GhiNho { get; set; }
    }
}