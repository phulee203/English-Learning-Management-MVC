using System;

namespace WebDemo.Models
{
    public class ThongBaoViewModel
    {
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public DateTime? NgayGui { get; set; }
        public string NguoiGui { get; set; } // Tên Admin gửi
        public bool DaDoc { get; set; }
    }
}