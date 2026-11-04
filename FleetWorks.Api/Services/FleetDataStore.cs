using FleetWorks.Api.Models;

namespace FleetWorks.Api.Services;

public sealed class FleetDataStore
{
    public List<UserAccount> Users { get; } = new();
    public List<TruckLocation> Trucks { get; } = new();

    public FleetDataStore()
    {
        SeedUsers();
        SeedTrucks();
    }

    public UserAccount? Authenticate(string email, string password)
    {
        return Users.FirstOrDefault(user =>
            string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(user.Password, password, StringComparison.Ordinal));
    }

    public UserAccount? GetUserByEmail(string email)
    {
        return Users.FirstOrDefault(user =>
            string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase));
    }

    private void SeedUsers()
    {
        Users.AddRange(new[]
        {
            new UserAccount
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FirstName = "Admin",
                LastName = "Fleet",
                Email = "admin@fleetworks.se",
                Password = "admin123",
                Role = UserRole.Admin,
            },
            new UserAccount
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                FirstName = "Maya",
                LastName = "Worker",
                Email = "user@fleetworks.se",
                Password = "user123",
                Role = UserRole.User,
            }
        });
    }

    private void SeedTrucks()
    {
        Trucks.AddRange(new[]
        {
            new TruckLocation
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                RegistrationNumber = "ABC 123",
                Name = "Forge Flame Grill",
                Area = "North Industrial Park",
                Latitude = 57.7089,
                Longitude = 11.9746,
                IsActive = true,
                UpdatedBy = "Admin Fleet",
                UpdatedAt = DateTime.UtcNow,
            },
            new TruckLocation
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                RegistrationNumber = "DEF 456",
                Name = "Shift Lunch Cart",
                Area = "Rivergate Works",
                Latitude = 57.7065,
                Longitude = 11.9820,
                IsActive = true,
                UpdatedBy = "Admin Fleet",
                UpdatedAt = DateTime.UtcNow,
            },
            new TruckLocation
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                RegistrationNumber = "GHI 789",
                Name = "Steel Bowl Express",
                Area = "South Yard",
                Latitude = 57.7008,
                Longitude = 11.9697,
                IsActive = true,
                UpdatedBy = "Admin Fleet",
                UpdatedAt = DateTime.UtcNow,
            }
        });
    }
}
