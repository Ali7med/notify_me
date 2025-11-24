using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace NotifyMe.UI.Windows
{
    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public partial class CustomNotificationWindow : Window
    {
        private readonly DispatcherTimer _autoCloseTimer;
        private bool _isClosing;

        public CustomNotificationWindow(NotificationType type, string title, string message, string position)
        {
            InitializeComponent();
            
            // Set Content
            TitleText.Text = title;
            MessageText.Text = message;
            
            // Apply Style based on Type
            ApplyTypeStyle(type);
            
            // Position Window
            Loaded += (s, e) => 
            {
                SetPosition(position);
                SlideIn();
            };

            // Auto-close timer
            _autoCloseTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _autoCloseTimer.Tick += (s, e) => SlideOut();
            _autoCloseTimer.Start();
        }

        private void ApplyTypeStyle(NotificationType type)
        {
            switch (type)
            {
                case NotificationType.Success:
                    AccentBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CC2FF")); // Blue/Greenish
                    IconText.Text = "✅";
                    break;
                case NotificationType.Error:
                    AccentBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF4C4C")); // Red
                    IconText.Text = "❌";
                    break;
                case NotificationType.Warning:
                    AccentBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFD700")); // Yellow
                    IconText.Text = "⚠️";
                    break;
                case NotificationType.Info:
                default:
                    AccentBar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00A8E8")); // Blue
                    IconText.Text = "ℹ️";
                    break;
            }
        }

        private void SetPosition(string position)
        {
            var workArea = SystemParameters.WorkArea;
            var margin = 20;

            switch (position)
            {
                case "TopLeft":
                    Left = workArea.Left + margin;
                    Top = workArea.Top + margin;
                    break;
                case "BottomRight":
                    Left = workArea.Right - Width - margin;
                    Top = workArea.Bottom - Height - margin;
                    break;
                case "BottomLeft":
                    Left = workArea.Left + margin;
                    Top = workArea.Bottom - Height - margin;
                    break;
                case "TopRight":
                default:
                    Left = workArea.Right - Width - margin;
                    Top = workArea.Top + margin;
                    break;
            }
        }

        private void SlideIn()
        {
            var anim = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.3))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            BeginAnimation(OpacityProperty, anim);
        }

        private void SlideOut()
        {
            if (_isClosing) return;
            _isClosing = true;
            _autoCloseTimer.Stop();

            var anim = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.3))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            anim.Completed += (s, e) => Close();
            BeginAnimation(OpacityProperty, anim);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            SlideOut();
        }
    }
}
