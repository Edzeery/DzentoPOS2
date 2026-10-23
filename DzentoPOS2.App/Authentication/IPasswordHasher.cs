namespace DzentoPOS2.App.Authentication
{
    public interface IPasswordHasher
    {
        string Hash(string password);

        bool Verify(
            string password,
            string hash);
    }
}