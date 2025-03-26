using Microsoft.AspNetCore.Mvc;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Views.Shared.Components.TongTien
{
    public class TongTienViewComponent : ViewComponent
    {
        private readonly QlnhaHangBtlContext _context;
        public TongTienViewComponent(QlnhaHangBtlContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? DhId)
        {
            ViewModelCart viewModel = new ViewModelCart(_context);
            return View("TongTien", viewModel.TongtienById(DhId));
        }
    }
}
