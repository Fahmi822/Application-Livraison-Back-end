using Application_Livraison_Backend.Data;
using Application_Livraison_Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Ajouter la chaîne de connexion de MySQL à partir de appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 30)) // Vérifiez votre version MySQL
    )
);

// Configuration CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// Ajouter Swagger pour faciliter les tests d'API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Ajouter le service AuthService pour l'injection de dépendances
builder.Services.AddScoped<AuthService>();
// Ajouter le service EmailService pour l'injection de dépendances
builder.Services.AddScoped<EmailService>();

// Ajouter les services pour les contrôleurs API
builder.Services.AddControllers();

// Configuration de l'authentification JWT
builder.Services.AddAuthentication(options =>
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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 Mo
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Création de l'application
var app = builder.Build();

// Gestion des erreurs et HSTS
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();  // Afficher les erreurs détaillées en mode dev
}

// Swagger (documentation de l'API)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Livraison v1");
});

// Application des règles CORS
app.UseCors("AllowAll");

// Redirection HTTPS si nécessaire
// app.UseHttpsRedirection(); // Décommentez si vous souhaitez forcer HTTPS

app.UseRouting();
app.UseStaticFiles();

// Activer l'authentification et l'autorisation
app.UseAuthentication();
app.UseAuthorization();

// Mappage des contrôleurs API
app.MapControllers();

// Endpoint par défaut pour vérifier que l'application fonctionne bien
app.MapGet("/", () => "Bienvenue dans l'API de gestion de livraison !");

// Lancer l'application
app.Run();
