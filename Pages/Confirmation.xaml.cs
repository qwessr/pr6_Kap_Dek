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
    /// Логика взаимодействия для Confirmation.xaml
    /// </summary>
    public partial class Confirmation : Page
    {
        /// <summary>
        /// Тип перечисления для чего используется подтверждение
        /// </summary>
        public enum TypeConfirmation
        {
            Login,
            Regin
        }

        /// <summary>
        /// Для его используется подтверждение
        /// </summary>
        TypeConfirmation ThisTypeConfirmation;

        /// <summary>
        /// Код отправленный на почту пользователя
        /// </summary>
        public int Code = 0;
        public Confirmation(TypeConfirmation TypeConfirmation)
        {
            InitializeComponent();
            // Запоминаем полученный тип подтверждения
            ThisTypeConfirmation = TypeConfirmation;
            // Отправляем сообщение на почту пользователя
            SendMailCode();
        }

        /// <summary>
        /// Метод отправки сообщения на почту
        /// </summary>
        public void SendMailCode()
        {
            // Генерируем случайное число
            Code = new Random().Next(100000, 999999);
            // Отправляем число на почту авторизуемого пользователя
            Classes.SendMail.SendMessage($"Login code: {Code}", MainWindow.mainWindow.UserLogIn.Login);
            // Инициализируем процесс в потоке для отправки повторного письма
            Thread TSendMailCode = new Thread(TimerSendMailCode);
            // Отправляем письмо
            TSendMailCode.Start();
        }

        /// <summary>
        /// Ожидание отправки нового кода
        /// </summary>
        public void TimerSendMailCode()
        {
            // Запускаем цикл в 60 шагов
            for (int i = 0; i < 60; i++)
            {
                // Выполняем вне потока
                Dispatcher.Invoke(() =>
                {
                    // Изменяем данные на текстовом поле
                    LTimer.Content = $"A second message can be sent after {(60 - i)} seconds";
                });
                // Ждём 1 секунду
                Thread.Sleep(1000);
            }
            // По истечению таймера вне потока
            Dispatcher.Invoke(() =>
            {
                // Включаем кнопку отправить повторно
                BSendMessage.IsEnabled = true;
                // Изменяем данные на текстовом поле
                LTimer.Content = "";
            });
        }

        /// <summary>
        /// Отправка кода подтверждения на почту пользователя
        /// </summary>
        private void SendMail(object sender, RoutedEventArgs e)
        {
            // Вызываем метод отправки сообщения на почту пользователя
            SendMailCode();
        }

        /// <summary>
        /// Вызов метода проверки отправленного кода на почту и введённого пользователем
        /// </summary>
        private void SetCode(object sender, KeyEventArgs e)
        {
            // Если текст введённый в поле 6 символов
            if (TbCode.Text.Length == 6)
                // Вызываем метод проверки кода
                SetCode();
        }

        /// <summary>
        /// Вызов метода проверки отправленного кода на почту и введённого пользователем
        /// </summary>
        private void SetCode(object sender, RoutedEventArgs e) =>
            // Вызываем метод проверки кода
            SetCode();

        /// <summary>
        /// метода проверки отправленного кода на почту и введённого пользователем
        /// </summary>
        void SetCode()
        {
            // Если текст в текстовом поле совпадает с кодом отправленным на почту
            // Если текстовое поле активировано
            if (TbCode.Text == Code.ToString() && TbCode.IsEnabled == true)
            {
                // Выключаем активацию поля
                TbCode.IsEnabled = false;
                // Если тип подтверждения является авторизацией
                if (ThisTypeConfirmation == TypeConfirmation.Login)
                    // Выводим сообщение о том что пользователь авторизовался
                    MessageBox.Show("Авторизация пользователя успешно подтверждена.");
                else
                {
                    // Если тип подтверждения является регистрацией
                    MainWindow.mainWindow.UserLogIn.SetUser();
                    // Выводим сообщение о том что пользователь зарегистрировался
                    MessageBox.Show("Регистрация пользователя успешно подтверждена.");
                }
            }
        }

        /// <summary>
        /// Метод открытия страницы авторизации
        /// </summary>
        private void OpenLogin(object sender, MouseButtonEventArgs e)
        {
            MainWindow.mainWindow.OpenPage(new Login());
        }
    }
}
