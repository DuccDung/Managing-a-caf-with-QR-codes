using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.ViewModel
{
    public class ViewModelBan
    {
        private readonly QlnhaHangBtlContext _context;
        public ViewModelBan(QlnhaHangBtlContext context) {
           _context = context;
        }
        public List<Ban> RenBan()
        {
            return _context.Bans.ToList();
        }
        public int DonHangByBan(int id) // từ id Bàn Tìm ra số lượng đơn hàng và trạng thái chưa thanh toán
        {
           int? sl =  _context.DonHangs.Count(e => e.BanId == id && e.TrangThai == false);
            return sl.HasValue ? sl.Value : 0;
        }
    }
}
