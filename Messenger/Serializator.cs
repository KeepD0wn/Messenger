using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Messenger
{
    class Serializator
    {
        Server server = new Server();

        public List<string[]> GetAllMessages()
        {
            byte[] dataByte = GetSerializedMessages();
            return DeserializeMessages(dataByte);
        }

        private byte[] GetSerializedMessages()
        {
            server.Send("3");

            int bufferSize = 1024;
            int bytesRead = 0;
            int allBytesRead = 0;

            byte[] length = new byte[4];
            bytesRead = MainWindow.Stream.Read(length, 0, 4); //записываем размер файла в первые 4 байта
            int fileLength = BitConverter.ToInt32(length, 0);

            int bytesLeft = fileLength;
            byte[] dataByte = new byte[fileLength];

            while (bytesLeft > 0)
            {
                int PacketSize = (bytesLeft > bufferSize) ? bufferSize : bytesLeft;

                bytesRead = MainWindow.Stream.Read(dataByte, allBytesRead, PacketSize);
                allBytesRead += bytesRead;
                bytesLeft -= bytesRead;
            }
            return dataByte;
        }

        private List<string[]> DeserializeMessages(byte[] data)
        {
            string path = $@"C:\Users\{Environment.UserName}\Messenger\txtMes.txt";
            File.WriteAllBytes(path, data);
            using (FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                XmlSerializer ser = new XmlSerializer(typeof(List<string[]>));
                return (List<string[]>)ser.Deserialize(file);
            }
        }
    }
}
