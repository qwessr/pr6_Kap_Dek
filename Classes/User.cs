
using System.Windows;
using MySql.Data.MySqlClient;

namespace RegIN.Classes
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = ""; 
        public string Password { get; set; } = "";
        public string Name { get; set; } = "";
        public byte[] Image = new byte[0];
        public string Pincode { get; set; } = "";
        public DateTime DateUpdate { get; set; }
        public DateTime DateCreate { get; set; }
        public CorrectLogin HandlerCorrectLogin;
        public InCorrectLogin HandlerInCorrectLogin;
        public delegate void CorrectLogin();
        public delegate void InCorrectLogin();

        public bool HasPin => !string.IsNullOrEmpty(Pincode);

        public void GetUserLogin(string Login)
        {
            this.Id = -1;
            this.Login = String.Empty;
            this.Password = String.Empty;
            this.Name = String.Empty;
            this.Image = new byte[0];
            this.Pincode = String.Empty; 
            MySqlConnection mySqlConnection = WorkingDB.OpenConnection();

            if (WorkingDB.OpenConnection(mySqlConnection))
            {
                MySqlDataReader userQuery = WorkingDB.Query($"SELECT * FROM `users` WHERE `Login` = '{Login}'", mySqlConnection);

                if (userQuery.HasRows)
                {
                    userQuery.Read();
                    this.Id = userQuery.GetInt32(0);
                    this.Login = userQuery.GetString(1);
                    this.Password = userQuery.GetString(2);
                    this.Name = userQuery.GetString(3);

                    if (!userQuery.IsDBNull(4))
                    {
                        this.Image = new byte[64 * 1024];
                        userQuery.GetBytes(4, 0, Image, 0, Image.Length);
                    }

                    try
                    {
                        if (!userQuery.IsDBNull(7))
                        {
                            string pinValue = userQuery.GetString(7);
                            this.Pincode = pinValue ?? ""; 
                        }
                    }
                    catch
                    {
                        this.Pincode = ""; 
                    }

                    this.DateUpdate = userQuery.GetDateTime(5);
                    this.DateCreate = userQuery.GetDateTime(6); 

                    HandlerCorrectLogin?.Invoke();
                }
                else
                {
                    HandlerInCorrectLogin?.Invoke();
                }
            }
            else
            {
                HandlerInCorrectLogin?.Invoke();
            }

            WorkingDB.CloseConnection(mySqlConnection);
        }

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
        public void SetPin(string pin)
        {
            // ПРОВЕРКА НА NULL
            if (string.IsNullOrEmpty(pin))
            {
                MessageBox.Show("PIN не может быть пустым");
                return;
            }

            MySqlConnection mySqlConnection = WorkingDB.OpenConnection();

            if (WorkingDB.OpenConnection(mySqlConnection))
            {
                WorkingDB.Query($"UPDATE `users` SET `Pincode` = '{pin}' WHERE `Login` = '{this.Login}'", mySqlConnection);
            }
            WorkingDB.CloseConnection(mySqlConnection);

            // ОБНОВЛЯЕМ СВОЙСТВО В ОБЪЕКТЕ
            this.Pincode = pin;

            SendMail.SendMessage($"PIN code has been set for your account: {pin}", this.Login);
        }

        /// <summary>
        /// Функция создания нового пароля
        /// </summary>
        public void CrateNewPassword()
        {
            if (Login != String.Empty)
            {
                Password = GeneratePass();
                MySqlConnection mySqlConnection = WorkingDB.OpenConnection();
                if (WorkingDB.OpenConnection(mySqlConnection))
                {
                    WorkingDB.Query($"UPDATE `users` SET `Password`='{this.Password}' WHERE `Login` = '{this.Login}'", mySqlConnection);
                }
                WorkingDB.CloseConnection(mySqlConnection);
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