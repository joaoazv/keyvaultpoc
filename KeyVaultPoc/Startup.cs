using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace KeyVaultPoc
{
    public class Startup
    {
        protected IConfiguration Configuration { get; }

        protected SecretClient _secretClient { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            SecretClientOptions options = new SecretClientOptions()
            {
                Retry = {
                    Delay= TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(16),
                    MaxRetries = 5,
                    Mode = RetryMode.Exponential
                }
            };

            TokenCredential credential = new DefaultAzureCredential();
#if DEBUG
            credential = new ClientSecretCredential(Configuration["AZURE_TENANT_ID"], Configuration["AZURE_CLIENT_ID"], Configuration["AZURE_CLIENT_SECRET"]);
#endif
            _secretClient = new SecretClient(new Uri(Configuration.GetValue<string>("KeyVaultSettings:Url")), credential, options);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();



            KeyVaultSecret secret = _secretClient.GetSecret("TestKey");

            string secretValue = secret.Value;

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync(secretValue);
                    // await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
