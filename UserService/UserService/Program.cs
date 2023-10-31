using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using UserService.Data;
using UserService.Helpers;
using UserService.Repositories;


var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);
 
// Add services to the container.
builder.Services.AddControllers();
// Add sql server db
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultSQLConnection")));
// Add redis cache
builder.Services.AddStackExchangeRedisCache(action =>{
    action.Configuration = builder.Configuration.GetConnectionString("DefaultRedisConnection");
});

// Add HealthCheck
builder.Services
    .AddHealthChecks()

    .AddNpgSql(
    connectionString: builder.Configuration.GetConnectionString("DefaultSQLConnection"),
    healthQuery: "SELECT 1",
    name: "Postgre SQL Server Check",
    failureStatus: HealthStatus.Unhealthy | HealthStatus.Unhealthy,
    tags: new string[] { "db", "sql", "postgres" })
    
    .AddRedis(
    redisConnectionString: builder.Configuration.GetConnectionString("DefaultRedisConnection"),
    name: "Redis Check",
    failureStatus: HealthStatus.Unhealthy | HealthStatus.Unhealthy,
    tags: new string[] { "redis" });

// Add auto mapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
// Add cors
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy  =>
                      {
                          policy.WithOrigins("http://localhost:3000")
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials();
                      });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IUserRepositoryLayer, UserRepositoryLayer>();
builder.Services.AddScoped<IUserServiceLayer, UserServiceLayer>();
builder.Services.AddScoped<IJwtHelper, JwtHelper>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

// Create health check endpoint
app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Apply migration to database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<ApplicationDbContext>();
    if (context.Database.GetPendingMigrations().Any())
    {
        context.Database.Migrate();
    }
}

app.Run();
