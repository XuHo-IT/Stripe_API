var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Register Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});
var app = builder.Build();
app.UseCors("AllowAll");
// Configure the HTTP request pipeline

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Stripe API");
        options.RoutePrefix = string.Empty; // optional: serve UI at root (/)
    });

app.UseHttpsRedirection();
// Enable static files
app.UseStaticFiles();

// Enable default files (serve index.html by default)
app.UseDefaultFiles();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => "Stripe API is running! Visit /swagger for API documentation.");
app.Run();
