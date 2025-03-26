using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.ViewModel
{
    public class DoanhThu_LoiNhan
    {
        private readonly QlnhaHangBtlContext _qlnhaHangBtlContext;
        public DoanhThu_LoiNhan(QlnhaHangBtlContext qlnhaHangBtlContext)
        {
            _qlnhaHangBtlContext = qlnhaHangBtlContext;
        }

        public double DoanhThu1TuanTheoGioRa()
        {
            // Lấy ngày hiện tại
            var ngayHienTai = DateTime.Now;

            // Tính tổng doanh thu trong 7 ngày gần nhất dựa vào GioRa và chuyển đổi kiểu dữ liệu
            var doanhThuTuan = _qlnhaHangBtlContext.DonHangs
                .Where(dh => dh.GioRa != null && dh.GioRa >= ngayHienTai.AddDays(-7) && dh.GioRa <= ngayHienTai)
                .Sum(dh => (double)(dh.TongTien ?? 0)); // Ép kiểu TongTien từ decimal? sang double

            return doanhThuTuan;
        }

        public double DoanhThu1ThangTheoGioRa()
        {
            // Lấy ngày hiện tại
            var ngayHienTai = DateTime.Now;

            // Tính tổng doanh thu trong 30 ngày gần nhất dựa vào GioRa và chuyển đổi kiểu dữ liệu
            var doanhThuThang = _qlnhaHangBtlContext.DonHangs
                .Where(dh => dh.GioRa != null && dh.GioRa >= ngayHienTai.AddDays(-30) && dh.GioRa <= ngayHienTai)
                .Sum(dh => (double)(dh.TongTien ?? 0)); // Ép kiểu TongTien từ decimal? sang double

            return doanhThuThang;
        }


        public int DemSoLuongDonHangCoBan()
        {
            // Đếm số lượng đơn hàng có ID bàn khác NULL
            var soLuongDonHang = _qlnhaHangBtlContext.DonHangs
                .Count(e => e.TrangThai == false && e.BanId != null); // Đơn Hàng có trạng Thái Bằng false thì là chưa thanh toán Và đơn Hàng đó là tại quán

            return soLuongDonHang;
        }
        public int DemSoLuongDonHangOnline()
        {
            // Đếm số lượng đơn hàng có ID bàn khác NULL
            var soLuongDonHang = _qlnhaHangBtlContext.DonHangs
                .Count(e => e.TrangThai == true && e.VanChuyen==false && e.BanId == null); // Đơn Hàng có trạng Thái true và vận chuyển false là chưa xử lý

            return soLuongDonHang;
        }


    }
}
