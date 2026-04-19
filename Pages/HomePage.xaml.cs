using WaveMusic.Models;
using WaveMusic.Services;
using System.Windows.Media.Animation;

namespace WaveMusic.Pages;

public partial class HomePage : WpfUserControl
{
    private List<Song> _allSongs  = new();
    private List<Song> _displayed = new();
    private const int SONGS_PER_ROW = 8;

    public HomePage()
    {
        InitializeComponent();
        LoadFromPersistence();
    }

    // ── Load saved library from AppData on startup ────────────────────────
    public void LoadFromPersistence()
    {
        var saved = PersistentLibraryService.LoadSavedLibrary();
        if (saved.Count > 0)
        {
            _allSongs = saved;
            MusicPlayerService.Instance.SetQueue(_allSongs, 0);
            MusicPlayerService.Instance.Pause();
        }
        else
        {
            // Show sample songs until user uploads real ones
            _allSongs = MusicLibraryService.GetSampleSongs();
        }
        ApplySearch(SearchBox?.Text ?? "");
    }

    // ── Reload (called after adding new songs) ────────────────────────────
    public void LoadSongs()
    {
        ApplySearch(SearchBox?.Text ?? "");
    }

    // ── Apply search filter and rebuild grid ──────────────────────────────
    private void ApplySearch(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            _displayed = _allSongs.ToList();
            SectionTitle.Text = "Your Music";
        }
        else
        {
            _displayed = _allSongs
                .Where(s => s.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
                         || s.Artist.Contains(query, StringComparison.OrdinalIgnoreCase)
                         || s.Album.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
            SectionTitle.Text = $"Results for \"{query}\"";
        }

        SongCountText.Text = $"{_displayed.Count} song{(_displayed.Count == 1 ? "" : "s")}";

        if (_allSongs.Count == 0)
        {
            EmptyState.Visibility = Visibility.Visible;
            NoResults.Visibility  = Visibility.Collapsed;
            MainScroll.Visibility = Visibility.Collapsed;
            return;
        }

        if (_displayed.Count == 0)
        {
            EmptyState.Visibility = Visibility.Collapsed;
            NoResults.Visibility  = Visibility.Visible;
            MainScroll.Visibility = Visibility.Collapsed;
            NoResultsText.Text    = $"No songs match \"{query}\"";
            return;
        }

        EmptyState.Visibility = Visibility.Collapsed;
        NoResults.Visibility  = Visibility.Collapsed;
        MainScroll.Visibility = Visibility.Visible;

        // Populate the SongGrid ItemsControl
        SongGrid.ItemsSource = _displayed;
    }

    // ── Upload individual audio files ─────────────────────────────────────
    private void UploadFiles_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title       = "Select audio files",
            Multiselect = true,
            Filter      = "Audio Files|*.mp3;*.wav;*.flac;*.wma;*.m4a;*.ogg;*.aac|All Files|*.*"
        };

        if (dialog.ShowDialog() != true) return;

        var newSongs = PersistentLibraryService.AddFiles(dialog.FileNames);

        if (newSongs.Count == 0)
        {
            MessageBox.Show("Those files are already in your library.", "Already Added",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // Merge into existing list (avoid duplicates)
        var existingPaths = _allSongs.Select(s => s.FilePath).ToHashSet();
        foreach (var s in newSongs)
            if (!existingPaths.Contains(s.FilePath))
                _allSongs.Add(s);

        // Update player queue with all real songs (non-sample)
        var realSongs = _allSongs.Where(s => !s.IsSample).ToList();
        if (realSongs.Count > 0)
        {
            // Remove sample songs now that we have real ones
            _allSongs = realSongs;
            MusicPlayerService.Instance.SetQueue(_allSongs, 0);
            MusicPlayerService.Instance.Pause();
        }

        MessageBox.Show($"✅ Added {newSongs.Count} new song(s) to your library.\nYour library now has {_allSongs.Count} songs.",
            "Songs Added", MessageBoxButton.OK, MessageBoxImage.Information);

        ApplySearch(SearchBox.Text);
    }

    // ── Upload entire folder ──────────────────────────────────────────────
    private void UploadFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title       = "Select music folder",
            Multiselect = false
        };

        if (dialog.ShowDialog() != true) return;

        var folder   = dialog.FolderName;
        var newSongs = PersistentLibraryService.AddFolder(folder);

        if (newSongs.Count == 0)
        {
            var audioCount = MusicLibraryService.GetAudioFilesInFolder(folder).Count;
            if (audioCount == 0)
                MessageBox.Show($"No audio files found in:\n{folder}\n\nSupported: MP3, WAV, FLAC, WMA, M4A, OGG",
                    "No Music Found", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show($"All {audioCount} songs from this folder are already in your library.",
                    "Already Added", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var existingPaths = _allSongs.Select(s => s.FilePath).ToHashSet();
        foreach (var s in newSongs)
            if (!existingPaths.Contains(s.FilePath))
                _allSongs.Add(s);

        // Remove sample songs now that we have real music
        var realSongs = _allSongs.Where(s => !s.IsSample).ToList();
        if (realSongs.Count > 0)
        {
            _allSongs = realSongs;
            MusicPlayerService.Instance.SetQueue(_allSongs, 0);
            MusicPlayerService.Instance.Pause();
        }

        MessageBox.Show($"✅ Added {newSongs.Count} song(s) from:\n{folder}\n\nLibrary total: {_allSongs.Count} songs.",
            "Folder Added", MessageBoxButton.OK, MessageBoxImage.Information);

        ApplySearch(SearchBox.Text);
    }

    // ── Song card click → play ────────────────────────────────────────────
    private void SongCard_Click(object sender, WpfMouseEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not Song song) return;

        if (song.IsSample)
        {
            MessageBox.Show($"'{song.Title}' is a sample.\nClick 'Add Songs' or 'Add Folder' to load your own music.",
                "Sample Song", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var player = MusicPlayerService.Instance;
        var idx    = _allSongs.IndexOf(song);
        if (idx < 0) return;

        if (!player.Queue.SequenceEqual(_allSongs))
            player.SetQueue(_allSongs, idx);
        else
            player.PlaySong(song);
    }

    // ── Search ────────────────────────────────────────────────────────────
    private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        var query = SearchBox.Text;
        ClearSearch.Visibility = string.IsNullOrEmpty(query) ? Visibility.Collapsed : Visibility.Visible;
        ApplySearch(query);
    }

    private void ClearSearch_Click(object sender, RoutedEventArgs e)
    {
        SearchBox.Clear();
        ClearSearch.Visibility = Visibility.Collapsed;
    }

    // ── Scroll arrows (one row = card height 160 + text ~60 + margin 20 = ~240) ──
    private void ScrollUp_Click(object sender, RoutedEventArgs e)
        => MainScroll.ScrollToVerticalOffset(Math.Max(0, MainScroll.VerticalOffset - 240));

    private void ScrollDown_Click(object sender, RoutedEventArgs e)
        => MainScroll.ScrollToVerticalOffset(MainScroll.VerticalOffset + 240);

    // ── Mood chips ────────────────────────────────────────────────────────
    private void MoodChip_Click(object sender, RoutedEventArgs e)
    {
        var mood = ((Button)sender).Content?.ToString() ?? "";
        // Filter library by mood-based keywords
        var keywords = mood.ToLower() switch
        {
            "relax"     => new[] { "relax", "chill", "calm", "soft", "slow" },
            "feel good" => new[] { "happy", "joy", "good", "smile", "fun" },
            "workout"   => new[] { "energy", "pump", "rock", "power", "run" },
            "energize"  => new[] { "energy", "fast", "beat", "remix" },
            "party"     => new[] { "party", "dance", "club", "dj", "remix" },
            "romance"   => new[] { "love", "dil", "pyaar", "heart", "romantic", "tere" },
            "sad"       => new[] { "sad", "dard", "rona", "alone", "miss" },
            "focus"     => new[] { "focus", "study", "ambient", "instrumental" },
            "sleep"     => new[] { "sleep", "lullaby", "night", "soft", "quiet" },
            _           => Array.Empty<string>()
        };

        if (keywords.Length > 0)
        {
            _displayed = _allSongs
                .Where(s => keywords.Any(k =>
                    s.Title.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    s.Artist.Contains(k, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (_displayed.Count > 0)
            {
                SectionTitle.Text  = $"{mood} — {_displayed.Count} songs";
                SongCountText.Text = "";
                SongGrid.ItemsSource = _displayed;
                EmptyState.Visibility = Visibility.Collapsed;
                NoResults.Visibility  = Visibility.Collapsed;
                MainScroll.Visibility = Visibility.Visible;
                SearchBox.Text = "";
                return;
            }
        }

        // Fallback if no matches
        MessageBox.Show($"No songs matching the '{mood}' mood found in your library.",
            mood, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
