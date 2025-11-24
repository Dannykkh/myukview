using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MyukView.Controls
{
    /// <summary>
    /// 이미지 자르기 컨트롤
    /// </summary>
    public partial class CropControl : UserControl
    {
        private Point _startPoint;
        private bool _isDragging = false;
        private bool _isResizing = false;
        private string _resizeHandle = "";
        private Rect _cropArea;

        public CropControl()
        {
            InitializeComponent();
            InitializeHandleEvents();
        }

        /// <summary>
        /// 현재 이미지
        /// </summary>
        public BitmapImage? CurrentImage
        {
            get => imageControl.Source as BitmapImage;
            set
            {
                imageControl.Source = value;
                if (value != null)
                {
                    ResetCropArea();
                }
            }
        }

        /// <summary>
        /// 자르기 영역 (이미지 좌표)
        /// </summary>
        public Rect CropArea => _cropArea;

        /// <summary>
        /// 핸들 이벤트 초기화
        /// </summary>
        private void InitializeHandleEvents()
        {
            handleTopLeft.MouseLeftButtonDown += (s, e) => StartResize("TopLeft", e);
            handleTopRight.MouseLeftButtonDown += (s, e) => StartResize("TopRight", e);
            handleBottomLeft.MouseLeftButtonDown += (s, e) => StartResize("BottomLeft", e);
            handleBottomRight.MouseLeftButtonDown += (s, e) => StartResize("BottomRight", e);
            handleTop.MouseLeftButtonDown += (s, e) => StartResize("Top", e);
            handleBottom.MouseLeftButtonDown += (s, e) => StartResize("Bottom", e);
            handleLeft.MouseLeftButtonDown += (s, e) => StartResize("Left", e);
            handleRight.MouseLeftButtonDown += (s, e) => StartResize("Right", e);
        }

        /// <summary>
        /// 자르기 영역 초기화
        /// </summary>
        private void ResetCropArea()
        {
            if (CurrentImage == null) return;

            double width = imageControl.ActualWidth > 0 ? imageControl.ActualWidth : CurrentImage.PixelWidth;
            double height = imageControl.ActualHeight > 0 ? imageControl.ActualHeight : CurrentImage.PixelHeight;

            _cropArea = new Rect(
                width * 0.1,
                height * 0.1,
                width * 0.8,
                height * 0.8
            );

            UpdateCropVisuals();
        }

        /// <summary>
        /// 캔버스 마우스 다운
        /// </summary>
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(canvas);
            _isDragging = true;
            canvas.CaptureMouse();
            e.Handled = true;
        }

        /// <summary>
        /// 캔버스 마우스 이동
        /// </summary>
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging && !_isResizing) return;

            Point currentPoint = e.GetPosition(canvas);

            if (_isDragging && !_isResizing)
            {
                // 새 자르기 영역 그리기
                double x = Math.Min(_startPoint.X, currentPoint.X);
                double y = Math.Min(_startPoint.Y, currentPoint.Y);
                double width = Math.Abs(currentPoint.X - _startPoint.X);
                double height = Math.Abs(currentPoint.Y - _startPoint.Y);

                _cropArea = new Rect(x, y, width, height);
                UpdateCropVisuals();
            }
            else if (_isResizing)
            {
                // 리사이즈
                ResizeCropArea(currentPoint);
            }
        }

        /// <summary>
        /// 캔버스 마우스 업
        /// </summary>
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            _isResizing = false;
            canvas.ReleaseMouseCapture();
        }

        /// <summary>
        /// 리사이즈 시작
        /// </summary>
        private void StartResize(string handle, MouseButtonEventArgs e)
        {
            _resizeHandle = handle;
            _isResizing = true;
            _startPoint = e.GetPosition(canvas);
            canvas.CaptureMouse();
            e.Handled = true;
        }

        /// <summary>
        /// 자르기 영역 리사이즈
        /// </summary>
        private void ResizeCropArea(Point currentPoint)
        {
            double deltaX = currentPoint.X - _startPoint.X;
            double deltaY = currentPoint.Y - _startPoint.Y;

            Rect newArea = _cropArea;

            switch (_resizeHandle)
            {
                case "TopLeft":
                    newArea = new Rect(_cropArea.X + deltaX, _cropArea.Y + deltaY,
                        _cropArea.Width - deltaX, _cropArea.Height - deltaY);
                    break;
                case "TopRight":
                    newArea = new Rect(_cropArea.X, _cropArea.Y + deltaY,
                        _cropArea.Width + deltaX, _cropArea.Height - deltaY);
                    break;
                case "BottomLeft":
                    newArea = new Rect(_cropArea.X + deltaX, _cropArea.Y,
                        _cropArea.Width - deltaX, _cropArea.Height + deltaY);
                    break;
                case "BottomRight":
                    newArea = new Rect(_cropArea.X, _cropArea.Y,
                        _cropArea.Width + deltaX, _cropArea.Height + deltaY);
                    break;
                case "Top":
                    newArea = new Rect(_cropArea.X, _cropArea.Y + deltaY,
                        _cropArea.Width, _cropArea.Height - deltaY);
                    break;
                case "Bottom":
                    newArea = new Rect(_cropArea.X, _cropArea.Y,
                        _cropArea.Width, _cropArea.Height + deltaY);
                    break;
                case "Left":
                    newArea = new Rect(_cropArea.X + deltaX, _cropArea.Y,
                        _cropArea.Width - deltaX, _cropArea.Height);
                    break;
                case "Right":
                    newArea = new Rect(_cropArea.X, _cropArea.Y,
                        _cropArea.Width + deltaX, _cropArea.Height);
                    break;
            }

            if (newArea.Width > 20 && newArea.Height > 20)
            {
                _cropArea = newArea;
                _startPoint = currentPoint;
                UpdateCropVisuals();
            }
        }

        /// <summary>
        /// 자르기 영역 시각화 업데이트
        /// </summary>
        private void UpdateCropVisuals()
        {
            // 자르기 사각형
            Canvas.SetLeft(cropRect, _cropArea.X);
            Canvas.SetTop(cropRect, _cropArea.Y);
            cropRect.Width = _cropArea.Width;
            cropRect.Height = _cropArea.Height;

            // 오버레이 (어두운 부분)
            double canvasWidth = canvas.ActualWidth;
            double canvasHeight = canvas.ActualHeight;

            // 상단 오버레이
            Canvas.SetLeft(topOverlay, 0);
            Canvas.SetTop(topOverlay, 0);
            topOverlay.Width = canvasWidth;
            topOverlay.Height = _cropArea.Y;

            // 하단 오버레이
            Canvas.SetLeft(bottomOverlay, 0);
            Canvas.SetTop(bottomOverlay, _cropArea.Y + _cropArea.Height);
            bottomOverlay.Width = canvasWidth;
            bottomOverlay.Height = canvasHeight - (_cropArea.Y + _cropArea.Height);

            // 좌측 오버레이
            Canvas.SetLeft(leftOverlay, 0);
            Canvas.SetTop(leftOverlay, _cropArea.Y);
            leftOverlay.Width = _cropArea.X;
            leftOverlay.Height = _cropArea.Height;

            // 우측 오버레이
            Canvas.SetLeft(rightOverlay, _cropArea.X + _cropArea.Width);
            Canvas.SetTop(rightOverlay, _cropArea.Y);
            rightOverlay.Width = canvasWidth - (_cropArea.X + _cropArea.Width);
            rightOverlay.Height = _cropArea.Height;

            // 핸들 위치 업데이트
            UpdateHandlePositions();

            // 정보 업데이트
            txtCropInfo.Text = $"{(int)_cropArea.Width} × {(int)_cropArea.Height}";
        }

        /// <summary>
        /// 핸들 위치 업데이트
        /// </summary>
        private void UpdateHandlePositions()
        {
            double x = _cropArea.X;
            double y = _cropArea.Y;
            double w = _cropArea.Width;
            double h = _cropArea.Height;

            SetHandlePosition(handleTopLeft, x - 6, y - 6);
            SetHandlePosition(handleTopRight, x + w - 6, y - 6);
            SetHandlePosition(handleBottomLeft, x - 6, y + h - 6);
            SetHandlePosition(handleBottomRight, x + w - 6, y + h - 6);
            SetHandlePosition(handleTop, x + w / 2 - 6, y - 6);
            SetHandlePosition(handleBottom, x + w / 2 - 6, y + h - 6);
            SetHandlePosition(handleLeft, x - 6, y + h / 2 - 6);
            SetHandlePosition(handleRight, x + w - 6, y + h / 2 - 6);
        }

        private void SetHandlePosition(UIElement element, double x, double y)
        {
            Canvas.SetLeft(element, x);
            Canvas.SetTop(element, y);
        }

        /// <summary>
        /// 이미지 좌표로 변환된 자르기 영역 가져오기
        /// </summary>
        public Rect GetImageCropArea()
        {
            if (CurrentImage == null) return Rect.Empty;

            double scaleX = CurrentImage.PixelWidth / imageControl.ActualWidth;
            double scaleY = CurrentImage.PixelHeight / imageControl.ActualHeight;

            return new Rect(
                _cropArea.X * scaleX,
                _cropArea.Y * scaleY,
                _cropArea.Width * scaleX,
                _cropArea.Height * scaleY
            );
        }
    }
}
