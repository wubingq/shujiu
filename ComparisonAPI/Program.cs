using ComparisonAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 添加 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        builder =>
        {
            builder.SetIsOriginAllowed(origin => true) // 允许任何来源
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
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

// 删除 HTTPS 重定向
// app.UseHttpsRedirection();  // 注释掉这行

app.UseCors("AllowVueApp");  // 确保这行在其他中间件之前
app.UseAuthorization();
app.MapControllers();

// 只监听 HTTP
app.Run("http://localhost:5000");
