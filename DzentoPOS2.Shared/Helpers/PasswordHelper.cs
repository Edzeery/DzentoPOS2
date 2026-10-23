namespace DzentoPOS2.Shared.Helpers
{
    public static class PasswordHelper
    {
        public static string Hash(
            string password)
        {
            return BCrypt.Net.BCrypt
                .HashPassword(password);
        }

        public static bool Verify(
            string password,
            string hash)
        {
            // BCrypt.Verify throws (SaltParseException) when the stored value is
            // not a valid BCrypt hash - which happens for accounts created before
            // hashing was introduced (their password is stored in clear text).
            // Such a value must be treated as "not matching" instead of crashing
            // the caller, so the caller can migrate the account.
            if (string.IsNullOrEmpty(password) || !IsHashed(hash))
                return false;

            try
            {
                return BCrypt.Net.BCrypt
                    .Verify(password, hash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                return false;
            }
        }

        /// <summary>
        /// True when <paramref name="value"/> already looks like a BCrypt hash
        /// (starts with a known version marker). Used to detect legacy accounts
        /// whose password was stored before hashing was introduced.
        /// </summary>
        public static bool IsHashed(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            return value.StartsWith("$2a$", System.StringComparison.Ordinal)
                || value.StartsWith("$2b$", System.StringComparison.Ordinal)
                || value.StartsWith("$2y$", System.StringComparison.Ordinal)
                || value.StartsWith("$2x$", System.StringComparison.Ordinal);
        }
    }
}