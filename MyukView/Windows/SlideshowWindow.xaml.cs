using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MyukView.Models;

namespace MyukView.Windows
{
    /// <summary>
    /// 슬라이드쇼 윈도우
    /// </summary>
    public partial class SlideshowWindow : Window
    {
        private List<ImageFile> _images;
        private int _currentIndex;
        private DispatcherTimer _timer;
        private bool _isPlaying;
        private int _intervalSeconds;

        public SlideshowWindow(List<ImageFile> images, int currentIndex = 0, int intervalSeconds = 3)
        {
            InitializeComponent();

            _images = images;
            _currentIndex = currentIndex;
            _intervalSeconds = intervalSeconds;
            _isPlaying = true;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(_intervalSeconds)
            };
            _timer.Tick += Timer_Tick;

            LoadCurrentImage();
            _timer.Start();
        }

        private void LoadCurrentImage()
        {
            if (_currentIndex >= 0 && _currentIndex < _images.Count)
            {
                var imageFile = _images[_currentIndex];
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(imageFile.FilePath, UriKind.Absolute);
                    bitmap.EndInit();

                    slideImage.Source = bitmap;
                    txtImageInfo.Text = $"{_currentIndex + 1} / {_images.Count} - {imageFile.FileName}";
                }
                catch
                {
                    txtImageInfo.Text = $"이미지 로드 실패: {imageFile.FileName}";
                }
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            NextImage();
        }

        private void NextImage()
        {
            _currentIndex++;
            if (_currentIndex >= _images.Count)
            {
                _currentIndex = 0; // 루프
            }
            LoadCurrentImage();
        }

        private void PreviousImage()
        {
            _currentIndex--;
            if (_currentIndex < 0)
            {
                _currentIndex = _images.Count - 1;
            }
            LoadCurrentImage();
        }

        private void TogglePlayPause()
        {
            _isPlaying = !_isPlaying;
            if (_isPlaying)
            {
                _timer.Start();
                btnPlayPause.Content = "❚❚";
            }
            else
            {
                _timer.Stop();
                btnPlayPause.Content = "▶";
            }
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            PreviousImage();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            NextImage();
        }

        private void btnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            TogglePlayPause();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
                case Key.Right:
                case Key.Space:
                    NextImage();
                    break;
                case Key.Left:
                    PreviousImage();
                    break;
                case Key.P:
                    TogglePlayPause();
                    break;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _timer.Stop();
            base.OnClosed(e);
        }
    }
}
