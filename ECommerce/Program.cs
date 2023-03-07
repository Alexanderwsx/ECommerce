using ECommerce.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ECommerce.DataAccess.Repository.IRepository;
using ECommerce.DataAccess.Repository;
using Stripe;
using ECommerce.Utility;

var builder = WebApplication.CreateBuilder(args);

//configure les services n�cessaires pour prendre en charge les
//contr�leurs MVC et les vues Razor dans une application ASP.NET Core.
builder.Services.AddControllersWithViews();

// configure Entity Framework Core pour qu'il utilise une base de donn�es SQL
// Server et une cha�ne de connexion sp�cifique pour l'application.
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")));

/*Toutes les propriétés de l'objet StripeSettings 
 * seront initialisées à partir des valeurs de configuration fournies 
 * dans la section "Stripe" du fichier de configuration de l'application.
 * De cette façon, l'objet StripeSettings peut être facilement injecté dans
 * d'autres classes qui en ont besoin, sans avoir à spécifier manuellement les
 * valeurs de configuration chaque fois que l'objet est utilisé.*/
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));



//pour ajouter des custom utilisateurs
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultUI();



//faciliter la gestion de transactions de base de donn�es complexes
//et garantir la consistance de vos donn�es en cas d'erreur. 
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

//addRazonRuntimeCompilation ca aide a �ffectuer les changement sans fermer les pages
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

//cookies pour rediriger 
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});



//activer les sessions
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});



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


/*Cela permet à la bibliothèque Stripe d'utiliser la clé d'API secrète 
 * correcte pour toutes les opérations d'API, en fonction de la configuration
 * de l'application. Cela permet également de séparer la configuration de l'application
 * de la logique de l'API Stripe, ce qui facilite la maintenance et
 * la mise à jour de l'application en cas de changement de clé d'API.*/
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
//

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapRazorPages(); //permet l'utilisation de razorpage

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();
