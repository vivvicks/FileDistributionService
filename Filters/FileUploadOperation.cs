using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FileDistributionService.Filters
{
    public class FileUploadOperation : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var uploadEndpoints = new[] { "api/files/upload" }; // Add more upload endpoint paths if needed

            if (uploadEndpoints.Contains(context.ApiDescription.RelativePath, StringComparer.OrdinalIgnoreCase) &&
                context.ApiDescription.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content =
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties =
                                {
                                    ["file"] = new OpenApiSchema { Type = "string", Format = "binary" },
                                    ["uploadedBy"] = new OpenApiSchema { Type = "string" }
                                }
                            }
                        }
                    }
                };
            }
            if (context.ApiDescription.RelativePath.StartsWith("api/files/download/"))
            {
                // Set the content type for the download operation
                operation.Responses["200"].Content["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    }
                };
            }
        }
    }
}
