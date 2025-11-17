using System;
using System.IO;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace MyukView
{
    internal class AudioConv
    {
        /// <summary>
        /// FFmpeg를 이용해 입력 영상에서 음성만 추출하여 M4A 파일로 변환
        /// </summary>
        /// <param name="videoPath">영상 파일 경로</param>
        /// <returns>변환된 M4A 파일 경로</returns>
        public async Task<string> ExtractAudioAsync(string videoPath, AudioSettings settings, Action<double> onProgress = null)
        {
            string outputPath = Path.ChangeExtension(videoPath, settings.Extension);

            var conversion = FFmpeg.Conversions.New();

            conversion.OnProgress += (s, args) =>
            {
                onProgress?.Invoke(args.Percent);
            };

            conversion.AddParameter(
                $"-i \"{videoPath}\" -vn -c:a {settings.Codec} -b:a {settings.Bitrate}k \"{outputPath}\"",
                ParameterPosition.PreInput);

            await conversion.Start();
            return outputPath;
        }
    }
}
