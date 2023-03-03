using ECommerce.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.DataAccess.Repository;

var builder = WebApplication.CreateBuilder(args);

//configure les services n�cessaires pour prendre en charge les
//contr�leurs MVC et les vues Razor dans une application ASP.NET Core.
builder.Services.AddControllersWithViews();

// configure Entity Framework Core pour qu'il utilise une base de donn�es SQL
// Server et une cha�ne de connexion sp�cifique pour l'application.
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<IdentityUser>
    (options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();


//faciliter la gestion de transactions de base de donn�es complexes
//et garantir la consistance de vos donn�es en cas d'erreur. 
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

//addRazonRuntimeCompilation ca aide a �ffectuer les changement sans fermer les pages
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages(); //permet l'utilisation de razorpage

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();
