using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyukView.Services
{
    /// <summary>
    /// Motion Photo (갤럭시) 및 Live Photo (아이폰) 처리 서비스
    /// </summary>
    public class MotionPhotoService
    {
        // Motion Photo MP4 시작 시그니처
        private static readonly byte[] MP4_SIGNATURE = { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70 };
        private static readonly byte[] MP4_SIGNATURE_ALT = { 0x00, 0x00, 0x00, 0x1C, 0x66, 0x74, 0x79, 0x70 };
        private static readonly byte[] MP4_SIGNATURE_SHORT = { 0x66, 0x74, 0x79, 0x70 }; // "ftyp"

        /// <summary>
        /// JPG 파일이 Motion Photo인지 확인
        /// </summary>
        public bool IsMotionPhoto(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                var ext = Path.GetExtension(filePath).ToLower();
                if (ext != ".jpg" && ext != ".jpeg")
                    return false;

                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var fileSize = fs.Length;

                // 파일이 너무 작으면 Motion Photo가 아님
                if (fileSize < 1024 * 100) // 100KB 미만
                    return false;

                // 파일의 뒷부분에서 MP4 시그니처 찾기
                // JPG는 보통 앞쪽, MP4는 뒷쪽에 있음
                var searchStart = fileSize / 2; // 파일 중간부터 검색
                var bufferSize = (int)Math.Min(fileSize - searchStart, 1024 * 1024); // 최대 1MB

                fs.Seek(searchStart, SeekOrigin.Begin);
                var buffer = new byte[bufferSize];
                fs.Read(buffer, 0, bufferSize);

                // MP4 시그니처 찾기
                return FindMP4Signature(buffer) >= 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Motion Photo에서 동영상 추출
        /// </summary>
        public async Task<string?> ExtractVideoFromMotionPhoto(string filePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!IsMotionPhoto(filePath))
                        return null;

                    using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    var fileSize = fs.Length;

                    // 전체 파일을 메모리에 로드 (큰 파일은 주의)
                    byte[] fileData;
                    if (fileSize > 50 * 1024 * 1024) // 50MB 이상
                    {
                        // 큰 파일은 청크로 읽기
                        return ExtractVideoFromLargeFile(filePath);
                    }
                    else
                    {
                        fileData = new byte[fileSize];
                        fs.Read(fileData, 0, (int)fileSize);
                    }

                    // MP4 시작 위치 찾기
                    var mp4StartIndex = FindMP4SignatureInFile(fileData);
                    if (mp4StartIndex < 0)
                        return null;

                    // MP4 데이터 추출
                    var mp4DataLength = fileData.Length - mp4StartIndex;
                    var mp4Data = new byte[mp4DataLength];
                    Array.Copy(fileData, mp4StartIndex, mp4Data, 0, mp4DataLength);

                    // 출력 파일 경로
                    var outputPath = Path.ChangeExtension(filePath, ".mp4");
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                    var directory = Path.GetDirectoryName(filePath) ?? "";
                    outputPath = Path.Combine(directory, $"{fileNameWithoutExt}_motion.mp4");

                    // MP4 파일 저장
                    File.WriteAllBytes(outputPath, mp4Data);

                    return outputPath;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Motion Photo 추출 실패: {ex.Message}");
                    return null;
                }
            });
        }

        /// <summary>
        /// 큰 파일에서 동영상 추출
        /// </summary>
        private string? ExtractVideoFromLargeFile(string filePath)
        {
            try
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var fileSize = fs.Length;

                // 중간부터 끝까지 검색
                var searchStart = fileSize / 2;
                var chunkSize = 1024 * 1024; // 1MB 청크
                var buffer = new byte[chunkSize];

                long mp4StartPosition = -1;

                for (long pos = searchStart; pos < fileSize - 1024; pos += chunkSize / 2)
                {
                    fs.Seek(pos, SeekOrigin.Begin);
                    var bytesRead = fs.Read(buffer, 0, chunkSize);

                    var signatureIndex = FindMP4Signature(buffer);
                    if (signatureIndex >= 0)
                    {
                        mp4StartPosition = pos + signatureIndex;
                        break;
                    }
                }

                if (mp4StartPosition < 0)
                    return null;

                // MP4 데이터를 파일로 직접 쓰기
                var outputPath = Path.ChangeExtension(filePath, ".mp4");
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                var directory = Path.GetDirectoryName(filePath) ?? "";
                outputPath = Path.Combine(directory, $"{fileNameWithoutExt}_motion.mp4");

                using var outputFs = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
                fs.Seek(mp4StartPosition, SeekOrigin.Begin);

                var copyBuffer = new byte[1024 * 1024]; // 1MB 버퍼
                int bytesRead;
                while ((bytesRead = fs.Read(copyBuffer, 0, copyBuffer.Length)) > 0)
                {
                    outputFs.Write(copyBuffer, 0, bytesRead);
                }

                return outputPath;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 버퍼에서 MP4 시그니처 찾기
        /// </summary>
        private int FindMP4Signature(byte[] buffer)
        {
            // 여러 MP4 시그니처 패턴 확인
            for (int i = 0; i < buffer.Length - 8; i++)
            {
                // ftyp 시그니처 확인
                if (buffer.Length > i + 7 &&
                    buffer[i + 4] == 0x66 && // 'f'
                    buffer[i + 5] == 0x74 && // 't'
                    buffer[i + 6] == 0x79 && // 'y'
                    buffer[i + 7] == 0x70)   // 'p'
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 파일 데이터에서 MP4 시작 위치 찾기
        /// </summary>
        private int FindMP4SignatureInFile(byte[] fileData)
        {
            // 파일 중간부터 검색 (JPG는 보통 앞쪽)
            var searchStart = fileData.Length / 2;

            for (int i = searchStart; i < fileData.Length - 8; i++)
            {
                // ftyp 시그니처 확인
                if (fileData[i + 4] == 0x66 && // 'f'
                    fileData[i + 5] == 0x74 && // 't'
                    fileData[i + 6] == 0x79 && // 'y'
                    fileData[i + 7] == 0x70)   // 'p'
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Motion Photo 정보 가져오기
        /// </summary>
        public MotionPhotoInfo? GetMotionPhotoInfo(string filePath)
        {
            if (!IsMotionPhoto(filePath))
                return null;

            try
            {
                var fileInfo = new FileInfo(filePath);
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var fileSize = fs.Length;

                // MP4 시작 위치 찾기
                var searchStart = fileSize / 2;
                var bufferSize = (int)Math.Min(fileSize - searchStart, 1024 * 1024);

                fs.Seek(searchStart, SeekOrigin.Begin);
                var buffer = new byte[bufferSize];
                fs.Read(buffer, 0, bufferSize);

                var signatureIndex = FindMP4Signature(buffer);
                if (signatureIndex < 0)
                    return null;

                var mp4StartPosition = searchStart + signatureIndex;
                var imageSize = mp4StartPosition;
                var videoSize = fileSize - mp4StartPosition;

                return new MotionPhotoInfo
                {
                    FilePath = filePath,
                    TotalSize = fileSize,
                    ImageSize = imageSize,
                    VideoSize = videoSize,
                    VideoStartPosition = mp4StartPosition,
                    HasMotionVideo = true
                };
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Motion Photo 정보
    /// </summary>
    public class MotionPhotoInfo
    {
        public string FilePath { get; set; } = "";
        public long TotalSize { get; set; }
        public long ImageSize { get; set; }
        public long VideoSize { get; set; }
        public long VideoStartPosition { get; set; }
        public bool HasMotionVideo { get; set; }

        public string GetReadableImageSize()
        {
            return FormatFileSize(ImageSize);
        }

        public string GetReadableVideoSize()
        {
            return FormatFileSize(VideoSize);
        }

        public string GetReadableTotalSize()
        {
            return FormatFileSize(TotalSize);
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double size = bytes;
            int order = 0;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {sizes[order]}";
        }
    }
}
