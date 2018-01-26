﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Project_storage.Data;
using System;
using System.Linq;

namespace Project_storage
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ProjectStorageContext>(options => options.UseSqlServer(Configuration.GetConnectionString("ProjectStorageConnection")));

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(options => options.AllowAnyOrigin());
            try
            {
                app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("x-hello-human", "Hoi Micheal & Thijs & Dennie");

                await next.Invoke();

            });
            }
            catch (Exception e)
            {
                app.Run(async context =>
                {
                    await context.Response.WriteAsync(e.ToString());
                });
            }

            using (var context = app.ApplicationServices.GetService<ProjectStorageContext>())
            {
                if (context.Database.GetPendingMigrations().Any())
                    context.Database.Migrate();
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "areas",
                    template: "{area:exists}/{controller=Products}/{action=Index}/{id?}");
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Products}/{action=Index}/{id?}");
            });
        }
    }
}
