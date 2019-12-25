using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

namespace Messenger
{
    class SoundClass
    {
        public void SendSoundToServer()
        {
            byte[] package = GetVoiceFile();

            int bufferSize = 1024;
            int bytesSent = 0; //отталкиваемся с какого байта отправлять
            int bytesLeft = package.Length; //смотрим сколько осталось

            while (bytesLeft > 0)
            {

                int packetSize = (bytesLeft > bufferSize) ? bufferSize : bytesLeft; //если больше отправляем 1024, если меньше то остаток

                MainWindow.StreamVoice.Write(package, bytesSent, packetSize);
                bytesSent += packetSize;
                bytesLeft -= packetSize;
            }
        }

        public void GetVoiceMsg()
        {
            try
            {
                byte[] length = new byte[4];
                int bytesRead = MainWindow.StreamVoice.Read(length, 0, 4);
                int fileLength = BitConverter.ToInt32(length, 0);

                int bufferSize = 1024;
                int allBytesRead = 0;
                int bytesLeft = fileLength;
                byte[] data = new byte[fileLength];

                while (bytesLeft > 0)
                {
                    int PacketSize = (bytesLeft > bufferSize) ? bufferSize : bytesLeft;

                    bytesRead = MainWindow.StreamVoice.Read(data, allBytesRead, PacketSize);
                    allBytesRead += bytesRead;
                    bytesLeft -= bytesRead;
                }
                File.WriteAllBytes($@"C:\Users\{Environment.UserName}\Messenger\ClientSoundMes.wav", data);
            }
            catch
            {
                MessageBox.Show("Не удалось принять голосовое сообщение", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static byte[] GetVoiceFile()
        {
            byte[] file = File.ReadAllBytes($@"C:\Users\{Environment.UserName}\Messenger\SoundMessage.wav");
            byte[] fileLength = BitConverter.GetBytes(file.Length); //4 байта
            byte[] package = new byte[4 + file.Length];
            fileLength.CopyTo(package, 0);
            file.CopyTo(package, 4); //начиная с 4 байта пишем файл
            return package;
        }
    }
}
