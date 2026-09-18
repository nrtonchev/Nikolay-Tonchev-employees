using WebAPI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
            .WithHeaders("Content-Type", "Authorization", "Accept-Language")
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/api/extract-data-from-file", async (IFormFile file) =>
    {
        if (file.Length == 0)
        {
            return Results.BadRequest("Provided file is empty.");
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await DataExtractor.ExtractDataFromFile(stream);
            
            return Results.Ok(result);
        }
        catch (FormatException problem)
        {
            return Results.BadRequest(problem.Message);
        }
    })
    .DisableAntiforgery() 
    .WithName("ExtractDataFromFile");

app.UseCors();

app.Run();