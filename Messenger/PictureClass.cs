using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Messenger
{
    /// <summary>
    /// устанавливаем картинку "введите сообщение" в левый верхний угол
    /// </summary>
    class PictureClass
    {
        public void SetPicture(TextBox userTxt)
        {
            if (userTxt.Text == string.Empty)
            {
                ImageBrush textImageBrush = new ImageBrush();
                textImageBrush.ImageSource =
                    new BitmapImage(
                        new Uri("pack://application:,,,/Resources/Writt.png", UriKind.Absolute)
                    );
                SetImage(textImageBrush,userTxt);
            }
        }

        private void SetImage(ImageBrush textImageBrush, TextBox userTxt)
        {
            textImageBrush.AlignmentX = AlignmentX.Left;
            textImageBrush.AlignmentY = AlignmentY.Top;
            textImageBrush.Stretch = Stretch.None;
            userTxt.Background = textImageBrush;
        }
    }
}
