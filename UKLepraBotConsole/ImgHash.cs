using System;
using System.Drawing;
using System.IO;
using System.Text;

/// <summary>
/// Taken from https://github.com/ukushu/ImgComparator
/// </summary>
/// 
namespace UKLepraBotConsole
{
    public class ImgHash
    {
        private readonly int _hashSide = 16;

        public string HashData { get; private set;}

        public Image Img
        {
            get
            {
                return Image.FromFile(FilePath);
            }
        }

        public string FilePath { get; private set; }

        public string FileName
        {
            get
            {
                return Path.GetFileName(FilePath);
            }
        }

        public string FileLocation
        {
            get { return Path.GetDirectoryName(FilePath); }
        }

        public string ImgSize { get; private set; }


        /// <summary>
        /// Method to compare 2 image hashes
        /// </summary>
        /// <returns>% of similarity</returns>
        public double CompareWith(ImgHash compareWith)
        {
            return CompareWith(compareWith.HashData);
        }
        
        /// <summary>
        /// Method to compare 2 image hashes
        /// </summary>
        /// <returns>% of similarity</returns>
        public double CompareWith(string hashData)
        {
            if (HashData.Length != hashData.Length)
            {
                throw new Exception("Cannot compare hashes with different sizes");
            }

            int differenceCounter = 0;

            for (int i = 0; i < HashData.Length; i++)
            {
                if (HashData[i] != hashData[i])
                {
                    differenceCounter++;
                }
            }

            return 100 - differenceCounter / 100.0 * HashData.Length / 2.0;
        }

        public void GenerateFromPath(string path)
        {
            FilePath = path;

            var image = (Bitmap)Image.FromFile(path, true);

            ImgSize = $"{image.Size.Width}x{image.Size.Height}";

            GenerateFromImage(image);

            image.Dispose();
        }

        private void GenerateFromImage(Bitmap img)
        {
            var result = new StringBuilder();

            //resize img to 16x16px (by default) or with configured size 
            var bmpMin = new Bitmap(img, new Size(_hashSide, _hashSide));

            for (int j = 0; j < bmpMin.Height; j++)
            {
                for (int i = 0; i < bmpMin.Width; i++)
                {
                    //reduce colors to true and false
                    result.Append(bmpMin.GetPixel(i, j).GetBrightness() < 0.5f ? '0' : '1');
                }
            }

            HashData = result.ToString();

            bmpMin.Dispose();
        }
    }
}
