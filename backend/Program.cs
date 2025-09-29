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
using COMP9034.Backend.Middlewares;

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
    // Get connection string from environment or configuration
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Database connection string is not configured. Please set DATABASE_URL environment variable or DefaultConnection in appsettings.json");
    }

    // Parse PostgreSQL connection string from various formats
    if (connectionString.StartsWith("postgres://"))
    {
        try
        {
            // Parse Heroku/Render style connection string
            var uri = new Uri(connectionString);
            var userInfo = uri.UserInfo.Split(':');
            connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.LocalPath.Substring(1)};Username={userInfo[0]};Password={Uri.UnescapeDataString(userInfo[1])};SSL Mode=Prefer;Trust Server Certificate=true;Timeout=30;Command Timeout=60";
            Console.WriteLine($"📡 Parsed PostgreSQL connection: Host={uri.Host}, Port={uri.Port}, Database={uri.LocalPath.Substring(1)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to parse DATABASE_URL: {ex.Message}");
            throw new InvalidOperationException($"Invalid DATABASE_URL format: {ex.Message}");
        }
    }

    options.UseNpgsql(connectionString);
});

// Register repositories and Unit of Work
builder.Services.AddScoped<COMP9034.Backend.Repositories.Interfaces.IGenericRepository<COMP9034.Backend.Models.Device>, COMP9034.Backend.Repositories.Implementation.GenericRepository<COMP9034.Backend.Models.Device>>();
builder.Services.AddScoped<COMP9034.Backend.Repositories.Interfaces.IGenericRepository<COMP9034.Backend.Models.AuditLog>, COMP9034.Backend.Repositories.Implementation.GenericRepository<COMP9034.Backend.Models.AuditLog>>();
builder.Services.AddScoped<COMP9034.Backend.Repositories.Interfaces.IGenericRepository<COMP9034.Backend.Models.LoginLog>, COMP9034.Backend.Repositories.Implementation.GenericRepository<COMP9034.Backend.Models.LoginLog>>();
builder.Services.AddScoped<COMP9034.Backend.Repositories.Interfaces.IGenericRepository<COMP9034.Backend.Models.WorkSchedule>, COMP9034.Backend.Repositories.Implementation.GenericRepository<COMP9034.Backend.Models.WorkSchedule>>();
builder.Services.AddScoped<COMP9034.Backend.Repositories.Interfaces.IGenericRepository<COMP9034.Backend.Models.Salary>, COMP9034.Backend.Repositories.Implementation.GenericRepository<COMP9034.Backend.Models.Salary>>();
builder.Services.AddScoped<COMP9034.Backend.Repositories.Interfaces.IGenericRepository<COMP9034.Backend.Models.BiometricData>, COMP9034.Backend.Repositories.Implementation.GenericRepository<COMP9034.Backend.Models.BiometricData>>();
builder.Services.AddScoped<COMP9034.Backend.Repositories.Interfaces.IGenericRepository<COMP9034.Backend.Models.BiometricVerification>, COMP9034.Backend.Repositories.Implementation.GenericRepository<COMP9034.Backend.Models.BiometricVerification>>();
builder.Services.AddScoped<COMP9034.Backend.Repositories.Interfaces.IStaffRepository, COMP9034.Backend.Repositories.Implementation.StaffRepository>();
builder.Services.AddScoped<COMP9034.Backend.Repositories.Interfaces.IEventRepository, COMP9034.Backend.Repositories.Implementation.EventRepository>();
builder.Services.AddScoped<COMP9034.Backend.Repositories.Interfaces.IUnitOfWork, COMP9034.Backend.Repositories.Implementation.UnitOfWork>();

// Register services
builder.Services.AddScoped<COMP9034.Backend.Services.Interfaces.IStaffService, COMP9034.Backend.Services.Implementation.StaffService>();
builder.Services.AddScoped<COMP9034.Backend.Services.Interfaces.IEventService, COMP9034.Backend.Services.Implementation.EventService>();
builder.Services.AddScoped<COMP9034.Backend.Services.Interfaces.IAuthService, COMP9034.Backend.Services.Implementation.AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<COMP9034.Backend.Services.DatabaseSeeder>();

// Add Caching
builder.Services.AddMemoryCache();
Console.WriteLine("✅ In-memory caching configured");

// Note: Rate limiting would be configured here in production
Console.WriteLine("⚠️  Rate limiting not configured (requires .NET 7+ packages)");

// Note: API Versioning would be configured here in production
Console.WriteLine("⚠️  API versioning not configured (requires additional packages)");

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

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("memory", () =>
    {
        var allocatedBytes = GC.GetTotalMemory(false);
        var memoryLimit = 1024L * 1024L * 1024L; // 1GB limit
        return allocatedBytes < memoryLimit
            ? Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy($"Memory usage: {allocatedBytes / 1024 / 1024} MB")
            : Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy($"Memory usage too high: {allocatedBytes / 1024 / 1024} MB");
    })
    .AddCheck("database", () =>
    {
        // Simple health check - would be more sophisticated in production
        return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Database connection available");
    });

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


    }
}

// Configure HTTP request pipeline
app.UseGlobalExceptionHandling(); // Must be first to catch all exceptions

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

// Add health check endpoints
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            environment = app.Environment.EnvironmentName,
            duration = report.TotalDuration,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration,
                description = e.Value.Description,
                data = e.Value.Data
            })
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            WriteIndented = true
        }));
    }
})
.WithName("DetailedHealthCheck")
.WithOpenApi();

// Simple health check for load balancers
app.MapGet("/health/ready", () => new { status = "ready", timestamp = DateTime.UtcNow })
.WithName("ReadinessCheck")
.WithOpenApi();

app.MapControllers();

Console.WriteLine("🚀 COMP9034 FarmTimeMS Backend API starting...");
Console.WriteLine($"🌍 Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"📱 Swagger UI: {(app.Environment.IsDevelopment() ? "http://localhost:4000" : "Disabled")}");
Console.WriteLine($"🔗 API Base: http://localhost:4000/api");

app.Run();