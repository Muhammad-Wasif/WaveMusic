using WaveMusic.Models;

namespace WaveMusic.Services;

/// <summary>
/// Manages persistent song library stored in %AppData%\WaveMusic\library.json
/// Survives app restarts - paths are saved and reloaded automatically.
/// </summary>
public static class PersistentLibraryService
{
    private static readonly string AppDataFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WaveMusic");

    private static readonly string LibraryFile =
        Path.Combine(AppDataFolder, "library.json");

    // ── Load all saved song paths and scan them ──────────────────────────

    public static List<Song> LoadSavedLibrary()
    {
        try
        {
            if (!File.Exists(LibraryFile)) return new();

            var json  = File.ReadAllText(LibraryFile);
            var paths = JsonSerializer.Deserialize<List<string>>(json) ?? new();

            // Filter out files that no longer exist
            var valid = paths.Where(File.Exists).ToList();

            // If some were removed, update the saved file
            if (valid.Count != paths.Count)
                SavePaths(valid);

            return MusicLibraryService.BuildSongsFromPaths(valid);
        }
        catch
        {
            return new();
        }
    }

    // ── Add individual files ─────────────────────────────────────────────

    public static List<Song> AddFiles(IEnumerable<string> filePaths)
    {
        var existing = LoadSavedPaths();
        var toAdd    = filePaths
            .Where(p => File.Exists(p) && !existing.Contains(p))
            .ToList();

        if (toAdd.Count == 0) return new();

        existing.AddRange(toAdd);
        SavePaths(existing);

        return MusicLibraryService.BuildSongsFromPaths(toAdd);
    }

    // ── Add entire folder ────────────────────────────────────────────────

    public static List<Song> AddFolder(string folderPath)
    {
        var audioPaths = MusicLibraryService.GetAudioFilesInFolder(folderPath);
        return AddFiles(audioPaths);
    }

    // ── Remove all saved paths (clear library) ───────────────────────────

    public static void ClearLibrary()
    {
        try { if (File.Exists(LibraryFile)) File.Delete(LibraryFile); }
        catch { /* ignore */ }
    }

    // ── Remove specific songs ────────────────────────────────────────────

    public static void RemoveSongs(IEnumerable<string> filePaths)
    {
        var existing = LoadSavedPaths();
        foreach (var p in filePaths) existing.Remove(p);
        SavePaths(existing);
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    public static List<string> LoadSavedPaths()
    {
        try
        {
            if (!File.Exists(LibraryFile)) return new();
            var json = File.ReadAllText(LibraryFile);
            return JsonSerializer.Deserialize<List<string>>(json) ?? new();
        }
        catch { return new(); }
    }

    private static void SavePaths(List<string> paths)
    {
        try
        {
            Directory.CreateDirectory(AppDataFolder);
            var json = JsonSerializer.Serialize(paths, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(LibraryFile, json);
        }
        catch { /* ignore write errors */ }
    }
}
