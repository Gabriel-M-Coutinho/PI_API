using LeadSearch.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PI_API.seeds;
using MongoDB.Driver;
using PI_API.db;
using PI_API.models;
using PI_API.services;
using PI_API.settings;
using System.Security.Claims;
using System.Text;

namespace PI_API
{
    public class Program
    {
        public static async Task Main(string[] args)   // ⬅️ AGORA É ASYNC
        {
            var builder = WebApplication.CreateBuilder(args);
            Stripe.StripeConfiguration.ApiKey = builder.Configuration.GetValue<string>("Stripe:SecretKey");
            builder.Services.AddControllers();

            // ScheduleService
            builder.Services.AddHostedService<ImporterScheduleService>();
            builder.Services.AddHostedService<BackupScheduleService>();

            //  MONGODB SETTINGS
            builder.Services.Configure<MongoDbSettings>(
                builder.Configuration.GetSection("MongoDbSettings"));

            builder.Services.AddSingleton<ContextMongodb>();

            ContextMongodb.ConnectionString = builder.Configuration.GetSection("MongoDbSettings:ConnectionString").Value;
            ContextMongodb.Database = builder.Configuration.GetSection("MongoDbSettings:DatabaseName").Value;
            ContextMongodb.IsSSL = true;

            // Serviços
            builder.Services.AddScoped<UserService>();
            builder.Services.AddSingleton<ImporterService>();
            builder.Services.AddScoped<BackupService>();
            builder.Services.AddScoped<DatabaseFormater>();
            builder.Services.AddScoped<CreditService>();

            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddSingleton<EmailService>();

            // IDENTITY + MONGODB
            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>(
                builder.Configuration["MongoDbSettings:ConnectionString"],
                builder.Configuration["MongoDbSettings:DatabaseName"])
            .AddDefaultTokenProviders();

            // JWT
            var jwtKey = builder.Configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new Exception("❌ Erro: A chave JWT (Jwt:Key) não foi configurada.");

            var jwtIssuer = builder.Configuration["Jwt:Issuer"];
            var jwtAudience = builder.Configuration["Jwt:Audience"];

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    RoleClaimType = ClaimTypes.Role,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/api"))
                            context.Token = accessToken;

                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            // SWAGGER
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Digite 'Bearer' + espaço + token JWT."
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
            });


            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapGet("/", () => Results.Redirect("/swagger"));

            // 🔥 CRIA AUTOMATICAMENTE AS ROLES E O ADMIN
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

                foreach (var enumValue in Enum.GetValues(typeof(ROLE)))
                {
                    string roleName = enumValue.ToString();

                    if (!await roleManager.RoleExistsAsync(roleName))  // ⬅️ await aqui
                    {
                        await roleManager.CreateAsync(new ApplicationRole   // ⬅️ await aqui
                        {
                            Name = roleName
                        });
                    }
                }

                string defaultAdminPassword = app.Configuration["AdminSettings:DefaultPassword"]
                    ?? throw new InvalidOperationException("AdminSettings:DefaultPassword não configurada.");

                // ⬅️ AGORA FUNCIONA
                await AdminSeeder.SeedRolesAndAdminUser(scope.ServiceProvider, defaultAdminPassword);
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
