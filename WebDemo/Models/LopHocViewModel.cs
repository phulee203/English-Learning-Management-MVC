using System;
using System.ComponentModel.DataAnnotations;

namespace WebDemo.Models // Hoặc WebDemo.ViewModels nếu bạn tạo thư mục riêng
{
    public class LopHocViewModel
    {
        public int MaLop { get; set; }
        public string TenLop { get; set; }
        public string TenKhoaHoc { get; set; }
        public string TenGiaoVien { get; set; }
        public string LichHoc { get; set; } // Bạn có thể thêm cột LichHoc vào bảng LopHoc
        public int SiSoHienTai { get; set; }
        public int SiSoToiDa { get; set; } = 12; // Mặc định là 12
    }
}