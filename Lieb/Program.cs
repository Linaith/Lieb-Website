using Discord.OAuth2;
using Lieb.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

#if DEBUG
builder.Services.AddDbContextFactory<LiebContext>(opt =>
        //opt.UseSqlServer(builder.Configuration.GetConnectionString("LiebContext")).EnableSensitiveDataLogging(), ServiceLifetime.Transient);
        opt.UseSqlite(builder.Configuration.GetConnectionString("LiebContext")));
#else
builder.Services.AddDbContextFactory<LiebContext>(opt =>
        opt.UseMySql(builder.Configuration.GetConnectionString("LiebContext"), ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("LiebContext"))), ServiceLifetime.Transient);
#endif

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddServerSideBlazor();

builder.Services.AddScoped<RaidService>();
builder.Services.AddScoped<RaidTemplateService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<GuildWars2AccountService>();
builder.Services.AddScoped<GuildWars2BuildService>();
builder.Services.AddScoped<RaidRandomizerService>();
builder.Services.AddScoped<TimeZoneService>();
builder.Services.AddScoped<DiscordService>();
builder.Services.AddHostedService<TimerService>();

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
    options.AddPolicy(Constants.Roles.User.Name, policy => policy.RequireClaim(Constants.ClaimType, new List<string>(){
        Constants.Roles.User.Name, Constants.Roles.RaidLead.Name, Constants.Roles.Moderator.Name, Constants.Roles.Admin.Name}));

    options.AddPolicy(Constants.Roles.RaidLead.Name, policy => policy.RequireClaim(Constants.ClaimType, new List<string>() { 
        Constants.Roles.RaidLead.Name, Constants.Roles.Moderator.Name, Constants.Roles.Admin.Name }));

    options.AddPolicy(Constants.Roles.Moderator.Name, policy => policy.RequireClaim(Constants.ClaimType, new List<string>() { 
        Constants.Roles.Moderator.Name, Constants.Roles.Admin.Name }));

    options.AddPolicy(Constants.Roles.Admin.Name, policy => policy.RequireClaim(Constants.ClaimType, new List<string>() { 
        Constants.Roles.Admin.Name }));
});

builder.Services.AddHttpClient(Constants.HttpClientName , httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.Configuration["HttpClients:DiscordBot"]);

    // using Microsoft.Net.Http.Headers;
    // The GitHub API requires two headers.
    httpClient.DefaultRequestHeaders.Add(
        HeaderNames.Accept, "application/vnd.github.v3+json");
    httpClient.DefaultRequestHeaders.Add(
        HeaderNames.UserAgent, "HttpRequestsSample");
}).ConfigurePrimaryHttpMessageHandler(() => {
                  var handler = new HttpClientHandler();
                  if (builder.Environment.IsDevelopment())
                  {
                      handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                  }
                  return handler;
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
    context.Database.Migrate();
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
