using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;


namespace LoadTarot
{
    internal class ImageService
    {
        public byte[] InvertImage(byte[] imageBytes)
        {
            using (var image = Image.Load(imageBytes))
            {
                // Invert the image
                image.Mutate(x => x.Invert());

                // Convert back to byte array
                using (var ms = new MemoryStream())
                {
                    image.SaveAsJpeg(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}
