using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ReverseProxy.Middleware;
using System;
using System.Threading.Tasks;

namespace Proxy
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            
            _configuration = configuration;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddReverseProxy()
                .LoadFromConfig(_configuration.GetSection("ReverseProxy"))
                .AddProxyConfigFilter<CustomConfigFilter>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app) =>
            app
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                        endpoints.MapReverseProxy(CustomReverseProxyMapper.MapReverseProxy);
                    });
    }

    public static class CustomReverseProxyMapper
    {
        public static void MapReverseProxy(IApplicationBuilder proxyPipeline) =>
            proxyPipeline
                .Use(SelectEndpoint)
                .UseAffinitizedDestinationLookup()
                .UseProxyLoadBalancing()
                .UseRequestAffinitizer();

        private static Task SelectEndpoint(HttpContext context, Func<Task> next)
        {
            var someCriteria = true; // MeetsCriteria(context);
            if (someCriteria)
            {
                var availableDestinationsFeature = context.Features.Get<IAvailableDestinationsFeature>();
                var destination = availableDestinationsFeature.Destinations[0]; // PickDestination(availableDestinationsFeature.Destinations);
                                                                                // Load balancing will no-op if we've already reduced the list of available destinations to 1.
                availableDestinationsFeature.Destinations = new[] { destination };

                Console.WriteLine($"Routing {context.Request.Path} to {destination.DestinationId}");
            }

            return next();
        }
    }
}