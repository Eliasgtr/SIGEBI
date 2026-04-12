using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Sigebi.Api;
using Sigebi.Application;
using Sigebi.Infrastructure;
using Sigebi.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("SigebiClients", policy =>
    {
        policy.WithOrigins(
                "https://localhost:7174",
                "http://localhost:5056")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SigebiDbContext>();
    await db.Database.EnsureCreatedAsync().ConfigureAwait(false);
    await SigebiDbSeeder.SeedAsync(db).ConfigureAwait(false);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Sirve "/" y "/health" aquí (antes del enrutado de controladores) para evitar 404 por conflicto con [ApiController] u orden de endpoints.
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? string.Empty;
    if (path.Length > 1 && path.EndsWith('/'))
        path = path.TrimEnd('/');

    if (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method))
    {
        if (path is "" or "/")
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync(PublicPages.HomeHtml).ConfigureAwait(false);
            return;
        }

        if (string.Equals(path, "/health", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsJsonAsync(new { status = "ok", service = "Sigebi.Api" }).ConfigureAwait(false);
            return;
        }
    }

    await next(context).ConfigureAwait(false);
});

app.UseCors("SigebiClients");
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

app.Run();
