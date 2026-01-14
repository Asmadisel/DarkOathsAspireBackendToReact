// Controllers/ProxyController.cs
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Web;

namespace DarkOathsAspireBackendToReact.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProxyController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProxyController> _logger;

        public ProxyController(IHttpClientFactory httpClientFactory, ILogger<ProxyController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpPost("auth/login")]
        public async Task<IActionResult> Login([FromBody] object loginData)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AuthService");
                var response = await client.PostAsJsonAsync("/api/auth/login", loginData);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Ok(content);
                }

                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error proxying login request");
                return StatusCode(500, "Internal server error");
            }
        }

        // Метод для начала OAuth авторизации Google
        [HttpGet("login-google")]
        public IActionResult StartGoogleLogin([FromQuery] string returnUrl = null)
        {
            try
            {
                var authServiceUrl = _httpClientFactory.CreateClient("AuthService").BaseAddress?.ToString();
                var redirectUrl = $"{authServiceUrl}/login-google";

                if (!string.IsNullOrEmpty(returnUrl))
                {
                    // Кодируем returnUrl для безопасной передачи
                    var encodedReturnUrl = HttpUtility.UrlEncode(returnUrl);
                    redirectUrl += $"?returnUrl={encodedReturnUrl}";
                }

                _logger.LogInformation("Redirecting to Google OAuth: {Url}", redirectUrl);
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Google OAuth");
                return StatusCode(500, "Failed to initiate Google login");
            }
        }

        // Callback для Google OAuth (AuthService вернет сюда после авторизации)
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string state, [FromQuery] string returnUrl = null)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AuthService");

                // Формируем URL для callback с параметрами
                var callbackUrl = $"/google-callback?code={Uri.EscapeDataString(code)}&state={Uri.EscapeDataString(state)}";

                if (!string.IsNullOrEmpty(returnUrl))
                {
                    callbackUrl += $"&returnUrl={Uri.EscapeDataString(returnUrl)}";
                }

                var response = await client.GetAsync(callbackUrl);

                if (response.IsSuccessStatusCode)
                {
                    // AuthService должен вернуть токен в JSON или перенаправить
                    var contentType = response.Content.Headers.ContentType?.MediaType;

                    if (contentType == "application/json")
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return Content(content, "application/json");
                    }
                    else
                    {
                        // Если это редирект, следуем за ним
                        var location = response.Headers.Location?.ToString();
                        if (!string.IsNullOrEmpty(location))
                        {
                            return Redirect(location);
                        }

                        // Или пытаемся прочитать как строку
                        var html = await response.Content.ReadAsStringAsync();
                        return Content(html, "text/html");
                    }
                }

                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Google callback");
                return StatusCode(500, "Failed to process Google authentication");
            }
        }

        // Метод для проверки токена/сессии
        [HttpGet("auth/check")]
        public async Task<IActionResult> CheckAuth()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AuthService");
                var response = await client.GetAsync("/api/auth/check");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Ok(content);
                }

                return StatusCode((int)response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking auth status");
                return StatusCode(500, "Internal server error");
            }
        }

        // Метод для выхода
        [HttpPost("auth/logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("AuthService");
                var response = await client.PostAsync("/api/auth/logout", null);

                if (response.IsSuccessStatusCode)
                {
                    return Ok();
                }

                return StatusCode((int)response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging out");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}