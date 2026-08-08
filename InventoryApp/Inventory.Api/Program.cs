using Inventory.Api.Extensions;
using Inventory.Api.Middleware;
using Inventory.Infrastructure.Data;
using Inventory.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ── Application services (AutoMapper, FluentValidation) scanning Core assembly ─
builder.Services.AddApplicationServices();

// ── Infrastructure (EF Core, repositories) ───────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── JWT Authentication ───────────────────────────────────────────────────────
builder.Services.AddJwtAuthentication(builder.Configuration);

// ── Swagger / OpenAPI ────────────────────────────────────────────────────────
builder.Services.AddSwaggerWithJwt();

// ── CORS (allow all origins during development; lock down in production) ──────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ──────────────────────────────────────────────────────────────────────────────
var app = builder.Build();
// ──────────────────────────────────────────────────────────────────────────────

// ── Global exception handler (must be first) ─────────────────────────────────
app.UseMiddleware<ExceptionMiddleware>();

// ── Database & Migrations Initializer ────────────────────────────────────────
await DbInitializer.InitializeAsync(app.Services);

// ── Swagger UI ───────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "InventoryApp API v1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// ── Auth middleware ──────────────────────────────────────────────────────────
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
