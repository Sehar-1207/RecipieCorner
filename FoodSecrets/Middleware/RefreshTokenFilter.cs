using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;

namespace FoodSecrets.Middleware
{
    public class RefreshTokenFilter : IActionFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthAccountService _authService;

        public RefreshTokenFilter(IHttpContextAccessor httpContextAccessor, IAuthAccountService authService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authService = authService;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var accessToken = session.GetString("AccessToken");
            var refreshToken = session.GetString("RefreshToken");

            if (!string.IsNullOrEmpty(accessToken))
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var expires = jwt.ValidTo;

                // If token expires in less than 1 minute
                if (expires < DateTime.UtcNow.AddMinutes(1) && !string.IsNullOrEmpty(refreshToken))
                {
                    var result = _authService.RefreshTokenAsync(refreshToken).GetAwaiter().GetResult();

                    if (result != null && result.Token != null)
                    {
                        session.SetString("AccessToken", result.Token.AccessToken);

                        if (!string.IsNullOrEmpty(result.Token.RefreshToken))
                            session.SetString("RefreshToken", result.Token.RefreshToken);
                    }
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }

    }
}
