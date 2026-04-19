
namespace WaveMusic.Converters;

public class BoolToPlayIconConverter : IValueConverter
{
    public object Convert(object v, Type t, object p, CultureInfo c) => (bool)v ? "⏸" : "▶";
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object v, Type t, object p, CultureInfo c)
        => (bool)v ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}

// Shows placeholder icon when image IS null
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object v, Type t, object p, CultureInfo c)
        => v == null ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}

// Shows image when value is NOT null
public class NullToCollapsedConverter : IValueConverter
{
    public object Convert(object v, Type t, object p, CultureInfo c)
        => v != null ? Visibility.Visible : Visibility.Collapsed;
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object v, Type t, object p, CultureInfo c)
        => (bool)v
            ? new SolidColorBrush(WpfColor.FromRgb(255, 0, 51))
            : new SolidColorBrush(Colors.White);
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}

// TimeSpan -> double (total seconds) — used by progress slider binding
public class TimeSpanToDoubleConverter : IValueConverter
{
    public object Convert(object v, Type t, object p, CultureInfo c)
        => v is TimeSpan ts ? ts.TotalSeconds : 0.0;
    public object ConvertBack(object v, Type t, object p, CultureInfo c)
        => v is double d ? TimeSpan.FromSeconds(d) : TimeSpan.Zero;
}

// TimeSpan -> "m:ss" string display
public class TimeSpanToStringConverter : IValueConverter
{
    public object Convert(object v, Type t, object p, CultureInfo c)
        => v is TimeSpan ts ? ts.ToString(@"m\:ss") : "0:00";
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}
