using Microsoft.OpenApi;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Vita.Api.Data;
using Vita.Api.Entities;
using Vita.Api.Repositories;
using Vita.Api.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Vita.Api.Middleware;
using DotNetEnv;

// Carga variables desde un archivo .env (solo existe en máquinas de desarrollo).
// Se cargan como variables de entorno ANTES de construir la configuración, para que
// ASP.NET Core las lea. En la jerarquía de config, las env vars ganan sobre los JSON.
var envDir = new DirectoryInfo(Directory.GetCurrentDirectory());
while (envDir is not null && !File.Exists(Path.Combine(envDir.FullName, ".env")))
    envDir = envDir.Parent;
if (envDir is not null)
    Env.Load(Path.Combine(envDir.FullName, ".env"));

var builder = WebApplication.CreateBuilder(args);

// DbContext → PostgreSQL usando la cadena 'Default' del appsettings
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// Identity → gestiona usuarios y roles sobre el ApplicationDbContext
builder.Services
    .AddIdentity<Usuario, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireDigit = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// JWT → valida los tokens que lleguen en el header Authorization
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // Conserva el claim "role" tal como lo emite AuthService (sin remapear a URI largas)
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!)),
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var mensaje = context.ModelState
                .Where(kv => kv.Value?.Errors.Count > 0)
                .SelectMany(kv => kv.Value!.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault() ?? "Datos inválidos.";

            return new BadRequestObjectResult(new { error = mensaje, statusCode = 400 });
        };
    });
builder.Services.AddEndpointsApiExplorer();

// ===== CORS SOLO PARA DESARROLLO =====
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowReactApp", policy =>
        {
            policy.SetIsOriginAllowed(origin =>
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                    return false;

                return uri.Host is "localhost" or "127.0.0.1";
            })
            .AllowAnyMethod()
            .AllowAnyHeader();
        });
    });
}
// ====================================

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pega tu token JWT (Swagger le agrega 'Bearer ' solo)."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document, null),
            new List<string>()
        }
    });
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ICourseOwnershipRepository, CourseOwnershipRepository>();
builder.Services.AddScoped<ICourseOwnershipService, CourseOwnershipService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

var app = builder.Build();

// Aplica migraciones; seed de desarrollo solo en Development
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var db = services.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    if (app.Environment.IsDevelopment())
    {
        await DbSeeder.SeedAsync(services);
    }
}

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS antes de HTTPS redirect y de auth (preflight OPTIONS en local)
if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowReactApp");
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
