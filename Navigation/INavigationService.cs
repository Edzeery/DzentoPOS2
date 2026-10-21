using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DzentoPOS2.WinForms.Navigation
{
    public interface INavigationService
    {
        void SetHost(Panel container, Label titleLabel);
        void Navigate<TForm>() where TForm : Form;
        void SetActiveButton(IconButton button);
    }
}
