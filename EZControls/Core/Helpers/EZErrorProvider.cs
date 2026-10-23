using EZControls.Core.Base;
using System.Windows.Forms;

namespace EZControls.Core.Helpers;

public sealed class EZErrorProvider
{
    private readonly ErrorProvider errorProvider;

    public EZErrorProvider(ContainerControl container)
    {
        errorProvider = new ErrorProvider
        {
            ContainerControl = container
        };
    }

    public void SetError(Control control, string message)
    {
        errorProvider.SetError(control, message);

        if (control is EZInputBase input)
        {
            input.SetError(message);
        }
    }

    public void Clear(Control control)
    {
        errorProvider.SetError(control, string.Empty);

        if (control is EZInputBase input)
        {
            input.ClearError();
        }
    }
}