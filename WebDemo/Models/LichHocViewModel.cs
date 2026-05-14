using System;

namespace WebDemo.Models
{
    public class LichHocViewModel
    {
        public string TenLop { get; set; }
        public string TenMon { get; set; }
        public string PhongHoc { get; set; }
        public string GiaoVien { get; set; }
        public string Thu { get; set; }
        public TimeSpan? GioBatDau { get; set; }
        public TimeSpan? GioKetThuc { get; set; }
    }
}