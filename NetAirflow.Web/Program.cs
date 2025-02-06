
using Dapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using NetAirflow.Web.Comun;
using Quartz;

namespace NetAirflow.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<DdlService>();

            //=======================================================================================================
            // Configuración de Quartz
            builder.Services.AddQuartz();

            // Registrar el servicio hospedado para que Quartz se inicie junto con la aplicación
            builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);


            builder.Services.AddHostedService<CronJobBackgroundService>();





            //=======================================================================================================


            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();


            //var ddlService = app.Services.GetRequiredService<DdlService>();
            //ddlService.LoadAll();


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

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
