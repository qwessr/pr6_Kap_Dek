using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System;

namespace RegIN.Classes
{
    public class User
    {
        /// <summary>
        /// Код пользователя
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Логин пользователя
        /// </summary>
        public string Login { get; set; }
        /// <summary>
        /// Пароль пользователя
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Изображение пользователя
        /// </summary>
        public byte[] Image = new byte[0];
        /// <summary>
        /// Дата и время обновления пользователя
        /// </summary>
        public DateTime DateUpdate { get; set; }
        /// <summary>
        /// Дата и время создания пользователя
        /// </summary>
        public DateTime DateCreate { get; set; }
        /// <summary>
        /// Событие успешной авторизации
        /// </summary>
        public CorrectLogin HandlerCorrectLogin;
        /// <summary>
        /// Событие не успешной авторизации
        /// </summary>
        public InCorrectLogin HandlerInCorrectLogin;

        /// <summary>
        /// Делегат успешной авторизации
        /// </summary>
        public delegate void CorrectLogin();
        /// <summary>
        /// Делегат не успешной авторизации
        /// </summary>
        public delegate void InCorrectLogin();

        /// <summary>
        /// Получение данных пользователя по логину
        /// </summary>
        /// <param name="Login">Логин пользователя</param>
        public void GetUserLogin(string Login)
        {
            // Устанавливаем первоначальные данные
            this.Id = -1;
            this.Login = String.Empty;
            this.Password = String.Empty;
            this.Name = String.Empty;
            this.Image = new byte[0];
            // Открываем соединение с базой данных
            MySqlConnection mySqlConnection = WorkingDB.OpenConnection();
            // Если соединение с базой данных успешно открыто
            if (WorkingDB.OpenConnection(mySqlConnection))
            {
                // Выполняем запрос получения пользователя по логину
                MySqlDataReader userQuery = WorkingDB.Query($"SELECT * FROM `users` WHERE `Login` = '{Login}'", mySqlConnection);
                // проверяем что существуют данные для чтения
                if (userQuery.HasRows)
                {
                    // Читаем пришедшие данные
                    userQuery.Read();
                    // Записываем код пользователя
                    this.Id = userQuery.GetInt32(0);
                    // Записываем логин пользователя
                    this.Login = userQuery.GetString(1);
                    // Записываем пароль пользователя
                    this.Password = userQuery.GetString(2);
                    // Записываем имя пользователя
                    this.Name = userQuery.GetString(3);
                    // Проверяем что изображение установлено
                    if (!userQuery.IsDBNull(4))
                    {
                        // Задаём размер массива
                        this.Image = new byte[64 * 1024];
                        // Записываем изображение пользователя
                        userQuery.GetBytes(4, 0, Image, 0, Image.Length);
                    }
                    // Записываем дату обновления
                    this.DateUpdate = userQuery.GetDateTime(5);
                    // Записываем дату создания
                    this.DateUpdate = userQuery.GetDateTime(6);
                    // Вызываем событие успешной авторизации
                    HandlerCorrectLogin.Invoke();
                }
                else
                    // Если данные для чтения не существуют, вызываем событие не успешной авторизации
                    HandlerInCorrectLogin.Invoke();
            }
            else
                // Если соединение открыть не удаётся, вызываем событие не успешной авторизации
                HandlerInCorrectLogin.Invoke();

            // Закрываем соединение с базой данных
            WorkingDB.CloseConnection(mySqlConnection);
        }

        /// <summary>
        /// Функция сохранения пользователя
        /// </summary>
        public void SetUser()
        {
            // Открываем соединение с базой данных
            MySqlConnection mySqlConnection = WorkingDB.OpenConnection();
            // Проверяем что соединение действительно открыто
            if (WorkingDB.OpenConnection(mySqlConnection))
            {
                // Создаём запрос на добавление пользователя
                MySqlCommand mySqlCommand = new MySqlCommand("INSERT INTO `users`(`Login`, `Password`, `Name`, `Image`, `DateUpdate`, `DateCreate`) VALUES (@Login, @Password, @Name, @Image, @DateUpdate, @DateCreate)", mySqlConnection);
                // Добавляем параметр логина
                mySqlCommand.Parameters.AddWithValue("@Login", this.Login);
                // Добавляем параметр пароля
                mySqlCommand.Parameters.AddWithValue("@Password", this.Password);
                // Добавляем параметр наименования
                mySqlCommand.Parameters.AddWithValue("@Name", this.Name);
                // Добавляем параметр изображения
                mySqlCommand.Parameters.AddWithValue("@Image", this.Image);
                // Добавляем параметр даты обновления
                mySqlCommand.Parameters.AddWithValue("@DateUpdate", this.DateUpdate);
                // Добавляем параметр даты создания
                mySqlCommand.Parameters.AddWithValue("@DateCreate", this.DateCreate);
                // Выполняем запрос без возврата результата
                mySqlCommand.ExecuteNonQuery();
            }
            // Закрываем подключение к базе данных
            WorkingDB.CloseConnection(mySqlConnection);
        }

        /// <summary>
        /// Функция создания нового пароля
        /// </summary>
        public void CrateNewPassword()
        {
            // Если наш логин не равне пустому значению
            // А это значит что наш пользователь существует
            if (Login != String.Empty)
            {
                // Вызываем функцию генерации пароля
                Password = GeneratePass();
                // Открываем подключение к базе данных
                MySqlConnection mySqlConnection = WorkingDB.OpenConnection();
                // Проверяем что подключение действительно открыто
                if (WorkingDB.OpenConnection(mySqlConnection))
                {
                    // Выполняем запрос, обновляя пароль у выбранного пользователя
                    WorkingDB.Query($"UPDATE `users` SET `Password`='{this.Password}' WHERE `Login` = '{this.Login}'", mySqlConnection);
                }
                // Закрываем подключение к базе данных
                WorkingDB.CloseConnection(mySqlConnection);
                // Отправляем сообщение на почту, о том что пароль изменён
                SendMail.SendMessage($"Your account password has been changed.\nNew password: {this.Password}", this.Login);
            }
        }

        /// <summary>
        /// Функция генерации пароля
        /// </summary>
        /// <returns></returns>
        public string GeneratePass()
        {
            // Создаём коллекцию, состоящую из символов
            List<char> NewPassword = new List<char>();
            // Инициализируем рандом, которая будет случайно выбирать символы
            Random rnd = new Random();
            // Символы нумерации
            char[] ArrNumbers = { '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            // Символы знаков
            char[] ArrSymbols = { '|', '-', '_', '!', '@', '#', '$', '%', '&', '*', '=', '+' };
            // Символы английской раскладки
            char[] ArrUppercase = { 'q', 'w', 'e', 'r', 't', 's', 'y', 'u', 'i', 'o', 'p', 'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'z', 'x', 'c', 'v', 'b', 'n', 'm' };

            // Выбираем 1 случайную цифру
            for (int i = 0; i < 1; i++)
                // Добавляем цифру в коллекцию
                NewPassword.Add(ArrNumbers[rnd.Next(0, ArrNumbers.Length)]);
            // Выбираем 1 случайный символ
            for (int i = 0; i < 1; i++)
                // Добавляем символ в коллекцию
                NewPassword.Add(ArrSymbols[rnd.Next(0, ArrSymbols.Length)]);
            // Выбираем 2 случайные буквы английской раскладки верхнего регистра
            for (int i = 0; i < 2; i++)
                // Добавляем букву английской раскладки в коллекцию
                NewPassword.Add(char.ToUpper(ArrUppercase[rnd.Next(0, ArrUppercase.Length)]));
            // Выбираем 6 случайные буквы английской раскладки нижнего регистра
            for (int i = 0; i < 6; i++)
                // Добавляем букву английской раскладки в коллекцию
                NewPassword.Add(ArrUppercase[rnd.Next(0, ArrUppercase.Length)]);

            // Перебираем коллекцию
            // Тем самым, перемешиваем коллекцию символов
            for (int i = 0; i < NewPassword.Count; i++)
            {
                // Выбираем случайный символ
                int RandomSymbol = rnd.Next(0, NewPassword.Count);
                // Запоминаем случайный символ
                char Symbol = NewPassword[RandomSymbol];
                // Меняем случайный символ на порядковый символ в коллекции
                NewPassword[RandomSymbol] = NewPassword[i];
                // Меняем порядковый символ в коллекции на случайны
                NewPassword[i] = Symbol;
            }
            // Объявляем переменную, которая будет содержать пароль
            string NPassword = "";
            // Перебираем коллекцию
            for (int i = 0; i < NewPassword.Count; i++)
                // Добавляем в переменную с паролем символ из коллекции
                NPassword += NewPassword[i];
            // Возвращаем пароль
            return NPassword;
        }
    }
}