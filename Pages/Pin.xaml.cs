using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RegIN.Pages
{
    /// <summary>
    /// Логика взаимодействия для Pin.xaml
    /// </summary>
    public partial class Pin : Page
    {
        bool isPin;
        public Pin(bool isPin)
        {
            InitializeComponent();
            this.isPin = isPin;
        }

        private void SetPin(object sender, RoutedEventArgs e)
        {
            if (isPin == true)
            {
                if (TbPin.Text == MainWindow.mainWindow.UserLogIn.Pincode)
                    MessageBox.Show("Успешная авторизация");
                else
                    MessageBox.Show("Не правильный пин-код");
            }
            else
            {
                if (!string.IsNullOrEmpty(TbPin.Text) && TbPin.Text.Length == 4)
                {
                    MainWindow.mainWindow.UserLogIn.SetPin(TbPin.Text);

                    MessageBox.Show("Пин-код успешно установлен");
                }
                else MessageBox.Show("Неккоректный пин код");
            }
        }

        private void SetCode(object sender, KeyEventArgs e)
        {

        }

        private void Miss(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mainWindow.frame.Navigate(new Login());
        }
    }
}
