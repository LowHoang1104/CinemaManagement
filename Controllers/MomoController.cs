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

            var oOrderId = WebUtility.HtmlEncode(orderId);
            var oAmount = WebUtility.HtmlEncode(amount);
            var oExtra = WebUtility.HtmlEncode(extraData);

            var html = $@"<!doctype html>
<html>
<head>
  <meta charset='utf-8'>
  <meta name='viewport' content='width=device-width,initial-scale=1'>
  <title>Thanh toán MoMo (Test)</title>
  <style>
    body {{ background:#f2f6fb; font-family:Arial,Helvetica,sans-serif; margin:0; padding:0; }}
    .card {{ max-width:480px; margin:60px auto; background:#fff; border-radius:12px; padding:22px; box-shadow:0 12px 30px rgba(11,46,90,0.06); }}
    h1 {{ color:#0b2e5a; margin:0 0 12px; font-size:20px; }}
    label {{ display:block; margin-bottom:6px; font-size:13px; color:#334; }}
    select,input {{ width:100%; padding:10px 12px; border-radius:8px; border:1px solid #d7e3f7; margin-bottom:12px; box-sizing:border-box; }}
    .row {{ display:flex; gap:12px; }}
    .small {{ color:#6b7b95; font-size:13px; }}
    .btn {{ background:#2b7cff; color:#fff; padding:10px 14px; border:none; border-radius:8px; cursor:pointer; font-weight:600; }}
    @media (max-width:520px) {{ .card {{ margin:20px; }} }}
  </style>
</head>
<body>
  <div class='card'>
    <h1>Thanh toán MoMo (Test)</h1>
    <div class='small'>??n hàng: <strong>{oOrderId}</strong></div>
    <div class='small' style='margin-bottom:16px;'>S? ti?n: <strong>{oAmount} VND</strong></div>

    <form method='post' action='/Momo/MockOtp'>
      <input type='hidden' name='orderId' value='{oOrderId}' />
      <input type='hidden' name='amount' value='{oAmount}' />
      <input type='hidden' name='extraData' value='{oExtra}' />

      <label for='bank'>Ch?n ngân hàng</label>
      <select id='bank' name='bank' required>
        <option value='Vietcombank'>Vietcombank</option>
        <option value='Techcombank'>Techcombank</option>
        <option value='BIDV'>BIDV</option>
      </select>

      <label for='cardNumber'>S? th?</label>
      <input id='cardNumber' name='cardNumber' placeholder='0123 4567 8901 2345' required />

      <label for='cardName'>Tên ch? th?</label>
      <input id='cardName' name='cardName' placeholder='NGUYEN VAN A' required />

      <label for='expiry'>Ngày h?t h?n (MM/YY)</label>
      <input id='expiry' name='expiry' placeholder='12/25' required />

      <div style='display:flex; justify-content:space-between; align-items:center; gap:12px; margin-top:8px;'>
        <div class='small'>T?ng: <strong>{oAmount} VND</strong></div>
        <button class='btn' type='submit'>Thanh toán</button>
      </div>
    </form>

    <div class='small' style='margin-top:14px;'>L?u ý: ?ây là trang gi? l?p dành cho môi tr??ng phát tri?n.</div>
  </div>
</body>
</html>";

            return Content(html, "text/html", System.Text.Encoding.UTF8);
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

            var oOrderId = WebUtility.HtmlEncode(orderId);
            var oAmount = WebUtility.HtmlEncode(amount);
            var oExtra = WebUtility.HtmlEncode(extraData);
            var oBank = WebUtility.HtmlEncode(bank);

            var masked = cardNumber ?? string.Empty;
            if (!string.IsNullOrEmpty(cardNumber) && cardNumber.Length > 4)
            {
                masked = new string('•', Math.Max(0, cardNumber.Length - 4)) + cardNumber.Substring(cardNumber.Length - 4);
            }

            var html = $@"<!doctype html>
<html>
<head>
  <meta charset='utf-8'>
  <meta name='viewport' content='width=device-width,initial-scale=1'>
  <title>Nh?p OTP - MoMo (Test)</title>
  <style>
    body {{ background:#f2f6fb; font-family:Arial,Helvetica,sans-serif; margin:0; padding:0; }}
    .card {{ max-width:420px; margin:60px auto; background:#fff; border-radius:12px; padding:22px; box-shadow:0 10px 30px rgba(11,46,90,0.06); }}
    h2 {{ color:#0b2e5a; margin:0 0 12px; }}
    .info {{ font-size:14px; color:#334; margin-bottom:12px; }}
    input {{ width:100%; padding:10px 12px; border-radius:8px; border:1px solid #d7e3f7; margin-bottom:12px; box-sizing:border-box; }}
    .btns {{ display:flex; gap:8px; }}
    .btn {{ flex:1; padding:10px 12px; border-radius:8px; border:none; cursor:pointer; color:#fff; font-weight:600; }}
    .success {{ background:#24a148; }}
    .fail {{ background:#d64545; }}
    .insuf {{ background:#ff9f1a; color:#000; }}
    @media (max-width:520px) {{ .card {{ margin:20px; }} .btns {{ flex-direction:column; }} }}
  </style>
</head>
<body>
  <div class='card'>
    <h2>Nh?p OTP</h2>
    <div class='info'>??n hàng: <strong>{oOrderId}</strong><br/>S? ti?n: <strong>{oAmount} VND</strong><br/>Ngân hàng: <strong>{oBank}</strong><br/>Th?: <strong>{WebUtility.HtmlEncode(masked)}</strong></div>

    <form method='post' action='/Momo/MockCallback'>
      <input type='hidden' name='orderId' value='{oOrderId}' />
      <input type='hidden' name='amount' value='{oAmount}' />
      <input type='hidden' name='extraData' value='{oExtra}' />

      <label for='otp'>Mã OTP</label>
      <input id='otp' name='otp' placeholder='123456' />

      <div class='btns'>
        <button type='submit' name='resultType' value='success' class='btn success'>Thành công</button>
        <button type='submit' name='resultType' value='fail' class='btn fail'>Th?t b?i</button>
        <button type='submit' name='resultType' value='insufficient' class='btn insuf'>Không ?? ti?n</button>
      </div>
    </form>

    <div style='margin-top:12px; color:#6b7b95; font-size:13px;'>L?u ý: ?ây là trang gi? l?p dành cho môi tr??ng phát tri?n.</div>
  </div>
</body>
</html>";

            return Content(html, "text/html", System.Text.Encoding.UTF8);
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
                : resultCode == 1006 ? "<div class='warn' style='color:#ff9f1a;font-weight:700;'>Không ?? ti?n</div>"
                : "<div class='err' style='color:#d64545;font-weight:700;'>Thanh toán th?t b?i</div>";

            var html = $@"<!doctype html>
<html>
<head>
  <meta charset='utf-8'>
  <meta name='viewport' content='width=device-width,initial-scale=1'>
  <title>K?t qu? thanh toán</title>
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
    <h2>K?t qu? thanh toán</h2>
    <div style='margin:14px 0; font-size:15px;'>??n hàng: <strong>{WebUtility.HtmlEncode(orderId)}</strong></div>
    <div style='font-size:18px;'>{statusHtml}</div>
    <div style='margin-top:12px;color:#6b7b95;'>B?n s? ???c chuy?n tr? l?i h? th?ng trong vài giây...</div>
    <a class='btn' href='{WebUtility.HtmlEncode(returnUrl)}'>Quay v? ngay</a>
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
        private Task ProcessIpnAsync(string ipnJson)
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
                                // simulate booking
                                _logger.LogInformation("[MockMoMo][IPN] Booking seats for showTime {showTimeId}: {seatIds}", showTimeId, string.Join(',', seatIds));
                            }
                            else
                            {
                                // simulate release
                                _logger.LogInformation("[MockMoMo][IPN] Releasing seats for showTime {showTimeId}: {seatIds}", showTimeId, string.Join(',', seatIds));
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

            return Task.CompletedTask;
        }

        public class CreatePaymentRequest
        {
            public Guid ShowTimeId { get; set; }
            public List<Guid> SeatIds { get; set; } = new List<Guid>();
            public long TotalPrice { get; set; }
        }
    }
}
