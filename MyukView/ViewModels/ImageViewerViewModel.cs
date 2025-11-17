using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using MyukView.Models;
using MyukView.Services;

namespace MyukView.ViewModels
{
    /// <summary>
    /// 이미지 뷰어 ViewModel
    /// </summary>
    public class ImageViewerViewModel : ViewModelBase
    {
        private readonly ImageService _imageService;
        private BitmapImage? _currentImage;
        private ImageFile? _currentImageFile;
        private int _currentImageIndex = -1;
        private string _statusText = "이미지를 열어주세요";
        private bool _isLoading = false;

        public ImageViewerViewModel()
        {
            _imageService = new ImageService();
            ImageFiles = new ObservableCollection<ImageFile>();

            // 명령 초기화
            OpenImageCommand = new RelayCommand(_ => OpenImage());
            OpenFolderCommand = new RelayCommand(_ => OpenFolder());
            NextImageCommand = new RelayCommand(_ => NextImage(), _ => CanGoNext());
            PreviousImageCommand = new RelayCommand(_ => PreviousImage(), _ => CanGoPrevious());
            ZoomInCommand = new RelayCommand(_ => OnZoomInRequested());
            ZoomOutCommand = new RelayCommand(_ => OnZoomOutRequested());
            ResetZoomCommand = new RelayCommand(_ => OnResetZoomRequested());
            RotateClockwiseCommand = new RelayCommand(_ => OnRotateClockwiseRequested());
            RotateCounterClockwiseCommand = new RelayCommand(_ => OnRotateCounterClockwiseRequested());
        }

        #region Properties

        public ObservableCollection<ImageFile> ImageFiles { get; }

        public BitmapImage? CurrentImage
        {
            get => _currentImage;
            set => SetProperty(ref _currentImage, value);
        }

        public ImageFile? CurrentImageFile
        {
            get => _currentImageFile;
            set
            {
                if (SetProperty(ref _currentImageFile, value))
                {
                    UpdateStatusText();
                }
            }
        }

        public int CurrentImageIndex
        {
            get => _currentImageIndex;
            set
            {
                if (SetProperty(ref _currentImageIndex, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        #endregion

        #region Commands

        public ICommand OpenImageCommand { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand NextImageCommand { get; }
        public ICommand PreviousImageCommand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand ResetZoomCommand { get; }
        public ICommand RotateClockwiseCommand { get; }
        public ICommand RotateCounterClockwiseCommand { get; }

        #endregion

        #region Events

        public event EventHandler? ZoomInRequested;
        public event EventHandler? ZoomOutRequested;
        public event EventHandler? ResetZoomRequested;
        public event EventHandler? RotateClockwiseRequested;
        public event EventHandler? RotateCounterClockwiseRequested;

        #endregion

        #region Methods

        /// <summary>
        /// 이미지 파일 열기
        /// </summary>
        private void OpenImage()
        {
            var dialog = new OpenFileDialog
            {
                Title = "이미지 파일 열기",
                Filter = "이미지 파일|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.tiff;*.webp;*.avif;*.ico|모든 파일|*.*",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                LoadImages(dialog.FileNames);
            }
        }

        /// <summary>
        /// 폴더 열기
        /// </summary>
        private void OpenFolder()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "이미지 폴더 선택",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var imageFiles = _imageService.GetImagesFromFolder(dialog.SelectedPath, includeSubfolders: false);
                if (imageFiles.Count > 0)
                {
                    LoadImages(imageFiles.Select(f => f.FilePath).ToArray());
                }
                else
                {
                    MessageBox.Show("선택한 폴더에 이미지 파일이 없습니다.", "알림",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        /// <summary>
        /// 이미지 목록 로드
        /// </summary>
        private void LoadImages(string[] filePaths)
        {
            ImageFiles.Clear();

            foreach (var path in filePaths)
            {
                if (ImageFile.IsSupportedFormat(path))
                {
                    ImageFiles.Add(new ImageFile(path));
                }
            }

            if (ImageFiles.Count > 0)
            {
                LoadImageAtIndex(0);
            }
        }

        /// <summary>
        /// 특정 인덱스의 이미지 로드
        /// </summary>
        private async void LoadImageAtIndex(int index)
        {
            if (index < 0 || index >= ImageFiles.Count)
                return;

            IsLoading = true;
            CurrentImageIndex = index;
            CurrentImageFile = ImageFiles[index];

            try
            {
                var bitmap = await _imageService.LoadImageAsync(CurrentImageFile.FilePath);
                if (bitmap != null)
                {
                    CurrentImage = bitmap;

                    // 메타데이터 로드
                    CurrentImageFile.Metadata = _imageService.GetMetadata(CurrentImageFile.FilePath);
                }
                else
                {
                    MessageBox.Show($"이미지를 로드할 수 없습니다: {CurrentImageFile.FileName}",
                        "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"이미지 로드 중 오류 발생: {ex.Message}",
                    "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 다음 이미지
        /// </summary>
        private void NextImage()
        {
            if (CanGoNext())
            {
                LoadImageAtIndex(CurrentImageIndex + 1);
            }
        }

        /// <summary>
        /// 이전 이미지
        /// </summary>
        private void PreviousImage()
        {
            if (CanGoPrevious())
            {
                LoadImageAtIndex(CurrentImageIndex - 1);
            }
        }

        /// <summary>
        /// 다음 이미지로 이동 가능 여부
        /// </summary>
        private bool CanGoNext()
        {
            return CurrentImageIndex < ImageFiles.Count - 1;
        }

        /// <summary>
        /// 이전 이미지로 이동 가능 여부
        /// </summary>
        private bool CanGoPrevious()
        {
            return CurrentImageIndex > 0;
        }

        /// <summary>
        /// 상태 텍스트 업데이트
        /// </summary>
        private void UpdateStatusText()
        {
            if (CurrentImageFile != null && CurrentImageFile.Metadata != null)
            {
                var metadata = CurrentImageFile.Metadata;
                StatusText = $"{CurrentImageFile.FileName} | " +
                            $"{metadata.GetResolutionString()} | " +
                            $"{metadata.GetMegapixels()} MP | " +
                            $"{CurrentImageFile.GetReadableFileSize()} | " +
                            $"{CurrentImageIndex + 1}/{ImageFiles.Count}";
            }
            else if (CurrentImageFile != null)
            {
                StatusText = $"{CurrentImageFile.FileName} | {CurrentImageIndex + 1}/{ImageFiles.Count}";
            }
            else
            {
                StatusText = "이미지를 열어주세요";
            }
        }

        private void OnZoomInRequested() => ZoomInRequested?.Invoke(this, EventArgs.Empty);
        private void OnZoomOutRequested() => ZoomOutRequested?.Invoke(this, EventArgs.Empty);
        private void OnResetZoomRequested() => ResetZoomRequested?.Invoke(this, EventArgs.Empty);
        private void OnRotateClockwiseRequested() => RotateClockwiseRequested?.Invoke(this, EventArgs.Empty);
        private void OnRotateCounterClockwiseRequested() => RotateCounterClockwiseRequested?.Invoke(this, EventArgs.Empty);

        #endregion
    }
}
