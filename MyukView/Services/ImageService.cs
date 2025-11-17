using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MyukView.Models;

namespace MyukView.Services
{
    /// <summary>
    /// 이미지 로드, 메타데이터 추출, 캐싱을 담당하는 서비스
    /// </summary>
    public class ImageService
    {
        private readonly Dictionary<string, BitmapImage> _imageCache;
        private const int MAX_CACHE_SIZE = 20; // 최대 캐시 개수

        public ImageService()
        {
            _imageCache = new Dictionary<string, BitmapImage>();
        }

        /// <summary>
        /// 이미지 파일을 비동기로 로드
        /// </summary>
        public async Task<BitmapImage?> LoadImageAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            // 캐시에서 확인
            if (_imageCache.ContainsKey(filePath))
                return _imageCache[filePath];

            try
            {
                return await Task.Run(() => LoadImage(filePath));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"이미지 로드 실패: {filePath}, 오류: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 이미지 파일을 동기로 로드
        /// </summary>
        public BitmapImage? LoadImage(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            // 캐시에서 확인
            if (_imageCache.ContainsKey(filePath))
                return _imageCache[filePath];

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze(); // 스레드 안전성을 위해 Freeze

                // 캐시에 추가
                AddToCache(filePath, bitmap);

                return bitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"이미지 로드 실패: {filePath}, 오류: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 썸네일 이미지 로드 (성능 최적화)
        /// </summary>
        public BitmapImage? LoadThumbnail(string filePath, int width = 150, int height = 150)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.DecodePixelWidth = width;
                bitmap.DecodePixelHeight = height;
                bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();

                return bitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"썸네일 로드 실패: {filePath}, 오류: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 이미지 메타데이터 추출
        /// </summary>
        public ImageMetadata? GetMetadata(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                using var stream = File.OpenRead(filePath);
                var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);

                if (decoder.Frames.Count == 0)
                    return null;

                var frame = decoder.Frames[0];
                var metadata = new ImageMetadata
                {
                    Width = frame.PixelWidth,
                    Height = frame.PixelHeight,
                    DpiX = frame.DpiX,
                    DpiY = frame.DpiY,
                    Format = decoder.CodecInfo?.FriendlyName ?? "Unknown",
                    BitsPerPixel = frame.Format.BitsPerPixel
                };

                // EXIF 데이터 추출 시도
                try
                {
                    if (frame.Metadata is BitmapMetadata bitmapMetadata)
                    {
                        metadata.DateTaken = GetExifDate(bitmapMetadata, "System.Photo.DateTaken");
                        metadata.CameraModel = GetExifString(bitmapMetadata, "System.Photo.CameraModel");
                        metadata.CameraMake = GetExifString(bitmapMetadata, "System.Photo.CameraManufacturer");
                        metadata.ISO = GetExifInt(bitmapMetadata, "System.Photo.ISOSpeed");
                    }
                }
                catch
                {
                    // EXIF 데이터가 없거나 읽을 수 없는 경우 무시
                }

                return metadata;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"메타데이터 추출 실패: {filePath}, 오류: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 폴더의 모든 이미지 파일 가져오기
        /// </summary>
        public List<ImageFile> GetImagesFromFolder(string folderPath, bool includeSubfolders = false)
        {
            var imageFiles = new List<ImageFile>();

            if (!Directory.Exists(folderPath))
                return imageFiles;

            try
            {
                var searchOption = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                var files = Directory.GetFiles(folderPath, "*.*", searchOption);

                foreach (var file in files)
                {
                    if (ImageFile.IsSupportedFormat(file))
                    {
                        var imageFile = new ImageFile(file);
                        imageFiles.Add(imageFile);
                    }
                }

                // 파일명으로 정렬
                imageFiles = imageFiles.OrderBy(f => f.FileName).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"폴더 스캔 실패: {folderPath}, 오류: {ex.Message}");
            }

            return imageFiles;
        }

        /// <summary>
        /// 캐시에 이미지 추가 (LRU 방식)
        /// </summary>
        private void AddToCache(string filePath, BitmapImage bitmap)
        {
            if (_imageCache.Count >= MAX_CACHE_SIZE)
            {
                // 가장 오래된 항목 제거
                var firstKey = _imageCache.Keys.First();
                _imageCache.Remove(firstKey);
            }

            _imageCache[filePath] = bitmap;
        }

        /// <summary>
        /// 캐시 초기화
        /// </summary>
        public void ClearCache()
        {
            _imageCache.Clear();
        }

        // EXIF 헬퍼 메서드들
        private DateTime? GetExifDate(BitmapMetadata metadata, string query)
        {
            try
            {
                var value = metadata.GetQuery(query);
                if (value is DateTime dateTime)
                    return dateTime;
                if (value is string dateString && DateTime.TryParse(dateString, out var parsedDate))
                    return parsedDate;
            }
            catch { }
            return null;
        }

        private string? GetExifString(BitmapMetadata metadata, string query)
        {
            try
            {
                var value = metadata.GetQuery(query);
                return value?.ToString();
            }
            catch { }
            return null;
        }

        private int? GetExifInt(BitmapMetadata metadata, string query)
        {
            try
            {
                var value = metadata.GetQuery(query);
                if (value is int intValue)
                    return intValue;
                if (value is ushort ushortValue)
                    return ushortValue;
                if (value != null && int.TryParse(value.ToString(), out var parsedValue))
                    return parsedValue;
            }
            catch { }
            return null;
        }
    }
}
