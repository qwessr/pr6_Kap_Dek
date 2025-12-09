using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
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
                SetNotification("Hi, " + MainWindow.mainWindow.UserLogIn.Name, Brushes.Black);

               
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
                };
                OldLogin = TbLogin.Text;
            }
        }


        public void InCorrectLogin()
        {
            if (LNameUser.Content != "")
            {
                LNameUser.Content = "";
                DoubleAnimation StartAnimation = new DoubleAnimation();
                StartAnimation.From = 1;
                StartAnimation.To = 0;
                StartAnimation.Duration = TimeSpan.FromSeconds(0.6);
                StartAnimation.Completed += delegate
                {
                    IUser.Source = new BitmapImage(new Uri("pack://application:,,,/Images/ic-user.png"));
                    DoubleAnimation EndAnimation = new DoubleAnimation();
                    EndAnimation.From = 0;
                    EndAnimation.To = 1;
                    EndAnimation.Duration = TimeSpan.FromSeconds(1.2);
                    IUser.BeginAnimation(OpacityProperty, EndAnimation);
                };
                IUser.BeginAnimation(OpacityProperty, StartAnimation);
            }

            if (TbLogin.Text.Length > 0)
                SetNotification("Login is incorrect", Brushes.Red);
        }

        public void CorrectCapture()
        {
            Capture.IsEnabled = false;
            IsCapture = true;
        }
        private void SetPassword(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SetPassword();
        }

        public void SetPassword()
        {
            if (MainWindow.mainWindow.UserLogIn.Password != String.Empty)
            {
                if (IsCapture == false)
                {
                    SetNotification("Enter capture", Brushes.Red);
                    return;
                }

                if (MainWindow.mainWindow.UserLogIn.Password == TbPassword.Password)
                {
                    MainWindow.mainWindow.OpenPage(new Pages.Confirmation(Confirmation.TypeConfirmation.Login));
                    return;
                }
                if (CountSetPassword > 0)
                {
                    SetNotification($"Password is incorrect, {CountSetPassword} attempts left", Brushes.Red);
                    CountSetPassword--;
                    return;
                }
                Thread TBlockAutorization = new Thread(BlockAutorization);
                TBlockAutorization.Start();

                SendMail.SendMessage("An attempt was made to log into your account.", MainWindow.mainWindow.UserLogIn.Login);
            }
        }
        public void BlockAutorization()
        {
            DateTime StartBlock = DateTime.Now.AddMinutes(3);
            Dispatcher.Invoke(() =>
            {
                TbLogin.IsEnabled = false;
                TbPassword.IsEnabled = false;
                IsEnabled = false;
            });
            // Запускаем цикл в 180 шагов | 180/60 = 3 минуты
            for (int i = 0; i < 180; i++)
            {
                // получаем оставшееся время
                TimeSpan TimeIdle = StartBlock.Subtract(DateTime.Now);
                // Получаем минуты
                string s_minutes = TimeIdle.Minutes.ToString();
                // Если минуты меньше 10
                if (TimeIdle.Minutes < 10)
                    // Добавляем 0
                    s_minutes = "0" + TimeIdle.Minutes;
                // Получаем секунды
                string s_seconds = TimeIdle.Seconds.ToString();
                // Если секунды меньше 10
                if (TimeIdle.Seconds < 10)
                    // Добавляем 0
                    s_seconds = "0" + TimeIdle.Seconds;
                // Вне потока
                Dispatcher.Invoke(() =>
                {
                    // Выводим время до разблокировки, цвет карсный
                    SetNotification($"Reauthorization available in: {s_minutes}:{s_seconds}", Brushes.Red);
                });
                // Ждём 1 секунду
                Thread.Sleep(1000);
            }
            // Вне потока
            Dispatcher.Invoke(() =>
            {
                // Выводим логин авторизованного пользователя, цвет чёрный
                SetNotification("Hi, " + MainWindow.mainWindow.UserLogIn.Name, Brushes.Black);
                // Включаем ввод логина
                TbLogin.IsEnabled = true;
                // Включаем ввод пароля
                TbPassword.IsEnabled = true;
                // Включаем капчу
                Capture.IsEnabled = true;
                // Вызываем генерацию новой капчи
                Capture.CreateCapture();
                // Запоминаем о том что капча не введена
                IsCapture = false;
                // Устанавливаем кол-во попыток 2
                CountSetPassword = 2;
            });
        }
        /// <summary>
        /// Метод уведомлений пользователя
        /// </summary>
        /// <param name="Message">Сообщение которое необходимо вывести</param>
        /// <param name="_Color">Цвет сообщения</param>
        public void SetNotification(string Message, SolidColorBrush _Color)
        {
            // Для текстового поля указываем текст
            LNameUser.Content = Message;
            // Для текстового поля указываем цвет
            LNameUser.Foreground = _Color;
        }

        /// <summary>
        /// Метод открытия страницы восстановления пароля
        /// </summary>
        private void RecoveryPassword(object sender, MouseButtonEventArgs e) =>
            MainWindow.mainWindow.OpenPage(new Recovery());

        /// <summary>
        /// Метод открытия страницы регистрации
        /// </summary>
        private void OpenRegin(object sender, MouseButtonEventArgs e) =>
            MainWindow.mainWindow.OpenPage(new RegIn());

        private void SetLogin(object sender, KeyEventArgs e)
        {
                // При нажатии на кнопку Enter
                if (e.Key == Key.Enter)
                {
                    // Вызываем метод получения данных пользователя по логину
                    MainWindow.mainWindow.UserLogIn.GetUserLogin(TbLogin.Text);

                    // Если пароль пользователя введён
                    if (TbPassword.Password.Length > 0)
                        // Вызываем метод ввода пароля
                        SetPassword();
                }
        }

        private void SetLogin(object sender, RoutedEventArgs e)
        {

        }
    }
}
