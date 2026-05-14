USE QuanLyHocThem;
GO

-- =============================================
-- BƯỚC 1: SỬA CẤU TRÚC BẢNG (Để lưu mật khẩu thường)
-- =============================================
-- Chuyển cột MatKhau sang NVARCHAR để không bị lỗi mã hóa
ALTER TABLE NguoiDung
ALTER COLUMN MatKhau NVARCHAR(100);
GO

-- =============================================
-- BƯỚC 2: XÓA SẠCH DỮ LIỆU CŨ (Tránh lỗi trùng khóa)
-- =============================================
DELETE FROM GuiThongBao;
DELETE FROM ThongBao;
DELETE FROM BaoCao;
DELETE FROM HocPhi;
DELETE FROM Diem;
DELETE FROM DangKyLop;
DELETE FROM LichHoc;
DELETE FROM PhanCongGiaoVien;
DELETE FROM LopHoc;
DELETE FROM KhoaHoc;
DELETE FROM Admin;
DELETE FROM GiaoVien;
DELETE FROM HocSinh;
DELETE FROM PhuHuynh;
DELETE FROM NguoiDung;

-- Reset bộ đếm ID về 0 để dữ liệu mới bắt đầu từ 1
DBCC CHECKIDENT ('NguoiDung', RESEED, 0);
DBCC CHECKIDENT ('KhoaHoc', RESEED, 0);
DBCC CHECKIDENT ('LopHoc', RESEED, 0);
DBCC CHECKIDENT ('ThongBao', RESEED, 0);
GO

-- =============================================
-- BƯỚC 3: CHÈN DỮ LIỆU MẪU MỚI (Mật khẩu: 123...)
-- =============================================

-- 1. Bảng NguoiDung (Mật khẩu không mã hóa)
-- ID sẽ tự tăng: 1=Admin, 2,3=GV, 4,5=PH, 6,7,8=HS
INSERT INTO NguoiDung (HoTen, Email, MatKhau, SoDienThoai, DiaChi, VaiTro)
VALUES
(N'Admin', 'admin@email.com', 'admin123', '0900000001', N'123 Đường Admin, Q1, TPHCM', 'Admin'),
(N'Trần Thị Bích', 'gv.bich@email.com', 'gv123', '0900000002', N'456 Đường Giáo Viên, Q3, TPHCM', 'GiaoVien'),
(N'Lê Minh Cường', 'gv.cuong@email.com', 'gv123', '0900000003', N'789 Đường Giáo Viên, Q5, TPHCM', 'GiaoVien'),
(N'Phạm Thị Diệu', 'ph.dieu@email.com', 'ph123', '0900000004', N'111 Đường Phụ Huynh, Q.Tân Bình, TPHCM', 'PhuHuynh'),
(N'Hoàng Văn E', 'ph.e@email.com', 'ph123', '0900000005', N'222 Đường Phụ Huynh, Q.Gò Vấp, TPHCM', 'PhuHuynh'),
(N'Phạm Gia Hân', 'hs.han@email.com', 'hs123', '0900000006', N'111 Đường Phụ Huynh, Q.Tân Bình, TPHCM', 'HocSinh'),
(N'Hoàng Minh Khôi', 'hs.khoi@email.com', 'hs123', '0900000007', N'222 Đường Phụ Huynh, Q.Gò Vấp, TPHCM', 'HocSinh'),
(N'Hoàng Bảo An', 'hs.an@email.com', 'hs123', '0900000008', N'222 Đường Phụ Huynh, Q.Gò Vấp, TPHCM', 'HocSinh');
GO

-- 2. Bảng Admin
INSERT INTO Admin (MaAdmin, QuyenHan)
VALUES (1, N'Toàn quyền hệ thống');

-- 3. Bảng GiaoVien
INSERT INTO GiaoVien (MaGiaoVien, ChuyenMon)
VALUES
(2, N'Chuyên Toán, Lý'),
(3, N'Chuyên Hóa, Anh Văn');

-- 4. Bảng PhuHuynh
INSERT INTO PhuHuynh (MaPhuHuynh)
VALUES (4), (5);

-- 5. Bảng HocSinh
INSERT INTO HocSinh (MaHocSinh, MaPhuHuynh, NgaySinh, GioiTinh)
VALUES
(6, 4, '2008-05-10', N'Nu'),
(7, 5, '2007-09-20', N'Nam'),
(8, 5, '2009-02-15', N'Nu');

-- 6. Bảng KhoaHoc
INSERT INTO KhoaHoc (TenKhoaHoc, MoTa, HocPhi)
VALUES
(N'Toán Nâng Cao Lớp 10', N'Chương trình bồi dưỡng học sinh giỏi Toán 10', 3000000.00),
(N'Vật Lý Luyện Thi Đại Học Lớp 12', N'Tổng ôn kiến thức và luyện đề Vật Lý 12', 4500000.00),
(N'Hóa Học Cơ Bản Lớp 11', N'Củng cố kiến thức Hóa học 11', 2500000.00),
(N'Tiếng Anh Giao Tiếp (IELTS 5.0+)', N'Tập trung kỹ năng Nghe - Nói', 5000000.00);

-- 7. Bảng LopHoc
INSERT INTO LopHoc (TenLop, MaKhoaHoc, PhongHoc, NgayBatDau, NgayKetThuc)
VALUES
(N'T10.A1', 1, N'P101', '2025-09-01', '2026-05-31'),
(N'L12.A1', 2, N'P102', '2025-08-15', '2026-06-30'),
(N'H11.CB', 3, N'P201', '2025-09-05', '2026-05-15'),
(N'AV.C1', 4, N'P202', '2025-09-10', '2026-01-10'),
(N'T10.A2', 1, N'P103', '2025-09-01', '2026-05-31');

-- 8. Bảng DangKyLop
INSERT INTO DangKyLop (MaHocSinh, MaLop, TrangThai)
VALUES
(6, 1, 'DaXacNhan'),
(7, 2, 'DaXacNhan'),
(8, 3, 'DaXacNhan'),
(6, 4, 'DangCho'),
(7, 3, 'DaXacNhan');

-- 9. Bảng PhanCongGiaoVien
INSERT INTO PhanCongGiaoVien (MaLop, MaGiaoVien, NgayPhanCong)
VALUES
(1, 2, '2025-08-20'),
(2, 2, '2025-08-20'),
(3, 3, '2025-08-21'),
(4, 3, '2025-08-21'),
(5, 2, '2025-08-22');

-- 10. Bảng Diem (Điểm giả lập)
INSERT INTO Diem (MaHocSinh, MaKhoaHoc, DiemGiuaKy, DiemCuoiKy, DiemTongKet, GhiChu)
VALUES
(6, 1, 8.5, 9.0, 8.8, N'Học tốt, cần cẩn thận hơn'),
(7, 2, 7.0, 8.0, 7.7, N'Có tiến bộ'),
(8, 3, 9.0, 9.5, 9.3, N'Xuất sắc'),
(7, 3, 6.5, 7.0, 6.8, N'Cần nắm vững lý thuyết hơn');

-- 11. Bảng LichHoc
INSERT INTO LichHoc (MaLop, ThuTrongTuan, GioBatDau, GioKetThuc)
VALUES
(1, '2', '18:00:00', '20:00:00'),
(1, '4', '18:00:00', '20:00:00'),
(2, '3', '18:30:00', '20:30:00'),
(2, '5', '18:30:00', '20:30:00'),
(3, '7', '08:00:00', '10:00:00'),
(4, 'CN', '14:00:00', '16:00:00');

-- 12. Bảng HocPhi
INSERT INTO HocPhi (MaHocSinh, MaKhoaHoc, SoTien, NgayDong, TrangThai)
VALUES
(6, 1, 3000000.00, '2025-08-25', 'DaDong'),
(7, 2, 4500000.00, '2025-08-10', 'DaDong'),
(8, 3, 2500000.00, '2025-08-30', 'DaDong'),
(7, 3, 2500000.00, '2025-08-30', 'DaDong'),
(6, 4, 5000000.00, NULL, 'ChuaDong');

-- 13. Bảng ThongBao
INSERT INTO ThongBao (TieuDe, NoiDung, MaAdmin)
VALUES
(N'Thông báo nghỉ Lễ', N'Nghỉ lễ 2/9.', 1),
(N'Thông báo khai giảng', N'Khai giảng khóa mới ngày 5/9.', 1),
(N'Lịch thi giữa kỳ', N'Thi từ 15/11 đến 20/11.', 1);

-- 14. Bảng GuiThongBao
INSERT INTO GuiThongBao (MaThongBao, MaNguoiDung)
VALUES
(1, 2), (1, 3), (1, 4), (1, 5), (1, 6), (1, 7), (1, 8),
(2, 2), (2, 3), (2, 4), (2, 5), (2, 6), (2, 7), (2, 8),
(3, 6), (3, 7), (3, 8);

-- 15. Bảng BaoCao
INSERT INTO BaoCao (TieuDe, NoiDung, LoaiBaoCao, MaNguoiTao)
VALUES
(N'Báo cáo doanh thu T9', N'Tổng thu: 150tr.', 'HocPhi', 1),
(N'Báo cáo chất lượng lớp T10', N'Đa số tốt.', 'Diem', 2);

GO
PRINT N'Đã XÓA CŨ và CHÈN MỚI thành công! Đăng nhập ngay: admin@email.com / admin123';

select * from NguoiDung