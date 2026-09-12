var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Conference Room Booking API",
        Version = "v1",
        Description = "API для бронювання конференц-залів"
    });
});

// TODO (крок 4 плану): AddDbContext<AppDbContext> з PostgreSQL connection string
// TODO (крок 6-7 плану): реєстрація Application-сервісів (IPricingService, IBookingService, ...)
// TODO: FluentValidation auto-validation

var app = builder.Build();

// --- Middleware pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Conference Room Booking API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Простий health check — не потребує додаткових пакетів
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

// Зручність: відкриваючи корінь сайту, одразу потрапляємо на Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();

// Потрібно для WebApplicationFactory в integration-тестах
public partial class Program { }
