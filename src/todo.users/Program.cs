﻿using System.Text.Json.Serialization;
using Amazon.SQS;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;
using todo.users.Clients;
using todo.users.model.Auth;
using todo.users.model.Notification;
using todo.users.model.Queue;
using todo.users.Services.Auth;
using todo.users.Services.Auth.Header;
using todo.users.Services.Auth.User;
using todo.users.Services.Notification;
using todo.users.Services.Queue;
using todo.users.Services.SocketService;
using todo.users.Services.Todo;
using todo.users.Services.User;

const string localHostUrl = "localhost:5173";
const string prodUrl = "huckandrose.com";
var builder = WebApplication.CreateBuilder(args);

// Ensure appsettings.json is loaded
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);
builder.Services.AddCognitoIdentity();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["Cognito:Authority"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Cognito:Authority"],
        ValidateIssuerSigningKey = true,
        ValidateAudience = false,
        RoleClaimType = "cognito:groups"
    };
});
builder.Services.Configure<CognitoSettings>(builder.Configuration.GetSection("Cognito"));
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.Converters.Add(new StringEnumConverter());
});
builder.Services.AddMemoryCache();
builder.Services.AddMvc(options => options.EnableEndpointRouting = false)
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    })
    .AddNewtonsoftJson(options =>
    {
        options.AllowInputFormatterExceptionMessages = false;
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IAuthHeaderProvider, AuthHeaderProvider>();
builder.Services.AddTransient<IAuthUserProvider, AuthUserProvider>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITodoService, TodoService>();

builder.Services.AddHttpClient();
builder.Services.AddHttpClient<IStorageServiceClient, StorageServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Todo.Storage:ServiceEndpointBase"] ?? string.Empty);
});

builder.Services.AddSingleton(new SqsData
{
    ToProcessQueueUrl = Environment.GetEnvironmentVariable("TO_PROC_QUEUE_URL") ?? string.Empty,
});
builder.Services.AddSingleton(new SnsData
{
    TopicArn = Environment.GetEnvironmentVariable("TOPIC_ARN") ?? string.Empty,
});
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddTransient<Connection>();
builder.Services.AddAWSService<IAmazonSQS>();
builder.Services.AddScoped<IQueueService, QueueService>();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "bearer"
                }
            },
            Array.Empty<string>()
        } 
    });
});
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseCors(x =>
{
    x.AllowAnyHeader()
        .AllowAnyMethod()
        .SetIsOriginAllowed(IsCorsOriginAllowed);
});
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/healthcheck");
app.MapControllers();
app.UseWebSockets();
app.Run();

bool IsCorsOriginAllowed(string origin)
{
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    string allowedHost;
    switch (env)
    {
        case "Development":
            allowedHost = localHostUrl;
            break;
        case "Production":
            allowedHost = prodUrl;
            break;
        default:
            return false;
    }
    var isAllowed = origin.StartsWith("http://" + allowedHost) || origin.StartsWith("https://" + allowedHost);
    return isAllowed;
}
