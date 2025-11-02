using LunkvayAPI.Auth.Models.Utils;
using LunkvayAPI.Auth.Services;
using LunkvayAPI.Avatars.Services;
using LunkvayAPI.Chats.Controllers;
using LunkvayAPI.Chats.Services;
using LunkvayAPI.Chats.Services.Interfaces;
using LunkvayAPI.Common.Filters;
using LunkvayAPI.Data;
using LunkvayAPI.Friends.Services;
using LunkvayAPI.Profiles.Services;
using LunkvayAPI.Users.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

namespace LunkvayAPI
{
    public class Program
    {
        private const string APPLICATION_CONNECTION_NAME = "DefaultConnection";

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureServices(builder);

            var app = builder.Build();

            ConfigureMiddleware(app);
            await InitializeDatabaseAsync(app);

            app.Run();
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            IServiceCollection services = builder.Services;
            ConfigurationManager configuration = builder.Configuration;

            services.AddDbContext<LunkvayDBContext>(options =>
                options.UseMySql(
                    configuration.GetConnectionString(APPLICATION_CONNECTION_NAME),
                    new MySqlServerVersion(new Version(8, 0))
                )
            );

            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(configuration["Jwt:Key"] 
                            ?? throw new ArgumentException("Отсуствует JWT ключ")
                            )
                        )
                    };
                    options.SaveToken = true;
                    options.MapInboundClaims = false;
                });

            services.AddAuthorization();

            services.AddCors(
                options => options.AddPolicy("Policy", policy => 
                    policy.WithOrigins(
                            "http://localhost:8080", 
                            "http://127.0.0.1:8080", 
                            "https://lexunok.github.io",
                            "https://lunkvay.lex48949.workers.dev"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                    )
            );

            //Auth
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            //Profiles
            services.AddScoped<IProfileService, ProfileService>();
            //Avatars
            services.AddScoped<IAvatarService, AvatarService>();
            services.AddHttpClient<IAvatarService, AvatarService>();
            //Friends
            services.AddScoped<IFriendshipsService, FriendshipsService>();
            services.AddScoped<IFriendshipLabelsService, FriendshipLabelsService>();
            //Chats
            services.AddScoped<IChatNotificationService, ChatNotificationService>();
            services.AddScoped<IChatImageService, ChatImageService>();
            services.AddHttpClient<IChatImageService, ChatImageService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IChatSystemService, ChatSystemService>();
            services.AddScoped<IChatMemberService, ChatMemberService>();
            services.AddScoped<IChatMemberSystemService, ChatMemberSystemService>();
            services.AddScoped<IChatMessageService, ChatMessageService>();
            services.AddScoped<IChatMessageSystemService, ChatMessageSystemService>();

            services.AddSignalR();

            services.AddControllers(options =>
            {
                options.Filters.Add<AuthorizationFilter>();
                options.Filters.Add<GlobalExceptionFilter>();
            });

            services.AddOpenApi("v1");
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors("Policy");
            app.MapHub<ChatHub>("/chatHub");
            app.MapControllers();
        }

        private static async Task InitializeDatabaseAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LunkvayDBContext>();

            await db.Database.MigrateAsync();

            //if (app.Environment.IsDevelopment())

            try
            {
                await SeedData.Initialize(db);
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogCritical(ex, "FATAL: Не удалось инициализировать тестовые данные");
                Environment.Exit(1);
            }

            app.MapOpenApi();
            app.MapScalarApiReference();
        }
    }
}
