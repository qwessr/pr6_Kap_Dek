using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RegIN.Elements
{
    /// <summary>
    /// Логика взаимодействия для ElementCapture.xaml
    /// </summary>
    public partial class ElementCapture : UserControl
    {
        /// <summary>
        /// Событие которое вызывается при успешном вводе капчи
        /// </summary>
        public CorrectCapture HandlerCorrectCapture;
        /// <summary>
        /// Делегат выполнения успешного ввода капчи
        /// </summary>
        public delegate void CorrectCapture();

        /// <summary>
        /// Тектовое зачение капчи
        /// </summary>
        string StrCapture = "";
        /// <summary>
        /// Ширина капчи
        /// </summary>
        int ElementWidth = 280;
        /// <summary>
        /// Высота капчи
        /// </summary>
        int ElementHieght = 50;

        public ElementCapture()
        {
            InitializeComponent();
            // Вызываем функцию создания капчи
            CreateCapture();
        }

        /// <summary>
        /// Функция создания капчи
        /// </summary>
        public void CreateCapture()
        {
            // Очищаем пользовательское поле с капчей
            InputCapture.Text = "";
            // Очищаем элементы которые были созданы до этого в капче
            Capture.Children.Clear();
            // Очищаем текстовое значение капчи
            StrCapture = "";
            // Вызываем функцию создания заднего фона капчи
            CreateBackground();
            // Вызываем функцию создания переднего фона капчи
            Background();
        }

        #region CreateCapture
        /// <summary>
        /// Функция создания заднего фона капчи
        /// </summary>
        void CreateBackground()
        {
            // Инициализируем рандом
            Random ThisRandom = new Random();
            // Запускаем цикл от 0 до 100
            for (int i = 0; i < 100; i++)
            {
                // Выбираем случайное число от 0 до 10
                int back = ThisRandom.Next(0, 10);
                // Инициализируем новый элемент типа Label
                Label LBackground = new Label()
                {
                    // Задаём значение элемента
                    Content = back,
                    // Выбираем случайный шрифт
                    FontSize = ThisRandom.Next(10, 16),
                    // Указываем жирный шрифт
                    FontWeight = FontWeights.Bold,
                    // Указываем цвет шрифта (случайный)
                    Foreground = new SolidColorBrush(Color.FromArgb(100, (byte)ThisRandom.Next(0, 255), (byte)ThisRandom.Next(0, 255), (byte)ThisRandom.Next(0, 255))),
                    // Задаём отступы элемента (случайные)
                    Margin = new Thickness(ThisRandom.Next(0, ElementWidth - 20), ThisRandom.Next(0, ElementHieght - 20), 0, 0)
                };
                // Добавляем новый элемент в Grid на сцене
                Capture.Children.Add(LBackground);
            }
        }

        /// <summary>
        /// Функция создания переднего плана капчи
        /// </summary>
        void Background()
        {
            // Инициализируем рандом
            Random ThisRandom = new Random();
            // Запускаем цикл от 0 до 4
            for (int i = 0; i < 4; i++)
            {
                // Выбираем случайное число от 0 до 10
                int back = ThisRandom.Next(0, 10);
                // Инициализируем новый элемент типа Label
                Label LCode = new Label()
                {
                    // Задаём значение элемента
                    Content = back,
                    // Указываем шрифт 30
                    FontSize = 30,
                    // Указываем толщину шрифта
                    FontWeight = FontWeights.Bold,
                    // Указываем цвет шрифта (случайный)
                    Foreground = new SolidColorBrush(Color.FromArgb(255, (byte)ThisRandom.Next(0, 255), (byte)ThisRandom.Next(0, 255), (byte)ThisRandom.Next(0, 255))),
                    // Указываем отступы элемента
                    Margin = new Thickness(ElementWidth / 2 - 60 + i * 30, ThisRandom.Next(-10, 10), 0, 0)
                };
                // Записываем цифру в текстовое значение капчи
                StrCapture += back.ToString();
                // Добавляем новый элемент в Grid на сцене
                Capture.Children.Add(LCode);
            }
        }
        #endregion

        /// <summary>
        /// Функция проверки капчи
        /// </summary>
        /// <returns>Правильно ли введена капча</returns>
        public bool OnCapture()
        {
            // Если значения равны : True
            // Иначе : False
            return StrCapture == InputCapture.Text;
        }

        /// <summary>
        /// Автоматический ввод капчи
        /// </summary>
        private void EnterCapture(object sender, KeyEventArgs e)
        {
            // Если кол-во символов введённых в окно капчи 4
            if (InputCapture.Text.Length == 4)
                // Если проверка на капчу не проходит
                if (!OnCapture())
                    // Создаём новую капчу
                    CreateCapture();
                // Если проверка на капчу проходит и на событие кто-то подписан
                else if (HandlerCorrectCapture != null)
                    // Вызываем событие
                    HandlerCorrectCapture.Invoke();
        }
    }
}