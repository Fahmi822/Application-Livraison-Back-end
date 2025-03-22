using Application_Livraison_Backend.Data;
using Application_Livraison_Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Ajouter la cha�ne de connexion de MySQL � partir de appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 30)) // V�rifiez votre version MySQL
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

// Ajouter le service AuthService pour l'injection de d�pendances
builder.Services.AddScoped<AuthService>();

// Ajouter les services pour les contr�leurs API
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

// Cr�ation de l'application
var app = builder.Build();

// Gestion des erreurs et HSTS
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();  // Afficher les erreurs d�taill�es en mode dev
}

// Swagger (documentation de l'API)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Livraison v1");
});

// Application des r�gles CORS
app.UseCors("AllowAll");

// Redirection HTTPS si n�cessaire
// app.UseHttpsRedirection(); // D�commentez si vous souhaitez forcer HTTPS

app.UseRouting();

// Activer l'authentification et l'autorisation
app.UseAuthentication();
app.UseAuthorization();

// Mappage des contr�leurs API
app.MapControllers();

// Endpoint par d�faut pour v�rifier que l'application fonctionne bien
app.MapGet("/", () => "Bienvenue dans l'API de gestion de livraison !");

// Lancer l'application
app.Run();
