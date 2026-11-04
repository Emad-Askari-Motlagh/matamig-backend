using System.Text.Json.Serialization;
using FleetWorks.Api.Models;
using FleetWorks.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<FleetDataStore>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "https://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("Frontend");
app.UseHttpsRedirection();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", service = "FleetWorks.Api" }));

app.MapPost("/api/auth/login", (LoginRequest request, FleetDataStore store) =>
{
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { message = "Email and password are required." });
    }

    var user = store.Authenticate(request.Email, request.Password);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    var token = $"demo-token-{user.Id}";
    var response = new AuthResponse(user.Id, user.FullName, user.Email, user.Role, token);
    return Results.Ok(response);
});

app.MapGet("/api/users", (FleetDataStore store) =>
{
    return Results.Ok(store.Users.Select(user => new
    {
        user.Id,
        user.FirstName,
        user.LastName,
        user.Email,
        user.Role,
        user.CreatedAt,
        user.UpdatedAt,
    }));
});

app.MapPost("/api/users", (CreateUserRequest request, FleetDataStore store) =>
{
    if (string.IsNullOrWhiteSpace(request.FirstName) ||
        string.IsNullOrWhiteSpace(request.LastName) ||
        string.IsNullOrWhiteSpace(request.Email) ||
        string.IsNullOrWhiteSpace(request.Password))
    {
        return Results.BadRequest(new { message = "All user fields are required." });
    }

    if (store.GetUserByEmail(request.Email) is not null)
    {
        return Results.Conflict(new { message = "A user with that email already exists." });
    }

    var user = new UserAccount
    {
        FirstName = request.FirstName,
        LastName = request.LastName,
        Email = request.Email,
        Password = request.Password,
        Role = request.Role,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
    };

    store.Users.Add(user);
    return Results.Created($"/api/users/{user.Id}", new
    {
        user.Id,
        user.FirstName,
        user.LastName,
        user.Email,
        user.Role,
    });
});

app.MapGet("/api/trucks", (FleetDataStore store) =>
{
    return Results.Ok(store.Trucks.OrderBy(truck => truck.Name));
});

app.MapPost("/api/trucks", (TruckLocationRequest request, FleetDataStore store) =>
{
    var adminUser = store.GetUserByEmail(request.AdminEmail);
    if (adminUser is null || adminUser.Role != UserRole.Admin || adminUser.Password != request.AdminPassword)
    {
        return Results.Unauthorized();
    }

    if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Area) || string.IsNullOrWhiteSpace(request.RegistrationNumber))
    {
        return Results.BadRequest(new { message = "Truck name, registration number and area are required." });
    }

    var truck = new TruckLocation
    {
        RegistrationNumber = request.RegistrationNumber,
        Name = request.Name,
        Area = request.Area,
        Latitude = request.Latitude,
        Longitude = request.Longitude,
        IsActive = request.IsActive,
        UpdatedBy = adminUser.FullName,
        UpdatedAt = DateTime.UtcNow,
    };

    store.Trucks.Add(truck);
    return Results.Created($"/api/trucks/{truck.Id}", truck);
});

app.MapPut("/api/trucks/{id:guid}", (Guid id, TruckLocationRequest request, FleetDataStore store) =>
{
    var adminUser = store.GetUserByEmail(request.AdminEmail);
    if (adminUser is null || adminUser.Role != UserRole.Admin || adminUser.Password != request.AdminPassword)
    {
        return Results.Unauthorized();
    }

    var truck = store.Trucks.FirstOrDefault(item => item.Id == id);
    if (truck is null)
    {
        return Results.NotFound(new { message = "Truck not found." });
    }

    truck.RegistrationNumber = string.IsNullOrWhiteSpace(request.RegistrationNumber) ? truck.RegistrationNumber : request.RegistrationNumber;
    truck.Name = request.Name;
    truck.Area = request.Area;
    truck.Latitude = request.Latitude;
    truck.Longitude = request.Longitude;
    truck.IsActive = request.IsActive;
    truck.UpdatedBy = adminUser.FullName;
    truck.UpdatedAt = DateTime.UtcNow;

    return Results.Ok(truck);
});

app.Run();
