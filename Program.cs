using Microsoft.AspNetCore.Authentication.Cookies;

using Microsoft.EntityFrameworkCore;
using WebApp.Db;
using WebApp.Models;

namespace WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<AgencyDBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("AgencyDBConnection")));

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(
                    option =>
                    {
                        option.LoginPath = new PathString("/Account/LoginIn");
                        option.AccessDeniedPath = new PathString("/Error/AccessDenied");  
                    }
            );
                


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


            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "account",
                    pattern: "Account/{action=Login}",
                    defaults: new { controller = "Account" });
            });

            app.Run();
        }
    }
}
