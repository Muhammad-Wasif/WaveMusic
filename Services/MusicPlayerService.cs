using WaveMusic.Models;

namespace WaveMusic.Services;

public enum RepeatMode { None, RepeatAll, RepeatOne }

public class MusicPlayerService
{
    private static MusicPlayerService? _instance;
    public static MusicPlayerService Instance => _instance ??= new MusicPlayerService();

    private readonly System.Windows.Media.MediaPlayer _player = new();
    private readonly DispatcherTimer _timer;

    public List<Song> Queue       { get; private set; } = new();
    public int CurrentIndex       { get; private set; } = -1;
    public Song? CurrentSong      => CurrentIndex >= 0 && CurrentIndex < Queue.Count ? Queue[CurrentIndex] : null;
    public bool IsPlaying         { get; private set; }
    public bool IsShuffle         { get; private set; }
    public RepeatMode Repeat      { get; private set; } = RepeatMode.None;
    public double Volume          { get => _player.Volume; set => _player.Volume = value; }
    public TimeSpan Position      => _player.Position;
    public TimeSpan Duration      => _player.NaturalDuration.HasTimeSpan ? _player.NaturalDuration.TimeSpan : TimeSpan.Zero;

    public event Action? SongChanged;
    public event Action? PlayStateChanged;
    public event Action? PositionChanged;
    public event Action? QueueChanged;

    private MusicPlayerService()
    {
        _player.Volume       = 0.8;
        _player.MediaEnded  += OnMediaEnded;
        _player.MediaFailed += (s, e) => { IsPlaying = false; PlayStateChanged?.Invoke(); };

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _timer.Tick += (_, _) => PositionChanged?.Invoke();
    }

    private void OnMediaEnded(object? sender, EventArgs e)
    {
        switch (Repeat)
        {
            case RepeatMode.RepeatOne:  Seek(TimeSpan.Zero); Play(); break;
            case RepeatMode.RepeatAll:  Next(); break;
            default:
                if (CurrentIndex < Queue.Count - 1) Next();
                else { IsPlaying = false; PlayStateChanged?.Invoke(); _timer.Stop(); }
                break;
        }
    }

    public void SetQueue(List<Song> songs, int startIndex = 0)
    {
        foreach (var s in Queue) s.IsPlaying = false;
        Queue        = new List<Song>(songs);
        CurrentIndex = Math.Clamp(startIndex, 0, Math.Max(0, songs.Count - 1));
        QueueChanged?.Invoke();
        if (Queue.Count > 0) LoadCurrent();
    }

    public void AddToQueue(Song song) { Queue.Add(song); QueueChanged?.Invoke(); }

    private void LoadCurrent()
    {
        if (CurrentSong == null) return;
        foreach (var s in Queue) s.IsPlaying = false;

        if (CurrentSong.IsSample || string.IsNullOrEmpty(CurrentSong.FilePath))
        {
            IsPlaying = false;
            PlayStateChanged?.Invoke();
            SongChanged?.Invoke();
            return;
        }

        try
        {
            _player.Open(new Uri(CurrentSong.FilePath, UriKind.Absolute));
            CurrentSong.IsPlaying = true;
            IsPlaying = true;
            _player.Play();
            _timer.Start();
            PlayStateChanged?.Invoke();
            SongChanged?.Invoke();
        }
        catch
        {
            IsPlaying = false;
            PlayStateChanged?.Invoke();
            SongChanged?.Invoke();
        }
    }

    public void Play()
    {
        if (CurrentSong == null || CurrentSong.IsSample) return;
        _player.Play();
        IsPlaying = true;
        if (CurrentSong != null) CurrentSong.IsPlaying = true;
        _timer.Start();
        PlayStateChanged?.Invoke();
    }

    public void Pause()
    {
        _player.Pause();
        IsPlaying = false;
        if (CurrentSong != null) CurrentSong.IsPlaying = false;
        _timer.Stop();
        PlayStateChanged?.Invoke();
    }

    public void TogglePlay() { if (IsPlaying) Pause(); else Play(); }

    public void Next()
    {
        if (Queue.Count == 0) return;
        CurrentIndex = IsShuffle ? new Random().Next(Queue.Count) : (CurrentIndex + 1) % Queue.Count;
        LoadCurrent();
    }

    public void Previous()
    {
        if (Queue.Count == 0) return;
        if (Position.TotalSeconds > 3) { Seek(TimeSpan.Zero); return; }
        CurrentIndex = CurrentIndex > 0 ? CurrentIndex - 1 : Queue.Count - 1;
        LoadCurrent();
    }

    public void PlaySong(Song song)
    {
        var idx = Queue.IndexOf(song);
        if (idx >= 0) { CurrentIndex = idx; LoadCurrent(); return; }
        var at = Math.Max(0, CurrentIndex + 1);
        Queue.Insert(at, song);
        CurrentIndex = at;
        LoadCurrent();
    }

    public void Seek(TimeSpan position) => _player.Position = position;
    public void ToggleShuffle() => IsShuffle = !IsShuffle;
    public void ToggleRepeat()
    {
        Repeat = Repeat switch
        {
            RepeatMode.None      => RepeatMode.RepeatAll,
            RepeatMode.RepeatAll => RepeatMode.RepeatOne,
            _                    => RepeatMode.None
        };
    }
}
