using System.Collections.Generic;
using System.Linq;
using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.ViewModel
{
    public class LichSuOderViewModel
    {
        private readonly QlnhaHangBtlContext _context;

        public LichSuOderViewModel(QlnhaHangBtlContext context)
        {
            _context = context;
        }

        public List<DonHang> FindOrdersByBanId(int? banId)
        {
            return _context.DonHangs
                .Where(dh => dh.BanId == banId && dh.TrangThai == false)  // Lấy đơn hàng chưa thanh toán trong bàn tại quán
                .ToList();
        }
        public List<CTDH_Product> FindProductsByOrderId(int? DhId) // Lấy Ctdh và product join và tìm kiếm nó theo đơn hàng; để phục vụ cart
        {
            var result = from CTHD in _context.ChiTietHoaDons
                         join product in _context.Products
                         on CTHD.ProductId equals product.ProductId
                         where CTHD.DhId == DhId
                         select new CTDH_Product
                         {
                             ProductId = product.ProductId,
                             DhId = DhId,
                             CthdId = CTHD.CthdId,
                             PathPhoto = product.PathPhoto,
                             SoLuong = CTHD.SoLuong,
                             TenSanPham = product.TenSanPham,
                             ThanhTien = CTHD.ThanhTien,
                             Condition = CTHD.Ghichu
                         };
            if (result == null) // nếu đơn hàng về null mà ép Tolist(); nó sẽ bị bug => ta tạo 1 list mới
            {
                return new List<CTDH_Product>();
            }
            else // result != null trả về list 
            {
                return result.ToList();
            }
        }

        public decimal? TinhTongTienTheoBanId(int? banId)
        {
            // Tính tổng tiền của các đơn hàng có Ban_ID bằng với banId
            var tongTien = _context.DonHangs
                .Where(dh => dh.BanId == banId && dh.TrangThai == false) // trạng thái chưa thanh toán
                .Sum(dh => dh.TongTien);

            return tongTien;
        }

    }
}
