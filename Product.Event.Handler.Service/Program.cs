

using Microsoft.EntityFrameworkCore;
using Product.Event.Handler.Service.Handlers;
using Product.Event.Handler.Service.Data;
using Shared.Services.Abstractions;
using Worker = Product.Event.Handler.Service.Services.EventStoreService;
using SharedEventStoreService = Shared.Services.EventStoreService;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<IEventStoreService, SharedEventStoreService>();
builder.Services.AddDbContextFactory<ProductDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ProductEventHandler>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();