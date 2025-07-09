using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using RewardsAndRecognitionSystem.Models;

namespace RewardsAndRecognitionSystem.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IModelMetadataProvider _metadataProvider;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IModelMetadataProvider metadataProvider)
        {
            _next = next;
            _logger = logger;
            _metadataProvider = metadataProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var statusCode = GetStatusCode(ex);

                // Get controller and action names
                var controllerName = "UnknownController";
                var actionName = "UnknownAction";

                var endpoint = context.GetEndpoint();
                var descriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
                if (descriptor != null)
                {
                    controllerName = descriptor.ControllerName;
                    actionName = descriptor.ActionName;
                }

                // Log the error
                LogError(statusCode, ex, controllerName, actionName);

                // Prepare the error model
                var errorObject = new MessageObject
                {
                    Message = ex.Message,
                    ErrorDescription = $"Error at Controller: {controllerName}, Method: {actionName}, Type: {ex.GetType().FullName}"
                };

                // Render the error view
                await RenderErrorViewAsync(context, errorObject, statusCode);
            }
        }

        private int GetStatusCode(Exception ex)
        {
            return ex switch
            {
                ArgumentNullException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };
        }

        private void LogError(int statusCode, Exception ex, string controllerName, string actionName)
        {
            var message = $"Status Code:{statusCode} => Error in {controllerName}/{actionName}.";

            switch (statusCode)
            {
                case StatusCodes.Status400BadRequest:
                    _logger.LogWarning(ex, $"{message} Bad request.");
                    break;
                case StatusCodes.Status401Unauthorized:
                    _logger.LogWarning(ex, $"{message} Unauthorized access.");
                    break;
                case StatusCodes.Status404NotFound:
                    _logger.LogWarning(ex, $"{message} Resource not found.");
                    break;
                case StatusCodes.Status500InternalServerError:
                default:
                    _logger.LogError(ex, $"{message} Internal server error.");
                    break;
            }
        }

        private async Task RenderErrorViewAsync(HttpContext context, MessageObject errorObject, int statusCode)
        {
            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "text/html";

            var actionContext = new ActionContext(context, context.GetRouteData(), new ControllerActionDescriptor());

            var viewResult = new ViewResult
            {
                ViewName = "Error",
                ViewData = new ViewDataDictionary<MessageObject>(_metadataProvider, new ModelStateDictionary())
                {
                    Model = errorObject
                }
            };

            await viewResult.ExecuteResultAsync(actionContext);
        }
    }
}
