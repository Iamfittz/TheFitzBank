using Microsoft.EntityFrameworkCore;
using TheFitzBankAPI.Application;
using TheFitzBankAPI.Application.Services;
using TheFitzBankAPI.Domain;
using TheFitzBankAPI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BankingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger включаем всегда (и в Docker тоже)
app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Banking API v1");
    c.RoutePrefix = string.Empty; // Swagger на корне: http://localhost:8080/
});

// В контейнере оставляем только HTTP
if (!app.Environment.IsProduction()) {
    app.UseHttpsRedirection();
}

app.MapControllers();
app.Run();
