using System;

namespace MyukView.Models
{
    /// <summary>
    /// 이미지 메타데이터 정보를 담는 클래스
    /// </summary>
    public class ImageMetadata
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public double DpiX { get; set; }
        public double DpiY { get; set; }
        public string Format { get; set; } = string.Empty;
        public int BitsPerPixel { get; set; }

        // EXIF 정보
        public DateTime? DateTaken { get; set; }
        public string? CameraModel { get; set; }
        public string? CameraMake { get; set; }
        public double? FocalLength { get; set; }
        public double? Aperture { get; set; }
        public string? ExposureTime { get; set; }
        public int? ISO { get; set; }

        /// <summary>
        /// 이미지 해상도를 문자열로 반환
        /// </summary>
        public string GetResolutionString()
        {
            return $"{Width} × {Height}";
        }

        /// <summary>
        /// 메가픽셀 계산
        /// </summary>
        public double GetMegapixels()
        {
            return Math.Round((Width * Height) / 1000000.0, 2);
        }

        /// <summary>
        /// 가로세로 비율 계산
        /// </summary>
        public string GetAspectRatio()
        {
            if (Width == 0 || Height == 0) return "N/A";

            int gcd = GCD(Width, Height);
            int ratioW = Width / gcd;
            int ratioH = Height / gcd;

            return $"{ratioW}:{ratioH}";
        }

        private int GCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }
    }
}
