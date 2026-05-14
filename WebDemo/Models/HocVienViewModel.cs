using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDemo.Models
{
    public class HocVienViewModel
    {
        public int MaHocSinh { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string TenPhuHuynh { get; set; }
    }
}