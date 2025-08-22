using FoodSecrets.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri("https://localhost:7230/api/"); // 👈 replace with your API URL
});
builder.Services.AddScoped<IRecipe, RecipeService>();
// Add this to your Program.cs file
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddDistributedMemoryCache(); // Required for in-memory session provider
builder.Services.AddSession(options =>
{
    // Set a short timeout for easy testing.
    options.IdleTimeout = TimeSpan.FromMinutes(20);
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
app.UseCors();
app.UseRouting();

app.UseAuthorization();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
