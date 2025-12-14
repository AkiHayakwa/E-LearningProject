
namespace LearningManagementSystem.Models
{
    public class VnPayConfig
    {
        public static string vnp_Url { get; set; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html"; // URL sandbox, đổi sang URL production khi triển khai thực tế
        public static string vnp_Returnurl { get; set; } = "https://localhost:7094/Cart/VnPayReturn"; // URL nhận kết quả trả về
        public static string vnp_TmnCode { get; set; } = "DB9JZRXK"; // Terminal ID VNPay cấp
        public static string vnp_HashSecret { get; set; } = "B26862DHT3IG1ZXK2CZXGJEVBVLXSE34"; // Secret key VNPay cấp
        public static string vnp_Version { get; set; } = "2.1.0";
        public static string vnp_Command { get; set; } = "pay";
        public static string vnp_CurrCode { get; set; } = "VND";
        public static string vnp_Locale { get; set; } = "vn";
    }
}