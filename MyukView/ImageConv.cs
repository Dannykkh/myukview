using ImageMagick;
using System;
using System.IO;

namespace MyukView
{
    internal class ImageConv
    {
        public void ConvertAvifToPng(string avifFilePath)
        {
            try
            {
                // AVIF 파일 로드 및 PNG로 변환
                using (MagickImage image = new MagickImage(avifFilePath))
                {
                    string outputPngPath = System.IO.Path.ChangeExtension(avifFilePath, ".png");
                    image.Write(outputPngPath);
                    Console.WriteLine($"변환 완료: {outputPngPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"변환 중 오류 발생: {ex.Message}");
            }
        }
    }
}
