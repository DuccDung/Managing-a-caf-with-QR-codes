using Microsoft.AspNetCore.Mvc;
using WebQuanLyNhaHang.Models;
using WebQuanLyNhaHang.ViewModel;

namespace WebQuanLyNhaHang.Views.Shared.Components.CTDH_Online
{
    public class CTDH_OnlineViewComponent:ViewComponent
    {
        private readonly QlnhaHangBtlContext _context;
        public CTDH_OnlineViewComponent(QlnhaHangBtlContext context) { 
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? DonHangID)
        {
            ViewModelCart viewModel = new ViewModelCart(_context);
            var CTDH = viewModel.CTHD_PctByDh(DonHangID);
            return View("ListCTDH_Online" , CTDH);
        }
    }
}
