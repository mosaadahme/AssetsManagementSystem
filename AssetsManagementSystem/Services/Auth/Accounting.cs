using AssetsManagementSystem.DTOs.AccountServiceDTOs.AllUsers;

namespace AssetsManagementSystem.Services.Auth
{
    public class Accounting : BaseClassForServices
    {
        private readonly AuthRules authRules;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<Role> roleManager;
        private readonly ITokenService tokenService;
        private readonly IConfiguration configuration;
        private readonly ILogger<Accounting> logger;


        public Accounting(IUnitOfWork unitOfWork,
                          Others.Interfaces.IAutoMapper.IMapper mapper,
                          IHttpContextAccessor httpContextAccessor,
                          AuthRules authRules,
                          UserManager<User> userManager,
                          RoleManager<Role> roleManager,
                          ITokenService tokenService,
                          IConfiguration configuration
                          )
                    : base(unitOfWork, mapper, httpContextAccessor)
        {
            this.authRules = authRules;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.tokenService = tokenService;
            this.configuration = configuration;
         }

        #region AddNewUser

        public async Task AddUser(AddUserRequestDTO registerationRequest)
        {
            await authRules.UserShouldnotBeExistsAsync(await userManager.FindByEmailAsync(registerationRequest.Email));

            await authRules.RoleShouldBeExistsAsync(await roleManager.RoleExistsAsync(registerationRequest.Role.ToString()));

            User user = Mapper.Map<User, AddUserRequestDTO>(registerationRequest);

            user.UserName = registerationRequest.Email;
 
            user.SecurityStamp = Guid.NewGuid().ToString();

            user.AddedOnDate = DateTime.Now;
            user.UserStatus=UserStatus.Suspended.ToString();

            IdentityResult result = await userManager.CreateAsync(user, registerationRequest.Password);

            if (await authRules.OperationSucceed(result.Succeeded))
                await userManager.AddToRoleAsync(user, registerationRequest.Role.ToString());



        }

        #endregion


        //GetAllUSers
        public async Task<List<AllUsersResponse>> AllUsers ( )
        {
            var users = await userManager.Users.ToListAsync ( );
            //var result=new List<User>();
            //var result=await UnitOfWork.readRepository<User>().GetAllAsync();
            var result = users.Select(u => new AllUsersResponse()
            {
                FirstName = u.FirstName,
                LastName = u.LastName,
                UserStatus = u.UserStatus,
                UserId=u.Id,
                Email=u.Email
            });
            return result.ToList ( );
        }
        #region Login

        public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequest)
        {
            User? user = await userManager.FindByEmailAsync(loginRequest.Email);

            bool CheckPassword = await userManager.CheckPasswordAsync(user, loginRequest.Password);

            await authRules.EmailOrPasswordShouldnotbeInvalidAsync(user, CheckPassword);

            IList<string> Roles = await userManager.GetRolesAsync(user);

            JwtSecurityToken securityToken = await tokenService.CreateToken(user, Roles);
            string _RefreshToken = tokenService.GenerateRefreshToken();

            int.TryParse(configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

            user.RefreshToken = _RefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);
            user.UserStatus=UserStatus.Active.ToString();
            await userManager.UpdateAsync(user);
            await userManager.UpdateSecurityStampAsync(user);

            string Token = new JwtSecurityTokenHandler().WriteToken(securityToken);


            await userManager.SetAuthenticationTokenAsync(user, "Default", "AccessToken", Token);



            return new()
            {
                Token = Token,
                RefreshToken = _RefreshToken,
                Expiration = securityToken.ValidTo
            };
        }


        #endregion

        #region RefreshTokenGeneration

        public async Task<RefreshTokenResponseDTO> RefreshToken(RefreshTokenRequestDTO refreshTokenRequest)
        {
            ClaimsPrincipal? principal = tokenService.GetClaimsPrincipalFromExpiredToken(refreshTokenRequest.AccessToken);

            string? email = principal?.FindFirstValue(ClaimTypes.Email);

            User? user = await userManager.FindByEmailAsync(email);

            IList<string> roles = await userManager.GetRolesAsync(user);

            await authRules.RefreshTokenExpiryTimeShouldnotbeExpiredAsync(user?.RefreshTokenExpiryTime);

            JwtSecurityToken newSecurityToken = await tokenService.CreateToken(user, roles);

            string newRefreshToken = tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;

            await userManager.UpdateAsync(user);

            return new()
            {
                RefreshToken = newRefreshToken,
                AccessToken = new JwtSecurityTokenHandler().WriteToken(newSecurityToken)
            };

        }

        #endregion

        #region reset Password
        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            User? user = await userManager.FindByEmailAsync(email);

            await authRules.UserShouldBeFoundAsync(user);

            string token = await userManager.GeneratePasswordResetTokenAsync(user);

            return string.IsNullOrEmpty(token) ?
                     throw new Exception("Failed to generate password reset token.")
                    : token;
        }

        public async Task ResetPasswordAsync(ResetPasswordRequestDTO resetPasswordDTO)
        {
           User? user = await userManager.FindByEmailAsync(resetPasswordDTO.Email);

           await authRules.UserShouldBeFoundAsync(user);
                
           IdentityResult result = 
                    await userManager.ResetPasswordAsync(user, resetPasswordDTO.Token, resetPasswordDTO.NewPassword);

           await authRules.OperationSucceed(result.Succeeded);
            
           await userManager.UpdateSecurityStampAsync(user);
        }

        #endregion

        #region LogOut
        public async Task Logout(Guid userId)
        {
            User? user = await userManager.FindByIdAsync(userId.ToString());

            await authRules.UserShouldBeFoundAsync(user);

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = DateTime.MinValue;

            await userManager.UpdateSecurityStampAsync(user);

            var result = await userManager.UpdateAsync(user);

            await authRules.OperationSucceed(result.Succeeded);

        }

        #endregion


    }
}


