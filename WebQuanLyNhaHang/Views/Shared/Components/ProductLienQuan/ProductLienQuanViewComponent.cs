using Microsoft.AspNetCore.Mvc;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Views.Shared.Components.ProductLienQuan
{
    public class ProductLienQuanViewComponent:ViewComponent
    {
        QlnhaHangBtlContext _qlnhaHangBtlContext;
        public ProductLienQuanViewComponent(QlnhaHangBtlContext qlnhaHangBtlContext)
        {
            _qlnhaHangBtlContext = qlnhaHangBtlContext;
        }
        public async Task<IViewComponentResult> InvokeAsync(int CateID) // liệt kê ra category
        {
            var listProduct = _qlnhaHangBtlContext.Products.Where(e => e.CateId == CateID).ToList();
            return View("ListProduct", listProduct);
        }
    }
}
