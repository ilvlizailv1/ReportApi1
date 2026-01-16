using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ✅ Лицензия QuestPDF (иначе PDF падает с ошибкой "configure your license")
QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ HttpClient для OtpravkaApi (берем URL из appsettings.json)
builder.Services.AddHttpClient("OtpravkaApi", client =>
{
    var baseUrl = builder.Configuration["OtpravkaApi:BaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("Не задан OtpravkaApi:BaseUrl в appsettings.json");

    client.BaseAddress = new Uri(baseUrl);
});

// ✅ CORS (чтобы фронт мог дергать API)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p =>
        p.AllowAnyOrigin()
         .AllowAnyHeader()
         .AllowAnyMethod()
    );
});

var app = builder.Build();

// ✅ Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ReportApi v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

// ✅ Раздача платформы из wwwroot
app.UseDefaultFiles(); // откроет wwwroot/index.html по "/"
app.UseStaticFiles();

app.MapControllers();

// ✅ Если зайти на /swagger — открывается swagger
// (если хочешь редирект на swagger, а не на платформу, скажи — поменяю)
app.Run();
