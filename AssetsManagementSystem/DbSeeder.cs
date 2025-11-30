using DocumentFormat.OpenXml.Bibliography;

namespace AssetsManagementSystem
{
    public static class DbSeeder
    {
        public static async Task SeedAsync ( IServiceProvider serviceProvider )
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext> ( );
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>> ( );
            var userManager = serviceProvider.GetRequiredService<UserManager<User>> ( );

            //await context.Database.MigrateAsync ( );



            // ✅ Roles
            string [] roles = { "Admin", "HR", "Employee", "Manager", "Auditor" };
            foreach ( var role in roles )
            {
                if ( !await roleManager.RoleExistsAsync ( role ) )
                    await roleManager.CreateAsync ( new Role { Name = role, NormalizedName = role.ToUpper ( ) } );
            }

            // ✅ User
            var adminEmail = "admin@namaa-il.com";
            var adminUser = await userManager.FindByEmailAsync ( adminEmail );
            if ( adminUser == null )
            {
                var newUser = new User
                {

                    FirstName = "مدير",
                    LastName = "النظام",
                    Email = adminEmail,
                    NormalizedEmail = adminEmail.ToUpper ( ),
                    UserName = adminEmail,
                    NormalizedUserName = adminEmail.ToUpper ( ),
                    EmailConfirmed = true,
                    UserStatus = UserStatus.Active.ToString ( ),
                    SecurityStamp = Guid.NewGuid ( ).ToString ( ),
                    //enter all others
                    AddedOnDate = DateTime.Now,
                    AccessFailedCount = 0,

                    PhoneNumber = "1234567890", // Example phone number, change as needed
                    ConcurrencyStamp = Guid.NewGuid ( ).ToString ( ),
                    IsDeleted = false
                };

                var result = await userManager.CreateAsync ( newUser, "Admin@123" );
                if ( result.Succeeded )
                {
                    await userManager.AddToRoleAsync ( newUser, "Admin" );
                }
            }
        }
    }
}
