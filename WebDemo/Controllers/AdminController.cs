using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebDemo.Models;
using System.Data.Entity.Migrations; // Thêm thư viện này để cập nhật

namespace WebDemo.Controllers
{
    public class AdminController : Controller
    {
        QuanLyHocThemEntities db = new QuanLyHocThemEntities();

        #region 1. QUẢN LÝ LỚP HỌC (INDEX)

        public ActionResult Index()
        {
            ViewBag.DanhSachKhoaHoc = new SelectList(db.KhoaHocs, "MaKhoaHoc", "TenKhoaHoc");
            var dsGiaoVien = db.NguoiDungs.Where(nd => nd.VaiTro == "GiaoVien")
                                         .Select(nd => new { nd.MaNguoiDung, nd.HoTen })
                                         .ToList();
            ViewBag.DanhSachGiaoVien = new SelectList(dsGiaoVien, "MaNguoiDung", "HoTen");
            var dsLopHoc = (from lop in db.LopHocs
                            join kh in db.KhoaHocs on lop.MaKhoaHoc equals kh.MaKhoaHoc
                            let gvPhuTrach = (from pc in db.PhanCongGiaoViens
                                              join gv in db.GiaoViens on pc.MaGiaoVien equals gv.MaGiaoVien
                                              join nd in db.NguoiDungs on gv.MaGiaoVien equals nd.MaNguoiDung
                                              where pc.MaLop == lop.MaLop
                                              select nd.HoTen).FirstOrDefault()
                            select new LopHocViewModel
                            {
                                MaLop = lop.MaLop,
                                TenLop = lop.TenLop,
                                TenKhoaHoc = kh.TenKhoaHoc,
                                LichHoc = lop.PhongHoc,
                                TenGiaoVien = gvPhuTrach ?? "(Chưa phân công)",
                                SiSoHienTai = db.DangKyLops.Count(dk => dk.MaLop == lop.MaLop && dk.TrangThai == "DaXacNhan")
                            }).ToList();
            return View(dsLopHoc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TaoLopMoi(LopHoc lopHoc)
        {
            if (ModelState.IsValid)
            {
                db.LopHocs.Add(lopHoc);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult XoaLop(int id)
        {
            try
            {
                var dsDangKy = db.DangKyLops.Where(d => d.MaLop == id);
                db.DangKyLops.RemoveRange(dsDangKy);
                var dsPhanCong = db.PhanCongGiaoViens.Where(p => p.MaLop == id);
                db.PhanCongGiaoViens.RemoveRange(dsPhanCong);
                LopHoc lop = db.LopHocs.Find(id);
                if (lop != null)
                {
                    db.LopHocs.Remove(lop);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Xóa lớp thành công." });
                }
                return Json(new { success = false, message = "Không tìm thấy lớp." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Xóa thất bại: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult PhanCongGiaoVien(int maLop, int maGiaoVien)
        {
            try
            {
                var phanCongCu = db.PhanCongGiaoViens.FirstOrDefault(p => p.MaLop == maLop);
                if (phanCongCu != null)
                {
                    phanCongCu.MaGiaoVien = maGiaoVien;
                    phanCongCu.NgayPhanCong = DateTime.Now;
                }
                else
                {
                    PhanCongGiaoVien phanCongMoi = new PhanCongGiaoVien { MaLop = maLop, MaGiaoVien = maGiaoVien, NgayPhanCong = DateTime.Now };
                    db.PhanCongGiaoViens.Add(phanCongMoi);
                }
                db.SaveChanges();
                var tenGiaoVienMoi = db.NguoiDungs.Find(maGiaoVien).HoTen;
                return Json(new { success = true, message = "Phân công thành công!", tenGv = tenGiaoVienMoi });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // GET: Admin/ChiTietLopHoc/5
        public ActionResult ChiTietLopHoc(int id)
        {
            LopHoc lop = db.LopHocs.Find(id);
            if (lop == null) return HttpNotFound();
            KhoaHoc kh = db.KhoaHocs.Find(lop.MaKhoaHoc);
            var gv = (from pc in db.PhanCongGiaoViens
                      join nd in db.NguoiDungs on pc.MaGiaoVien equals nd.MaNguoiDung
                      where pc.MaLop == id
                      select nd.HoTen).FirstOrDefault();
            var dsDaCo = (from dk in db.DangKyLops
                          join hs in db.HocSinhs on dk.MaHocSinh equals hs.MaHocSinh
                          join nd in db.NguoiDungs on hs.MaHocSinh equals nd.MaNguoiDung
                          where dk.MaLop == id && dk.TrangThai == "DaXacNhan"
                          select new HocSinhTrongLop
                          {
                              MaHocSinh = hs.MaHocSinh,
                              HoTen = nd.HoTen,
                              Email = nd.Email,
                              NgayDangKy = (DateTime)dk.NgayDangKy
                          }).ToList();
            var idDaCo = dsDaCo.Select(hs => hs.MaHocSinh).ToList();
            var dsChuaCo = db.NguoiDungs
                .Where(nd => nd.VaiTro == "HocSinh" && !idDaCo.Contains(nd.MaNguoiDung))
                .Select(nd => new { nd.MaNguoiDung, nd.HoTen })
                .ToList();
            var viewModel = new ChiTietLopHocViewModel
            {
                LopHoc = lop,
                KhoaHoc = kh,
                TenGiaoVien = gv ?? "(Chưa phân công)",
                DSHocSinhDaCo = dsDaCo,
                DSHocSinhChuaCo = new SelectList(dsChuaCo, "MaNguoiDung", "HoTen")
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemHocSinhVaoLop(int maLop, int maHocSinhCanThem)
        {
            if (maHocSinhCanThem > 0)
            {
                // 1. Kiểm tra xem đã học lớp này chưa
                bool daTonTai = db.DangKyLops.Any(dk => dk.MaLop == maLop && dk.MaHocSinh == maHocSinhCanThem);

                if (!daTonTai)
                {
                    // A. Thêm vào bảng Đăng Ký Lớp (Giữ nguyên)
                    DangKyLop dangKyMoi = new DangKyLop
                    {
                        MaHocSinh = maHocSinhCanThem,
                        MaLop = maLop,
                        NgayDangKy = DateTime.Now,
                        TrangThai = "DaXacNhan"
                    };
                    db.DangKyLops.Add(dangKyMoi);

                    // B. --- CODE MỚI: TỰ ĐỘNG TẠO HÓA ĐƠN ---
                    // Lấy thông tin lớp để biết thuộc khóa học nào
                    var lopHoc = db.LopHocs.Find(maLop);
                    if (lopHoc != null)
                    {
                        // Kiểm tra xem học sinh này đã có hóa đơn cho khóa học này chưa (tránh trùng)
                        bool daCoHoaDon = db.HocPhis.Any(hp => hp.MaHocSinh == maHocSinhCanThem && hp.MaKhoaHoc == lopHoc.MaKhoaHoc);

                        if (!daCoHoaDon)
                        {
                            var khoaHoc = db.KhoaHocs.Find(lopHoc.MaKhoaHoc);
                            HocPhi hp = new HocPhi
                            {
                                MaHocSinh = maHocSinhCanThem,
                                MaKhoaHoc = lopHoc.MaKhoaHoc,
                                SoTien = khoaHoc.HocPhi, // Lấy giá tiền từ khóa học
                                TrangThai = "ChuaDong",  // Mặc định chưa đóng
                                NgayDong = null
                            };
                            db.HocPhis.Add(hp);
                        }
                    }
                    // ----------------------------------------

                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Thêm học sinh vào lớp và tạo hóa đơn thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Học sinh này đã có trong lớp rồi!";
                }
            }
            return RedirectToAction("ChiTietLopHoc", new { id = maLop });
        }

        [HttpPost]
        public ActionResult XoaHocSinhKhoiLop(int maHocSinh, int maLop)
        {
            try
            {
                DangKyLop dangKy = db.DangKyLops.Find(maHocSinh, maLop);
                if (dangKy != null)
                {
                    db.DangKyLops.Remove(dangKy);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Đã xóa học sinh khỏi lớp." });
                }
                return Json(new { success = false, message = "Không tìm thấy đăng ký." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // TÍNH NĂNG MỚI: SỬA LỚP
        [HttpGet]
        public ActionResult SuaLop(int id)
        {
            var lop = db.LopHocs.Find(id);
            if (lop == null) return HttpNotFound();
            return Json(lop, JsonRequestBehavior.AllowGet); // Trả về data cho Modal
        }

        [HttpPost]
        public ActionResult SuaLop(LopHoc lop)
        {
            if (ModelState.IsValid)
            {
                db.LopHocs.AddOrUpdate(lop); // Cập nhật hoặc Thêm
                db.SaveChanges();
                return Json(new { success = true, message = "Cập nhật thành công!" });
            }
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
        }
        #endregion

        #region 2. QUẢN LÝ GIÁO VIÊN

        public ActionResult QuanLyGiaoVien()
        {
            var dsGiaoVien = (from gv in db.GiaoViens
                              join nd in db.NguoiDungs on gv.MaGiaoVien equals nd.MaNguoiDung
                              where nd.VaiTro == "GiaoVien"
                              select new GiaoVienViewModel
                              {
                                  MaGiaoVien = gv.MaGiaoVien,
                                  HoTen = nd.HoTen,
                                  Email = nd.Email,
                                  SoDienThoai = nd.SoDienThoai,
                                  ChuyenMon = gv.ChuyenMon
                              }).ToList();
            return View(dsGiaoVien);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemGiaoVien(NguoiDung nguoiDung, GiaoVien giaoVien)
        {
            try
            {
                nguoiDung.MatKhau = "123456";
                nguoiDung.VaiTro = "GiaoVien";
                nguoiDung.NgayTao = DateTime.Now;
                db.NguoiDungs.Add(nguoiDung);
                db.SaveChanges();
                giaoVien.MaGiaoVien = nguoiDung.MaNguoiDung;
                db.GiaoViens.Add(giaoVien);
                db.SaveChanges();
            }
            catch (Exception) { }
            return RedirectToAction("QuanLyGiaoVien");
        }

        [HttpPost]
        public ActionResult XoaGiaoVien(int id)
        {
            try
            {
                var dsPhanCong = db.PhanCongGiaoViens.Where(p => p.MaGiaoVien == id);
                db.PhanCongGiaoViens.RemoveRange(dsPhanCong);
                GiaoVien gv = db.GiaoViens.Find(id);
                NguoiDung nd = db.NguoiDungs.Find(id);
                if (gv != null) db.GiaoViens.Remove(gv);
                if (nd != null) db.NguoiDungs.Remove(nd);
                db.SaveChanges();
                return Json(new { success = true, message = "Xóa giáo viên thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Xóa thất bại: " + ex.Message });
            }
        }

        // TÍNH NĂNG MỚI: SỬA GIÁO VIÊN
        [HttpGet]
        public ActionResult SuaGiaoVien(int id)
        {
            // Lấy data từ cả 2 bảng
            var gv = db.GiaoViens.Find(id);
            var nd = db.NguoiDungs.Find(id);
            if (gv == null || nd == null) return HttpNotFound();

            // Gộp data lại để trả về
            var result = new
            {
                MaGiaoVien = gv.MaGiaoVien,
                HoTen = nd.HoTen,
                Email = nd.Email,
                SoDienThoai = nd.SoDienThoai,
                DiaChi = nd.DiaChi,
                ChuyenMon = gv.ChuyenMon
            };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SuaGiaoVien(NguoiDung nguoiDung, GiaoVien giaoVien)
        {
            // Lấy MaGiaoVien từ GiaoVien
            int id = giaoVien.MaGiaoVien;

            // 1. Cập nhật bảng NguoiDung
            var nd_db = db.NguoiDungs.Find(id);
            if (nd_db == null) return Json(new { success = false, message = "Không tìm thấy người dùng." });

            nd_db.HoTen = nguoiDung.HoTen;
            nd_db.Email = nguoiDung.Email;
            nd_db.SoDienThoai = nguoiDung.SoDienThoai;
            nd_db.DiaChi = nguoiDung.DiaChi;

            // 2. Cập nhật bảng GiaoVien
            var gv_db = db.GiaoViens.Find(id);
            if (gv_db == null) return Json(new { success = false, message = "Không tìm thấy giáo viên." });

            gv_db.ChuyenMon = giaoVien.ChuyenMon;

            db.SaveChanges();
            return Json(new { success = true, message = "Cập nhật thông tin giáo viên thành công!" });
        }
        #endregion

        #region 3. QUẢN LÝ HỌC VIÊN

        public ActionResult QuanLyHocVien()
        {
            var dsHocVien = (from hs in db.HocSinhs
                             join nd in db.NguoiDungs on hs.MaHocSinh equals nd.MaNguoiDung
                             let tenPH = (from ph in db.PhuHuynhs
                                          join nd_ph in db.NguoiDungs on ph.MaPhuHuynh equals nd_ph.MaNguoiDung
                                          where hs.MaPhuHuynh == ph.MaPhuHuynh
                                          select nd_ph.HoTen).FirstOrDefault()
                             where nd.VaiTro == "HocSinh"
                             select new HocVienViewModel
                             {
                                 MaHocSinh = hs.MaHocSinh,
                                 HoTen = nd.HoTen,
                                 Email = nd.Email,
                                 NgaySinh = hs.NgaySinh,
                                 TenPhuHuynh = tenPH ?? "(Chưa có)"
                             }).ToList();
            return View(dsHocVien);
        }

        public ActionResult ThemHocVien()
        {
            var dsPhuHuynh = db.NguoiDungs.Where(nd => nd.VaiTro == "PhuHuynh")
                                         .Select(nd => new { nd.MaNguoiDung, nd.HoTen })
                                         .ToList();
            ViewBag.DanhSachPhuHuynh = new SelectList(dsPhuHuynh, "MaNguoiDung", "HoTen");

            ViewBag.DanhSachKhoaHoc = new SelectList(db.KhoaHocs, "MaKhoaHoc", "TenKhoaHoc");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemHocVien(NguoiDung nguoiDung, HocSinh hocSinh, int? MaKhoaHoc)
        {
            try
            {
                nguoiDung.MatKhau = "123456";
                nguoiDung.VaiTro = "HocSinh";
                nguoiDung.NgayTao = DateTime.Now;
                db.NguoiDungs.Add(nguoiDung);
                db.SaveChanges();
                hocSinh.MaHocSinh = nguoiDung.MaNguoiDung;
                db.HocSinhs.Add(hocSinh);
                db.SaveChanges();
                if (MaKhoaHoc.HasValue)
                {
                    var khoaHoc = db.KhoaHocs.Find(MaKhoaHoc);
                    if (khoaHoc != null)
                    {
                        HocPhi hp = new HocPhi();
                        hp.MaHocSinh = hocSinh.MaHocSinh;
                        hp.MaKhoaHoc = MaKhoaHoc.Value;
                        hp.SoTien = khoaHoc.HocPhi; // Lấy giá tiền từ khóa học
                        hp.TrangThai = "ChuaDong";  // Mặc định là chưa đóng để Admin thu sau
                        hp.NgayDong = null;

                        db.HocPhis.Add(hp);
                        db.SaveChanges();
                    }
                }

                TempData["SuccessMessage"] = "Thêm học viên và tạo hóa đơn thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Thêm thất bại: " + ex.Message;

                // Load lại ViewBag nếu lỗi
                var dsPhuHuynh = db.NguoiDungs.Where(nd => nd.VaiTro == "PhuHuynh")
                                         .Select(nd => new { nd.MaNguoiDung, nd.HoTen })
                                         .ToList();
                ViewBag.DanhSachPhuHuynh = new SelectList(dsPhuHuynh, "MaNguoiDung", "HoTen", hocSinh.MaPhuHuynh);
                ViewBag.DanhSachKhoaHoc = new SelectList(db.KhoaHocs, "MaKhoaHoc", "TenKhoaHoc"); // Load lại khóa học

                return View(hocSinh);
            }
            return RedirectToAction("QuanLyHocVien");
        }

        [HttpPost]
        public ActionResult XoaHocVien(int id)
        {
            try
            {
                var dsDiem = db.Diems.Where(d => d.MaHocSinh == id);
                db.Diems.RemoveRange(dsDiem);
                var dsDangKy = db.DangKyLops.Where(d => d.MaHocSinh == id);
                db.DangKyLops.RemoveRange(dsDangKy);
                var dsHocPhi = db.HocPhis.Where(h => h.MaHocSinh == id);
                db.HocPhis.RemoveRange(dsHocPhi);
                HocSinh hs = db.HocSinhs.Find(id);
                NguoiDung nd = db.NguoiDungs.Find(id);
                if (hs != null) db.HocSinhs.Remove(hs);
                if (nd != null) db.NguoiDungs.Remove(nd);
                db.SaveChanges();
                return Json(new { success = true, message = "Xóa học viên thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Xóa thất bại: " + ex.Message });
            }
        }

        // TÍNH NĂNG MỚI: SỬA HỌC VIÊN
        [HttpGet]
        public ActionResult SuaHocVien(int id)
        {
            var hs = db.HocSinhs.Find(id);
            var nd = db.NguoiDungs.Find(id);
            if (hs == null || nd == null) return HttpNotFound();

            var dsPhuHuynh = db.NguoiDungs.Where(n => n.VaiTro == "PhuHuynh")
                                         .Select(n => new { n.MaNguoiDung, n.HoTen })
                                         .ToList();
            ViewBag.DanhSachPhuHuynh = new SelectList(dsPhuHuynh, "MaNguoiDung", "HoTen", hs.MaPhuHuynh);

            var result = new
            {
                // Bảng NguoiDung
                MaHocSinh = nd.MaNguoiDung,
                HoTen = nd.HoTen,
                Email = nd.Email,
                SoDienThoai = nd.SoDienThoai,
                DiaChi = nd.DiaChi,
                // Bảng HocSinh
                NgaySinh = hs.NgaySinh.HasValue ? hs.NgaySinh.Value.ToString("yyyy-MM-dd") : "",
                GioiTinh = hs.GioiTinh,
                MaPhuHuynh = hs.MaPhuHuynh
            };
            return Json(new { data = result, phuHuynhList = dsPhuHuynh }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SuaHocVien(NguoiDung nguoiDung, HocSinh hocSinh)
        {
            int id = hocSinh.MaHocSinh; // Lấy ID từ HocSinh

            // 1. Cập nhật NguoiDung
            var nd_db = db.NguoiDungs.Find(id);
            if (nd_db == null) return Json(new { success = false, message = "Không tìm thấy người dùng." });
            nd_db.HoTen = nguoiDung.HoTen;
            nd_db.Email = nguoiDung.Email;
            nd_db.SoDienThoai = nguoiDung.SoDienThoai;
            nd_db.DiaChi = nguoiDung.DiaChi;

            // 2. Cập nhật HocSinh
            var hs_db = db.HocSinhs.Find(id);
            if (hs_db == null) return Json(new { success = false, message = "Không tìm thấy học sinh." });
            hs_db.NgaySinh = hocSinh.NgaySinh;
            hs_db.GioiTinh = hocSinh.GioiTinh;
            hs_db.MaPhuHuynh = hocSinh.MaPhuHuynh;

            db.SaveChanges();
            return Json(new { success = true, message = "Cập nhật thông tin học viên thành công!" });
        }
        #endregion

        #region 4. TẠO BÁO CÁO

        public ActionResult TaoBaoCao()
        {
            return View();
        }

        [HttpPost]
        public ActionResult XuatBaoCaoHocPhi(DateTime? tuNgay, DateTime? denNgay)
        {
            if (!tuNgay.HasValue || !denNgay.HasValue)
            {
                TempData["BaoCaoError"] = "Vui lòng chọn cả ngày bắt đầu và kết thúc.";
                return RedirectToAction("TaoBaoCao");
            }
            var baoCao = db.HocPhis
                .Where(hp => hp.TrangThai == "DaDong" && hp.NgayDong >= tuNgay && hp.NgayDong <= denNgay)
                .Include(hp => hp.HocSinh.NguoiDung)
                .Include(hp => hp.KhoaHoc)
                .OrderByDescending(hp => hp.NgayDong)
                .ToList();
            ViewBag.TuNgay = tuNgay.Value.ToString("dd/MM/yyyy");
            ViewBag.DenNgay = denNgay.Value.ToString("dd/MM/yyyy");
            return View("KetQuaBaoCaoHocPhi", baoCao);
        }
        #endregion

        #region 5. GỬI THÔNG BÁO

        public ActionResult GuiThongBao()
        {
            var dsNguoiDung = db.NguoiDungs.Where(nd => nd.VaiTro != "Admin")
                                          .Select(nd => new { nd.MaNguoiDung, HoTenVaiTro = nd.HoTen + " (" + nd.VaiTro + ")" })
                                          .ToList();
            ViewBag.DanhSachNguoiDung = new MultiSelectList(dsNguoiDung, "MaNguoiDung", "HoTenVaiTro");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuiThongBao(ThongBao thongBao, int[] dsNguoiNhan)
        {
            try
            {
                thongBao.MaAdmin = 1;
                thongBao.NgayGui = DateTime.Now;
                db.ThongBaos.Add(thongBao);
                db.SaveChanges();
                if (dsNguoiNhan != null)
                {
                    foreach (int maND in dsNguoiNhan)
                    {
                        GuiThongBao gtb = new GuiThongBao { MaThongBao = thongBao.MaThongBao, MaNguoiDung = maND, DaDoc = false };
                        db.GuiThongBaos.Add(gtb);
                    }
                    db.SaveChanges();
                }
                TempData["GuiTB_Success"] = "Gửi thông báo thành công!";
            }
            catch (Exception ex)
            {
                TempData["GuiTB_Error"] = "Gửi thất bại: " + ex.Message;
            }
            return RedirectToAction("GuiThongBao");
        }
        #endregion

        #region 6. QUẢN LÝ HỌC PHÍ (Thu tiền)

        // Hiển thị danh sách học phí của tất cả học viên
        public ActionResult QuanLyHocPhi(string searchString)
        {
            var dsHocPhi = db.HocPhis.Include(h => h.HocSinh.NguoiDung).Include(h => h.KhoaHoc);

            // Tìm kiếm theo tên hoặc mã HS
            if (!String.IsNullOrEmpty(searchString))
            {
                dsHocPhi = dsHocPhi.Where(h => h.HocSinh.NguoiDung.HoTen.Contains(searchString)
                                            || h.HocSinh.NguoiDung.SoDienThoai.Contains(searchString));
            }

            return View(dsHocPhi.OrderByDescending(h => h.NgayDong).ToList());
        }

        // Xác nhận thu tiền (AJAX)
        [HttpPost]
        public ActionResult XacNhanThuTien(int maHocPhi)
        {
            try
            {
                var hp = db.HocPhis.Find(maHocPhi);
                if (hp != null)
                {
                    hp.TrangThai = "DaDong";
                    hp.NgayDong = DateTime.Now;
                    // hp.NguoiThu = Session["UserName"].ToString(); // Nếu DB có cột Người thu
                    db.SaveChanges();
                    return Json(new { success = true, message = "Đã cập nhật trạng thái: Đã đóng tiền!" });
                }
                return Json(new { success = false, message = "Không tìm thấy thông tin học phí." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        #endregion

        #region HÀM HỖ TRỢ (Dọn dẹp)
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}