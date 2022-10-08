using Microsoft.AspNetCore.Identity;

namespace FileServer.DbContext
{
    public static class DbInitializer
    {
        public static async Task SeedUsers(UserManager<IdentityUser>? userManager)
        {
            if (await userManager.FindByEmailAsync("alex@gmail.com").ConfigureAwait(false) == null)
            {
                var alex = new IdentityUser
                {
                    UserName = "alex@gmail.com",
                    Email = "alex@gmail.com"
                };

                var result = await userManager.CreateAsync(alex, "alexpassword").ConfigureAwait(false);
            }
        }
    }
}
