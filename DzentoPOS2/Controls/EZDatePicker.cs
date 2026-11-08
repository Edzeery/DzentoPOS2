using System;
using System.Windows.Forms;

namespace DzentoPOS2.WinForms.Controls
{
    public class EZDatePicker : DateTimePicker
    {
        public EZDatePicker()
        {
            Format = DateTimePickerFormat.Short;
            ShowUpDown = false;
        }
    }
}