using Innowise.Api;
using Innowise.Application;
using Innowise.Infrastructure.Persistence;
using Innowise.Api.Assistant;
using Microsoft.Extensions.AI;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;

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

builder.Services.AddMemoryCache();

 builder.Services.AddScoped<PaintCatalogTools>();
 builder.Services.AddScoped<PaletteAssistantService>();
 builder.Services.AddSingleton<IChatClient>(services =>
 {
     var configuration =
         services.GetRequiredService<IConfiguration>();

     var apiKey = configuration["OpenAI:ApiKey"]
         ?? throw new InvalidOperationException(
             "OpenAI:ApiKey is not configured.");

     var model = configuration["OpenAI:Model"]
         ?? throw new InvalidOperationException(
             "OpenAI:Model is not configured.");

     return new ChatClient(model, apiKey).AsIChatClient();
 });


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
