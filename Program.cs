using Microsoft.EntityFrameworkCore;
using Application_Livraison_Backend.Data;

var builder = WebApplication.CreateBuilder(args);

// Ajouter la cha�ne de connexion de MySQL � partir de appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(
        builder.Configuration.GetConnectionString("DefaultConnection") // Connexion � la base de donn�es MySQL
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
