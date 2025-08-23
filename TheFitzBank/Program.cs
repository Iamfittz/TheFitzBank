using Microsoft.EntityFrameworkCore;
using TheFitzBankAPI.Application;
using TheFitzBankAPI.Application.Services;
using TheFitzBankAPI.Domain;
using TheFitzBankAPI.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BankingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. DI ��� ����������� � �������
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();

// 3. AutoMapper (���� ������� � ������� Application)
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


// 4. Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 5. Middleware
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Banking API v1");
        c.RoutePrefix = string.Empty; // Swagger ����� ����������� �� https://localhost:7299/
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
