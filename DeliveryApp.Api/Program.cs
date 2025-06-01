using System.Reflection;
using CSharpFunctionalExtensions;
using DeliveryApp.Api;
using DeliveryApp.Api.Adapters.BackgroundJobs;
using DeliveryApp.Core.Application.Commands.AssignOrder;
using DeliveryApp.Core.Application.Commands.CreateCourier;
using DeliveryApp.Core.Application.Commands.CreateOrder;
using DeliveryApp.Core.Application.Commands.MoveCouriers;
using DeliveryApp.Core.Application.Queries.GetBusyCouriers;
using DeliveryApp.Core.Application.Queries.GetCouriers;
using DeliveryApp.Core.Application.Queries.GetUncompletedOrders;
using DeliveryApp.Core.Domain.Services.Dispatch;
using DeliveryApp.Core.Ports;
using DeliveryApp.Infrastructure.Adapters.Grpc.AddressesService;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Migrations;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Serialization;
using OpenApi.Filters;
using OpenApi.Formatters;
using OpenApi.OpenApi;
using Primitives;
using Quartz;

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

// Domain Services
builder.Services.AddSingleton<IDispatchService, DispatchService>();

// DB, ORM
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

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Repositories
builder.Services.AddScoped<ICouriersRepository, CouriersRepository>();
builder.Services.AddScoped<IOrdersRepository, OrdersRepository>();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Commands
builder.Services.AddScoped<IRequestHandler<CreateOrderCommand, UnitResult<Error>>, CreateOrderCommandHandler>();
builder.Services.AddScoped<IRequestHandler<AssignOrderCommand, UnitResult<Error>>, AssignOrderCommandHandler>();

builder.Services.AddScoped<IRequestHandler<CreateCourierCommand, UnitResult<Error>>, CreateCourierCommandHandler>();
builder.Services.AddScoped<IRequestHandler<MoveCouriersCommand, UnitResult<Error>>, MoveCouriersCommandHandler>();

// Queries
builder.Services.AddScoped<IRequestHandler<GetUncompletedOrdersQuery, GetUncomplitedOrdersResponse>>(_ => 
    new GetUncompletedOrdersQueryHandler(connectionString));

builder.Services.AddScoped<IRequestHandler<GetCouriersQuery, GetCouriersResponse>>(_ => 
    new GetCouriersQueryHandler(connectionString));

builder.Services.AddScoped<IRequestHandler<GetBusyCouriersQuery, GetBusyCouriersResponse>>(_ => 
    new GetBusyCouriersQueryHandler(connectionString));

// CRON Jobs
builder.Services.AddQuartz(configure =>
{
    var assignOrdersJobKey = new JobKey(nameof(AssignOrdersJob));
    var moveCouriersJobKey = new JobKey(nameof(MoveCouriersJob));
    
    configure
        .AddJob<AssignOrdersJob>(assignOrdersJobKey)
        .AddTrigger(
            trigger => trigger.ForJob(assignOrdersJobKey)
                .WithSimpleSchedule(
                    schedule => schedule.WithIntervalInSeconds(1)
                        .RepeatForever()))
        .AddJob<MoveCouriersJob>(moveCouriersJobKey)
        .AddTrigger(
            trigger => trigger.ForJob(moveCouriersJobKey)
                .WithSimpleSchedule(
                    schedule => schedule.WithIntervalInSeconds(2)
                        .RepeatForever()));
});

builder.Services.AddQuartzHostedService();

// Http Handlers
builder.Services.AddControllers(options => { options.InputFormatters.Insert(0, new InputFormatterStream()); })
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        options.SerializerSettings.Converters.Add(new StringEnumConverter
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        });
    });

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("1.0.0", new OpenApiInfo
    {
        Title = "Delivery Service",
        Description = "Отвечает за диспетчеризацию доставки",
        Contact = new OpenApiContact
        {
            Name = "Kirill Vetchinkin",
            Url = new Uri("https://microarch.ru"),
            Email = "info@microarch.ru"
        }
    });
    options.CustomSchemaIds(type => type.FriendlyId(true));
    options.IncludeXmlComments(
        $"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}{Assembly.GetEntryAssembly()?.GetName().Name}.xml");
    options.DocumentFilter<BasePathFilter>("");
    options.OperationFilter<GeneratePathParamsValidationFilter>();
});

builder.Services.AddSwaggerGenNewtonsoftSupport();

// gRPC
builder.Services.AddScoped<IGeoService, GeoService>();


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

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseSwagger(c => { c.RouteTemplate = "openapi/{documentName}/openapi.json"; })
    .UseSwaggerUI(options =>
    {
        options.RoutePrefix = "openapi";
        options.SwaggerEndpoint("/openapi/1.0.0/openapi.json", "Swagger Delivery Service");
        options.RoutePrefix = string.Empty;
        options.SwaggerEndpoint("/openapi-original.json", "Swagger Delivery Service");
    });

app.UseCors();
app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.Run();