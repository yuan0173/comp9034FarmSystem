using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using COMP9034.Backend.Data;
using COMP9034.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Clear JWT default claim mapping
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

// Add services to container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Handle circular references
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        // Better property naming
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Configure database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Use SQLite for all environments (development and production)
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
        ?? builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=Database/system.db";
    options.UseSqlite(connectionString);
});

// Register services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? 
    jwtSettings["SecretKey"] ?? 
    "0634178ecb250a5766e4d873595b429f"; // 与Render环境变量一致
var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? 
    jwtSettings["Issuer"] ?? 
    "COMP9034-FarmTimeMS";
var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? 
    jwtSettings["Audience"] ?? 
    "COMP9034-FarmTimeMS-Users";
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment(); // Enable HTTPS in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = "role"  // Use custom role claim
    };
});

builder.Services.AddAuthorization();

// Configure forwarded headers for real IP address detection
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    
    // Allow all private network ranges for development
    if (builder.Environment.IsDevelopment())
    {
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
        options.ForwardedForHeaderName = "X-Forwarded-For";
        options.ForwardedProtoHeaderName = "X-Forwarded-Proto";
        
        // Accept forwarded headers from any source in development
        options.ForwardLimit = null;
    }
    else
    {
        // Production: Configure known proxies and networks
        options.KnownProxies.Add(System.Net.IPAddress.Loopback);
        options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(System.Net.IPAddress.Parse("10.0.0.0"), 8));
        options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(System.Net.IPAddress.Parse("172.16.0.0"), 12));
        options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(System.Net.IPAddress.Parse("192.168.0.0"), 16));
    }
});

// 🌟 Industry standard: Dynamic CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // 🔧 Development environment: Allow any localhost port dynamically
            policy.SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrEmpty(origin)) return false;
                
                var uri = new Uri(origin);
                return uri.Host == "localhost" || 
                       uri.Host == "127.0.0.1" || 
                       uri.Host.StartsWith("192.168.") ||  // LAN
                       uri.Host.StartsWith("10.") ||       // Private network
                       uri.Host.StartsWith("172.");        // Private network
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
            
            Console.WriteLine("🔧 CORS: Development environment - Allow all local sources");
        }
        else
        {
            // 🚀 Production environment: Strict domain whitelist
            var allowedOrigins = builder.Configuration
                .GetSection("AllowedOrigins")
                .Get<string[]>() ?? Array.Empty<string>();
                
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
                  
            Console.WriteLine($"🚀 CORS: Production environment - Allowed domains: {string.Join(", ", allowedOrigins)}");
        }
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "COMP9034 FarmTimeMS API", 
        Version = "v1",
        Description = "Farm Time Management System Backend API",
        Contact = new OpenApiContact
        {
            Name = "COMP9034 Team",
            Email = "support@farmtimems.com"
        }
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
    // Add XML comments support
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Database already exists with migrated data - skip initialization
// using (var scope = app.Services.CreateScope())
// {
//     var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//     try 
//     {
//         context.Database.EnsureCreated();
//         Console.WriteLine("✅ Database initialization successful");
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine($"❌ Database initialization failed: {ex.Message}");
//     }
// }
Console.WriteLine("✅ Using existing database with migrated data");

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "COMP9034 FarmTimeMS API v1");
        c.RoutePrefix = ""; // Set Swagger UI as root path
    });
}

// HTTPS redirection (production environment)
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseForwardedHeaders(); // Must be early in the pipeline for IP forwarding
app.UseRouting();
app.UseCors("AllowFrontend");  // ✅ CORS must be before authentication, after routing
app.UseAuthentication();
app.UseAuthorization();

// Add health check endpoint
app.MapGet("/health", () => new { 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    version = "1.0.0",
    environment = app.Environment.EnvironmentName
})
.WithName("HealthCheck")
.WithOpenApi();

app.MapControllers();

Console.WriteLine("🚀 COMP9034 FarmTimeMS Backend API starting...");
Console.WriteLine($"🌍 Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"📱 Swagger UI: {(app.Environment.IsDevelopment() ? "http://localhost:4000" : "Disabled")}");
Console.WriteLine($"🔗 API Base: http://localhost:4000/api");

app.Run();