using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace NotifyMe.UI.Controls
{
    public partial class StatCard : UserControl
    {
        public StatCard()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(StatCard), new PropertyMetadata("", OnTitleChanged));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StatCard card) card.TitleText.Text = (string)e.NewValue;
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(StatCard), new PropertyMetadata("", OnValueChanged));

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StatCard card) card.ValueText.Text = (string)e.NewValue;
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(PackIconKind), typeof(StatCard), new PropertyMetadata(PackIconKind.Help, OnIconChanged));

        public PackIconKind Icon
        {
            get { return (PackIconKind)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StatCard card) card.IconControl.Kind = (PackIconKind)e.NewValue;
        }
        
        public static readonly DependencyProperty IconForegroundProperty =
            DependencyProperty.Register("IconForeground", typeof(Brush), typeof(StatCard), new PropertyMetadata(Brushes.Gray, OnIconForegroundChanged));

        public Brush IconForeground
        {
            get { return (Brush)GetValue(IconForegroundProperty); }
            set { SetValue(IconForegroundProperty, value); }
        }

        private static void OnIconForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StatCard card) card.IconControl.Foreground = (Brush)e.NewValue;
        }

        public static readonly DependencyProperty TrendProperty =
            DependencyProperty.Register("Trend", typeof(string), typeof(StatCard), new PropertyMetadata("", OnTrendChanged));

        public string Trend
        {
            get { return (string)GetValue(TrendProperty); }
            set { SetValue(TrendProperty, value); }
        }

        private static void OnTrendChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StatCard card) card.TrendText.Text = (string)e.NewValue;
        }

        public static readonly DependencyProperty IsTrendPositiveProperty =
            DependencyProperty.Register("IsTrendPositive", typeof(bool), typeof(StatCard), new PropertyMetadata(true, OnIsTrendPositiveChanged));

        public bool IsTrendPositive
        {
            get { return (bool)GetValue(IsTrendPositiveProperty); }
            set { SetValue(IsTrendPositiveProperty, value); }
        }

        private static void OnIsTrendPositiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StatCard card)
            {
                bool isPositive = (bool)e.NewValue;
                card.TrendIcon.Kind = isPositive ? PackIconKind.ArrowUp : PackIconKind.ArrowDown;
                card.TrendIcon.Foreground = isPositive ? Brushes.Green : Brushes.Red;
                card.TrendText.Foreground = isPositive ? Brushes.Green : Brushes.Red;
            }
        }
    }
}
