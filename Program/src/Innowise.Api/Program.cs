using Innowise.Api;
using Innowise.Application;
using Innowise.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<StoreExceptionHandler>();
var connectionString = builder.Configuration.GetConnectionString("InnoPaint")
    ?? throw new InvalidOperationException("Connection string 'InnoPaint' is not configured.");
builder.Services.AddDbContextFactory<InnoPaintDbContext>(options => options.UseMySQL(connectionString));
builder.Services.AddSingleton<IStoreRepository, EfStoreRepository>();
builder.Services.AddSingleton<PaintStoreService>();
builder.Services.AddHealthChecks().AddDbContextCheck<InnoPaintDbContext>();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
