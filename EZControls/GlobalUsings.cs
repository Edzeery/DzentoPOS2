// ══════════════════════════════════════════════════════════════════════════════
//  GlobalUsings.cs  –  Framework-wide implicit using directives
//
//  These are imported into every .cs file in the EZControls project.
//  Rules:
//    • Add only namespaces used by 3 or more files.
//    • Never add namespaces that introduce naming conflicts within the project.
//    • Third-party namespaces (SkiaSharp, etc.) are NOT global – they are only
//      needed by the files that use them and should remain local imports.
// ══════════════════════════════════════════════════════════════════════════════

// ── BCL ───────────────────────────────────────────────────────────────────────
global using System;
global using System.Collections.Generic;
global using System.ComponentModel;
global using System.Drawing;
global using System.Drawing.Drawing2D;
global using System.Linq;
global using System.Runtime.InteropServices;
global using System.Windows.Forms;

// ── WinForms design ───────────────────────────────────────────────────────────
global using System.ComponentModel.Design;
global using System.Drawing.Design;

// ── EZControls framework ─────────────────────────────────────────────────────
global using EZControls.Core.Animation;
global using EZControls.Core.Base;
global using EZControls.Core.Constants;
global using EZControls.Core.Drawing;
global using EZControls.Core.Enums;
global using EZControls.Core.Extensions;
global using EZControls.Core.Helpers;
global using EZControls.Core.Interfaces;
global using EZControls.Core.Themes;