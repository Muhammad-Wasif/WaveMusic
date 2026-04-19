using System.Runtime.InteropServices;
using System.Windows.Interop;
using WaveMusic.Pages;
using WaveMusic.Services;

namespace WaveMusic;

// Native struct needed for WM_GETMINMAXINFO (taskbar-safe maximize)
[StructLayout(LayoutKind.Sequential)]
internal struct POINT { public int x, y; }
[StructLayout(LayoutKind.Sequential)]
internal struct MINMAXINFO
{
    public POINT ptReserved, ptMaxSize, ptMaxPosition, ptMinTrackSize, ptMaxTrackSize;
}

public partial class MainWindow : Window
{
    private readonly MusicPlayerService _player = MusicPlayerService.Instance;
    private bool _isDraggingProgress = false;
    private HomePage? _homePage;

    public MainWindow()
    {
        InitializeComponent();
        SetupPlayer();
        NavigateTo("Home");
    }

    // ─── Taskbar-safe maximize (WM_GETMINMAXINFO hook) ────────────────────

    private void Window_SourceInitialized(object sender, EventArgs e)
    {
        var handle = new WindowInteropHelper(this).Handle;
        HwndSource.FromHwnd(handle)?.AddHook(WndProc);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        // WM_GETMINMAXINFO = 0x0024
        // Fires when Windows asks how big the window can be when maximized.
        // We clamp it to WorkArea (excludes taskbar).
        if (msg == 0x0024)
        {
            var mmi = Marshal.PtrToStructure<MINMAXINFO>(lParam);
            var screen = System.Windows.Forms.Screen.FromHandle(hwnd);
            var wa = screen.WorkingArea; // excludes taskbar

            mmi.ptMaxPosition.x = Math.Abs(wa.Left - screen.Bounds.Left);
            mmi.ptMaxPosition.y = Math.Abs(wa.Top  - screen.Bounds.Top);
            mmi.ptMaxSize.x     = wa.Width;
            mmi.ptMaxSize.y     = wa.Height;

            Marshal.StructureToPtr(mmi, lParam, true);
            handled = true;
        }
        return IntPtr.Zero;
    }

    private void Window_StateChanged(object sender, EventArgs e)
    {
        // When maximized with WindowStyle=None, add a small border thickness
        // to prevent content from bleeding under taskbar on some setups
        if (WindowState == WindowState.Maximized)
            BorderThickness = new Thickness(6);
        else
            BorderThickness = new Thickness(0);
    }

    // ─── Navigation ───────────────────────────────────────────────────────

    private void NavigateTo(string page)
    {
        foreach (var btn in new[] { BtnHome, BtnExplore, BtnLibrary, BtnUpgrade })
            btn.Style = (Style)FindResource("NavBtn");

        switch (page)
        {
            case "Home":
                BtnHome.Style = (Style)FindResource("NavBtnActive");
                if (_homePage == null)
                    _homePage = new HomePage();
                else
                    _homePage.LoadSongs(); // refresh on every Home visit
                ContentArea.Content = _homePage;
                break;
            case "Explore":
                BtnExplore.Style = (Style)FindResource("NavBtnActive");
                ContentArea.Content = new ExplorePage();
                break;
            case "Library":
                BtnLibrary.Style = (Style)FindResource("NavBtnActive");
                ContentArea.Content = new LibraryPage();
                break;
            case "Upgrade":
                BtnUpgrade.Style = (Style)FindResource("NavBtnActive");
                ContentArea.Content = new UpgradePage();
                break;
        }
    }

    private void Nav_Click(object sender, RoutedEventArgs e)
        => NavigateTo(((Button)sender).Tag?.ToString() ?? "Home");

    private void NewPlaylist_Click(object sender, RoutedEventArgs e)
        => MessageBox.Show("Load a music folder from Library or Home to get started.",
            "New Playlist", MessageBoxButton.OK, MessageBoxImage.Information);

    private void Playlist_Click(object sender, RoutedEventArgs e)
        => NavigateTo("Library");

    // ─── Title Bar ────────────────────────────────────────────────────────

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }

    private void MinimizeBtn_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void MaximizeBtn_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void CloseBtn_Click(object sender, RoutedEventArgs e) => WpfApplication.Current.Shutdown();

    // ─── Top Search ───────────────────────────────────────────────────────

    private void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || string.IsNullOrWhiteSpace(SearchBox.Text)) return;

        // Navigate to Home and trigger search there
        NavigateTo("Home");
        if (_homePage != null)
        {
            _homePage.SearchBox.Text = SearchBox.Text.Trim();
        }
    }

    // ─── Player Setup ─────────────────────────────────────────────────────

    private void SetupPlayer()
    {
        _player.SongChanged      += OnSongChanged;
        _player.PlayStateChanged += OnPlayStateChanged;
        _player.PositionChanged  += OnPositionChanged;
        VolumeSlider.Value = _player.Volume;
    }

    private void OnSongChanged()
    {
        Dispatcher.Invoke(() =>
        {
            var song = _player.CurrentSong;
            if (song == null) return;
            NowPlayingTitle.Text  = song.Title;
            NowPlayingArtist.Text = song.Artist;
            if (song.Thumbnail != null)
            {
                NowPlayingThumb.Source       = song.Thumbnail;
                NowPlayingDefault.Visibility = Visibility.Collapsed;
            }
            else
            {
                NowPlayingThumb.Source       = null;
                NowPlayingDefault.Visibility = Visibility.Visible;
            }
        });
    }

    private void OnPlayStateChanged()
    {
        Dispatcher.Invoke(() =>
        {
            PlayPauseBtn.ApplyTemplate();
            if (PlayPauseBtn.Template.FindName("PlayIcon", PlayPauseBtn) is TextBlock icon)
                icon.Text = _player.IsPlaying ? "⏸" : "▶";
        });
    }

    private bool _updateProgress = true;

    private void OnPositionChanged()
    {
        if (_isDraggingProgress) return;
        Dispatcher.Invoke(() =>
        {
            var dur = _player.Duration;
            var pos = _player.Position;
            DurationText.Text = dur.ToString(@"m\:ss");
            PositionText.Text  = pos.ToString(@"m\:ss");
            if (dur.TotalSeconds > 0)
            {
                _updateProgress = false;
                ProgressSlider.Maximum        = dur.TotalSeconds;
                ProgressSlider.Value          = pos.TotalSeconds;
                ProgressSlider.SelectionStart = 0;
                ProgressSlider.SelectionEnd   = pos.TotalSeconds;
                _updateProgress = true;
            }
        });
    }

    // ─── Player Controls ──────────────────────────────────────────────────

    private void PlayPause_Click(object sender, RoutedEventArgs e)
    {
        if (_player.CurrentSong == null)
        {
            MessageBox.Show("No song selected.\n\nUse 'Add Songs' or 'Add Folder' on the Home page, then click a song.",
                "WaveMusic", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        _player.TogglePlay();
    }

    private void Prev_Click(object sender, RoutedEventArgs e)  => _player.Previous();
    private void Next_Click(object sender, RoutedEventArgs e)  => _player.Next();

    private void Shuffle_Click(object sender, RoutedEventArgs e)
    {
        _player.ToggleShuffle();
        ShuffleBtn.Foreground = _player.IsShuffle
            ? (Brush)FindResource("AccentBrush")
            : (Brush)FindResource("SecondaryTextBrush");
    }

    private void Repeat_Click(object sender, RoutedEventArgs e)
    {
        _player.ToggleRepeat();
        RepeatBtn.Content = _player.Repeat switch
        {
            RepeatMode.RepeatAll => "🔁",
            RepeatMode.RepeatOne => "🔂",
            _                    => "🔁"
        };
        RepeatBtn.Foreground = _player.Repeat != RepeatMode.None
            ? (Brush)FindResource("AccentBrush")
            : (Brush)FindResource("SecondaryTextBrush");
    }

    private void Like_Click(object sender, RoutedEventArgs e)
    {
        var btn    = (Button)sender;
        var accent = (Brush)FindResource("AccentBrush");
        var dim    = (Brush)FindResource("SecondaryTextBrush");
        btn.Foreground = btn.Foreground.ToString() == accent.ToString() ? dim : accent;
    }

    private void Volume_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        => _player.Volume = VolumeSlider.Value;

    private void Progress_MouseDown(object sender, MouseButtonEventArgs e)
        => _isDraggingProgress = true;

    private void Progress_MouseUp(object sender, MouseButtonEventArgs e)
    {
        _isDraggingProgress = false;
        _player.Seek(TimeSpan.FromSeconds(ProgressSlider.Value));
    }

    private void Progress_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_updateProgress) return;
        if (_isDraggingProgress)
        {
            PositionText.Text = TimeSpan.FromSeconds(ProgressSlider.Value).ToString(@"m\:ss");
            ProgressSlider.SelectionStart = 0;
            ProgressSlider.SelectionEnd   = ProgressSlider.Value;
        }
    }

    private void Queue_Click(object sender, RoutedEventArgs e)
    {
        if (_player.Queue.Count == 0)
        {
            MessageBox.Show("Queue is empty.\n\nAdd songs from the Home page.",
                "Queue", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var txt = string.Join("\n", _player.Queue.Take(20)
            .Select((s, i) => $"{(i == _player.CurrentIndex ? "▶ " : "   ")}{i + 1}. {s.Title} — {s.Artist}"));
        if (_player.Queue.Count > 20)
            txt += $"\n   ...and {_player.Queue.Count - 20} more";
        MessageBox.Show(txt, $"Queue ({_player.Queue.Count} songs)", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
