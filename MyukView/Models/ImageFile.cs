using System;
using System.IO;

namespace MyukView.Models
{
    /// <summary>
    /// 이미지 파일 정보를 담는 모델 클래스
    /// </summary>
    public class ImageFile
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName => Path.GetFileName(FilePath);
        public string FileExtension => Path.GetExtension(FilePath);
        public long FileSize { get; set; }
        public DateTime LastModified { get; set; }
        public ImageMetadata? Metadata { get; set; }

        public ImageFile()
        {
        }

        public ImageFile(string filePath)
        {
            FilePath = filePath;
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                FileSize = fileInfo.Length;
                LastModified = fileInfo.LastWriteTime;
            }
        }

        /// <summary>
        /// 파일 크기를 사람이 읽기 쉬운 형태로 반환
        /// </summary>
        public string GetReadableFileSize()
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = FileSize;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// 지원하는 이미지 포맷인지 확인
        /// </summary>
        public static bool IsSupportedFormat(string filePath)
        {
            string[] supportedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp",
                                            ".tiff", ".tif", ".webp", ".avif", ".ico" };
            string ext = Path.GetExtension(filePath).ToLower();
            return Array.Exists(supportedExtensions, e => e == ext);
        }
    }
}
