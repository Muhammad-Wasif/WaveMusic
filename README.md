# 🎵 WaveMusic — YouTube Music Clone for Windows

A full-featured, dark-themed local music player built in **C# WPF (.NET 8)**, inspired by YouTube Music.

---

## ✅ Features

| Feature | Details |
|---|---|
| 🏠 **Home** | Mood chips, Listen Again cards, Quick Picks list |
| 🔍 **Explore** | New Releases, Charts, Trending list with gradients |
| 📚 **Library** | Browse local music folder, full song list with tracks |
| ⭐ **Upgrade** | Premium page with feature overview |
| ▶️ **Player Bar** | Play/Pause, Next, Prev, Shuffle, Repeat, Volume, Progress |
| 🖼️ **Thumbnails** | Auto-match image + audio by filename; embedded tags fallback |
| 🎵 **Formats** | MP3, WAV, FLAC, WMA, M4A, OGG, AAC |

---

## 📦 How to Build

### Requirements
- Windows 10/11
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 **or** just the .NET CLI

### Build & Run

```bash
cd WaveMusic
dotnet restore
dotnet build
dotnet run
```

Or open `WaveMusic.csproj` in **Visual Studio 2022** and press **F5**.

---

## 🎵 Loading Your Music

1. Launch the app
2. Click **Library** in the left sidebar
3. Click **Browse Folder**
4. Select any folder containing audio files

### Thumbnail Matching

Put image and audio files with the **same name** in the same folder:

```
Music/
├── Kesariya.mp3          ← audio
├── Kesariya.jpg          ← thumbnail (matched by name!)
├── Raatan Lambiyan.flac
├── Raatan Lambiyan.png   ← thumbnail matched!
└── Tere Bina.mp3         ← uses embedded album art from MP3 tags
```

Supported image formats: `.jpg`, `.jpeg`, `.png`, `.bmp`, `.webp`

If no matching image is found, the app uses the **embedded album art** from the audio file's metadata tags (ID3, etc.). If that's also missing, a music note placeholder is shown.

---

## 🗂️ Project Structure

```
WaveMusic/
├── App.xaml / App.xaml.cs          ← Global styles & dark theme
├── MainWindow.xaml / .cs           ← Main window: title bar, sidebar, player
├── Models/
│   └── Song.cs                     ← Song & Playlist data models
├── Services/
│   ├── MusicLibraryService.cs      ← Folder scanning + thumbnail matching
│   └── MusicPlayerService.cs       ← Audio playback engine (MediaPlayer)
├── Converters/
│   └── Converters.cs               ← XAML value converters
└── Pages/
    ├── HomePage.xaml / .cs          ← Home tab
    ├── ExplorePage.xaml / .cs       ← Explore tab
    ├── LibraryPage.xaml / .cs       ← Library + folder browser
    └── UpgradePage.xaml / .cs       ← Premium page
```

---

## 🎨 Theme

Exact dark theme matching YouTube Music:
- Background: `#0F0F0F`
- Sidebar: `#1F1F1F`
- Cards: `#272727`
- Accent: `#FF0033` (Red)
- Player bar: `#1A1A1A`

---

## ⌨️ Keyboard Shortcuts

| Key | Action |
|---|---|
| `Enter` in search | Search your library |
| `Space` | Play / Pause (click player) |

---

*Built with WPF + TagLibSharp for metadata reading.*
