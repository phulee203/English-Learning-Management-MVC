using System.Collections.Generic;
using System.Web.Mvc;
using System; // Thêm thư viện này

namespace WebDemo.Models
{
    // Dùng để hiển thị 1 học sinh trong danh sách lớp
    public class HocSinhTrongLop
    {
        public int MaHocSinh { get; set; } // Chúng ta sẽ dùng ID này
        public string HoTen { get; set; }
        public string Email { get; set; }
        public DateTime NgayDangKy { get; set; }
        // BỎ MaDangKy ĐI
    }

    // ViewModel chính cho trang
    public class ChiTietLopHocViewModel
    {
        public LopHoc LopHoc { get; set; }
        public KhoaHoc KhoaHoc { get; set; }
        public string TenGiaoVien { get; set; }
        public List<HocSinhTrongLop> DSHocSinhDaCo { get; set; }

        public SelectList DSHocSinhChuaCo { get; set; }

        public int MaHocSinhCanThem { get; set; }
    }
}