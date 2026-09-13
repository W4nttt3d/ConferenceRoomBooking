using ConferenceRoomBooking.Api.ExceptionHandling;
using ConferenceRoomBooking.Api.Filters;
using ConferenceRoomBooking.Application;
using ConferenceRoomBooking.Infrastructure;
using ConferenceRoomBooking.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Conference Room Booking API",
        Version = "v1",
        Description = "API для пошуку доступних конференц-залів, бронювання " +
            "та розрахунку вартості оренди залежно від часу й обраних послуг. " +
            "Усі помилки повертаються у форматі ProblemDetails (RFC 7807)."
    });

    // XML-коментарі (<summary>, <param>, <response>, <example>) з контролерів API-проєкту.
    var apiXmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXmlFile);
    if (File.Exists(apiXmlPath))
    {
        options.IncludeXmlComments(apiXmlPath);
    }

    var applicationXmlFile = "ConferenceRoomBooking.Application.xml";
    var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXmlFile);
    if (File.Exists(applicationXmlPath))
    {
        options.IncludeXmlComments(applicationXmlPath);
    }
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ProblemDetails для всіх контролерів в одному місці. AddProblemDetails()
// дає той самий формат і для помилок, які генерує сам ASP.NET Core
// (наприклад, 404 для невідомого route), а не тільки для наших винятків.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Має стояти якомога раніше в конвеєрі - щоб перехоплювати винятки з
// усіх наступних middleware та контролерів.
app.UseExceptionHandler();

// Seed початкових даних (зали, послуги) - ідемпотентно, безпечно при кожному старті.
// Тільки для Development: у production наповнення бази - окрема відповідальність (міграції + DBA).
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.SeedAsync(dbContext);
}

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

// Простий health check - не потребує додаткових пакетів
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

// Зручність: відкриваючи корінь сайту, одразу потрапляємо на Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.Run();

// Потрібно для WebApplicationFactory в integration-тестах
public partial class Program { }