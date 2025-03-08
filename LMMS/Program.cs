using LMMS.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()  // Add this line to support roles
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Create the Admin role if it doesn't exist
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    await CreateRolesAndAdminUser(roleManager, userManager);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=index}/{id?}");
app.MapRazorPages();

app.Run();

static async Task CreateRolesAndAdminUser(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
{
    // Create Admin role if it doesn't exist
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
    if (!await roleManager.RoleExistsAsync("Student"))
    {
        await roleManager.CreateAsync(new IdentityRole("Student"));
    }
    if (!await roleManager.RoleExistsAsync("Instructor"))
    {
        await roleManager.CreateAsync(new IdentityRole("Instructor"));
    }
    // Assign the Admin role to a specific user (change email as needed)
    var adminEmail = "admin@utb.com";
    var InstructorEmail = "Instructor@utb.com";
    var StudentEmail = "bh@utb.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    var InstructorUser = await userManager.FindByEmailAsync(InstructorEmail);
    var StudentUser = await userManager.FindByEmailAsync(StudentEmail);

    if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }


    if (InstructorUser != null && !await userManager.IsInRoleAsync(InstructorUser, "Instructor"))
    {
        await userManager.AddToRoleAsync(InstructorUser, "Instructor");
    }
    if (StudentUser != null && !await userManager.IsInRoleAsync(StudentUser, "Student"))
    {
        await userManager.AddToRoleAsync(StudentUser, "Student");
    }
}