namespace DzentoPOS2.Shared.Contracts.DTOs.Identity.Users
{
    public class UserDetailsDto : UserDto
    {
        public List<Guid> RoleIds { get; set; }
            = new();

        public List<string> Roles { get; set; }
            = new();
    }
}