namespace FileDistributionService.Middleware
{
    public class ContentTypeMiddleware
    {
        private readonly RequestDelegate _next;

        public ContentTypeMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        
        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the request does not have a Content-Type header
            if (string.IsNullOrEmpty(context.Request.ContentType))
            {
                // Set the Content-Type header to a default value
                //context.Request.ContentType = "multipart/form-data";
                context.Request.ContentType = "application/x-www-form-urlencoded";
            }

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }

}
