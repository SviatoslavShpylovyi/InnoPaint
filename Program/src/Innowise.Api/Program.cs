using BuildingBlocks.AspNetCore.Extensions;
using BuildingBlocks.AspNetCore.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

using Innowise.Application.Interfaces;
using Innowise.Application.Services;
using Innowise.Application.Validators;
using Innowise.Infrastructure.Data;
using Innowise.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("InnoPaint");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'InnoPaint' is missing. Configure it with user secrets or the ConnectionStrings__InnoPaint environment variable.");
}

builder.Services.AddDbContext<InnoPaintDbContext>(options => options.UseMySQL(connectionString));
builder.Services.AddScoped<IPaintRepository, PaintRepository>();
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.AddScoped<IStoreTransactionFactory, StoreTransactionFactory>();
builder.Services.AddScoped<IPaintService, PaintService>();
builder.Services.AddScoped<IBasketService, BasketService>();
builder.Services.AddScoped<PaintCatalogTools>();
builder.Services.AddScoped<IPaletteAssistantService, PaletteAssistantService>();
builder.Services.AddSingleton<IChatClient>(services =>
{
    var configuration = services.GetRequiredService<IConfiguration>();

    var apiKey = configuration["OpenAI:ApiKey"];
    if (string.IsNullOrWhiteSpace(apiKey))
    {
        throw new InvalidOperationException(
            "'OpenAI:ApiKey' is missing. Configure it with user secrets or the OpenAI__ApiKey environment variable.");
    }

    var model = configuration["OpenAI:Model"];
    if (string.IsNullOrWhiteSpace(model))
    {
        throw new InvalidOperationException(
            "'OpenAI:Model' is missing. Configure it with user secrets or the OpenAI__Model environment variable.");
    }

    return new ChatClient(model, apiKey).AsIChatClient();
});
builder.Services.AddMemoryCache();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalValidateModelFilter>();
});

builder.Services.AddGlobalExceptionHandling();
builder.Services.AddRequestValidation(typeof(CreatePaintRequestValidator).Assembly);
builder.Services.AddHealthChecks().AddDbContextCheck<InnoPaintDbContext>();

var app = builder.Build();

app.UseGlobalExceptionHandling();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
