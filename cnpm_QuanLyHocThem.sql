create database QuanLyHocThem

USE QuanLyHocThem;
GO

-- 1. Người dùng
CREATE TABLE NguoiDung (
    MaNguoiDung INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    MatKhau NVARCHAR(255) NOT NULL,
    SoDienThoai NVARCHAR(15),
    DiaChi NVARCHAR(255),
    VaiTro NVARCHAR(20) CHECK (VaiTro IN ('HocSinh','PhuHuynh','GiaoVien','Admin')),
    NgayTao DATETIME DEFAULT GETDATE()
);

-- 2. Học sinh
CREATE TABLE HocSinh (
    MaHocSinh INT PRIMARY KEY,
    MaPhuHuynh INT,
    NgaySinh DATE,
    GioiTinh NVARCHAR(10) CHECK (GioiTinh IN ('Nam','Nu','Khac')),
    FOREIGN KEY (MaHocSinh) REFERENCES NguoiDung(MaNguoiDung) ON DELETE CASCADE,
    FOREIGN KEY (MaPhuHuynh) REFERENCES NguoiDung(MaNguoiDung)
);

-- 3. Phụ huynh
CREATE TABLE PhuHuynh (
    MaPhuHuynh INT PRIMARY KEY,
    FOREIGN KEY (MaPhuHuynh) REFERENCES NguoiDung(MaNguoiDung) ON DELETE CASCADE
);

-- 4. Giáo viên
CREATE TABLE GiaoVien (
    MaGiaoVien INT PRIMARY KEY,
    ChuyenMon NVARCHAR(100),
    FOREIGN KEY (MaGiaoVien) REFERENCES NguoiDung(MaNguoiDung) ON DELETE CASCADE
);

-- 5. Admin
CREATE TABLE Admin (
    MaAdmin INT PRIMARY KEY,
    QuyenHan NVARCHAR(MAX),
    FOREIGN KEY (MaAdmin) REFERENCES NguoiDung(MaNguoiDung) ON DELETE CASCADE
);

-- 6. Khóa học
CREATE TABLE KhoaHoc (
    MaKhoaHoc INT IDENTITY(1,1) PRIMARY KEY,
    TenKhoaHoc NVARCHAR(150) NOT NULL,
    MoTa NVARCHAR(MAX),
    HocPhi DECIMAL(10,2) NOT NULL
);

-- 7. Lớp học
CREATE TABLE LopHoc (
    MaLop INT IDENTITY(1,1) PRIMARY KEY,
    TenLop NVARCHAR(50) NOT NULL,
    MaKhoaHoc INT,
    PhongHoc NVARCHAR(20),
    NgayBatDau DATE,
    NgayKetThuc DATE,
    FOREIGN KEY (MaKhoaHoc) REFERENCES KhoaHoc(MaKhoaHoc)
);

-- 8. Đăng ký lớp
CREATE TABLE DangKyLop (
    MaHocSinh INT,
    MaLop INT,
    NgayDangKy DATETIME DEFAULT GETDATE(),
    TrangThai NVARCHAR(20) CHECK (TrangThai IN ('DangCho','DaXacNhan','DaHuy')) DEFAULT 'DangCho',
    PRIMARY KEY (MaHocSinh, MaLop),
    FOREIGN KEY (MaHocSinh) REFERENCES HocSinh(MaHocSinh),
    FOREIGN KEY (MaLop) REFERENCES LopHoc(MaLop)
);

-- 9. Phân công giáo viên
CREATE TABLE PhanCongGiaoVien (
    MaPhanCong INT IDENTITY(1,1) PRIMARY KEY,
    MaLop INT,
    MaGiaoVien INT,
    NgayPhanCong DATE,
    FOREIGN KEY (MaLop) REFERENCES LopHoc(MaLop),
    FOREIGN KEY (MaGiaoVien) REFERENCES GiaoVien(MaGiaoVien)
);

-- 10. Điểm
CREATE TABLE Diem (
    MaDiem INT IDENTITY(1,1) PRIMARY KEY,
    MaHocSinh INT,
    MaKhoaHoc INT,
    DiemGiuaKy DECIMAL(4,2),
    DiemCuoiKy DECIMAL(4,2),
    DiemTongKet DECIMAL(4,2),
    GhiChu NVARCHAR(MAX),
    NgayCapNhat DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaHocSinh) REFERENCES HocSinh(MaHocSinh),
    FOREIGN KEY (MaKhoaHoc) REFERENCES KhoaHoc(MaKhoaHoc)
);

-- 11. Lịch học
CREATE TABLE LichHoc (
    MaLich INT IDENTITY(1,1) PRIMARY KEY,
    MaLop INT,
    ThuTrongTuan NVARCHAR(5) CHECK (ThuTrongTuan IN ('2','3','4','5','6','7','CN')),
    GioBatDau TIME,
    GioKetThuc TIME,
    FOREIGN KEY (MaLop) REFERENCES LopHoc(MaLop)
);

-- 12. Học phí
CREATE TABLE HocPhi (
    MaHocPhi INT IDENTITY(1,1) PRIMARY KEY,
    MaHocSinh INT,
    MaKhoaHoc INT,
    SoTien DECIMAL(10,2) NOT NULL,
    NgayDong DATE,
    TrangThai NVARCHAR(20) CHECK (TrangThai IN ('ChuaDong','DaDong','QuaHan')) DEFAULT 'ChuaDong',
    FOREIGN KEY (MaHocSinh) REFERENCES HocSinh(MaHocSinh),
    FOREIGN KEY (MaKhoaHoc) REFERENCES KhoaHoc(MaKhoaHoc)
);

-- 13. Thông báo
CREATE TABLE ThongBao (
    MaThongBao INT IDENTITY(1,1) PRIMARY KEY,
    TieuDe NVARCHAR(200) NOT NULL,
    NoiDung NVARCHAR(MAX) NOT NULL,
    MaAdmin INT,
    NgayGui DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaAdmin) REFERENCES Admin(MaAdmin)
);

-- 14. Gửi thông báo
CREATE TABLE GuiThongBao (
    MaThongBao INT,
    MaNguoiDung INT,
    DaDoc BIT DEFAULT 0,
    PRIMARY KEY (MaThongBao, MaNguoiDung),
    FOREIGN KEY (MaThongBao) REFERENCES ThongBao(MaThongBao),
    FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung)
);

-- 15. Báo cáo
CREATE TABLE BaoCao (
    MaBaoCao INT IDENTITY(1,1) PRIMARY KEY,
    TieuDe NVARCHAR(200) NOT NULL,
    NoiDung NVARCHAR(MAX),
    LoaiBaoCao NVARCHAR(20) CHECK (LoaiBaoCao IN ('Diem','HocPhi','LopHoc','Khac')),
    MaNguoiTao INT,
    NgayTao DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (MaNguoiTao) REFERENCES NguoiDung(MaNguoiDung)
);
GO
