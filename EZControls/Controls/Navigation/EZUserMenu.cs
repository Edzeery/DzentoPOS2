using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EZControls.Controls.Navigation
{
#pragma warning disable CS0067 // Events are part of the public API and raised by consumers.
    public class EZUserMenu
    {
        [Category("EZ Appearance")]
        public string UserName { get; set; } = "User";

        [Category("EZ Appearance")]
        public string UserRole { get; set; } = "Administrator";

        [Category("EZ Appearance")]
        public Image? Avatar { get; set; }

        [Category("EZ Appearance")]
        public bool ShowRole { get; set; } = true;

        [Category("EZ Appearance")]
        public bool ShowArrow { get; set; } = true;

        public event EventHandler? ProfileClicked;
        public event EventHandler? PreferencesClicked;
        public event EventHandler? ChangePasswordClicked;
        public event EventHandler? LockSessionClicked;
        public event EventHandler? LogoutClicked;
    }
#pragma warning restore CS0067
}