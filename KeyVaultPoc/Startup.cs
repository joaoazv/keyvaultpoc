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
        private IConfiguration Configuration { get; }

        private SecretClient SecretClient { get; set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            TokenCredential credential = new DefaultAzureCredential();
#if DEBUG
            // dotnet user-secrets
            string tenantId = Configuration["AZURE_TENANT_ID"];
            string clientId = Configuration["AZURE_CLIENT_ID"];
            string clientSecret = Configuration["AZURE_CLIENT_SECRET"];
            credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
#endif
            // appsettings.json
            string keyVaultUrl = Configuration["KeyVaultSettings:Url"];
            SecretClient = new SecretClient(new Uri(keyVaultUrl), credential);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();

            KeyVaultSecret secret = SecretClient.GetSecret("TestKey");
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
