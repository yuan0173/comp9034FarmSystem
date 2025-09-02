using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
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
        ?? "Data Source=./Data/farmtimems-dev.db";
    options.UseSqlite(connectionString);
});

// Register services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? 
    Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? 
    "dev-only-key-change-in-production-32chars-min"; // Development environment default
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
        ValidIssuer = jwtSettings["Issuer"] ?? "COMP9034-FarmTimeMS-Dev",
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"] ?? "COMP9034-FarmTimeMS-Users-Dev",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = "role"  // Use custom role claim
    };
});

builder.Services.AddAuthorization();

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

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try 
    {
        context.Database.EnsureCreated();
        Console.WriteLine("✅ Database initialization successful");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database initialization failed: {ex.Message}");
    }
}

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