using BluelinesPortal.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
// Explicitly declare the free Community License
QuestPDF.Settings.License = LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
// 1. Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 2. Identity Setup (MUST come before AddAuthentication/AddGoogle)
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
// 3. Google Authentication
builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    });
//builder.Services.AddRazorPages();
builder.Services.AddRazorPages(options =>
{
    // This locks down the entire Admin folder
    options.Conventions.AuthorizeFolder("/Admin", "RequireAdminRole");
});
// Register the Custom Email Service
builder.Services.AddTransient<BluelinesPortal.Services.IEmailService, BluelinesPortal.Services.EmailService>();
// Define the policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
// Run the Database Seeder
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await BluelinesPortal.Data.DbSeeder.SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database roles.");
    }
}
app.Run();
