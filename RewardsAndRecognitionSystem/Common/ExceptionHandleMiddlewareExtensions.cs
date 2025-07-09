using Microsoft.AspNetCore.Diagnostics;
using RewardsAndRecognitionSystem.Filters;

namespace RewardsAndRecognitionSystem.Common
{
    public static class ExceptionHandleMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandleMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomExceptionFilter>();
        }
    }

}
