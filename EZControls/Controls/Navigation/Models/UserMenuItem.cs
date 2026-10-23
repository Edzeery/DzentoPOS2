using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZControls.Controls.Navigation.Models
{
    public sealed class UserMenuItem
    {
        public string Text { get; set; } = string.Empty;

        public Image? Icon { get; set; }

        public object? Tag { get; set; }
    }
}
