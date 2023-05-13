using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemoDotNet.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zeebe.Client.Accelerator.Extensions;
using Zeebe.Client.Accelerator.Options;
using Zeebe.Client.Impl.Builder;

namespace DemoDotNet
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
            services.AddMemoryCache();
            services.AddControllersWithViews();
            services.AddTransient(typeof(TaskListClientProvider));
            services.AddTransient(typeof(TaskListService));
            services.AddTransient(typeof(ZeebeClientProvider));
            services.BootstrapZeebe(Configuration.GetSection("ZeebeConfiguration"),typeof(Program).Assembly);
    //        services.BootstrapZeebe(
    //Configuration.GetSection("ZeebeConfiguration"),
    //o => {
    //    o.Client = new ZeebeClientAcceleratorOptions.ClientOptions()
    //    {
    //        GatewayAddress = "2f2c62c7-173c-4846-a972-f63b564fe26f.dsm-1.zeebe.camunda.io:443",
    //        TransportEncryption = new ZeebeClientAcceleratorOptions.ClientOptions.TransportEncryptionOptions()
    //        {
    //            AccessTokenSupplier = CamundaCloudTokenProvider.Builder()
    //                .UseClientId("H-iQWktKbOnx5LfpOZrbQieVUpqVugIb")
    //                .UseClientSecret("fB3p1rYoseBonH3TQNsu68IHWLU6syVQE5QehI4eVA2KHJOH~CAEVIf-_9RHbo0Z")
    //                .UseAudience("2f2c62c7-173c-4846-a972-f63b564fe26f.dsm-1.zeebe.camunda.io")
    //                .Build()
    //        }
    //    };
    //},
    //typeof(Program).Assembly);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();



            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Task}/{action=Index}/{id?}");
            });
            
        }
    }
}

