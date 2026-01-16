using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (нужно для фронта / Netlify)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ReportApi v1");
    c.RoutePrefix = "swagger";
});

// CORS
app.UseCors("AllowAll");

// ✅ Раздача статических файлов
app.UseStaticFiles();

// ✅ Раздача твоей папки "frontend" по адресу /frontend
var frontendPath = Path.Combine(app.Environment.ContentRootPath, "frontend");
if (Directory.Exists(frontendPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(frontendPath),
        RequestPath = "/frontend"
    });
}

// ✅ Главная страница — твоя платформа
app.MapGet("/", () => Results.Redirect("/frontend/index.html"));

// (если хочешь — Swagger всегда доступен тут)
app.MapGet("/docs", () => Results.Redirect("/swagger"));

app.UseAuthorization();
app.MapControllers();

app.Run();
