using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Saba.Web.Data;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<HafezDbCOntext>(options =>
{
    options.UseSqlite("Data Source=hafezdata.sqlite");
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//caching
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder =>
        builder.Expire(TimeSpan.FromSeconds(3)));
    options.AddPolicy("short", builder =>
        builder.Expire(TimeSpan.FromSeconds(10)));
    options.AddPolicy("long", builder =>
        builder.Expire(TimeSpan.FromSeconds(60)));
});


//----------------------Rate Limiter--------------------------------------
//builder.Services.AddRateLimiter(rateLimiterOptions =>
//{
//    rateLimiterOptions.AddFixedWindowLimiter("slowdown", options =>
//    {
//        options.Window = TimeSpan.FromSeconds(1);
//        options.PermitLimit = 1;
//        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
//        options.QueueLimit = 10;
//    });
//});

var app = builder.Build();


app.UseOutputCache();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//----------------------Rate Limiter--------------------------------------
//app.UseRateLimiter();

app.MapGet("/api/fal", (HafezDbCOntext db) => {

    var count = db.Fals.Count();
    var skip = new Random().Next(1, count - 1);

    var fal = db.Fals.OrderBy(x=>x.Id).Skip(skip).FirstOrDefault();
    if(fal is not null)
        return Results.Ok(fal.Beit?.Split('*'));
    return Results.BadRequest();
    }
).CacheOutput()
    .WithOpenApi();

//.RequireRateLimiting("slowdown")


app.Run();

