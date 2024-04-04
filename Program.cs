using FileDistributionService.Entities;
using FileDistributionService.Filters;
using FileDistributionService.Middleware;
using FileDistributionService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "File Distribution Service", Version = "v1" });
    c.OperationFilter<FileUploadOperation>();
});
builder.Services.AddDbContext<FileDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.Configure<FileSettings>(builder.Configuration.GetSection("FileSettings"));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "File Distribution Service V1");
});


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseMiddleware<ContentTypeMiddleware>();

app.UseMiddleware<IPFilterMiddleware>();

app.UseMiddleware<FileFilterMiddleware>();

app.Run();
