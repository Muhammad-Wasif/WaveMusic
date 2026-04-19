using WaveMusic.Models;

namespace WaveMusic.Services;

public static class MusicLibraryService
{
    public static readonly string[] AudioExtensions = { ".mp3", ".wav", ".wma", ".flac", ".ogg", ".m4a", ".aac" };
    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".webp" };

    // ── Get all audio file paths in a folder (recursive one level) ───────
    public static List<string> GetAudioFilesInFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath)) return new();
        var result = new List<string>();
        try
        {
            result.AddRange(Directory.GetFiles(folderPath)
                .Where(f => AudioExtensions.Contains(Path.GetExtension(f).ToLower())));
            foreach (var dir in Directory.GetDirectories(folderPath))
            {
                try
                {
                    result.AddRange(Directory.GetFiles(dir)
                        .Where(f => AudioExtensions.Contains(Path.GetExtension(f).ToLower())));
                }
                catch { }
            }
        }
        catch { }
        return result;
    }

    // ── Build Song objects from a list of file paths ─────────────────────
    public static List<Song> BuildSongsFromPaths(IEnumerable<string> paths)
    {
        var songs = new List<Song>();
        foreach (var path in paths)
        {
            if (!File.Exists(path)) continue;
            var dir = Path.GetDirectoryName(path) ?? "";
            var imageMap = Directory.GetFiles(dir)
                .Where(f => ImageExtensions.Contains(Path.GetExtension(f).ToLower()))
                .ToDictionary(f => Path.GetFileNameWithoutExtension(f).ToLower(), f => f);

            var song = BuildSong(path, imageMap);
            if (song != null) songs.Add(song);
        }
        return songs;
    }

    // ── Scan an entire folder (legacy method kept for compatibility) ──────
    public static List<Song> ScanFolder(string folderPath)
    {
        var paths = GetAudioFilesInFolder(folderPath);
        return BuildSongsFromPaths(paths);
    }

    // ── Build a single Song from a file path ─────────────────────────────
    public static Song? BuildSong(string audioPath, Dictionary<string, string>? imageMap = null)
    {
        try
        {
            var song    = new Song { FilePath = audioPath };
            var nameKey = Path.GetFileNameWithoutExtension(audioPath).ToLower();

            // Build image map for the file's directory if not provided
            if (imageMap == null)
            {
                var dir = Path.GetDirectoryName(audioPath) ?? "";
                imageMap = Directory.GetFiles(dir)
                    .Where(f => ImageExtensions.Contains(Path.GetExtension(f).ToLower()))
                    .ToDictionary(f => Path.GetFileNameWithoutExtension(f).ToLower(), f => f);
            }

            // Read tags via TagLib
            try
            {
                using var file = TagLib.File.Create(audioPath);
                song.Title    = !string.IsNullOrWhiteSpace(file.Tag.Title)
                    ? file.Tag.Title : Path.GetFileNameWithoutExtension(audioPath);
                song.Artist   = file.Tag.Performers.Length > 0
                    ? string.Join(", ", file.Tag.Performers) : "Unknown Artist";
                song.Album    = !string.IsNullOrWhiteSpace(file.Tag.Album)
                    ? file.Tag.Album : "Unknown Album";
                song.Duration = file.Properties.Duration;

                var pic = file.Tag.Pictures.FirstOrDefault();
                if (pic != null) song.Thumbnail = BytesToBitmap(pic.Data.Data);
            }
            catch
            {
                song.Title = Path.GetFileNameWithoutExtension(audioPath);
            }

            // Matching image file (same name) overrides embedded art
            if (imageMap.TryGetValue(nameKey, out var imgPath))
            {
                song.ImagePath = imgPath;
                var loaded = LoadBitmapSafe(imgPath);
                if (loaded != null) song.Thumbnail = loaded;
            }

            return song;
        }
        catch { return null; }
    }

    private static BitmapImage? LoadBitmapSafe(string path)
    {
        try
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource   = new Uri(path, UriKind.Absolute);
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        catch { return null; }
    }

    private static BitmapImage? BytesToBitmap(byte[] data)
    {
        try
        {
            using var ms = new MemoryStream(data);
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption  = BitmapCacheOption.OnLoad;
            bmp.StreamSource = ms;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        catch { return null; }
    }

    public static List<Song> GetSampleSongs() => new()
    {
        new() { Title="Aasa Kooda (Think Indie)",      Artist="Sai Abhyankkar",             Album="Think Indie",        ViewCount=383_000_000, IsSample=true },
        new() { Title="Meri Zindagi Hai Tu",           Artist="Asim Azhar & Sabri Sisters", Album="Single",             IsSample=true },
        new() { Title="Ae Dil Hai Mushkil",            Artist="Pritam",                     Album="Ae Dil Hai Mushkil", ViewCount=714_000_000, IsSample=true },
        new() { Title="Bachke Ji Bachke (Jawaak)",     Artist="Ammy Virk & Vari Rai",       Album="Jawaak",             IsSample=true },
        new() { Title="Kaash Tu Mera Na Hunda",        Artist="B Praak, Jyoti Nooran",      Album="Shera",              IsSample=true },
        new() { Title="Ishq Bukhaar - Viral Version",  Artist="Shreya Ghoshal, B Praak",    Album="Single",             IsSample=true },
        new() { Title="Braj Ras",                      Artist="B Praak, Jaani & Mir Desai", Album="Braj Ras EP",        IsSample=true },
        new() { Title="Jiya Lagena - Shararat",        Artist="Shashwat Sachdev",           Album="Shararat",           IsSample=true },
        new() { Title="Tere Bina",                     Artist="Atif Aslam",                 Album="Soundtrack",         IsSample=true },
        new() { Title="Raatan Lambiyan",               Artist="Jubin Nautiyal",             Album="Shershaah",          ViewCount=520_000_000, IsSample=true },
        new() { Title="Kesariya",                      Artist="Arijit Singh",               Album="Brahmastra",         ViewCount=450_000_000, IsSample=true },
        new() { Title="Main Rahoon Ya Na Rahoon",      Artist="Armaan Malik",               Album="Creed",              IsSample=true },
    };
}
