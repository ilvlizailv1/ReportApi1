using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ ВАЖНО: лицензия QuestPDF (убирает окно/ошибку)
QuestPDF.Settings.License = LicenseType.Community;

// ✅ HttpClient для OtpravkaApi
builder.Services.AddHttpClient("OtpravkaApi", client =>
{
    var baseUrl = builder.Configuration["OtpravkaApi:BaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("OtpravkaApi:BaseUrl не задан в appsettings.json");

    client.BaseAddress = new Uri(baseUrl);
});

// ✅ CORS (если фронт будет отдельно)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p =>
        p.AllowAnyOrigin()
         .AllowAnyHeader()
         .AllowAnyMethod());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ReportApi v1");
    c.RoutePrefix = "swagger";
});

// ✅ Чтобы открывалось по корню
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

app.Run();
