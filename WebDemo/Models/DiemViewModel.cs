using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDemo.Models
{
    public class DiemViewModel
    {
        public int MaHocSinh { get; set; }
        public string HoTen { get; set; }

        public decimal? DiemGiuaKy { get; set; }
        public decimal? DiemCuoiKy { get; set; }

        public string NhanXet { get; set; }
    }
}