using System;
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
            var smptClient = new SmtpClient("smtp.yandex.ru")
            {
                Port = 587,
                Credentials = new NetworkCredential("V0vanch4@yandex.ru", "yaldnjqwwwfwrpwp"),
                EnableSsl = true,
            };
            smptClient.Send("V0vanch4@yandex.ru", To, "Проект RegIn", Message);
        }
    }
} 