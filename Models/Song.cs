
namespace WaveMusic.Models;

public class Song : INotifyPropertyChanged
{
    private bool _isPlaying;

    public string Title         { get; set; } = "Unknown Title";
    public string Artist        { get; set; } = "Unknown Artist";
    public string Album         { get; set; } = "Unknown Album";
    public string FilePath      { get; set; } = string.Empty;
    public string? ImagePath    { get; set; }
    public BitmapImage? Thumbnail { get; set; }
    public TimeSpan Duration    { get; set; }
    public string DurationString => Duration == TimeSpan.Zero ? "--:--" : Duration.ToString(@"m\:ss");
    public long ViewCount       { get; set; }
    public string ViewCountString => ViewCount > 0 ? $"{ViewCount / 1_000_000}M views" : string.Empty;
    public bool IsSample        { get; set; }

    public bool IsPlaying
    {
        get => _isPlaying;
        set { _isPlaying = value; OnPropertyChanged(nameof(IsPlaying)); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class Playlist
{
    public string  Name           { get; set; } = "New Playlist";
    public string? Description    { get; set; }
    public BitmapImage? Cover     { get; set; }
    public List<Song> Songs       { get; set; } = new();
    public string TrackCount      => $"{Songs.Count} tracks";
    public bool IsAutoPlaylist    { get; set; }
}
