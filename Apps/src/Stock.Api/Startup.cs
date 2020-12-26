using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SharedKernel.Api.ServiceCollectionExtensions;
using SharedKernel.Infrastructure;
using SharedKernel.Infrastructure.Caching;
using SharedKernel.Infrastructure.Cqrs.Commands;
using SharedKernel.Infrastructure.Cqrs.Queries;
using SharedKernel.Infrastructure.Events;
using Stock.Infrastructure;
using Stock.Infrastructure.Products.Validators;

namespace Stock.Api
{
    public class Startup
    {
        #region Comentado

        //public Startup(IConfiguration configuration)
        //{
        //    Configuration = configuration;
        //}

        //public IConfiguration Configuration { get; }

        //// This method gets called by the runtime. Use this method to add services to the container.
        //public void ConfigureServices(IServiceCollection services)
        //{
        //    services
        //        .AddSharedKernelApi<ProductValidators>(Configuration)
        //        .AddStockModule(Configuration,"nombreCadenaDeConexion")
        //        .AddDomainEventSubscribersInformation();

        //}

        //// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        //public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<OpenApiOptions> options)
        //{
        //    if (env.IsDevelopment())
        //    {
        //        app.UseDeveloperExceptionPage();
        //    }

        //    app.UseHttpsRedirection();

        //    app.UseRouting();

        //    app.UseAuthorization();

        //    app.UseEndpoints(endpoints =>
        //    {
        //        endpoints.MapControllers();
        //    });

        //    app.UseOpenApi(options);
        //}

        #endregion

        private const string CorsPolicy = "CorsPolicy";

        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddSharedKernel()
                .AddSharedKernelApi<GetProductQueryValidator>(CorsPolicy,
                    Configuration.GetSection("Origins").Get<string[]>())
                .AddSharedKernelHealthChecks()
                .AddSharedKernelOpenApi(Configuration)

                // CACH�
                //.AddRedisDistributedCache(Configuration)
                .AddInMemoryCache()

                // COMMAND BUS
                .AddInMemoryCommandBus()

                // QUERY BUS
                .AddInMemoryQueryBus()

                // EVENT BUS
                //.AddRabbitMqEventBus(Configuration)
                //.AddInMemoryEventBus()
                .AddRedisEventBus(Configuration)

                // MODULES
                .AddStockModule(Configuration, "StockConnection")

            // Register all domain event subscribers
            .AddDomainEventSubscribers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<OpenApiOptions> options)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(CorsPolicy);

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapHealthChecks("/health", new HealthCheckOptions
                    {
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });

                    endpoints.MapControllers();
                })
                .UseSharedKernelMetrics()
                .UseSharedKernelOpenApi(options);
        }
    }
}
