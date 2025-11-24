using System;
using System.IO;
using System.Threading.Tasks;
using ImageMagick;

namespace MyukView.Services
{
    /// <summary>
    /// 이미지 처리 서비스 - 크기 조정, 자르기, 회전, 필터 등
    /// </summary>
    public class ImageProcessingService
    {
        /// <summary>
        /// 이미지 크기 조정
        /// </summary>
        public async Task<string> ResizeImageAsync(string inputPath, int width, int height, bool maintainAspectRatio = true)
        {
            return await Task.Run(() =>
            {
                using var image = new MagickImage(inputPath);

                if (maintainAspectRatio)
                {
                    var geometry = new MagickGeometry(width, height)
                    {
                        IgnoreAspectRatio = false
                    };
                    image.Resize(geometry);
                }
                else
                {
                    image.Resize(width, height);
                }

                string outputPath = GenerateOutputPath(inputPath, "_resized");
                image.Write(outputPath);
                return outputPath;
            });
        }

        /// <summary>
        /// 이미지 자르기
        /// </summary>
        public async Task<string> CropImageAsync(string inputPath, int x, int y, int width, int height)
        {
            return await Task.Run(() =>
            {
                using var image = new MagickImage(inputPath);

                var geometry = new MagickGeometry(x, y, width, height);
                image.Crop(geometry);
                image.RePage();

                string outputPath = GenerateOutputPath(inputPath, "_cropped");
                image.Write(outputPath);
                return outputPath;
            });
        }

        /// <summary>
        /// 이미지 회전
        /// </summary>
        public async Task<string> RotateImageAsync(string inputPath, double degrees)
        {
            return await Task.Run(() =>
            {
                using var image = new MagickImage(inputPath);
                image.Rotate(degrees);

                string outputPath = GenerateOutputPath(inputPath, $"_rotated_{degrees}");
                image.Write(outputPath);
                return outputPath;
            });
        }

        /// <summary>
        /// 이미지 좌우 반전
        /// </summary>
        public async Task<string> FlipHorizontalAsync(string inputPath)
        {
            return await Task.Run(() =>
            {
                using var image = new MagickImage(inputPath);
                image.Flop();

                string outputPath = GenerateOutputPath(inputPath, "_flipped_h");
                image.Write(outputPath);
                return outputPath;
            });
        }

        /// <summary>
        /// 이미지 상하 반전
        /// </summary>
        public async Task<string> FlipVerticalAsync(string inputPath)
        {
            return await Task.Run(() =>
            {
                using var image = new MagickImage(inputPath);
                image.Flip();

                string outputPath = GenerateOutputPath(inputPath, "_flipped_v");
                image.Write(outputPath);
                return outputPath;
            });
        }

        /// <summary>
        /// 밝기 조정
        /// </summary>
        public async Task<string> AdjustBrightnessAsync(string inputPath, int brightness)
        {
            return await Task.Run(() =>
            {
                using var image = new MagickImage(inputPath);
                image.BrightnessContrast(new Percentage(brightness), new Percentage(0));

                string outputPath = GenerateOutputPath(inputPath, $"_brightness_{brightness}");
                image.Write(outputPath);
                return outputPath;
            });
        }

        /// <summary>
        /// 대비 조정
        /// </summary>
        public async Task<string> AdjustContrastAsync(string inputPath, int contrast)
        {
            return await Task.Run(() =>
            {
                using var image = new MagickImage(inputPath);
                image.BrightnessContrast(new Percentage(0), new Percentage(contrast));

                string outputPath = GenerateOutputPath(inputPath, $"_contrast_{contrast}");
                image.Write(outputPath);
                return outputPath;
            });
        }

        /// <summary>
        /// 채도 조정
        /// </summary>
        public async Task<string> AdjustSaturationAsync(string inputPath, int saturation)
        {
            return await Task.Run(() =>
            {
                using var image = new MagickImage(inputPath);
                image.Modulate(new Percentage(100), new Percentage(100 + saturation), new Percentage(100));

                string outputPath = GenerateOutputPath(inputPath, $"_saturation_{saturation}");
                image.Write(outputPath);
                return outputPath;
            });
        }

        /// <summary>
        /// 흑백 변환
        /// </summary>
        public async Task<string> ConvertToGrayscaleAsync(string inputPath)
        {
            return await Task.Run(() =>
            {
                using var image = new MagickImage(inputPath);
                image.Grayscale();

                string outputPath = GenerateOutputPath(inputPath, "_grayscale");
                image.Write(outputPath);
                return outputPath;
            });
        }

        /// <summary>
        /// 세피아 효과
        /// </summary>
        public async Task<string> ApplySepiaAsync(string inputPath)
        {
            return await Task.Run(() =>
            {
                using var image = new MagickImage(inputPath);
                image.SepiaTone();

                string outputPath = GenerateOutputPath(inputPath, "_sepia");
                image.Write(outputPath);
                return outputPath;
            });
        }

        /// <summary>
        /// 블러 효과
        /// </summary>
        public async Task<string> ApplyBlurAsync(string inputPath, double radius = 5.0)
        {
            return await Task.Run(() =>
            {
                using var image = new MagickImage(inputPath);
                image.Blur(radius, 1.0);

                string outputPath = GenerateOutputPath(inputPath, "_blur");
                image.Write(outputPath);
                return outputPath;
            });
        }

        /// <summary>
        /// 선명화 효과
        /// </summary>
        public async Task<string> ApplySharpenAsync(string inputPath, double radius = 2.0)
        {
            return await Task.Run(() =>
            {
                using var image = new MagickImage(inputPath);
                image.Sharpen(radius, 1.0);

                string outputPath = GenerateOutputPath(inputPath, "_sharpen");
                image.Write(outputPath);
                return outputPath;
            });
        }

        /// <summary>
        /// 엠보스 효과
        /// </summary>
        public async Task<string> ApplyEmbossAsync(string inputPath)
        {
            return await Task.Run(() =>
            {
                using var image = new MagickImage(inputPath);
                image.Emboss(2.0, 1.0);

                string outputPath = GenerateOutputPath(inputPath, "_emboss");
                image.Write(outputPath);
                return outputPath;
            });
        }

        /// <summary>
        /// 엣지 감지
        /// </summary>
        public async Task<string> ApplyEdgeDetectionAsync(string inputPath)
        {
            return await Task.Run(() =>
            {
                using var image = new MagickImage(inputPath);
                image.Edge(2.0);

                string outputPath = GenerateOutputPath(inputPath, "_edge");
                image.Write(outputPath);
                return outputPath;
            });
        }

        /// <summary>
        /// 네거티브 효과
        /// </summary>
        public async Task<string> ApplyNegativeAsync(string inputPath)
        {
            return await Task.Run(() =>
            {
                using var image = new MagickImage(inputPath);
                image.Negate();

                string outputPath = GenerateOutputPath(inputPath, "_negative");
                image.Write(outputPath);
                return outputPath;
            });
        }

        /// <summary>
        /// 워터마크 추가
        /// </summary>
        public async Task<string> AddWatermarkAsync(string inputPath, string watermarkText,
            int fontSize = 40, string color = "white", int opacity = 128)
        {
            return await Task.Run(() =>
            {
                using var image = new MagickImage(inputPath);

                var readSettings = new MagickReadSettings
                {
                    BackgroundColor = MagickColors.Transparent,
                    FillColor = new MagickColor(color),
                    FontPointsize = fontSize,
                    Width = image.Width,
                    Height = image.Height
                };

                using var watermark = new MagickImage($"caption:{watermarkText}", readSettings);
                watermark.Evaluate(Channels.Alpha, EvaluateOperator.Divide, 255.0 / opacity);

                image.Composite(watermark, Gravity.Southeast, CompositeOperator.Over);

                string outputPath = GenerateOutputPath(inputPath, "_watermarked");
                image.Write(outputPath);
                return outputPath;
            });
        }

        /// <summary>
        /// 배치 처리
        /// </summary>
        public async Task<string[]> BatchProcessAsync(string[] inputPaths,
            Func<string, Task<string>> processFunc)
        {
            var outputPaths = new string[inputPaths.Length];

            for (int i = 0; i < inputPaths.Length; i++)
            {
                outputPaths[i] = await processFunc(inputPaths[i]);
            }

            return outputPaths;
        }

        /// <summary>
        /// 출력 파일 경로 생성
        /// </summary>
        private string GenerateOutputPath(string inputPath, string suffix)
        {
            string directory = Path.GetDirectoryName(inputPath) ?? "";
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(inputPath);
            string extension = Path.GetExtension(inputPath);

            return Path.Combine(directory, $"{fileNameWithoutExt}{suffix}{extension}");
        }
    }
}
