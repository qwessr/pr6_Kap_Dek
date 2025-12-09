using Aspose.Imaging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace RegIN.Pages
{
    /// <summary>
    /// Логика взаимодействия для RegIn.xaml
    /// </summary>
    public partial class RegIn : Page
    {
        OpenFileDialog FileDialogImage = new OpenFileDialog();
        bool BCorrectLogin = false;
        bool BCorrectPassword = false;  
        bool BCorrectConfirmPassword = false;
        bool BSetImages = false;
        public RegIn()
        {
            InitializeComponent();
            MainWindow.mainWindow.UserLogIn.HandlerCorrectLogin += CorrectLogin;
            MainWindow.mainWindow.UserLogIn.HandlerInCorrectLogin += InCorrectLogin;
            FileDialogImage.Filter = "PNG (*.png)|*.png|JPG (*.jpg)|*.jpg";
            FileDialogImage.RestoreDirectory = true;
            FileDialogImage.Title = "Choose a photo for your avatar";
        }
        public void SetNotification(string Message, SolidColorBrush _Color)
        {
            lNameUser.Content = Message;
            lNameUser.Foreground = _Color;
        }
        private void CorrectLogin()
        {
            SetNotification("Login already in use", Brushes.Red);
            BCorrectLogin = false;
        }

        private void InCorrectLogin() => SetNotification("", Brushes.Black);
        private void SetLogin(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SetLogin();
        }

        private void SetLogin(object sender, System.Windows.RoutedEventArgs e) => SetLogin();

        public void SetLogin()
        {
            Regex regex = new Regex(@"([a-zA-Z0-9._-]{4,})@[a-zA-Z0-9._-]{2,}.[a-zA-Z0-9._-]{2,}");
            BCorrectLogin = regex.IsMatch(TbLogin.Text);

            if (regex.IsMatch(TbLogin.Text) == true)
            {
                SetNotification("", Brushes.Black);
                MainWindow.mainWindow.UserLogIn.GetUserLogin(TbLogin.Text);
            }
            else
                SetNotification("Invalid login", Brushes.Red);

            OnRegIn();
        }
        #region SetPassword

        private void SetPassword(object sender, System.Windows.RoutedEventArgs e) => SetPassword();

        private void SetPassword(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SetPassword();
        }

        public void SetPassword()
        {
            Regex regex = new Regex(@"(?=.*[0-9])(?=.*[!@#$%^&?*\-\-=])(?=.*[a-z])(?=.*[A-Z])[0-9a-zA-Z!@#$%^&?*\-\-=]{10,}");

            BCorrectPassword = regex.IsMatch(PbPassword.Password);

            if (regex.IsMatch(PbPassword.Password) == true)
            {
                SetNotification("", Brushes.Black);

                if (PbConfirmPassword.Password.Length > 0)
                    ConfirmPassword(true);

                OnRegIn();
            }
            else
                SetNotification("Invalid password", Brushes.Red);
        }
        #endregion

        #region SetConfirmPassword

        private void ConfirmPassword(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ConfirmPassword();
        }

        private void ConfirmPassword(object sender, System.Windows.RoutedEventArgs e) => ConfirmPassword();

        public void ConfirmPassword(bool Pass = false)
        {
            BCorrectConfirmPassword = PbConfirmPassword.Password == PbPassword.Password;

            if (PbConfirmPassword.Password != PbPassword.Password)
                SetNotification("Passwords do not match", Brushes.Red);
            else
            {
                SetNotification("", Brushes.Black);

                if (!Pass)
                    SetPassword();
            }
        }
        #endregion
        void OnRegIn()
        {
            if (!BCorrectLogin)
                return;

            if (TbName.Text.Length == 0)
                return;

            if (!BCorrectPassword)
                return;

            if (!BCorrectConfirmPassword)
                return;

            // Заполняем объект User данными из UI
            MainWindow.mainWindow.UserLogIn.Login = TbLogin.Text;
            MainWindow.mainWindow.UserLogIn.Password = PbPassword.Password;
            MainWindow.mainWindow.UserLogIn.Name = TbName.Text;

            if (BSetImages)
                MainWindow.mainWindow.UserLogIn.Image = File.ReadAllBytes(Directory.GetCurrentDirectory() + @"\IUser.jpg");
            else
                MainWindow.mainWindow.UserLogIn.Image = new byte[0]; // Убедимся, что Image не null

            MainWindow.mainWindow.UserLogIn.DateUpdate = DateTime.Now;
            MainWindow.mainWindow.UserLogIn.DateCreate = DateTime.Now;

            // --- НОВЫЙ КОД: Вызов сохранения в БД ---
            try
            {
                // Вызываем метод сохранения пользователя в БД
                MainWindow.mainWindow.UserLogIn.SetUser();
                // Сообщаем пользователю об успехе (опционально)
                // MessageBox.Show("Данные пользователя успешно сохранены в базу данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Обрабатываем возможные ошибки при сохранении
                MessageBox.Show($"Ошибка при сохранении данных пользователя в базу данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                // В зависимости от логики, можно не переходить на страницу подтверждения при ошибке
                // return; // Если нужно остановить выполнение
            }
            // --- КОНЕЦ НОВОГО КОДА ---

            // Переходим на страницу подтверждения
            MainWindow.mainWindow.OpenPage(new Confirmation(Confirmation.TypeConfirmation.Regin));
        }
        private void SetName(object sender,TextCompositionEventArgs e)
        {
            e.Handled = !(Char.IsLetter(e.Text, 0));
        }
        private void SelectImage(object sender, MouseButtonEventArgs e)
        {
            if (FileDialogImage.ShowDialog() == true)
            {
                using (Aspose.Imaging.Image image = Aspose.Imaging.Image.Load(FileDialogImage.FileName))
                {
                    int NewWidth = 0;
                    int NewHeight = 0;

                    if (image.Width > image.Height)
                    {
                        NewWidth = (int)(image.Width * (256f / image.Height));
                        NewHeight = 256;
                    }
                    else
                    {
                        NewWidth = 256;
                        NewHeight = (int)(image.Height * (256f / image.Width));
                    }

                    image.Resize(NewWidth, NewHeight);
                    image.Save("IUser.jpg");
                }

                using (Aspose.Imaging.RasterImage rasterImage = (Aspose.Imaging.RasterImage)Aspose.Imaging.Image.Load("IUser.jpg"))
                {
                    if (!rasterImage.IsCached)
                    {
                        rasterImage.CacheData();
                    }

                    int X = 0;
                    int Width = 256;
                    int Y = 0;
                    int Height = 256;

                    if (rasterImage.Width > rasterImage.Height)
                    {
                        X = (int)((rasterImage.Width - 256f) / 2);
                    }
                    else
                    {
                        Y = (int)((rasterImage.Height - 256f) / 2);
                    }

                    Aspose.Imaging.Rectangle rectangle = new Aspose.Imaging.Rectangle(X, Y, Width, Height);
                    rasterImage.Crop(rectangle);
                    rasterImage.Save("IUser.jpg");
                }

                DoubleAnimation StartAnimation = new DoubleAnimation();
                StartAnimation.From = 1;
                StartAnimation.To = 0;
                StartAnimation.Duration = TimeSpan.FromSeconds(0.6);

                StartAnimation.Completed += delegate
                {
                    IUser.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\IUser.jpg"));

                    DoubleAnimation EndAnimation = new DoubleAnimation();
                    EndAnimation.From = 0;
                    EndAnimation.To = 1;
                    EndAnimation.Duration = TimeSpan.FromSeconds(1.2);

                    IUser.BeginAnimation(OpacityProperty, EndAnimation);
                };

                IUser.BeginAnimation(OpacityProperty, StartAnimation);
                BSetImages = true;
            }
            else
            {
                BSetImages = false;
            }
        }
        private void OpenLogin(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mainWindow.OpenPage(new Login());
        }
    }
}
