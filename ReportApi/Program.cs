var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p =>
        p.AllowAnyOrigin()
         .AllowAnyHeader()
         .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("AllowAll");

// 1) Статика и главная страница из wwwroot (index.html)
app.UseDefaultFiles();   // ищет index.html
app.UseStaticFiles();    // разрешает отдавать файлы из wwwroot

// 2) Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ReportApi v1");
    c.RoutePrefix = "swagger";
});

// 3) Если нет index.html — делаем редирект на swagger
app.MapGet("/", (IWebHostEnvironment env) =>
{
    // если фронт есть, UseDefaultFiles откроет его сам
    // если фронта нет — отправим в swagger
    return Results.Redirect("/swagger");
});

app.MapControllers();
app.Run();
