using Microsoft.EntityFrameworkCore;
using TheFitzBankAPI.Application;
using TheFitzBankAPI.Application.Services;
using TheFitzBankAPI.Infrastructure;
using TheFitzBankAPI.Infrastructure.RandomDataBase;
using TheFitzBankAPI.Mapping;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BankingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAccountService, AccountService>();
// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Banking API v1");
    c.RoutePrefix = string.Empty; // Swagger на корне: http://localhost:8080/
});

if (!app.Environment.IsProduction()) {
    app.UseHttpsRedirection();
}

app.MapControllers();

using (var scope = app.Services.CreateScope()) {
    var services = scope.ServiceProvider;
    try {
        var context = services.GetRequiredService<BankingContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Running database migrations...");
        await context.Database.MigrateAsync(); 

        logger.LogInformation("Seeding database with initial data...");
        await DbInitializer.SeedAsync(context);

        logger.LogInformation("Database initialization completed successfully.");
    } catch (Exception ex) {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
        throw;
    }
}
app.Run();
