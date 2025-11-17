using System.Windows;
using System.Windows.Media.Imaging;

namespace MyukView.Windows
{
    /// <summary>
    /// 이미지 자르기 윈도우
    /// </summary>
    public partial class CropWindow : Window
    {
        public Rect CropArea { get; private set; }
        public bool IsApplied { get; private set; }

        public CropWindow(BitmapImage image)
        {
            InitializeComponent();
            cropControl.CurrentImage = image;
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            CropArea = cropControl.GetImageCropArea();
            IsApplied = true;
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            IsApplied = false;
            DialogResult = false;
            Close();
        }
    }
}
