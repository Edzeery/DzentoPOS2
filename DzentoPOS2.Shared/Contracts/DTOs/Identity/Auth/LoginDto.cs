namespace DzentoPOS2.Shared.Contracts.DTOs.Identity.Auth
{
    public class LoginDto
    {
        public string UserName { get; set; }
            = string.Empty;

        public string Password { get; set; }
            = string.Empty;

        public bool RememberMe { get; set; }
    }
}