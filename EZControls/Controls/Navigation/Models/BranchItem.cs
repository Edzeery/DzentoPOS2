using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZControls.Controls.Navigation.Models
{
    public sealed class BranchItem 
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;
        public bool IsMainBranch { get; set; }
        public Image? Icon { get; set; }

        public bool IsActive { get; set; } = true;
        public override string ToString()
            => Name;
    }
 
}
