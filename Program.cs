using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using WebApp.Db;
using WebApp.Models;
using WebApp.Services;

namespace WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            // ========== ВАЖЛИВО: ДОДАЄМО GOOGLE АВТЕНТИФІКАЦІЮ ==========
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/LoginIn";
                options.AccessDeniedPath = "/Error/AccessDenied";
                options.LogoutPath = "/Account/Logout";
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;
            })
            .AddGoogle(options => // ЦЕЙ РЯДОК ДОДАЙТЕ!
            {
                // Для тесту можете використати тестові значення
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
                options.CallbackPath = "/signin-google";
                options.SaveTokens = true;

                // Додаємо scope для отримання email
                options.Scope.Add("profile");
                options.Scope.Add("email");

                // Додаємо обробку подій для дебагу
                options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
                {
                    OnRemoteFailure = context =>
                    {
                        context.Response.Redirect("/Account/LoginIn?error=" +
                            Uri.EscapeDataString(context.Failure.Message));
                        context.HandleResponse();
                        return Task.CompletedTask;
                    },
                    OnAccessDenied = context =>
                    {
                        context.Response.Redirect("/Account/LoginIn?error=AccessDenied");
                        context.HandleResponse();
                        return Task.CompletedTask;
                    }
                };
            });


            // 2. Email служба
            builder.Services.Configure<SmtpGmailConfig>(builder.Configuration.GetSection("SmtpGmailConfig"));
            builder.Services.AddScoped<IEmailService, EmailService>();


            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<AgencyDBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("AgencyDBConnection")));

            //builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(
            //        option =>
            //        {
            //            option.LoginPath = new PathString("/Account/LoginIn");
            //            option.AccessDeniedPath = new PathString("/Error/AccessDenied");  
            //        }
            //);
                


            builder.Services.AddScoped<OptionModels>();
            builder.Services.AddScoped<TagModel>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AgencyDBContext>();

                // Створюємо базу якщо її немає
                context.Database.EnsureCreated();

                // Заповнюємо даними
                DbInitializer.Initialize(context);
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            


            app.UseAuthentication();
            app.UseAuthorization();


            


            app.MapControllers();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");


            app.Run();
        }
    }
}
