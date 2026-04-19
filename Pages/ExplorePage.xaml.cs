// GlobalUsings.cs already defines WpfColor, System.Windows.*, etc.

namespace WaveMusic.Pages;

public class AlbumItem
{
    public string Title { get; set; } = "";
    public string SubTitle { get; set; } = "";
    public string ShortTitle { get; set; } = "";
    public string Glyph { get; set; } = "🎵";
    public WpfColor GradColor1 { get; set; }
    public WpfColor GradColor2 { get; set; }
}

public class TrendingItem
{
    public string Rank { get; set; } = "";
    public string Title { get; set; } = "";
    public string Artist { get; set; } = "";
    public string Glyph { get; set; } = "";
    public string Trend { get; set; } = "▲";
}

public partial class ExplorePage : UserControl
{
    public ExplorePage()
    {
        InitializeComponent();
        LoadData();
    }

    private void LoadData()
    {
        NewReleasesPanel.ItemsSource = new List<AlbumItem>
        {
            new() { Title = "Bachke Ji Bachke (Jawaak)", SubTitle = "Single · Ammy Virk & Vari Rai",
                    ShortTitle = "JAWAAK", Glyph = "🎭",
                    GradColor1 = WpfColor.FromRgb(180, 100, 20), GradColor2 = WpfColor.FromRgb(80, 20, 5) },
            new() { Title = "Kaash Tu Mera Na Hunda", SubTitle = "Single · B Praak, Jyoti Nooran",
                    ShortTitle = "SHERA", Glyph = "🌊",
                    GradColor1 = WpfColor.FromRgb(30, 60, 130), GradColor2 = WpfColor.FromRgb(10, 20, 60) },
            new() { Title = "Ishq Bukhaar - New Viral Version", SubTitle = "Single · Shreya Ghoshal, B Praak",
                    ShortTitle = "IS HQ", Glyph = "❤️",
                    GradColor1 = WpfColor.FromRgb(160, 30, 30), GradColor2 = WpfColor.FromRgb(60, 5, 5) },
            new() { Title = "Braj Ras", SubTitle = "EP · B Praak, Jaani & Mir Desai",
                    ShortTitle = "BRAJ RAS", Glyph = "🪗",
                    GradColor1 = WpfColor.FromRgb(80, 50, 10), GradColor2 = WpfColor.FromRgb(30, 15, 0) },
            new() { Title = "Jiya Lagena - Shararat Bengali Version", SubTitle = "Single · Shashwat Sachdev",
                    ShortTitle = "JIYA", Glyph = "💃",
                    GradColor1 = WpfColor.FromRgb(120, 20, 80), GradColor2 = WpfColor.FromRgb(50, 5, 30) },
            new() { Title = "Kesariya (Remastered)", SubTitle = "Single · Arijit Singh",
                    ShortTitle = "KESARIYA", Glyph = "🌅",
                    GradColor1 = WpfColor.FromRgb(180, 100, 0), GradColor2 = WpfColor.FromRgb(80, 30, 0) },
            new() { Title = "Raatan Lambiyan 2.0", SubTitle = "Single · Jubin Nautiyal",
                    ShortTitle = "RAATAN", Glyph = "🌙",
                    GradColor1 = WpfColor.FromRgb(20, 20, 80), GradColor2 = WpfColor.FromRgb(5, 5, 30) },
            new() { Title = "Tere Bina – Unplugged", SubTitle = "Single · Atif Aslam",
                    ShortTitle = "TERE BINA", Glyph = "🎸",
                    GradColor1 = WpfColor.FromRgb(40, 80, 40), GradColor2 = WpfColor.FromRgb(10, 25, 10) },
        };

        TrendingList.ItemsSource = new List<TrendingItem>
        {
            new() { Rank = "1", Title = "Kesariya",               Artist = "Arijit Singh",    Glyph = "🌅", Trend = "▲ 1" },
            new() { Rank = "2", Title = "Raatan Lambiyan",        Artist = "Jubin Nautiyal",  Glyph = "🌙", Trend = "▲ 3" },
            new() { Rank = "3", Title = "Tere Bina",              Artist = "Atif Aslam",      Glyph = "🎸", Trend = "= 0" },
            new() { Rank = "4", Title = "Ae Dil Hai Mushkil",     Artist = "Pritam",          Glyph = "💔", Trend = "▼ 1" },
            new() { Rank = "5", Title = "Meri Zindagi Hai Tu",    Artist = "Asim Azhar",      Glyph = "🎵", Trend = "▲ 2" },
            new() { Rank = "6", Title = "Ishq Bukhaar",           Artist = "Shreya Ghoshal",  Glyph = "🔥", Trend = "▲ 5" },
            new() { Rank = "7", Title = "Braj Ras",               Artist = "B Praak, Jaani",  Glyph = "🪗", Trend = "▲ 4" },
            new() { Rank = "8", Title = "Main Rahoon Ya Na Rahoon", Artist = "Armaan Malik",  Glyph = "💫", Trend = "= 0" },
        };
    }

    private void NewReleases_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        => MessageBox.Show("Showing latest releases.", "New Releases", MessageBoxButton.OK, MessageBoxImage.Information);

    private void Charts_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        => MessageBox.Show("Top charts based on trending songs.", "Charts", MessageBoxButton.OK, MessageBoxImage.Information);

    private void Moods_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        => MessageBox.Show("Filter music by mood, genre and more.", "Moods & Genres", MessageBoxButton.OK, MessageBoxImage.Information);

    private void AlbumCard_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is AlbumItem item)
            MessageBox.Show($"{item.Title}\n{item.SubTitle}", "Album", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
