using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MyukView.Controls
{
    /// <summary>
    /// 확대/축소 및 팬 기능을 가진 이미지 컨트롤
    /// </summary>
    public partial class ZoomableImageControl : UserControl
    {
        private Point _lastMousePosition;
        private bool _isDragging = false;
        private double _currentZoom = 1.0;
        private const double MIN_ZOOM = 0.1;
        private const double MAX_ZOOM = 10.0;
        private const double ZOOM_SPEED = 0.1;
        private DispatcherTimer? _zoomInfoTimer;

        public ZoomableImageControl()
        {
            InitializeComponent();
            InitializeZoomInfoTimer();
        }

        /// <summary>
        /// 현재 표시 중인 이미지
        /// </summary>
        public BitmapImage? CurrentImage
        {
            get => imageControl.Source as BitmapImage;
            set
            {
                imageControl.Source = value;
                ResetZoom();
                UpdateImageInfo();
            }
        }

        /// <summary>
        /// 현재 확대/축소 비율
        /// </summary>
        public double ZoomLevel
        {
            get => _currentZoom;
            set
            {
                _currentZoom = Math.Max(MIN_ZOOM, Math.Min(MAX_ZOOM, value));
                scaleTransform.ScaleX = _currentZoom;
                scaleTransform.ScaleY = _currentZoom;
                UpdateZoomInfo();
            }
        }

        /// <summary>
        /// 이미지 회전 각도
        /// </summary>
        public double RotationAngle
        {
            get => rotateTransform.Angle;
            set => rotateTransform.Angle = value;
        }

        /// <summary>
        /// 확대/축소 정보 타이머 초기화
        /// </summary>
        private void InitializeZoomInfoTimer()
        {
            _zoomInfoTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _zoomInfoTimer.Tick += (s, e) =>
            {
                zoomInfoText.Visibility = Visibility.Collapsed;
                _zoomInfoTimer?.Stop();
            };
        }

        /// <summary>
        /// 마우스 휠로 확대/축소
        /// </summary>
        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (imageControl.Source == null) return;

            double zoomFactor = e.Delta > 0 ? 1 + ZOOM_SPEED : 1 - ZOOM_SPEED;
            double newZoom = _currentZoom * zoomFactor;

            // 마우스 위치를 중심으로 확대/축소
            Point mousePos = e.GetPosition(imageControl);
            Point centerPoint = new Point(
                mousePos.X / imageControl.ActualWidth,
                mousePos.Y / imageControl.ActualHeight
            );

            ZoomLevel = newZoom;
            e.Handled = true;
        }

        /// <summary>
        /// 마우스 드래그 시작
        /// </summary>
        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (imageControl.Source == null) return;

            _isDragging = true;
            _lastMousePosition = e.GetPosition(scrollViewer);
            imageControl.Cursor = Cursors.Hand;
            imageControl.CaptureMouse();
            e.Handled = true;
        }

        /// <summary>
        /// 마우스 드래그 종료
        /// </summary>
        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                imageControl.Cursor = Cursors.Arrow;
                imageControl.ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        /// <summary>
        /// 마우스 드래그 이동
        /// </summary>
        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || imageControl.Source == null) return;

            Point currentPosition = e.GetPosition(scrollViewer);
            Vector delta = currentPosition - _lastMousePosition;

            // ScrollViewer의 오프셋 조정
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - delta.X);
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - delta.Y);

            _lastMousePosition = currentPosition;
            e.Handled = true;
        }

        /// <summary>
        /// 마우스가 컨트롤을 벗어날 때
        /// </summary>
        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                imageControl.Cursor = Cursors.Arrow;
                imageControl.ReleaseMouseCapture();
            }
        }

        /// <summary>
        /// 확대/축소 정보 업데이트
        /// </summary>
        private void UpdateZoomInfo()
        {
            zoomInfoText.Text = $"{(_currentZoom * 100):F0}%";
            zoomInfoText.Visibility = Visibility.Visible;

            _zoomInfoTimer?.Stop();
            _zoomInfoTimer?.Start();
        }

        /// <summary>
        /// 이미지 정보 업데이트
        /// </summary>
        private void UpdateImageInfo()
        {
            if (CurrentImage != null)
            {
                imageInfoText.Text = $"{CurrentImage.PixelWidth} × {CurrentImage.PixelHeight}";
                imageInfoText.Visibility = Visibility.Visible;
            }
            else
            {
                imageInfoText.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 확대/축소 리셋 (창에 맞춤)
        /// </summary>
        public void ResetZoom()
        {
            ZoomLevel = 1.0;
            translateTransform.X = 0;
            translateTransform.Y = 0;
            scrollViewer.ScrollToHorizontalOffset(0);
            scrollViewer.ScrollToVerticalOffset(0);
        }

        /// <summary>
        /// 실제 크기로 보기 (100%)
        /// </summary>
        public void ZoomToActualSize()
        {
            if (CurrentImage == null) return;

            ZoomLevel = 1.0;
        }

        /// <summary>
        /// 창에 맞춤
        /// </summary>
        public void ZoomToFit()
        {
            if (CurrentImage == null) return;

            double widthRatio = ActualWidth / CurrentImage.PixelWidth;
            double heightRatio = ActualHeight / CurrentImage.PixelHeight;
            ZoomLevel = Math.Min(widthRatio, heightRatio) * 0.9; // 여백을 위해 0.9 곱함
        }

        /// <summary>
        /// 확대
        /// </summary>
        public void ZoomIn()
        {
            ZoomLevel = _currentZoom * (1 + ZOOM_SPEED * 2);
        }

        /// <summary>
        /// 축소
        /// </summary>
        public void ZoomOut()
        {
            ZoomLevel = _currentZoom * (1 - ZOOM_SPEED * 2);
        }

        /// <summary>
        /// 시계방향으로 90도 회전
        /// </summary>
        public void RotateClockwise()
        {
            RotationAngle = (RotationAngle + 90) % 360;
        }

        /// <summary>
        /// 반시계방향으로 90도 회전
        /// </summary>
        public void RotateCounterClockwise()
        {
            RotationAngle = (RotationAngle - 90 + 360) % 360;
        }

        /// <summary>
        /// 회전 리셋
        /// </summary>
        public void ResetRotation()
        {
            RotationAngle = 0;
        }
    }
}
