using Microsoft.IdentityModel.Tokens;
using System.Net;

namespace FileDistributionService.Middleware
{
    public class IPFilterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public IPFilterMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var allowedIPAddresses = _configuration.GetSection("AllowedIPAddresses").Get<string[]>();
            var remoteIPAddress = context.Connection.RemoteIpAddress;

            if (!remoteIPAddress.Equals(IPAddress.IPv6Loopback)) 
            {
                if (allowedIPAddresses != null && !allowedIPAddresses.Any(ip => IPAddress.Parse(ip).Equals(remoteIPAddress)))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }
            }
            

            await _next(context);
        }
    }

}
