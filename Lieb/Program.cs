using Discord.OAuth2;
using Lieb.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

//builder.Services.AddDbContext<LiebContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("LiebContext")), ServiceLifetime.Transient);
builder.Services.AddDbContextFactory<LiebContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString("LiebContext")), ServiceLifetime.Transient);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddServerSideBlazor();

builder.Services.AddScoped<RaidService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<GuildWars2AccountService>();




//builder.Services.AddTransient<RaidService>();
//builder.Services.AddTransient<UserService>();
//builder.Services.AddTransient<GuildWars2AccountService>();

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    opt.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = DiscordDefaults.AuthenticationScheme;
})
    .AddCookie()
    .AddDiscord(x =>
    {
        x.AppId = builder.Configuration["Discord:AppId"];
        x.AppSecret = builder.Configuration["Discord:AppSecret"];

        x.SaveTokens = true;
    });

builder.Services.AddAuthorization(options =>
{
    foreach(string role in Constants.Roles.GetAllRoles())
    {
        options.AddPolicy(role, policy => policy.RequireClaim(Constants.ClaimType, role));
    }
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<LiebContext>();
    context.Database.EnsureCreated();
    DbInitializer.Initialize(context);
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
});

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
