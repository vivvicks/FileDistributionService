using FileDistributionService.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Net;

namespace FileDistributionService.Middleware
{
    public class FileFilterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly FileSettings _fileSettings;

        public FileFilterMiddleware(RequestDelegate next, IOptions<FileSettings> fileSettings)
        {
            _next = next;
            _fileSettings = fileSettings.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if file upload/download is enabled
            if (!_fileSettings.EnableFileUpload || !_fileSettings.EnableFileDownload)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync("File upload/download is disabled.");
                return;
            }

            // Check file size limit
            var maxFileSizeMB = _fileSettings.MaxFileSizeMB;
            var contentLength = context.Request.ContentLength ?? 0;
            if (contentLength > maxFileSizeMB * 1024L * 1024L)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync($"File size exceeds the limit of {maxFileSizeMB} MB.");
                return;
            }

            // Check file type for upload
            if (context.Request.Path.StartsWithSegments("/api/files/upload", StringComparison.OrdinalIgnoreCase))
            {
                var allowedFileTypes = _fileSettings.AllowedFileTypes;
                if (context.Request.Form.Files.Count > 0)
                {
                    var fileExtension = Path.GetExtension(context.Request.Form.Files.FirstOrDefault().FileName);
                    if (!allowedFileTypes.Contains(fileExtension))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        await context.Response.WriteAsync($"File type {fileExtension} is not allowed.");
                        return;
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await context.Response.WriteAsync("No file uploaded.");
                    return;
                }
            }

            await _next(context);
        }
    }
}
