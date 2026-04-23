using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PetSearchHome_WEB.Application.Auth;
using PetSearchHome_WEB.Application.Catalog;
using PetSearchHome_WEB.Application.Favorites;
using PetSearchHome_WEB.Application.Listing;
using PetSearchHome_WEB.Application.Moderation;
using PetSearchHome_WEB.Application.Profiles;
using PetSearchHome_WEB.Application.Reviews;
using PetSearchHome_WEB.Application.Services;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Infrastructure.Logging;
using PetSearchHome_WEB.Infrastructure.Persistence;
using PetSearchHome_WEB.Infrastructure.Repositories;
using Serilog;
using System.Reflection;
using PetSearchHome_WEB.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
 configuration.ReadFrom.Configuration(context.Configuration));

// Load user secrets in Development
if (builder.Environment.IsDevelopment())
{
 builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true);
}

builder.Services.AddControllersWithViews();

builder.Services
 .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
 .AddCookie(options =>
 {
 options.LoginPath = "/Account/Login";
 options.LogoutPath = "/Account/Logout";
 options.AccessDeniedPath = "/Account/Login";
 });

builder.Services.AddAuthorization();

// configure caching
builder.Services.AddMemoryCache();

var baseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
 ?? throw new InvalidOperationException("DefaultConnection is not configured.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
 options.UseNpgsql(baseConnectionString));

// repository registrations
builder.Services.AddScoped<EfListingRepository>();
builder.Services.AddScoped<IListingRepository>(sp => sp.GetRequiredService<EfListingRepository>());
builder.Services.AddScoped<ISearchGateway>(sp => sp.GetRequiredService<EfListingRepository>());

if (builder.Environment.IsDevelopment())
{
 builder.Services.AddSingleton<IComplaintRepository, InMemoryComplaintRepository>();
}
else
{
 builder.Services.AddScoped<EfComplaintRepository>();
 builder.Services.AddScoped<IComplaintRepository>(sp => sp.GetRequiredService<EfComplaintRepository>());
}

builder.Services.AddScoped<ListingService>();
builder.Services.AddScoped<IAuditLogGateway, AuditLogGateway>();
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<IFavoriteRepository, EfFavoriteRepository>();
builder.Services.AddSingleton<IShelterRepository, InMemoryShelterRepository>();
builder.Services.AddSingleton<IReviewRepository, InMemoryReviewRepository>();
builder.Services.AddSingleton<IOrgStatsRepository, InMemoryOrgStatsRepository>();
builder.Services.AddSingleton<INotificationGateway, InMemoryNotificationGateway>();
builder.Services.AddSingleton<IPasswordHasher, SimplePasswordHasher>();
builder.Services.AddSingleton<IAuthTokenService, DummyAuthTokenService>();
builder.Services.AddSingleton<IModerationQueue, InMemoryModerationQueue>();

builder.Services.AddScoped<SearchAnimalsUseCase>();
builder.Services.AddScoped<ViewListingDetailUseCase>();
// inject IMemoryCache into EfListingRepository via constructor DI already
builder.Services.AddScoped<CreateListingUseCase>();
builder.Services.AddScoped<ListMyListingsUseCase>();
builder.Services.AddScoped<DeleteListingUseCase>();
builder.Services.AddScoped<EditListingUseCase>();
builder.Services.AddScoped<SubmitListingForModerationUseCase>();
builder.Services.AddScoped<LoginUseCase>();
builder.Services.AddScoped<RegisterUserUseCase>();
builder.Services.AddScoped<RegisterShelterUseCase>();
builder.Services.AddScoped<LogoutUseCase>();
builder.Services.AddScoped<ModerateListingUseCase>();
builder.Services.AddScoped<BlockUserUseCase>();
builder.Services.AddScoped<HandleComplaintUseCase>();
builder.Services.AddScoped<SubmitComplaintUseCase>();
builder.Services.AddScoped<SubmitUserComplaintUseCase>();
builder.Services.AddScoped<GetPendingListingsUseCase>();
builder.Services.AddScoped<GetOpenComplaintsUseCase>();
builder.Services.AddScoped<ViewProfileUseCase>();
builder.Services.AddScoped<ViewProfileDetailsUseCase>();
builder.Services.AddScoped<UpdateProfileUseCase>();
builder.Services.AddScoped<UpdateShelterProfileUseCase>();
builder.Services.AddScoped<ViewOrgStatsUseCase>();
builder.Services.AddScoped<LeaveReviewUseCase>();
builder.Services.AddScoped<ToggleFavoriteUseCase>();
builder.Services.AddScoped<ListFavoritesUseCase>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
 app.UseExceptionHandler("/Home/Error");
}
else
{
 app.UseExceptionHandler("/Home/Error");
}

app.UseSerilogRequestLogging();

if (!app.Environment.IsDevelopment())
{
 app.UseExceptionHandler("/Home/Error");
 app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<RequestTimingMiddleware>();

app.MapStaticAssets();

app.MapControllerRoute(
 name: "default",
 pattern: "{controller=Home}/{action=Index}/{id?}")
 .WithStaticAssets();

await app.RunAsync();
