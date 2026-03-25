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
using PetSearchHome_WEB.Infrastructure.Repositories;
using Serilog;
using Microsoft.EntityFrameworkCore;
using PetSearchHome_WEB.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.Cookies;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));
// Add services to the container.
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
builder.Services.AddSingleton<InMemoryListingRepository>();
builder.Services.AddSingleton<IListingRepository>(sp => sp.GetRequiredService<InMemoryListingRepository>());
builder.Services.AddSingleton<ISearchGateway>(sp => sp.GetRequiredService<InMemoryListingRepository>());
builder.Services.AddScoped<ListingService>();

builder.Services.AddScoped<IAuditLogGateway, AuditLogGateway>();
builder.Services.AddScoped<SearchAnimalsUseCase>();
builder.Services.AddScoped<ViewListingDetailUseCase>();

var baseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is not configured.");
var dbPassword = builder.Configuration["Database:Password"];
var connectionString = string.IsNullOrWhiteSpace(dbPassword)
    ? baseConnectionString
    : new NpgsqlConnectionStringBuilder(baseConnectionString)
    {
        Password = dbPassword
    }.ConnectionString;

builder.Services.AddDbContext<ApplicationDbContext>(options =>
       options.UseNpgsql(connectionString));

// --- Реєстрація Infrastructure (Репозиторії та Сервіси) ---
builder.Services.AddSingleton<IListingRepository, PetSearchHome_WEB.Infrastructure.Repositories.InMemoryListingRepository>();
builder.Services.AddSingleton<IAuditLogGateway, PetSearchHome_WEB.Infrastructure.Logging.AuditLogGateway>();

builder.Services.AddSingleton<ISearchGateway, PetSearchHome_WEB.Infrastructure.Repositories.InMemorySearchGateway>();
builder.Services.AddScoped<IUserRepository, PetSearchHome_WEB.Infrastructure.Repositories.EfUserRepository>();
builder.Services.AddSingleton<IShelterRepository, PetSearchHome_WEB.Infrastructure.Repositories.InMemoryShelterRepository>();
builder.Services.AddSingleton<IComplaintRepository, PetSearchHome_WEB.Infrastructure.Repositories.InMemoryComplaintRepository>();
builder.Services.AddSingleton<IFavoriteRepository, PetSearchHome_WEB.Infrastructure.Repositories.InMemoryFavoriteRepository>();
builder.Services.AddSingleton<IReviewRepository, PetSearchHome_WEB.Infrastructure.Repositories.InMemoryReviewRepository>();
builder.Services.AddSingleton<IOrgStatsRepository, PetSearchHome_WEB.Infrastructure.Repositories.InMemoryOrgStatsRepository>();
builder.Services.AddSingleton<INotificationGateway, PetSearchHome_WEB.Infrastructure.Repositories.InMemoryNotificationGateway>();
builder.Services.AddSingleton<IPasswordHasher, PetSearchHome_WEB.Infrastructure.Repositories.SimplePasswordHasher>();
builder.Services.AddSingleton<IAuthTokenService, PetSearchHome_WEB.Infrastructure.Repositories.DummyAuthTokenService>();
builder.Services.AddSingleton<IModerationQueue, PetSearchHome_WEB.Infrastructure.Repositories.InMemoryModerationQueue>();
// --- Реєстрація UseCases (Application Layer) ---

// Catalog & Listings
builder.Services.AddScoped<SearchAnimalsUseCase>();
builder.Services.AddScoped<ViewListingDetailUseCase>();
builder.Services.AddScoped<CreateListingUseCase>();
builder.Services.AddScoped<ListMyListingsUseCase>();
builder.Services.AddScoped<DeleteListingUseCase>();
builder.Services.AddScoped<EditListingUseCase>();
builder.Services.AddScoped<SubmitListingForModerationUseCase>();

// Auth (Account)
builder.Services.AddScoped<LoginUseCase>();
builder.Services.AddScoped<RegisterUserUseCase>();
builder.Services.AddScoped<RegisterShelterUseCase>();
builder.Services.AddScoped<LogoutUseCase>();

// Moderation (Admin)
builder.Services.AddScoped<ModerateListingUseCase>();
builder.Services.AddScoped<BlockUserUseCase>();
builder.Services.AddScoped<HandleComplaintUseCase>();
builder.Services.AddScoped<GetPendingListingsUseCase>();
builder.Services.AddScoped<GetOpenComplaintsUseCase>();

// Profiles & Reviews
builder.Services.AddScoped<ViewProfileUseCase>();
builder.Services.AddScoped<UpdateProfileUseCase>();
builder.Services.AddScoped<UpdateShelterProfileUseCase>();
builder.Services.AddScoped<ViewOrgStatsUseCase>();
builder.Services.AddScoped<LeaveReviewUseCase>();

// Favorites
builder.Services.AddScoped<ToggleFavoriteUseCase>();
builder.Services.AddScoped<ListFavoritesUseCase>();



var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

await app.RunAsync();
