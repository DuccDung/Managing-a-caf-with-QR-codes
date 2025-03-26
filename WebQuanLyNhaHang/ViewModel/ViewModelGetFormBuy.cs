using Microsoft.AspNetCore.Mvc;
using WebQuanLyNhaHang.Models;

namespace WebQuanLyNhaHang.ViewModel
{
    public class ViewModelGetFormBuy 
    {
        private readonly QlnhaHangBtlContext _qlnhaHangBtlContext;
      public ViewModelGetFormBuy(QlnhaHangBtlContext qlnhaHangBtlContext) {
        _qlnhaHangBtlContext = qlnhaHangBtlContext;
      }
        public List<CTDH_Product> CTDH_Product(int BanID) // chức năng của Hàm này là dùng để xem bàn đó gọi những món gì và (số lượng từng sản phẩm ... )
        {
            var result = from ban in _qlnhaHangBtlContext.Bans // join ban - donhang - ctdh - sp
                         join dh in _qlnhaHangBtlContext.DonHangs
                         on ban.BanId equals dh.BanId
                         join cthd in _qlnhaHangBtlContext.ChiTietHoaDons
                         on dh.DhId equals cthd.DhId
                         join product in _qlnhaHangBtlContext.Products
                         on cthd.ProductId equals product.ProductId
                         where ban.BanId == BanID && dh.TrangThai == false
                         select new CTDH_Product // -> rồi ép nó về đối tượng CTDH_Product
                         {
                           DhId = dh.DhId,
                           PathPhoto = product.PathPhoto,
                           SoLuong = cthd.SoLuong,
                           TenSanPham = product.TenSanPham,
                           ThanhTien = cthd.ThanhTien,
                           DonGia = product.GiaTien,
                           TongTien = dh.TongTien,
                           BanId = BanID,
                         };
            return result.ToList();
        }
    }
}
