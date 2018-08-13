using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Lykke.Service.KrakenAdapter.Services
{
    public static class KrakenExceptionsMiddleware
    {
        public static void UseForwardBitstampExceptionsMiddleware(this IApplicationBuilder app)
        {
            app.Use(SetStatusOnError);
        }

        private static async Task SetStatusOnError(HttpContext httpContext, Func<Task> next)
        {
            try
            {
                await next();
            }
            catch (KrakenApiException ex)
            {
                using (var body = new MemoryStream(Encoding.UTF8.GetBytes(ex.Message)))
                {
                    httpContext.Response.ContentType = "text/plain";
                    httpContext.Response.StatusCode = 500;
                    body.CopyTo(httpContext.Response.Body);
                }
            }
        }
    }
}
