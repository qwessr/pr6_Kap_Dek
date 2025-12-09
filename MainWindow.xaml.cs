using Mysqlx.Notice;
using RegIN.Classes;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace RegIN
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow mainWindow;
        public User UserLogIn = new User();

        public MainWindow()
        {
            InitializeComponent();
            mainWindow = this;
            OpenPage(new Pages.Login());
        }

        public void OpenPage(Page page)
        {
            DoubleAnimation StartAnimation = new DoubleAnimation();
            StartAnimation.From = 1;
            StartAnimation.To = 0;
            
            StartAnimation.Duration = TimeSpan.FromSeconds(0.6);
            StartAnimation.Completed += delegate
            {
                frame.Navigate(page);
                DoubleAnimation EndAnimation = new DoubleAnimation();
                EndAnimation.From = 0;
                EndAnimation.To = 1;
                EndAnimation.Duration = TimeSpan.FromSeconds(1.2);
                frame.BeginAnimation(OpacityProperty, EndAnimation);
            };
            frame.BeginAnimation(OpacityProperty, StartAnimation);
        }
    }
}