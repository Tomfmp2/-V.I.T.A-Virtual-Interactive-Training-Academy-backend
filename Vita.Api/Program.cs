using Microsoft.OpenApi;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Vita.Api.Data;
using Vita.Api.Entities;
using Vita.Api.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Vita.Api.Middleware;

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
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!))
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
builder.Services.AddSwaggerGen( options =>
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

var app = builder.Build();

// Crea las tablas y siembra roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    //  Aplica migraciones (crea las tablas)
    var db = services.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    //  Siembra los roles base si no existen
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "Admin", "Instructor", "Estudiante" };
    foreach (var rol in roles)
    {
        if (!await roleManager.RoleExistsAsync(rol))
        {
            await roleManager.CreateAsync(new IdentityRole(rol));
        }
    }
}

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();   // primero autentica (¿quién eres?)
app.UseAuthorization();    // luego autoriza (¿puedes hacerlo?)
app.MapControllers();

app.Run();
