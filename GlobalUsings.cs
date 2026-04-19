// ── All usings for the entire project live here ───────────────────────────
// ImplicitUsings=disable means ONLY these apply. No auto-imported namespaces.
// This guarantees zero ambiguity between WPF and WinForms types.

global using System;
global using System.Collections.Generic;
global using System.ComponentModel;
global using System.Globalization;
global using System.IO;
global using System.Linq;
global using System.Runtime.InteropServices;
global using System.Text.Json;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Windows;
global using System.Windows.Controls;
global using System.Windows.Data;
global using System.Windows.Input;
global using System.Windows.Interop;
global using System.Windows.Media;
global using System.Windows.Media.Imaging;
global using System.Windows.Threading;

// ── Explicit aliases — WPF types only, no WinForms equivalents ────────────
global using WpfApplication    = System.Windows.Application;
global using WpfColor          = System.Windows.Media.Color;
global using WpfUserControl    = System.Windows.Controls.UserControl;
global using WpfKeyEventArgs   = System.Windows.Input.KeyEventArgs;
global using WpfMouseEventArgs = System.Windows.Input.MouseButtonEventArgs;

// NOTE: System.Windows.Forms.Screen is used ONLY in MainWindow.xaml.cs
// via its full qualified name. It is NOT imported globally to avoid
// the CS0104 ambiguity that caused all previous build errors.
