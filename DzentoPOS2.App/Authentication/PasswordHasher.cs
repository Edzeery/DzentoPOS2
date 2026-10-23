using DzentoPOS2.Shared.Helpers;

namespace DzentoPOS2.App.Authentication
{
    public class PasswordHasher
        : IPasswordHasher
    {
        public string Hash(string password)
        {
            return PasswordHelper
                .Hash(password);
        }

        public bool Verify(
            string password,
            string hash)
        {
            return PasswordHelper
                .Verify(password, hash);
        }
    }
}