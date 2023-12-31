using BlogService.Models;
using BlogService.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);
// Register mongodb
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));

builder.Services.AddScoped<IMongoCollection<Blog>>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    var client = new MongoClient(settings.ConnectionURI);
    var database = client.GetDatabase(settings.DatabaseName);
    return database.GetCollection<Blog>(settings.CollectionName);
});

// Register repository, service layers
builder.Services.AddScoped<IBlogRepositoryLayer, BlogRepositoryLayer>();
builder.Services.AddScoped<IBlogServiceLayer, BlogServiceLayer>();

// Register redis cache
builder.Services.AddStackExchangeRedisCache(action =>{
    action.Configuration = builder.Configuration.GetConnectionString("DefaultRedisConnection");
});

// Add healthchecks for mongodb and redis
builder.Services.AddHealthChecks()
    .AddCheck<MongoDBHealthCheck>(
        name: "MongoDB Check", 
        failureStatus: HealthStatus.Unhealthy | HealthStatus.Unhealthy,
        tags: new string[] {"Blog service mongo db"}
    )

    .AddRedis(
        redisConnectionString: builder.Configuration.GetConnectionString("DefaultRedisConnection"),
        name: "Redis Check",
        failureStatus: HealthStatus.Unhealthy | HealthStatus.Unhealthy,
        tags: new string[] { "redis" }
    );

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.Run();
