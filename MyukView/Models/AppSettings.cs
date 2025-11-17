using System;

namespace MyukView.Models
{
    /// <summary>
    /// 사용자 설정 정보
    /// </summary>
    public class AppSettings
    {
        public string LastOpenedFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        public WindowSettings WindowSettings { get; set; } = new WindowSettings();
        public ViewerSettings ViewerSettings { get; set; } = new ViewerSettings();
        public SlideshowSettings SlideshowSettings { get; set; } = new SlideshowSettings();
    }

    public class WindowSettings
    {
        public double Width { get; set; } = 1000;
        public double Height { get; set; } = 700;
        public double Left { get; set; } = 100;
        public double Top { get; set; } = 100;
        public bool IsMaximized { get; set; } = false;
    }

    public class ViewerSettings
    {
        public bool ShowThumbnails { get; set; } = true;
        public int ThumbnailSize { get; set; } = 100;
        public string BackgroundColor { get; set; } = "#1E1E1E";
        public bool ShowImageInfo { get; set; } = true;
    }

    public class SlideshowSettings
    {
        public int IntervalSeconds { get; set; } = 3;
        public bool Loop { get; set; } = true;
        public bool ShowControls { get; set; } = true;
    }
}
