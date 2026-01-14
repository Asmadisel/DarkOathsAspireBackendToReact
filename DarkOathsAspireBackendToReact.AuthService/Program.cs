using DarkOathsAspireBackendToReact.AuthService.Data;
using DarkOathsAspireBackendToReact.AuthService.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 1. Регистрация авторизации
builder.Services.AddAuthorization();
builder.Services.AddAdminAuthorization();

// 2. Настройка БД
builder.Services.AddDbContext<AuthDbContext>(options =>
{
    var databaseConnection = builder.Configuration.GetConnectionString("authdb");
    options.UseNpgsql(databaseConnection);
});

// 3. КРИТИЧЕСКИ ВАЖНАЯ НАСТРОЙКА АУТЕНТИФИКАЦИИ
builder.Services.AddAuthentication(options =>
{
    // Схема по умолчанию — JWT для защищенных API
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddCookie("GoogleCookies", options =>
    {
        options.Cookie.Name = "GoogleAuth";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.Path = "/";
    })
    .AddJwtBearer(options =>
    {
        var jwtKey = "your_super_long_and_secure_secret_key_here_32_characters_min";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "DarkOathsAuth",
            ValidAudience = "DarkOathsApp",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey))
        };
        // === ЭТА СТРОКА КЛЮЧ К УСПЕХУ ===
        // Если запрос не содержит Bearer-токена, не пытайся его аутентифицировать.
        // Это позволяет другим схемам (GoogleCookies) работать в своих эндпоинтах.
        options.ForwardDefault = null;
    })
    .AddGoogle("Google", options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

        // ✅ ВАЖНО: Укажите ТОЧНО такой же redirect URI как в Google Console
        options.CallbackPath = "/api/auth/google-callback";

        // ✅ Явно указываем redirect URI (если нужно)
        // options.AccessType = "offline";
        // options.Prompt = "consent";

        options.Scope.Add("openid");
        options.Scope.Add("email");
        options.Scope.Add("profile");
        options.SignInScheme = "GoogleCookies";
        options.SaveTokens = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (builder.Environment.IsDevelopment()) // Обратите внимание: builder.Environment, а не app.Environment
{
    builder.Services.Configure<CookiePolicyOptions>(options =>
    {
        options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
        options.OnAppendCookie = cookieContext =>
            SameSiteHandling(cookieContext.Context, cookieContext.CookieOptions);
        options.OnDeleteCookie = cookieContext =>
            SameSiteHandling(cookieContext.Context, cookieContext.CookieOptions);
    });
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await dbContext.Database.MigrateAsync();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthService v1");
    });
}

app.UseCors("AllowReactApp");
app.UseHttpsRedirection();
app.UseAuthentication(); // <-- Должен быть перед UseAuthorization()
app.UseAuthorization();
app.MapUsersManagementEndpoints();
app.MapGoogleAuthEndpoints();
app.MapAuthEndpoints();
app.MapDefaultEndpoints();

app.Run();

// Вспомогательный метод для корректной обработки SameSite
static void SameSiteHandling(HttpContext httpContext, CookieOptions options)
{
    if (options.SameSite == SameSiteMode.None)
    {
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        // Если браузер не поддерживает SameSite=None, меняем на Unspecified
        if (DisallowsSameSiteNone(userAgent))
        {
            options.SameSite = SameSiteMode.Unspecified;
        }
    }
}

// Простая проверка для старых браузеров (часто можно опустить)
static bool DisallowsSameSiteNone(string userAgent)
{
    // Упрощенная проверка
    return userAgent.Contains("CPU iPhone OS 12") ||
           userAgent.Contains("iPad; CPU OS 12") ||
           (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") &&
            userAgent.Contains("Safari") &&
            !userAgent.Contains("Chrome")) ||
           (userAgent.Contains("Chrome/5") ||
            userAgent.Contains("Chrome/6") ||
            userAgent.Contains("Chrome/7") ||
            userAgent.Contains("Chrome/8") ||
            userAgent.Contains("Chrome/9") ||
            userAgent.Contains("Chrome/10") ||
            userAgent.Contains("Chrome/11") ||
            userAgent.Contains("Chrome/12") ||
            userAgent.Contains("Chrome/13") ||
            userAgent.Contains("Chrome/14") ||
            userAgent.Contains("Chrome/15") ||
            userAgent.Contains("Chrome/16") ||
            userAgent.Contains("Chrome/17") ||
            userAgent.Contains("Chrome/18") ||
            userAgent.Contains("Chrome/19") ||
            userAgent.Contains("Chrome/20") ||
            userAgent.Contains("Chrome/21") ||
            userAgent.Contains("Chrome/22") ||
            userAgent.Contains("Chrome/23") ||
            userAgent.Contains("Chrome/24") ||
            userAgent.Contains("Chrome/25") ||
            userAgent.Contains("Chrome/26") ||
            userAgent.Contains("Chrome/27") ||
            userAgent.Contains("Chrome/28") ||
            userAgent.Contains("Chrome/29") ||
            userAgent.Contains("Chrome/30") ||
            userAgent.Contains("Chrome/31") ||
            userAgent.Contains("Chrome/32") ||
            userAgent.Contains("Chrome/33") ||
            userAgent.Contains("Chrome/34") ||
            userAgent.Contains("Chrome/35") ||
            userAgent.Contains("Chrome/36") ||
            userAgent.Contains("Chrome/37") ||
            userAgent.Contains("Chrome/38") ||
            userAgent.Contains("Chrome/39") ||
            userAgent.Contains("Chrome/40") ||
            userAgent.Contains("Chrome/41") ||
            userAgent.Contains("Chrome/42") ||
            userAgent.Contains("Chrome/43") ||
            userAgent.Contains("Chrome/44") ||
            userAgent.Contains("Chrome/45") ||
            userAgent.Contains("Chrome/46") ||
            userAgent.Contains("Chrome/47") ||
            userAgent.Contains("Chrome/48") ||
            userAgent.Contains("Chrome/49") ||
            userAgent.Contains("Chrome/50") ||
            userAgent.Contains("Chrome/51") ||
            userAgent.Contains("Chrome/52") ||
            userAgent.Contains("Chrome/53") ||
            userAgent.Contains("Chrome/54") ||
            userAgent.Contains("Chrome/55") ||
            userAgent.Contains("Chrome/56") ||
            userAgent.Contains("Chrome/57") ||
            userAgent.Contains("Chrome/58") ||
            userAgent.Contains("Chrome/59") ||
            userAgent.Contains("Chrome/60") ||
            userAgent.Contains("Chrome/61") ||
            userAgent.Contains("Chrome/62") ||
            userAgent.Contains("Chrome/63") ||
            userAgent.Contains("Chrome/64") ||
            userAgent.Contains("Chrome/65") ||
            userAgent.Contains("Chrome/66") ||
            userAgent.Contains("Chrome/67") ||
            userAgent.Contains("Chrome/68") ||
            userAgent.Contains("Chrome/69") ||
            userAgent.Contains("Chrome/70") ||
            userAgent.Contains("Chrome/71") ||
            userAgent.Contains("Chrome/72") ||
            userAgent.Contains("Chrome/73") ||
            userAgent.Contains("Chrome/74") ||
            userAgent.Contains("Chrome/75") ||
            userAgent.Contains("Chrome/76") ||
            userAgent.Contains("Chrome/77") ||
            userAgent.Contains("Chrome/78") ||
            userAgent.Contains("Chrome/79") ||
            userAgent.Contains("Chrome/80") ||
            userAgent.Contains("Safari") && !userAgent.Contains("Chrome"));
}