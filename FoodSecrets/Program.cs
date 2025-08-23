using FoodSecrets.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure HttpClient for API access
builder.Services.AddHttpClient<IAuthAccountService, AuthAccountService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7230/"); // API base URL
});

// Example of other service
builder.Services.AddScoped<IRecipe, RecipeService>();

// Add session support
builder.Services.AddDistributedMemoryCache(); // Required for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20); // Session timeout
    options.Cookie.HttpOnly = true; // Prevent JavaScript access
    options.Cookie.IsEssential = true; // Required for GDPR compliance
});

// Build the app
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

// ✅ Session middleware must come before Authorization
app.UseSession();

app.UseAuthorization();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
