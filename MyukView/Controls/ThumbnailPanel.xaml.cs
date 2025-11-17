using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MyukView.Models;

namespace MyukView.Controls
{
    /// <summary>
    /// 썸네일 패널 컨트롤
    /// </summary>
    public partial class ThumbnailPanel : UserControl
    {
        public class ThumbnailItem
        {
            public ImageFile ImageFile { get; set; } = new ImageFile();
            public BitmapImage? ThumbnailImage { get; set; }
            public string FileName => ImageFile.FileName;
        }

        private ObservableCollection<ThumbnailItem> _items;

        public ThumbnailPanel()
        {
            InitializeComponent();
            _items = new ObservableCollection<ThumbnailItem>();
            thumbnailList.ItemsSource = _items;
        }

        /// <summary>
        /// 썸네일 선택 이벤트
        /// </summary>
        public event EventHandler<ImageFile>? ThumbnailSelected;

        /// <summary>
        /// 썸네일 추가
        /// </summary>
        public void AddThumbnail(ImageFile imageFile, BitmapImage? thumbnail)
        {
            _items.Add(new ThumbnailItem
            {
                ImageFile = imageFile,
                ThumbnailImage = thumbnail
            });

            UpdateImageCount();
        }

        /// <summary>
        /// 썸네일 클리어
        /// </summary>
        public void Clear()
        {
            _items.Clear();
            UpdateImageCount();
        }

        /// <summary>
        /// 이미지 개수 업데이트
        /// </summary>
        private void UpdateImageCount()
        {
            txtImageCount.Text = $"({_items.Count}개)";
        }

        /// <summary>
        /// 썸네일 클릭
        /// </summary>
        private void Thumbnail_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is ThumbnailItem item)
            {
                ThumbnailSelected?.Invoke(this, item.ImageFile);

                // 선택된 썸네일 하이라이트
                foreach (var child in thumbnailList.Items)
                {
                    if (thumbnailList.ItemContainerGenerator.ContainerFromItem(child) is ContentPresenter presenter)
                    {
                        var itemBorder = FindVisualChild<Border>(presenter);
                        if (itemBorder != null)
                        {
                            itemBorder.BorderBrush = itemBorder == border ?
                                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 122, 204)) :
                                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(85, 85, 85));
                        }
                    }
                }
            }
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }
    }
}
