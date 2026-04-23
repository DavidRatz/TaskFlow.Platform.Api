using TaskFlow.Platform.Domain.Abstractions;
using TaskFlow.Platform.Domain.Auth.Entities;
using TaskFlow.Platform.Domain.Users.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using TaskFlow.Platform.Persistence.Context;
using TaskFlow.Platform.Persistence.Seeding.Options;

namespace TaskFlow.Platform.Persistence.Seeding;

public sealed class UserSeeder(
    ApiContext context,
    UserManager<ApplicationUser> userManager,
    IOptions<AdminSeedOptions> adminSeedOptions)
    : ISeeder
{
    public async Task SeedAsync()
    {
        // ──────────────────────────────────────────────
        // Admin (configurable from appsettings.json)
        // ──────────────────────────────────────────────
        var adminOptions = adminSeedOptions.Value;
        if (adminOptions.Enabled)
        {
            await SeedUserAsync(adminOptions.Email, adminOptions.Password, "Admin", "CWF", "0470000001",
                new Address(Guid.NewGuid(), "1 Place Royale", "Bruxelles", "1000", "Belgique"));
        }

        // ──────────────────────────────────────────────
        // OrganizationManagers (owners of organizations)
        // ──────────────────────────────────────────────
        await SeedUserAsync("orgmanager.sophie@carwashflow.com", "password", "Sophie", "Dumont", "0470000002",
            new Address(Guid.NewGuid(), "12 Avenue Louise", "Bruxelles", "1050", "Belgique"));

        await SeedUserAsync("orgmanager.marc@carwashflow.com", "password", "Marc", "Janssen", "0470000003",
            new Address(Guid.NewGuid(), "45 Rue de la Station", "Liege", "4000", "Belgique"));

        // ──────────────────────────────────────────────
        // CarWashOwners (owners of partner car washes)
        // ──────────────────────────────────────────────
        await SeedUserAsync("carwashowner.pierre@carwashflow.com", "password", "Pierre", "Lambert", "0470000004",
            new Address(Guid.NewGuid(), "8 Boulevard du Souverain", "Bruxelles", "1170", "Belgique"));

        await SeedUserAsync("carwashowner.nathalie@carwashflow.com", "password", "Nathalie", "Peeters", "0470000005",
            new Address(Guid.NewGuid(), "23 Meir", "Anvers", "2000", "Belgique"));

        // ──────────────────────────────────────────────
        // Members of "Flotte Belge SA" (org owned by sophie)
        // ──────────────────────────────────────────────
        await SeedUserAsync("member.lucas@carwashflow.com", "password", "Lucas", "Martin", "0470000006",
            new Address(Guid.NewGuid(), "5 Rue Neuve", "Bruxelles", "1000", "Belgique"));

        await SeedUserAsync("member.emma@carwashflow.com", "password", "Emma", "Dubois", "0470000007",
            new Address(Guid.NewGuid(), "17 Rue du Midi", "Bruxelles", "1000", "Belgique"));

        await SeedUserAsync("member.thomas@carwashflow.com", "password", "Thomas", "Leroy", "0470000008",
            new Address(Guid.NewGuid(), "34 Chaussee de Waterloo", "Bruxelles", "1060", "Belgique"));

        await SeedUserAsync("member.julie@carwashflow.com", "password", "Julie", "Claes", "0470000009",
            new Address(Guid.NewGuid(), "9 Place Flagey", "Bruxelles", "1050", "Belgique"));

        // ──────────────────────────────────────────────
        // Members of "Transport Express Liege" (org owned by marc)
        // ──────────────────────────────────────────────
        await SeedUserAsync("member.antoine@carwashflow.com", "password", "Antoine", "Wouters", "0470000010",
            new Address(Guid.NewGuid(), "11 Place Saint-Lambert", "Liege", "4000", "Belgique"));

        await SeedUserAsync("member.clara@carwashflow.com", "password", "Clara", "Maes", "0470000011",
            new Address(Guid.NewGuid(), "27 Rue de l'Universite", "Liege", "4000", "Belgique"));

        await SeedUserAsync("member.nicolas@carwashflow.com", "password", "Nicolas", "Willems", "0470000012",
            new Address(Guid.NewGuid(), "3 Quai de la Batte", "Liege", "4000", "Belgique"));

        // ──────────────────────────────────────────────
        // Members of "Brussels Fleet Management" (org owned by admin)
        // ──────────────────────────────────────────────
        await SeedUserAsync("member.sarah@carwashflow.com", "password", "Sarah", "Jacobs", "0470000013",
            new Address(Guid.NewGuid(), "6 Rue de la Loi", "Bruxelles", "1000", "Belgique"));

        await SeedUserAsync("member.kevin@carwashflow.com", "password", "Kevin", "Mertens", "0470000014",
            new Address(Guid.NewGuid(), "14 Avenue de Tervueren", "Bruxelles", "1040", "Belgique"));

        await SeedUserAsync("member.laura@carwashflow.com", "password", "Laura", "Goossens", "0470000015",
            new Address(Guid.NewGuid(), "22 Boulevard Anspach", "Bruxelles", "1000", "Belgique"));

        // ──────────────────────────────────────────────
        // Standalone users (no organization, no partner)
        // ──────────────────────────────────────────────
        await SeedUserAsync("user.david@carwashflow.com", "password", "David", "Vermeersch", "0470000016",
            new Address(Guid.NewGuid(), "19 Grote Markt", "Gand", "9000", "Belgique"));

        await SeedUserAsync("user.marie@carwashflow.com", "password", "Marie", "Leclercq", "0470000017",
            new Address(Guid.NewGuid(), "7 Place du Jeu de Balle", "Bruxelles", "1000", "Belgique"));

        await SeedUserAsync("user.julien@carwashflow.com", "password", "Julien", "Renard", "0470000018",
            new Address(Guid.NewGuid(), "31 Rue Haute", "Bruxelles", "1000", "Belgique"));

        await SeedUserAsync("user.camille@carwashflow.com", "password", "Camille", "Fontaine", "0470000019",
            new Address(Guid.NewGuid(), "15 Place de Brouckere", "Bruxelles", "1000", "Belgique"));

        await SeedUserAsync("user.romain@carwashflow.com", "password", "Romain", "Delvaux", "0470000020",
            new Address(Guid.NewGuid(), "42 Rue de Namur", "Bruxelles", "1000", "Belgique"));
    }

    private async Task SeedUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string? phone,
        Address address)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            return;
        }

        context.Addresses.Add(address);
        await context.SaveChangesAsync(default);

        var identityUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true,
        };

        var createResult = await userManager.CreateAsync(identityUser, password);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create user '{email}': {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
        }

        var profile = new User(identityUser.Id, firstName, lastName);
        profile.SetPhone(phone);
        profile.SetAddress(address);

        context.UserProfiles.Add(profile);

        await context.SaveChangesAsync(default);
    }
}
