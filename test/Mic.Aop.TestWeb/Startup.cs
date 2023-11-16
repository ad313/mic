using Mic.Aop.TestWeb.InterceptorExtend;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SaiLing.WaterPlantAttendance.Services.Services.Common;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Mic.Aop.TestWeb
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

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mic.Aop.TestWeb", Version = "v1" });
            });

            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
            });

            services.AddMemoryCache();

            services.AddTransient<Log2Attribute>();

            //services.AddScoped<TaskSchedulerService, TaskSchedulerService_g>();

            //new TaskSchedulerService_g(services.BuildServiceProvider()).InitCalendar();

            //services.RegisterInjectionForMicAopTestWeb();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mic.Aop.TestWeb v1"));

            //lifetime.ApplicationStarted.Register(async () =>
            //{
            //    var service = app.ApplicationServices.GetRequiredService<ITestService>();
            //    service.GetNameSync();
            //    service.SetNameSync("haha", 123);
            //    await service.GetNameAsync("aop");
            //});

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    [JsonSerializable(typeof(DateTime))]
    internal partial class AppJsonSerializerContext : JsonSerializerContext
    {
       
    }

}
