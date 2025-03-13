using Microsoft.EntityFrameworkCore;
using Application_Livraison_Backend.Data;

var builder = WebApplication.CreateBuilder(args);

// Ajouter la chaîne de connexion de MySQL à partir de appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(
        builder.Configuration.GetConnectionString("DefaultConnection") // Connexion à la base de données MySQL
    )
);

// Ajouter les services Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();
app.Run();
