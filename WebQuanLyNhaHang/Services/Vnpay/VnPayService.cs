using WebQuanLyNhaHang.Libraries;
using WebQuanLyNhaHang.Models.Vnpay;

namespace WebQuanLyNhaHang.Services.Vnpay
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;

        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VnPayLibrary();
            var urlCallBack = _configuration["PaymentCallBack:ReturnUrl"];

            //pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            //pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            //pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            //pay.AddRequestData("vnp_Amount", ((int)model.Amount * 100).ToString());  // Tổng Tiền
            //pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            //pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            //pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            //pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            //pay.AddRequestData("vnp_OrderInfo", $"{model.Name} {model.OrderDescription} {model.Amount}");  // Thông tin
            //pay.AddRequestData("vnp_OrderType", model.OrderType);
            //pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            //pay.AddRequestData("vnp_TxnRef", tick);

            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", "2YH1X4JB"); // Thay bằng TmnCode thực của bạn
            pay.AddRequestData("vnp_Amount", ((int)model.Amount * 100).ToString());  // Tổng Tiền, nhân 100 để đổi sang đơn vị đồng
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", "VND");
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", "vn");
            pay.AddRequestData("vnp_OrderInfo", $"{model.Name} {model.OrderDescription} {model.Amount}");  // Thông tin đơn hàng
            pay.AddRequestData("vnp_OrderType", model.OrderType); // Đặt cứng nếu muốn ví dụ "other"
            pay.AddRequestData("vnp_ReturnUrl", "https://localhost:7005/Payment/PaymentCallbackVnpay"); // Thay bằng URL callback thực tế khi lên production
            pay.AddRequestData("vnp_TxnRef", tick);


            Console.WriteLine("vnp_Version: " + _configuration["Vnpay:Version"]);
            Console.WriteLine("vnp_Command: " + _configuration["Vnpay:Command"]);
            Console.WriteLine("vnp_TmnCode: " + _configuration["Vnpay:TmnCode"]);
            Console.WriteLine("vnp_Amount: " + ((int)model.Amount * 100).ToString());
            Console.WriteLine("vnp_CurrCode: " + _configuration["Vnpay:CurrCode"]);
            Console.WriteLine("vnp_Locale: " + _configuration["Vnpay:Locale"]);

            // In thêm các giá trị khác nếu cần
            var paymentUrl =
                pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

            return paymentUrl;
        }
        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var pay = new VnPayLibrary();
            var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);

            return response;
        }

    }
}
