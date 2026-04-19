namespace WaveMusic.Pages;

public partial class UpgradePage : WpfUserControl
{
    public UpgradePage() => InitializeComponent();

    private void TryFree_Click(object sender, RoutedEventArgs e)
        => MessageBox.Show(
            "🎵 Wave Premium\n\nThis is a local music player — enjoy it free forever!\nAll features are available without any subscription.",
            "Wave Premium", MessageBoxButton.OK, MessageBoxImage.Information);
}
