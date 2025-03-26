using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.ViewModel
{
    public class ViewModelDonHangOnline
    {
        private readonly QlnhaHangBtlContext _qlnhaHangBtlContext;
        public ViewModelDonHangOnline(QlnhaHangBtlContext qlnhaHangBtlContext) { 
            _qlnhaHangBtlContext = qlnhaHangBtlContext;
        }

        public List<DonHang> ListDonHang() // Liệt kê ra những đơn hàng chưa được sử lý
        {
            // Lọc ra những đơn hàng đã đặt mà chưa được vận chuyển(TrangThai == true : Đặt Hàng thành công ; VanChuyen == false : đơn hàng chưa được vận chuyển)
            var result = _qlnhaHangBtlContext.DonHangs.Where(e => e.TrangThai == true && e.VanChuyen == false && e.KhId != null);
            return result.ToList();
        }
        public DonHang_KhachHang? FindDonHangByID(int DonHangID)
        {
            var result = from dh in _qlnhaHangBtlContext.DonHangs
                         join kh in _qlnhaHangBtlContext.KhachHangs
                         on dh.KhId equals kh.KhId
                         where dh.DhId == DonHangID
                         select new DonHang_KhachHang
                         {
                             DhId = dh.DhId,
                             DiaChi = kh.DiaChi,
                             SoDienThoai = kh.SoDienThoai,
                             TenKhachHang = kh.TenKhachHang,
                             TongTien = dh.TongTien
                         };

            return result.FirstOrDefault(); // Lấy đối tượng đầu tiên hoặc `null` nếu không tìm thấy
        }

    }
}
