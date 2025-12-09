using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using RegIN.Classes;
using RegIN.Elements;

namespace RegIN.Pages
{
    /// <summary>
    /// Логика взаимодействия для Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        string OldLogin;
        int CountSetPassword = 2;
        bool IsCapture = false;
        public Login()
        {
            InitializeComponent();

            MainWindow.mainWindow.UserLogIn.HandlerCorrectLogin += CorrectLogin;
            MainWindow.mainWindow.UserLogIn.HandlerInCorrectLogin += InCorrectLogin;
            Capture.HandlerCorrectCapture += CorrectCapture;
        }
        public void CorrectLogin()
        {
            if (OldLogin != TbLogin.Text)
            {
                OldLogin = TbLogin.Text;

                SetNotification("Hi, " + MainWindow.mainWindow.UserLogIn.Name, Brushes.Black);

                UpdateUserImage();
            }
        }

        private void UpdateUserImage()
        {
            try
            {
                BitmapImage biImg = new BitmapImage();
                MemoryStream ms = new MemoryStream(MainWindow.mainWindow.UserLogIn.Image);

                biImg.BeginInit();
                biImg.StreamSource = ms;
                biImg.EndInit();

                ImageSource imgSrc = biImg;

                DoubleAnimation StartAnimation = new DoubleAnimation();
                StartAnimation.From = 1;
                StartAnimation.To = 0;
                StartAnimation.Duration = TimeSpan.FromSeconds(0.6);
                StartAnimation.Completed += delegate
                {
                    IUser.Source = imgSrc;
                    DoubleAnimation EndAnimation = new DoubleAnimation();
                    EndAnimation.From = 0;
                    EndAnimation.To = 1;
                    EndAnimation.Duration = TimeSpan.FromSeconds(1.2);
                    IUser.BeginAnimation(Image.OpacityProperty, EndAnimation);
                };
                IUser.BeginAnimation(Image.OpacityProperty, StartAnimation);
            }
            catch (Exception exp)
            {
                Debug.WriteLine(exp.Message);
            }
        }

        private void ResetUserImage()
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.6));
            fadeOut.Completed += (sender, e) =>
            {
                IUser.Source = new BitmapImage(new Uri("pack://application:,,,/Images/users.jpg"));
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(1.2));
                IUser.BeginAnimation(Image.OpacityProperty, fadeIn);
            };
            IUser.BeginAnimation(Image.OpacityProperty, fadeOut);
        }

        public void InCorrectLogin()
        {
            if (LNameUser.Content.ToString() != "")
            {
                LNameUser.Content = "";

                ResetUserImage();
            }

            if (TbLogin.Text.Length > 0)
            {
                SetNotification("Login is incorrect", Brushes.Red);
            }
        }
        public void CorrectCapture()
        {
            Capture.IsEnabled = false;
            IsCapture = true;
        }

        private void SetPassword(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetPassword();
            }
        }


        public void SetPassword()
        {
            var userLogIn = MainWindow.mainWindow?.UserLogIn;

            if (userLogIn == null)
            {
                SetNotification("Ошибка: пользователь не найден", Brushes.Red);
                return;
            }

            if (string.IsNullOrEmpty(userLogIn.Password))
            {
                return;
            }

            if (!IsCapture)
            {
                SetNotification("Enter capture", Brushes.Red);
                return;
            }

            if (userLogIn.Password == TbPassword.Password)
            {
                bool hasPin = false;

                try
                {
                    hasPin = userLogIn.HasPin;
                }
                catch (NullReferenceException ex)
                {
                    Debug.WriteLine($"Ошибка проверки PIN: {ex.Message}");
                    hasPin = false;
                }

                MainWindow.mainWindow.OpenPage(hasPin
                    ? new Pin(true)
                    : new Pages.Confirmation(Confirmation.TypeConfirmation.Login));

                return;
            }

            if (CountSetPassword > 0)
            {
                SetNotification($"Password is incorrect, {CountSetPassword} attempts left", Brushes.Red);
                CountSetPassword--;
                return;
            }

            Thread blockThread = new Thread(BlockAutorization);
            blockThread.Start();

            if (!string.IsNullOrEmpty(userLogIn.Login))
            {
                SendMail.SendMessage("An attempt was made to log into your account.", userLogIn.Login);
            }
        }

        public void BlockAutorization()
        {
            DateTime StartBlock = DateTime.Now.AddMinutes(3);

            Dispatcher.Invoke(() =>
            {
                TbLogin.IsEnabled = false;
                TbPassword.IsEnabled = false;
                Capture.IsEnabled = false;
            });

            for (int i = 0; i < 180; i++)
            {
                TimeSpan TimeIdle = StartBlock.Subtract(DateTime.Now);
                string s_minutes = TimeIdle.Minutes.ToString();
                if (TimeIdle.Minutes < 10)
                    s_minutes = "0" + TimeIdle.Minutes;
                string s_second = TimeIdle.Seconds.ToString();
                if (TimeIdle.Seconds < 10)

                    s_second = "0" + TimeIdle.Seconds;

                Dispatcher.Invoke(() =>
                {
                    SetNotification($"Reauthorization available in : {s_minutes}:{s_second}", Brushes.Red);
                });
                Thread.Sleep(1000);
            }

            Dispatcher.Invoke(() =>
            {
                SetNotification("Hi, " + MainWindow.mainWindow.UserLogIn.Name, Brushes.Black);
                TbLogin.IsEnabled = true;
                TbPassword.IsEnabled = true;
                Capture.IsEnabled = true;
                Capture.CreateCapture();
                IsCapture = false;
                CountSetPassword = 2;
            });
        }
        private void OpenRegin(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mainWindow.OpenPage(new RegIn());
        }

        private void RecoveryPassword(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mainWindow.OpenPage(new Recovery());
        }

        private void SetLogin(object sender, RoutedEventArgs e) =>
            ValidateLogin();

        private void SetLogin(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ValidateLogin();
        }

        private void ValidateLogin()
        {
            if (string.IsNullOrEmpty(TbLogin.Text)) return;

            MainWindow.mainWindow.UserLogIn.GetUserLogin(TbLogin.Text);

            if (!string.IsNullOrEmpty(TbPassword.Password))
                SetPassword();
        }

        public void SetNotification(string Message, SolidColorBrush _Color)
        {
            LNameUser.Content = Message;
            LNameUser.Foreground = _Color;
        }
    }
}