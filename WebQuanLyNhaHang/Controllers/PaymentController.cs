using Microsoft.AspNetCore.Mvc;
using WebQuanLyNhaHang.Models.Vnpay;
using WebQuanLyNhaHang.Services.Vnpay;

namespace WebQuanLyNhaHang.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IVnPayService _vnPayService;
        public PaymentController(IVnPayService vnPayService)
        {

            _vnPayService = vnPayService;
        }

        public IActionResult CreatePaymentUrlVnpay(string Name , double Amount ,string OrderDescription , string OrderType)
        {
            var model = new PaymentInformationModel
            {
                Name = Name ,
                Amount = Amount ,
                OrderDescription = OrderDescription ,
                OrderType = OrderType
            };
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            return Redirect(url);
        }
        [HttpGet]
        public IActionResult PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
          //  return Json(response);
            return View(response);
        }

    }
}
