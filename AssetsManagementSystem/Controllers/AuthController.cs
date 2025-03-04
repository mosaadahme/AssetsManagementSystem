using AssetsManagementSystem.DTOs.AccountServiceDTOs.AddUserRequestDTOs;
using AssetsManagementSystem.DTOs.AccountServiceDTOs.LoginDTOs;
using AssetsManagementSystem.DTOs.AccountServiceDTOs.RefreshToken;
using AssetsManagementSystem.DTOs.AccountServiceDTOs.ResetPassword;

namespace AssetsManagementSystem.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly Accounting _accountingService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(Accounting accountingService, ILogger<AuthController> logger)
        {
            _accountingService = accountingService;
            _logger = logger;
        }

        #region AddNewUser

        [HttpPost("AddUser")]
     //   [Authorize(Roles ="Manager,Admin")]
         public async Task<IActionResult> AddUser([FromForm] AddUserRequestDTO registrationRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _accountingService.AddUser(registrationRequest);
                return Ok(new { Message = "User registered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during user registration: {ex.Message}");
                return StatusCode(500, new { Error = "User registration failed", Details = ex.Message });
            }
        }

        #endregion

        [HttpGet ("AllUsers")]
        public async Task<IActionResult> AllUsers ( )
        {
            var result = await _accountingService.AllUsers ( );
            return Ok ( result );
        }

        #region Login

        [HttpPost("Login")]
         public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _accountingService.Login(loginRequest);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during login: {ex.Message}");
                return Unauthorized(new { Error = "Login failed", Details = ex.Message });
            }
        }

        #endregion

        #region RefreshToken

        [HttpPost("RefreshToken")]
         public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO refreshTokenRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _accountingService.RefreshToken(refreshTokenRequest);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during token refresh: {ex.Message}");
                return Unauthorized(new { Error = "Token refresh failed", Details = ex.Message });
            }
        }

        #endregion

        #region ResetPassword

        [HttpPost("GeneratePasswordResetToken")]
         public async Task<IActionResult> GeneratePasswordResetToken([FromBody] ResetPasswordRequestDTO resetPasswordRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var token = await _accountingService.GeneratePasswordResetTokenAsync(resetPasswordRequest.Email);
                return Ok(new { ResetToken = token });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating password reset token: {ex.Message}");
                return StatusCode(500, new { Error = "Password reset token generation failed", Details = ex.Message });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO resetPasswordRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _accountingService.ResetPasswordAsync(resetPasswordRequest);
                return Ok(new { Message = "Password reset successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error resetting password: {ex.Message}");
                return StatusCode(500, new { Error = "Password reset failed", Details = ex.Message });
            }
        }

        #endregion

        #region Logout

        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            { 
                var userId = _accountingService.UserId;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Error = "Logout failed", Details = "User ID is missing" });
                }

                await _accountingService.Logout(Guid.Parse(userId));
                return Ok(new { Message = "Logout successful" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during logout: {ex.Message}");
                return StatusCode(500, new { Error = "Logout failed", Details = ex.Message });
            }
        }

        #endregion
    }
}
