namespace CinemaManagement.Models
{
    public class MomoCreatePaymentResponse
    {
        public string partnerCode { get; set; }

        public string requestId { get; set; }

        public string orderId { get; set; }

        public long amount { get; set; }

        public long responseTime { get; set; }

        public string message { get; set; }

        public int resultCode { get; set; }

        public string payUrl { get; set; }

        public string deeplink { get; set; }

        public string qrCodeUrl { get; set; }

        public string deeplinkMiniApp { get; set; }
    }
}
