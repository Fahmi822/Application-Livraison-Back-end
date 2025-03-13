using Microsoft.EntityFrameworkCore;
using Application_Livraison_Backend.Data;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure; 


var builder = WebApplication.CreateBuilder(args);

// Ajouter la cha�ne de connexion de MySQL � partir de appsettings.json

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 30)) // Assurez-vous d'utiliser la bonne version
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
