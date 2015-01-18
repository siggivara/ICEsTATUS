using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace ICEsTATUS
{
    class IconHelper
    {
        public static Icon GetIconFromString(string iconText, int width, int height)
        {
            var iconBitmap = CreateBitmapImage(iconText, width, height);
            var bitMapIcon = iconBitmap.GetHicon();
            return Icon.FromHandle(bitMapIcon);
        }

        private static Bitmap CreateBitmapImage(string imageText, int width, int height)
        {
            Bitmap bmpImage = new Bitmap(1, 1);

            // Create the Font object for the image text drawing.
            Font objFont = new Font("Arial", 30, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);

            // Create a graphics object to measure the text's width and height.
            //Graphics graphics = Graphics.FromImage(bmpImage);

            // This is where the bitmap size is determined.
            //intWidth = (int)objGraphics.MeasureString(imageText, objFont).Width;
            //intHeight = (int)objGraphics.MeasureString(imageText, objFont).Height;

            // Create the bmpImage again with the correct size for the text and font.
            bmpImage = new Bitmap(bmpImage, new Size(width, height));

            // Add the colors to the new bitmap.
            var graphics = Graphics.FromImage(bmpImage);

            // Set Background color
            graphics.Clear(Color.Transparent);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            graphics.DrawString(imageText, objFont, new SolidBrush(Color.Red), 0, height / 4);
            graphics.Flush();

            return bmpImage;
        }
    }
}
