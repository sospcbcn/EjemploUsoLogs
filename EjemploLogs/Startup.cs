using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EjemploLogs.Services;
using Serilog;

namespace EjemploLogs
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        [Obsolete]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton<IEmailService, EmailService>();

            services.AddSingleton<Serilog.ILogger>(options =>
            {
            // Con este servicio de Terceros de Serilog se pueden almacenar Logs en SQL-Server.
            // Los valores DefaultConnection y TableName proceden de la sección ConnectionstringLogsDB de Appsettings
            // Con LogEnventLevel estamos definiendo la categoría mínima de Log que se guardará.
            // autoCreateSqlTable a True creará la tabla de Logs automáticamente en el caso de no existir.
                var connString = Configuration["ConnectionstringLogsDB:DefaultConnection"];
                var tableName = Configuration["ConnectionstringLogsDB:TableName"];
                return new LoggerConfiguration().WriteTo.MSSqlServer(connString, tableName,
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning,
                autoCreateSqlTable: true).CreateLogger();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // ILoggerFactory es una clase del  Proveedor de Terceros de Serilog.Extensions.
        // Gracias a este Proveedor podemos grabar Logs en archivos de Texto y en SQL-Server.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            // Creamos el Archivo de Log tomando la fecha actual como parte del nombre.
            loggerFactory.AddFile("Log-{Date}.txt");

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}
