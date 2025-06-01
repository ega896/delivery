using System.Reflection;
using CSharpFunctionalExtensions;
using DeliveryApp.Api;
using DeliveryApp.Core.Application.Commands.AssignOrder;
using DeliveryApp.Core.Application.Commands.CreateOrder;
using DeliveryApp.Core.Application.Commands.MoveCouriers;
using DeliveryApp.Core.Application.Queries.GetBusyCouriers;
using DeliveryApp.Core.Application.Queries.GetUncompletedOrders;
using DeliveryApp.Core.Domain.Services.Dispatch;
using DeliveryApp.Core.Ports;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Migrations;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Primitives;

var builder = WebApplication.CreateBuilder(args);

// Health Checks
builder.Services.AddHealthChecks();

// Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.AllowAnyOrigin(); // Не делайте так в проде!
        });
});

// Configuration
builder.Services.ConfigureOptions<SettingsSetup>();
var connectionString = builder.Configuration["CONNECTION_STRING"];

builder.Services.AddSingleton<IDispatchService, DispatchService>();

var migrationsAssembly = typeof(Init).Assembly.GetName().Name;
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseNpgsql(connectionString,
            sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(migrationsAssembly);
            });
        
        options.EnableSensitiveDataLogging();
    }
);

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repositories
builder.Services.AddScoped<ICouriersRepository, CouriersRepository>();
builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Commands
builder.Services.AddScoped<IRequestHandler<CreateOrderCommand, UnitResult<Error>>, CreateOrderCommandHandler>();
builder.Services.AddScoped<IRequestHandler<MoveCouriersCommand, UnitResult<Error>>, MoveCouriersCommandHandler>();
builder.Services.AddScoped<IRequestHandler<AssignOrderCommand, UnitResult<Error>>, AssignOrderCommandHandler>();

// Queries
builder.Services.AddScoped<IRequestHandler<GetUncompletedOrdersQuery, GetUncomplitedOrdersResponse>>(_ => 
    new GetUncompletedOrdersQueryHandler(connectionString));

builder.Services.AddScoped<IRequestHandler<GetBusyCouriersQuery, GetBusyCouriersResponse>>(_ => 
    new GetBusyCouriersQueryHandler(connectionString));

var app = builder.Build();

// -----------------------------------
// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseHsts();

app.UseHealthChecks("/health");
app.UseRouting();

// Apply Migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.Run();