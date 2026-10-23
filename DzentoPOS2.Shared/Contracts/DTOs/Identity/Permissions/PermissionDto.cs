using System;

namespace DzentoPOS2.Shared.Contracts.DTOs.Identity.Permissions
{
    public class PermissionDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
            = string.Empty;

        public string Code { get; set; }
            = string.Empty;

        public string? Module { get; set; }

        public string? Description { get; set; }
    }
}