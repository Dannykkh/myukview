namespace MyukView.Models
{
    /// <summary>
    /// 오디오 변환 설정 정보
    /// </summary>
    public class AudioSettings
    {
        public string Codec { get; set; } = "aac";
        public int Bitrate { get; set; } = 192;
        public string Extension { get; set; } = ".m4a";
    }
}
