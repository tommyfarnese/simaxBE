using Microsoft.AspNetCore.Identity;

namespace SiMax.Api.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        const string adminRole = "Admin";

        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            var roleResult = await roleManager.CreateAsync(
                new IdentityRole(adminRole));

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Impossibile creare il ruolo Admin.");
            }
        }

        var usersSection = configuration.GetSection("AdminBootstrap:Users");

        if (!usersSection.Exists())
        {
            throw new InvalidOperationException(
                "Configurazione AdminBootstrap:Users mancante.");
        }

        var users = usersSection.GetChildren().ToList();

        if (users.Count != 2)
        {
            throw new InvalidOperationException(
                "Devono essere configurati esattamente due amministratori.");
        }

        foreach (var userSection in users)
        {
            var email = userSection["Email"];
            var password = userSection["Password"];

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "Email e password degli amministratori sono obbligatorie.");
            }

            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(
                    user,
                    password);

                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Impossibile creare l'utente amministratore {email}.");
                }
            }

            if (!await userManager.IsInRoleAsync(user, adminRole))
            {
                var roleResult = await userManager.AddToRoleAsync(
                    user,
                    adminRole);

                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Impossibile assegnare il ruolo Admin a {email}.");
                }
            }
        }
    }
}