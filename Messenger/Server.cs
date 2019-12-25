using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger
{
    delegate void SendPackage();
    class Server
    {
        public void Send(params string[] data)
        {
            string message = default;
            foreach (string s in data)
            {
                message += $"{s}:&#:";
            }
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            MainWindow.Stream.Write(buffer, 0, buffer.Length);
        }

        public byte[] GetServerAnswer()
        {
            byte[] IncomingMessage = new byte[256];
            do
            {
                int bytes = MainWindow.Stream.Read(IncomingMessage, 0, IncomingMessage.Length); //ждём сообщения
            }
            while (MainWindow.Stream.DataAvailable); // пока данные есть в потоке
            return IncomingMessage;
        }

        public string[] DecodeServerAnswer(byte[] IncomingMessage)
        {
            string msgWrite = Encoding.UTF8.GetString(IncomingMessage).TrimEnd('\0');
            string[] words = msgWrite.Split(new char[] { ':', '&', '#', ':' }, StringSplitOptions.RemoveEmptyEntries); //разделяем пришедшую команду
            return words;
        }

        public string[] GetConfirmLine()
        {
            byte[] IncomingMessage = GetServerAnswer();
            return DecodeServerAnswer(IncomingMessage);
        }

        public string[] GetConfirmLine(SendPackage sendPackage)
        {
            sendPackage();
            byte[] IncomingMessage = GetServerAnswer();
            return DecodeServerAnswer(IncomingMessage);
        }
    }
}
