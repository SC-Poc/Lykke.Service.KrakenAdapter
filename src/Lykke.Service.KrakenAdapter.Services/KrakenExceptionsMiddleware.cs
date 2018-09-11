using System;
using System.IO;
using System.Net;
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
                Respond(httpContext, HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private static void Respond(HttpContext httpContext, HttpStatusCode status, string message)
        {
            using (var body = new MemoryStream(Encoding.UTF8.GetBytes(message)))
            {
                httpContext.Response.ContentType = "text/plain";
                httpContext.Response.StatusCode = (int)status;
                body.CopyTo(httpContext.Response.Body);
            }
        }
    }
}
