using System;
using System.IO;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using MyukView.Models;

namespace MyukView.Services
{
    /// <summary>
    /// 오디오 추출 및 변환 서비스
    /// </summary>
    public class AudioService
    {
        /// <summary>
        /// FFmpeg를 이용해 입력 영상에서 음성만 추출
        /// </summary>
        /// <param name="videoPath">영상 파일 경로</param>
        /// <param name="settings">오디오 설정</param>
        /// <param name="onProgress">진행률 콜백</param>
        /// <returns>변환된 오디오 파일 경로</returns>
        public async Task<string> ExtractAudioAsync(string videoPath, AudioSettings settings, Action<double>? onProgress = null)
        {
            if (string.IsNullOrEmpty(videoPath) || !File.Exists(videoPath))
                throw new FileNotFoundException("영상 파일을 찾을 수 없습니다.", videoPath);

            string outputPath = Path.ChangeExtension(videoPath, settings.Extension);

            try
            {
                var conversion = FFmpeg.Conversions.New();

                if (onProgress != null)
                {
                    conversion.OnProgress += (s, args) =>
                    {
                        onProgress?.Invoke(args.Percent);
                    };
                }

                conversion.AddParameter(
                    $"-i \"{videoPath}\" -vn -c:a {settings.Codec} -b:a {settings.Bitrate}k \"{outputPath}\"",
                    ParameterPosition.PreInput);

                await conversion.Start();
                return outputPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"오디오 추출 중 오류 발생: {ex.Message}", ex);
            }
        }
    }
}
