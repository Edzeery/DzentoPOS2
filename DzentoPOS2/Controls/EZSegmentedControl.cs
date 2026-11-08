using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DzentoPOS2.WinForms.Controls
{
    public class EZSegmentedControl : UserControl
    {
        private readonly FlowLayoutPanel _panel = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        private readonly List<Button> _buttons = new();
        private int _selectedIndex = -1;

        public event EventHandler<int>? SelectedIndexChanged;

        public EZSegmentedControl()
        {
            BackColor = Color.Transparent;
            Controls.Add(_panel);
            Height = 34;
        }

        public void SetItems(params string[] items)
        {
            _panel.SuspendLayout();
            foreach (var b in _buttons) { _panel.Controls.Remove(b); b.Dispose(); }
            _buttons.Clear();
            _selectedIndex = -1;

            for (int i = 0; i < items.Length; i++)
            {
                var idx = i;
                var btn = new Button
                {
                    Text = items[i],
                    AutoSize = false,
                    Width = 80,
                    Height = 30,
                    FlatStyle = FlatStyle.Flat,
                    Margin = new Padding(2),
                    Tag = idx
                };
                btn.Click += (_, _) => SelectIndex(idx);
                _buttons.Add(btn);
                _panel.Controls.Add(btn);
            }
            _panel.ResumeLayout();
            Width = items.Length * (80 + 4) + 8;
            if (_buttons.Count > 0) SelectIndex(0);
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SelectIndex(value);
        }

        public void SelectIndex(int index)
        {
            if (index < 0 || index >= _buttons.Count || index == _selectedIndex) return;
            _selectedIndex = index;
            for (int i = 0; i < _buttons.Count; i++)
            {
                _buttons[i].BackColor = i == index ? SystemColors.Highlight : SystemColors.Control;
                _buttons[i].ForeColor = i == index ? SystemColors.HighlightText : SystemColors.ControlText;
            }
            SelectedIndexChanged?.Invoke(this, index);
        }
    }
}