using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Stock.Context;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Newtonsoft (ไม่ต้อง AddControllersWithViews ซ้ำ)
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
        options.SerializerSettings.ContractResolver = new DefaultContractResolver(); // PascalCase
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // PascalCase
    });

// DbContext
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseLazyLoadingProxies(); // ต้องติดตั้ง Microsoft.EntityFrameworkCore.Proxies
    options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext"));
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Stock.API", Version = "v1" });
    c.CustomSchemaIds(x => x.FullName);
});
// ต้องติดตั้ง Swashbuckle.AspNetCore.Newtonsoft
builder.Services.AddSwaggerGenNewtonsoftSupport();

// CORS (แล้วแต่จะจำกัด origin ภายหลัง)
//builder.Services.AddCors();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
                "http://localhost:7002",
                "http://192.168.0.18:7002"         
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});


var app = builder.Build();

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Stock.API v1");
    c.ConfigObject.AdditionalItems.Add("syntaxHighlight", false);
    c.ConfigObject.AdditionalItems.Add("theme", "agate");
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors(c => c.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

// ลำดับที่ถูกต้อง: Authentication -> Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
