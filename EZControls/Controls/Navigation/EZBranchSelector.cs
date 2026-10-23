using EZControls.Controls.Navigation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZControls.Controls.Navigation
{
    public class EZBranchSelector
    {
        [Category("EZ Data")]
        public BindingList<BranchItem>? Branches { get; }

        [Category("EZ Data")]
        public BranchItem? SelectedBranch { get; set; }

        [Category("EZ Appearance")]
        public bool ShowCode { get; set; } = true;

        [Category("EZ Appearance")]
        public bool ShowSearchBox { get; set; } = true;

        [Category("EZ Appearance")]
        public bool ShowArrow { get; set; } = true;
    }
}
