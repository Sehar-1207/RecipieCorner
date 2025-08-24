using Microsoft.AspNetCore.Authentication.Cookies;
using FoodSecrets.Middleware;
using FoodSecrets.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ✅ Register HttpContextAccessor (needed for session access in controllers/views)
builder.Services.AddHttpContextAccessor();

// ✅ Register HttpClient factory (required if your services take IHttpClientFactory)
builder.Services.AddHttpClient();

// ✅ Add your services as Scoped
builder.Services.AddScoped<IAuthAccountService, AuthAccountService>();
builder.Services.AddScoped<IRecipeMvc, RecipeService>();
builder.Services.AddScoped<IIngredientMvc, IngredientService>();
builder.Services.AddScoped<RefreshTokenFilter>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<RefreshTokenFilter>();
});

// ✅ Add Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/AuthAccount/Login";   // redirect if not logged in
        options.LogoutPath = "/AuthAccount/Logout"; // logout path
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // keep cookie for 7 days
        options.SlidingExpiration = true; // refresh cookie on activity
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.Name = ".FoodSecrets.Auth";
    });


// ✅ Add Session
builder.Services.AddDistributedMemoryCache(); // Required for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Middleware order matters
app.UseAuthentication();  // <-- Cookie login
app.UseSession();         // <-- Session
app.UseAuthorization();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=RecipeUi}/{action=Index}/{id?}");

app.Run();
