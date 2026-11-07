using System;
using System.Windows.Forms;

namespace DzentoPOS2.WinForms.Controls
{
    /// <summary>
    /// Thin wrapper around DateTimePicker used by the dashboard filter bar.
    /// </summary>
    public class EZDatePicker : DateTimePicker
    {
        public EZDatePicker()
        {
            Format = DateTimePickerFormat.Short;
            ShowUpDown = false;
        }
    }
}