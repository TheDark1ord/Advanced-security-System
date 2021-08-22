using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Linq;


namespace Advanced_security_System_C
{
    public class MultiValueEqualityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values[0] == values[1];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ThumbnailSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureinfo)
        {
            if (value.GetType() != typeof(double))
                throw new ArgumentException("Bad argument type, doiuble requaired");

            return (double)value * 0.85;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureinfo)
        {
            throw new NotImplementedException();
        }
    }
    public class SideFolderControl : Control
    {
        public static readonly DependencyProperty IndexProperty =
        DependencyProperty.Register(nameof(Index), typeof(int), typeof(SideFolderControl),
        new PropertyMetadata(default(int)));
        public int Index
        {
            get { return (int)GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }

        public static readonly DependencyProperty GalleryNameSideProperty =
            DependencyProperty.Register(nameof(GalleryNameSide), typeof(string), typeof(SideFolderControl),
                new PropertyMetadata(default(string), new PropertyChangedCallback(OnPropertyChanged)));

        private static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FolderControl parent = obj as FolderControl;
            if (parent is null)
                return;
            ControlTemplate template = parent.Template;

            (template.FindName("LabelControl", parent) as Label).Content = (e.NewValue as String);
        }
        public string GalleryNameSide
        {
            get { return (string)GetValue(GalleryNameSideProperty); }
            set { SetValue(GalleryNameSideProperty, value); GalleryNameSideChanged(); }
        }

        private void GalleryNameSideChanged()
        {
            ControlTemplate template = this.Template;
            Label labelControl = (template.FindName("LabelControl", this) as Label);
            if (labelControl is null)
                return;
            labelControl.Content = GalleryNameSide.ToString();
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(string), typeof(SideFolderControl),
                new FrameworkPropertyMetadata("../../Resource files/billy.jpeg"));

        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        public SideFolderControl()
        {
            this.Loaded += OnLoadded;
        }

        private void OnLoadded(object sender, RoutedEventArgs e)
        {
            MainAppWindow window = Window.GetWindow(this) as MainAppWindow;
            window.RegisterName("SG" + Index.ToString(), this);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ControlTemplate template = this.Template;

            if(selected)
            {
                Border outsideBorder = template.FindName("OuterBorder", this) as Border;
                VisualStateManager.GoToElementState(outsideBorder, "Selected", true);
            }

            (template.FindName("DataPanel", this) as StackPanel).MouseLeftButtonDown += new MouseButtonEventHandler(Click);
            (template.FindName("DataPanel", this) as StackPanel).MouseEnter += new MouseEventHandler(MouseEntered);
            (template.FindName("DataPanel", this) as StackPanel).MouseLeave += new MouseEventHandler(MouseLeft);
            loadImage();
        }

        public void Select()
        {
            selected = true;
            ControlTemplate template = this.Template;
            Border OutsideBorder = template.FindName("OuterBorder", this) as Border;

            if (OutsideBorder is null)
                return;
            VisualStateManager.GoToElementState(OutsideBorder, "Selected", true);
        }

        public void Deselect()
        {
            selected = false;
            var OutsideBorder = (this.Template).FindName("OuterBorder", this) as Border;

            if (OutsideBorder is null)
                return;
            VisualStateManager.GoToElementState(OutsideBorder, "Default", true);
        }

        private void loadImage()
        {
            ControlTemplate template = this.Template;
            Image img = template.FindName("ImageControl", this) as Image;
            BitmapImage img_src = new BitmapImage(new Uri(Source, UriKind.Relative));
            //img_src.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            img.Source = img_src;
        }

        private void Click(object sender, MouseButtonEventArgs e)
        {
            if (selected)
                return;
            MainAppWindow window = Window.GetWindow(this) as MainAppWindow;
            window.SelectFolder(this.Index);
        }

        private void MouseEntered(object sender, MouseEventArgs e)
        {
            if (selected)
                return;

            ControlTemplate template = this.Template;
            Border OutsideBorder = template.FindName("OuterBorder", this) as Border;
            VisualStateManager.GoToElementState(OutsideBorder, "MouseOver", true);
        }

        private void MouseLeft(object sender, MouseEventArgs e)
        {
            ControlTemplate template = this.Template;
            Border OutsideBorder = template.FindName("OuterBorder", this) as Border;

            if (selected)
                VisualStateManager.GoToElementState(OutsideBorder, "SelectedInst", true);
            else
                VisualStateManager.GoToElementState(OutsideBorder, "Default", true);
        }

        private bool selected = false;
    }
}
