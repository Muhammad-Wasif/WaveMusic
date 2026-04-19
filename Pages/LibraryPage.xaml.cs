using WaveMusic.Models;
using WaveMusic.Services;

namespace WaveMusic.Pages;

public class SongRow
{
    public string TrackNum       { get; set; } = "";
    public string Title          { get; set; } = "";
    public string Artist         { get; set; } = "";
    public string Album          { get; set; } = "";
    public string DurationString { get; set; } = "--:--";
    public BitmapImage? Thumbnail { get; set; }
    public Song? Source          { get; set; }
}

public partial class LibraryPage : WpfUserControl
{
    private List<Song> _songs     = new();
    private List<Playlist> _playlists = new();
    private string _currentFilter = "All";

    public LibraryPage()
    {
        InitializeComponent();
        LoadLibrary();
    }

    public void LoadLibrary()
    {
        // Always load from persistent store
        _songs = PersistentLibraryService.LoadSavedLibrary();
        if (_songs.Count > 0)
        {
            MusicPlayerService.Instance.SetQueue(_songs, 0);
            MusicPlayerService.Instance.Pause();
        }

        _playlists = new List<Playlist>
        {
            new() { Name = "Liked Music",        IsAutoPlaylist = true },
            new() { Name = "Episodes for Later", IsAutoPlaylist = true },
        };

        if (_songs.Count > 0)
            _playlists.Add(new Playlist { Name = "My Music", Songs = _songs });

        RefreshUI();
    }

    private void RefreshUI()
    {
        PlaylistGrid.ItemsSource = _playlists;
        var rows = _songs.Select((s, i) => new SongRow
        {
            TrackNum       = (i + 1).ToString(),
            Title          = s.Title,
            Artist         = s.Artist,
            Album          = s.Album,
            DurationString = s.DurationString,
            Thumbnail      = s.Thumbnail,
            Source         = s
        }).ToList();
        SongsList.ItemsSource     = rows;
        EmptyState.Visibility     = _songs.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        SongsSection.Visibility   = _songs.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void FilterBtn_Click(object sender, RoutedEventArgs e)
    {
        var btn = (Button)sender;
        _currentFilter = btn.Tag?.ToString() ?? "All";
        foreach (var b in new[] { BtnAll, BtnPlaylists, BtnSongs, BtnAlbums, BtnArtists })
            b.Background = (Brush)WpfApplication.Current.Resources["Card2Brush"];
        btn.Background = (Brush)WpfApplication.Current.Resources["AccentBrush"];
        PlaylistSection.Visibility = _currentFilter is "All" or "Playlists" ? Visibility.Visible : Visibility.Collapsed;
        SongsSection.Visibility    = _currentFilter is "All" or "Songs"     ? Visibility.Visible : Visibility.Collapsed;
    }

    private void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title       = "Select your music folder",
            Multiselect = false
        };
        if (dialog.ShowDialog() != true) return;

        var folder   = dialog.FolderName;
        var newSongs = PersistentLibraryService.AddFolder(folder);

        if (newSongs.Count == 0)
        {
            var total = MusicLibraryService.GetAudioFilesInFolder(folder).Count;
            MessageBox.Show(total == 0
                ? $"No audio files found in:\n{folder}"
                : $"All {total} songs already in your library.",
                "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // Merge new songs
        var existing = _songs.Select(s => s.FilePath).ToHashSet();
        foreach (var s in newSongs)
            if (!existing.Contains(s.FilePath))
                _songs.Add(s);

        MusicPlayerService.Instance.SetQueue(_songs, 0);
        MusicPlayerService.Instance.Pause();

        MessageBox.Show($"✅ Added {newSongs.Count} song(s). Library: {_songs.Count} total.",
            "Folder Added", MessageBoxButton.OK, MessageBoxImage.Information);

        RefreshUI();
    }

    private void Song_Click(object sender, WpfMouseEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is SongRow row && row.Source != null)
            MusicPlayerService.Instance.PlaySong(row.Source);
    }
}
