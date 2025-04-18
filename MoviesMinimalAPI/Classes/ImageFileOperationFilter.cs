using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using MoviesMinimalAPI.DTOs;

namespace MoviesMinimalAPI.Classes
{
    public class ImageFileOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var parameter = context.MethodInfo.GetParameters()
                .FirstOrDefault(p => p.ParameterType == typeof(MovieCreateDTO) ||
                                    p.ParameterType == typeof(MovieUpdateDTO));

            if (parameter == null)
                return;

            var isUpdateDTO = parameter.ParameterType == typeof(MovieUpdateDTO);
            var schemaProperties = new Dictionary<string, OpenApiSchema>();

            if (isUpdateDTO)
            {
                schemaProperties["IdMovie"] = new OpenApiSchema
                {
                    Type = "integer",
                    Format = "int32"
                };
            }

            schemaProperties["Name"] = new OpenApiSchema { Type = "string" };
            schemaProperties["Synopsis"] = new OpenApiSchema { Type = "string" };
            schemaProperties["Duration"] = new OpenApiSchema { Type = "integer", Format = "int32" };
            schemaProperties["AllPublic"] = new OpenApiSchema { Type = "boolean" };
            schemaProperties["CategoryId"] = new OpenApiSchema { Type = "integer", Format = "int32" };
            schemaProperties["ImageFile"] = new OpenApiSchema { Type = "string", Format = "binary" };

            operation.RequestBody = new OpenApiRequestBody
            {
                Content =
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = schemaProperties
                        }
                    }
                }
            };
        }
    }
}
