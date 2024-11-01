using ComparisonAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 添加内存缓存服务
builder.Services.AddDistributedMemoryCache();

// 添加 Session 服务
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 添加 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueApp", builder =>
    {
        builder.WithOrigins("http://localhost:8080")
               .AllowCredentials()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// 注册服务
builder.Services.AddScoped<IComparisonService, ComparisonService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("VueApp");  // 确保这行在其他中间件之前

app.UseSession();  // Session 中间件

app.UseAuthorization();

app.MapControllers();

// 只监听 HTTP
app.Run("http://localhost:5000");
