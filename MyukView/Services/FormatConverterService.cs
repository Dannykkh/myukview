using System;
using System.IO;
using System.Threading.Tasks;
using ImageMagick;

namespace MyukView.Services
{
    /// <summary>
    /// 이미지 포맷 변환 서비스
    /// </summary>
    public class FormatConverterService
    {
        /// <summary>
        /// 이미지를 PNG 포맷으로 변환
        /// </summary>
        /// <param name="inputFilePath">입력 이미지 파일 경로</param>
        /// <returns>변환된 PNG 파일 경로</returns>
        public string ConvertToPng(string inputFilePath)
        {
            if (string.IsNullOrEmpty(inputFilePath) || !File.Exists(inputFilePath))
                throw new FileNotFoundException("입력 파일을 찾을 수 없습니다.", inputFilePath);

            try
            {
                using (var image = new MagickImage(inputFilePath))
                {
                    string outputPath = Path.ChangeExtension(inputFilePath, ".png");
                    image.Write(outputPath);
                    Console.WriteLine($"변환 완료: {outputPath}");
                    return outputPath;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"이미지 변환 중 오류 발생: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 이미지를 지정한 포맷으로 변환
        /// </summary>
        /// <param name="inputFilePath">입력 이미지 파일 경로</param>
        /// <param name="targetFormat">대상 포맷 (예: "jpg", "png", "webp")</param>
        /// <param name="quality">품질 (1-100, 기본값: 90)</param>
        /// <returns>변환된 파일 경로</returns>
        public string ConvertToFormat(string inputFilePath, string targetFormat, int quality = 90)
        {
            if (string.IsNullOrEmpty(inputFilePath) || !File.Exists(inputFilePath))
                throw new FileNotFoundException("입력 파일을 찾을 수 없습니다.", inputFilePath);

            try
            {
                using (var image = new MagickImage(inputFilePath))
                {
                    // 품질 설정
                    image.Quality = Math.Max(1, Math.Min(100, quality));

                    string outputPath = Path.ChangeExtension(inputFilePath, $".{targetFormat.TrimStart('.')}");
                    image.Write(outputPath);
                    Console.WriteLine($"변환 완료: {outputPath}");
                    return outputPath;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"이미지 변환 중 오류 발생: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 이미지를 비동기로 PNG 포맷으로 변환
        /// </summary>
        public async Task<string> ConvertToPngAsync(string inputFilePath)
        {
            return await Task.Run(() => ConvertToPng(inputFilePath));
        }

        /// <summary>
        /// 이미지를 비동기로 지정한 포맷으로 변환
        /// </summary>
        public async Task<string> ConvertToFormatAsync(string inputFilePath, string targetFormat, int quality = 90)
        {
            return await Task.Run(() => ConvertToFormat(inputFilePath, targetFormat, quality));
        }
    }
}
