using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;

namespace CinemaManagement.Controllers
{
    [Route("Momo")]
    public class MomoController : Controller
    {
        private readonly ILogger<MomoController> _logger;

        public MomoController(ILogger<MomoController> logger)
        {
            _logger = logger;
        }

        // POST /Momo/CreatePayment
        [HttpPost("CreatePayment")]
        [IgnoreAntiforgeryToken]
        [AllowAnonymous]
        [Produces("application/json")]
        public IActionResult CreatePayment([FromBody] JsonElement body)
        {
            try
            {
                _logger.LogInformation("[MockMoMo] CreatePayment raw body: {body}", body.ToString());

                Guid showTimeId = Guid.Empty;
                var seatIds = new List<Guid>();
                long totalPrice = 0;

                if (body.ValueKind == JsonValueKind.Object)
                {
                    if (body.TryGetProperty("showTimeId", out var st))
                    {
                        if (st.ValueKind == JsonValueKind.String && Guid.TryParse(st.GetString(), out var g))
                            showTimeId = g;
                        else if (st.ValueKind == JsonValueKind.Null) { }
                    }

                    if (body.TryGetProperty("seatIds", out var sids) && sids.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var it in sids.EnumerateArray())
                        {
                            if (it.ValueKind == JsonValueKind.String && Guid.TryParse(it.GetString(), out var sg))
                                seatIds.Add(sg);
                        }
                    }

                    if (body.TryGetProperty("totalPrice", out var tp) || body.TryGetProperty("totalprice", out tp) || body.TryGetProperty("TotalPrice", out tp))
                    {
                        if (tp.ValueKind == JsonValueKind.Number && tp.TryGetInt64(out var v))
                            totalPrice = v;
                        else if (tp.ValueKind == JsonValueKind.String && long.TryParse(tp.GetString(), out var vs))
                            totalPrice = vs;
                    }
                }

                // fallback: if values not present, try to read common camelCase names
                if (showTimeId == Guid.Empty && body.TryGetProperty("showTimeId", out var _)) { }

                var orderId = Guid.NewGuid().ToString();

                var extraObj = new
                {
                    showTimeId = showTimeId,
                    seatIds = seatIds
                };

                var extraJson = JsonSerializer.Serialize(extraObj);
                var extraBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(extraJson));

                var payUrl = $"/Momo/MockPaymentPage?orderId={WebUtility.UrlEncode(orderId)}&amount={totalPrice}&extraData={WebUtility.UrlEncode(extraBase64)}";

                _logger.LogInformation("[MockMoMo] CreatePayment -> orderId={orderId} amount={amount} payUrl={payUrl}", orderId, totalPrice, payUrl);

                // Return 200 JSON with payUrl
                return Ok(new { payUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MockMoMo] CreatePayment error parsing body");
                return BadRequest(new { error = "invalid request" });
            }
        }

        // GET /Momo/MockPaymentPage
        [HttpGet("MockPaymentPage")]
        [AllowAnonymous]
        public IActionResult MockPaymentPage(string orderId, string amount, string extraData)
        {
            orderId ??= string.Empty;
            amount ??= "0";
            extraData ??= string.Empty;

            ViewBag.OrderId = orderId;
            ViewBag.Amount = amount;
            ViewBag.ExtraData = extraData;

            return View("MockPaymentPage");
        }

        // POST /Momo/MockOtp
        [HttpPost("MockOtp")]
        [IgnoreAntiforgeryToken]
        [AllowAnonymous]
        public IActionResult MockOtp()
        {
            var orderId = Request.Form["orderId"].ToString();
            var amount = Request.Form["amount"].ToString();
            var extraData = Request.Form["extraData"].ToString();
            var bank = Request.Form["bank"].ToString();
            var cardNumber = Request.Form["cardNumber"].ToString();

            var masked = cardNumber ?? string.Empty;
            if (!string.IsNullOrEmpty(cardNumber) && cardNumber.Length > 4)
            {
                masked = new string('*', Math.Max(0, cardNumber.Length - 4)) + cardNumber.Substring(cardNumber.Length - 4);
            }

            ViewBag.OrderId = orderId;
            ViewBag.Amount = amount;
            ViewBag.ExtraData = extraData;
            ViewBag.Bank = bank;
            ViewBag.Masked = masked;

            return View("MockOtp");
        }

        // POST /Momo/MockCallback
        [HttpPost("MockCallback")]
        [IgnoreAntiforgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> MockCallback()
        {
            var orderId = Request.Form["orderId"].ToString();
            var amount = Request.Form["amount"].ToString();
            var extraData = Request.Form["extraData"].ToString();
            var resultType = Request.Form["resultType"].ToString();

            int resultCode = resultType == "success" ? 0 : resultType == "insufficient" ? 1006 : 1;

            var ipn = new
            {
                orderId = orderId,
                amount = amount,
                resultCode = resultCode,
                extraData = extraData
            };

            _logger.LogInformation("[MockMoMo] MockCallback received order={orderId} result={resultCode}", orderId, resultCode);

            // Extract showtimeId from extraData
            string showtimeId = "";
            try
            {
                if (!string.IsNullOrEmpty(extraData))
                {
                    var bytes = Convert.FromBase64String(extraData);
                    var json = System.Text.Encoding.UTF8.GetString(bytes);
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("showTimeId", out var stProp))
                    {
                        showtimeId = stProp.GetString() ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[MockMoMo] Failed to extract showtimeId from extraData");
            }

            // Process IPN internally (simulate MoMo server-to-server)
            try
            {
                await ProcessIpnAsync(JsonSerializer.Serialize(ipn));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing IPN");
            }

            var returnUrl = $"/Momo/Return?resultCode={resultCode}&orderId={WebUtility.UrlEncode(orderId)}&showtimeId={WebUtility.UrlEncode(showtimeId)}";

            var statusHtml = resultCode == 0 ? "<div class='ok' style='color:#24a148;font-weight:700;'>Thanh toán thành công</div>"
                : resultCode == 1006 ? "<div class='warn' style='color:#ff9f1a;font-weight:700;'>Không đủ tiền</div>"
                : "<div class='err' style='color:#d64545;font-weight:700;'>Thanh toán thất bại</div>";

            var html = $@"<!doctype html>
<html>
<head>
  <meta charset='utf-8'>
  <meta name='viewport' content='width=device-width,initial-scale=1'>
    <title>Kết quả thanh toán</title>
  <meta http-equiv='refresh' content='3;url={WebUtility.HtmlEncode(returnUrl)}' />
  <style>
    body {{ background:#f2f6fb; font-family:Arial,Helvetica,sans-serif; margin:0; padding:0; }}
    .card {{ max-width:520px; margin:60px auto; background:#fff; border-radius:12px; padding:28px; box-shadow:0 12px 36px rgba(11,46,90,0.06); text-align:center; }}
    h2 {{ color:#0b2e5a; }}
    a.btn {{ display:inline-block; margin-top:16px; padding:10px 14px; background:#2b7cff; color:#fff; border-radius:8px; text-decoration:none; }}
  </style>
</head>
<body>
  <div class='card'>
        <h2>Kết quả thanh toán</h2>
        <div style='margin:14px 0; font-size:15px;'>Đơn hàng: <strong>{WebUtility.HtmlEncode(orderId)}</strong></div>
    <div style='font-size:18px;'>{statusHtml}</div>
        <div style='margin-top:12px;color:#6b7b95;'>Bạn sẽ được chuyển trở lại hệ thống trong vài giây...</div>
        <a class='btn' href='{WebUtility.HtmlEncode(returnUrl)}'>Quay về ngay</a>
  </div>
</body>
</html>";

            return Content(html, "text/html", System.Text.Encoding.UTF8);
        }

        // GET /Momo/Return
        [HttpGet("Return")]
        [AllowAnonymous]
        public IActionResult Return(int resultCode, string orderId = "", string showtimeId = "")
        {
            // Redirect back to SelectSeats with resultCode, orderId and showtimeId
            var redirectUrl = $"/Ticket/SelectSeats?showtimeId={WebUtility.UrlEncode(showtimeId)}&resultCode={resultCode}&orderId={WebUtility.UrlEncode(orderId ?? "")}";
            return Redirect(redirectUrl);
        }

        // GET /Momo/Ping - debug endpoint
        [HttpGet("Ping")]
        [AllowAnonymous]
        public IActionResult Ping()
        {
            _logger.LogInformation("[MockMoMo] Ping received");
            return Content("pong");
        }

        // Internal: process IPN-like payload (JSON string)
        private async Task ProcessIpnAsync(string ipnJson)
        {
            // ipnJson contains orderId, amount, resultCode, extraData
            try
            {
                using var doc = JsonDocument.Parse(ipnJson);
                var root = doc.RootElement;
                var orderId = root.GetProperty("orderId").GetString();
                var amount = root.GetProperty("amount").GetString();
                var resultCode = root.GetProperty("resultCode").GetInt32();
                var extraData = root.GetProperty("extraData").GetString();

                // decode extraData (base64 -> json)
                if (!string.IsNullOrEmpty(extraData))
                {
                    string json = string.Empty;
                    try
                    {
                        var bytes = Convert.FromBase64String(extraData);
                        json = System.Text.Encoding.UTF8.GetString(bytes);
                    }
                    catch
                    {
                        _logger.LogWarning("[MockMoMo] extraData is not valid base64");
                    }

                    if (!string.IsNullOrEmpty(json))
                    {
                        try
                        {
                            using var ed = JsonDocument.Parse(json);
                            var rootEd = ed.RootElement;
                            Guid showTimeId = Guid.Empty;
                            try { showTimeId = rootEd.GetProperty("showTimeId").GetGuid(); } catch { }
                            var seatIds = new List<Guid>();
                            if (rootEd.TryGetProperty("seatIds", out var sids))
                            {
                                foreach (var it in sids.EnumerateArray())
                                {
                                    if (it.ValueKind == JsonValueKind.String && Guid.TryParse(it.GetString(), out var g))
                                        seatIds.Add(g);
                                }
                            }

                            if (resultCode == 0)
                            {
                                // Thanh toán thành công - booking seats
                                _logger.LogInformation("[MockMoMo][IPN] Booking seats for showTime {showTimeId}: {seatIds}", showTimeId, string.Join(',', seatIds));
                            }
                            else
                            {
                                // Thanh toán thất bại - release seats
                                _logger.LogInformation("[MockMoMo][IPN] Releasing seats for showTime {showTimeId}: {seatIds} (Payment failed)", showTimeId, string.Join(',', seatIds));
                                
                                // Call internal release endpoint to reset seats and broadcast via SignalR
                                await ReleaseSeatsInternalAsync(showTimeId, seatIds);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "[MockMoMo] failed to parse extraData json");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MockMoMo] invalid ipn json");
            }
        }

        /// <summary>
        /// Internal helper để release seats khi thanh toán thất bại
        /// </summary>
        private async Task ReleaseSeatsInternalAsync(Guid showTimeId, List<Guid> seatIds)
        {
            // Gọi /Booking/ReleaseSeats endpoint
            try
            {
                using var httpClient = new HttpClient();
                var releaseRequest = new
                {
                    showTimeId = showTimeId,
                    seatIds = seatIds
                };

                var json = JsonSerializer.Serialize(releaseRequest);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                // Construct the URL - adjust domain as needed for your environment
                var releaseUrl = $"http://localhost:5000/Booking/ReleaseSeats";
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
                {
                    // Try to use current request's base URL
                    releaseUrl = $"http://localhost/Booking/ReleaseSeats";
                }

                var response = await httpClient.PostAsync(releaseUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("[MockMoMo][IPN] Successfully released {seatCount} seats for showTimeId {showTimeId}", 
                        seatIds.Count, showTimeId);
                }
                else
                {
                    _logger.LogWarning("[MockMoMo][IPN] Failed to release seats: HTTP {statusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MockMoMo][IPN] Error calling ReleaseSeats endpoint");
            }
        }

        public class CreatePaymentRequest
        {
            public Guid ShowTimeId { get; set; }
            public List<Guid> SeatIds { get; set; } = new List<Guid>();
            public long TotalPrice { get; set; }
        }
    }
}
