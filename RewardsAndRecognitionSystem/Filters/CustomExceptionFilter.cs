using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RewardsAndRecognitionSystem.Models;

namespace RewardsAndRecognitionSystem.Filters
{
    public class CustomExceptionFilter : IExceptionFilter
    {
        private readonly IModelMetadataProvider _metadataProvider;
        private readonly ILogger<CustomExceptionFilter> _logger;
        public CustomExceptionFilter(IModelMetadataProvider metadataProvider, ILogger<CustomExceptionFilter> logger)
        {
            _metadataProvider = metadataProvider;
            _logger = logger;
        }

       public void OnException(ExceptionContext context)
        {
            var actionDescriptor = context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;

            var controllerName = actionDescriptor?.ControllerName ?? "UnknownController";
            var actionName = actionDescriptor?.ActionName ?? "UnknownAction";

            var ex = context.Exception;
            var statusCode = GetStatusCode(ex);

            // Log based on status code (like your middleware LogError)
            switch (statusCode)
            {
                case 400:
                    _logger.LogWarning(ex, $"Status Code:{statusCode} => Bad request error occurred in {controllerName}/{actionName}.");
                    break;
                case 401:
                    _logger.LogWarning(ex, $"Status Code:{statusCode} => Unauthorized access in {controllerName}/{actionName}.");
                    break;
                case 404:
                    _logger.LogWarning(ex, $"Status Code:{statusCode} => Resource not found in {controllerName}/{actionName}.");
                    break;
                case 500:
                default:
                    _logger.LogError(ex, $"Status Code:{statusCode} => Internal server error in {controllerName}/{actionName}.");
                    break;
            }
            // Create error model
            var errorObject = new MessageObject
            {
                Message = context.Exception.Message,
                ErrorDescription = $"Error at Controller: {controllerName}, Method: {actionName}, Type: {context.Exception.GetType().FullName}"
            };

            // Create view result and pass the model to the view
            var viewResult = new ViewResult
            {
                ViewName = "Error",
                ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<MessageObject>(
                    _metadataProvider,
                    context.ModelState)
                {
                    Model = errorObject
                }
            };

            context.ExceptionHandled = true;
            context.Result = viewResult;
        }

        private int GetStatusCode(Exception ex)
        {
            return ex switch
            {
                ArgumentNullException => 400,
                UnauthorizedAccessException => 401,
                KeyNotFoundException => 404,
                _ => 500
            };
        }
    }

}
