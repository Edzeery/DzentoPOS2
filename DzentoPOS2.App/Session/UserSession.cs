using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzentoPOS2.App.Session
{
    public class UserSession
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = "";
        public string FullName { get; set; } = "";
        public string RoleName { get; set; } = "";
        public string BranchName { get; set; } = "";
        public Guid? BranchId { get; set; }
        public bool IsAdmin { get; set; }
    }
}
