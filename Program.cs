using BugTracker.Data;
using BugTracker.Extensions;
using BugTracker.Models;
using BugTracker.Services;
using BugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = DataUtility.GetConnectionString(builder.Configuration) ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<BTUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddClaimsPrincipalFactory<BTUserClaimsPrincipalFactory>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();

// Custom Service Registrations
builder.Services.AddScoped<IBTRolesService, BTRolesService>();
builder.Services.AddScoped<IBTCompanyService, BTCompanyService>();
builder.Services.AddScoped<IBTProjectService, BTProjectService>();
builder.Services.AddScoped<IBTFileService, BTFileService>();
builder.Services.AddScoped<IBTTicketService, BTTicketService>();
builder.Services.AddScoped<IBTInviteService, BTInviteService>();
builder.Services.AddScoped<IBTNotificationService, BTNotificationService>();
builder.Services.AddScoped<IBTTicketHistoryService, BTTicketHistoryService>();
builder.Services.AddScoped<IEmailSender, EmailService>();


//bind the email settings to the EmailSettings object
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));


builder.Services.AddMvc();

var app = builder.Build();

var scope = app.Services.CreateScope();

await DataUtility.ManageDataAsync(scope.ServiceProvider);

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

app.Use(async (context, next) =>
{

    switch (context.Response.StatusCode)
    {
        case 404:
            context.Request.Path = "/Home/Bug404";
            await next();
            break;
        case 500:
            context.Request.Path = "/Home/Bug500";
            await next();
            break;
        default:
            await next();
            break;
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "project",
    pattern: "Project/{slug}",
    defaults: new { controller = "Projects", action = "Details" }
    );

app.MapControllerRoute(
    name: "ticket",
    pattern: "Ticket/{slug}",
    defaults: new { controller = "Tickets", action = "Details" }
    );

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Landing}/{id?}");
app.MapRazorPages();

app.Run();
