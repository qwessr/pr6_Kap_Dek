using System.Net;
using System.Net.Mail;

namespace RegIN.Classes
{
    public class SendMail
    {
        /// <summary>
        /// Функция отправки сообщения
        /// </summary>
        /// <param name="Message">Сообщение которое необходимо отправить</param>
        /// <param name="To">Почта на которую отправляется сообщение</param>
        public static void SendMessage(string Message, string To)
        {
            // Создаём SMTP клиент, в качестве хоста указываем яндекс
            var smtpClient = new SmtpClient("smtp.yandex.ru")
            {
                // Указываем порт по которому передаём сообщение
                Port = 587,
                // Указываем почту, с которой будет отправляться сообщение, и пароль от этой почты
                Credentials = new NetworkCredential("yandex@yandex.ru", "password"),
                // Включаем поддержку SSL
                EnableSsl = true,
            };
                // Вызываем метод Send, который отправляет письмо на указанный адрес
        smtpClient.Send("landaxer@yandex.ru", To, "Проект RegIN", Message);
        }
    }
}