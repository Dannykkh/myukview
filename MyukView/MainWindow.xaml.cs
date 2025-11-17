using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;
using MyukView.ViewModels;
using MyukView.Services;
using MyukView.Models;

namespace MyukView
{
    public partial class MainWindow : Window
    {
        private readonly ImageViewerViewModel _viewModel;
        private readonly FormatConverterService _formatConverter;
        private readonly AudioService _audioService;
        private readonly ImageProcessingService _imageProcessor;
        private string[] _imageFilePaths = Array.Empty<string>();
        private string _videoFilePath = string.Empty;
        private WindowState _previousWindowState;
        private WindowStyle _previousWindowStyle;

        public MainWindow()
        {
            InitializeComponent();

            // ViewModel 초기화
            _viewModel = new ImageViewerViewModel();
            DataContext = _viewModel;

            // 서비스 초기화
            _formatConverter = new FormatConverterService();
            _audioService = new AudioService();
            _imageProcessor = new ImageProcessingService();

            // 이벤트 연결
            _viewModel.ZoomInRequested += (s, e) => imageViewer.ZoomIn();
            _viewModel.ZoomOutRequested += (s, e) => imageViewer.ZoomOut();
            _viewModel.ResetZoomRequested += (s, e) => imageViewer.ResetZoom();
            _viewModel.RotateClockwiseRequested += (s, e) => imageViewer.RotateClockwise();
            _viewModel.RotateCounterClockwiseRequested += (s, e) => imageViewer.RotateCounterClockwise();

            // ViewModel의 CurrentImage 변경 시 컨트롤 업데이트
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_viewModel.CurrentImage))
                {
                    imageViewer.CurrentImage = _viewModel.CurrentImage;
                }
            };

            // FFmpeg 초기화
            InitFFmpeg();
        }

        private async void InitFFmpeg()
        {
            try
            {
                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);
                FFmpeg.SetExecutablesPath(FFmpeg.ExecutablesPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"FFmpeg 다운로드 또는 설정에 실패했습니다.\n오류: {ex.Message}",
                    "FFmpeg 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #region 메뉴 이벤트

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuFormatConverter_Click(object sender, RoutedEventArgs e)
        {
            mainTabControl.SelectedItem = tabFormatConverter;
        }

        private void MenuVideoProcessing_Click(object sender, RoutedEventArgs e)
        {
            mainTabControl.SelectedItem = tabVideoProcessing;
        }

        #endregion

        #region 탭 변경 이벤트

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 탭이 변경될 때 필요한 처리
        }

        #endregion

        #region 포맷 변환 탭

        private void btnLoadImages_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "이미지 파일 선택",
                Filter = "이미지 파일|*.avif;*.webp;*.tiff;*.bmp;*.jpg;*.jpeg;*.png;*.gif|모든 파일|*.*",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                _imageFilePaths = dialog.FileNames;
                txtFileCount.Text = $"선택된 파일: {_imageFilePaths.Length}개";
                txtConversionStatus.Text = $"{_imageFilePaths.Length}개의 파일이 로드되었습니다.\n변환 설정을 지정한 후 '변환 시작'을 클릭하세요.";
            }
        }

        private async void btnConvertFormat_Click(object sender, RoutedEventArgs e)
        {
            if (_imageFilePaths == null || _imageFilePaths.Length == 0)
            {
                MessageBox.Show("먼저 이미지 파일을 불러오세요.", "알림",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            btnConvertFormat.IsEnabled = false;
            progressBar.Visibility = Visibility.Visible;
            progressBar.Value = 0;

            try
            {
                var targetFormat = ((ComboBoxItem)cmbTargetFormat.SelectedItem).Content.ToString()?.ToLower() ?? "png";
                int quality = (int)sliderQuality.Value;

                var statusBuilder = new StringBuilder();
                statusBuilder.AppendLine($"변환 시작: {_imageFilePaths.Length}개 파일");
                statusBuilder.AppendLine($"대상 포맷: {targetFormat.ToUpper()}, 품질: {quality}");
                statusBuilder.AppendLine();

                for (int i = 0; i < _imageFilePaths.Length; i++)
                {
                    string inputPath = _imageFilePaths[i];
                    try
                    {
                        string outputPath;
                        if (targetFormat == "png")
                        {
                            outputPath = await _formatConverter.ConvertToPngAsync(inputPath);
                        }
                        else
                        {
                            outputPath = await _formatConverter.ConvertToFormatAsync(inputPath, targetFormat, quality);
                        }

                        statusBuilder.AppendLine($"✓ {Path.GetFileName(inputPath)} → {Path.GetFileName(outputPath)}");
                        progressBar.Value = ((i + 1) * 100.0) / _imageFilePaths.Length;
                    }
                    catch (Exception ex)
                    {
                        statusBuilder.AppendLine($"✗ {Path.GetFileName(inputPath)}: {ex.Message}");
                    }

                    txtConversionStatus.Text = statusBuilder.ToString();
                }

                statusBuilder.AppendLine();
                statusBuilder.AppendLine("변환 완료!");
                txtConversionStatus.Text = statusBuilder.ToString();

                MessageBox.Show($"{_imageFilePaths.Length}개 파일 변환 완료!", "완료",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"변환 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnConvertFormat.IsEnabled = true;
                progressBar.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region 영상 처리 탭

        private void btnLoadVideo_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "영상 파일 선택",
                Filter = "영상 파일|*.mp4;*.mov;*.avi;*.mkv;*.wmv|모든 파일|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                _videoFilePath = dialog.FileName;
                txtVideoFileName.Text = Path.GetFileName(_videoFilePath);
            }
        }

        private async void btnExtractAudio_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_videoFilePath) || !File.Exists(_videoFilePath))
            {
                MessageBox.Show("먼저 영상 파일을 불러오세요.", "알림",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            btnExtractAudio.IsEnabled = false;
            progressBar.Visibility = Visibility.Visible;
            progressBar.Value = 0;

            try
            {
                // 설정 가져오기
                var codecItem = ((ComboBoxItem)cmbCodec.SelectedItem).Content.ToString() ?? "AAC";
                string codec = codecItem switch
                {
                    "AAC" => "aac",
                    "MP3 (libmp3lame)" => "libmp3lame",
                    "FLAC" => "flac",
                    _ => "aac"
                };

                var bitrateItem = ((ComboBoxItem)cmbBitrate.SelectedItem).Content.ToString() ?? "192 kbps";
                int bitrate = int.Parse(bitrateItem.Replace(" kbps", ""));

                var ext = ((ComboBoxItem)cmbOutputExt.SelectedItem).Content.ToString() ?? ".m4a";

                var settings = new AudioSettings
                {
                    Codec = codec,
                    Bitrate = bitrate,
                    Extension = ext
                };

                // 진행률 콜백
                string outputPath = await _audioService.ExtractAudioAsync(_videoFilePath, settings, percent =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        progressBar.Value = percent;
                    });
                });

                progressBar.Value = 100;
                MessageBox.Show($"음성 추출 완료!\n{outputPath}", "완료",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // 출력 폴더 열기
                string? folder = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
                {
                    Process.Start("explorer.exe", folder);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"음성 추출 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnExtractAudio.IsEnabled = true;
                progressBar.Visibility = Visibility.Collapsed;
            }
        }

        private async void btnExtractText_Click(object sender, RoutedEventArgs e)
        {
            // 오디오 파일 경로 확인
            if (string.IsNullOrEmpty(_videoFilePath))
            {
                MessageBox.Show("먼저 영상 파일을 선택하고 음성을 추출하세요.", "알림",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var audioPath = Path.ChangeExtension(_videoFilePath, ".m4a");
            if (!File.Exists(audioPath))
            {
                MessageBox.Show("먼저 음성 추출을 진행하세요.", "알림",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            btnExtractText.IsEnabled = false;

            try
            {
                string model = ((ComboBoxItem)cmbModel.SelectedItem).Content.ToString() ?? "base";
                bool timestamp = chkTimestamp.IsChecked == true;
                bool speaker = chkSpeaker.IsChecked == true;
                bool markdown = chkMarkdown.IsChecked == true;

                // 파이썬 스크립트 인자 구성
                var argsBuilder = new StringBuilder();
                argsBuilder.Append($"\"scripts\\whisper_run.py\" \"{audioPath}\" {model}");
                if (timestamp) argsBuilder.Append(" --timestamp");
                if (speaker) argsBuilder.Append(" --speaker");
                if (markdown) argsBuilder.Append(" --markdown");

                string args = argsBuilder.ToString();
                string pythonExe = @"C:\Path\To\python.exe"; // 환경에 맞게 수정 필요

                var psi = new ProcessStartInfo
                {
                    FileName = pythonExe,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = new Process { StartInfo = psi };
                var outputBuilder = new StringBuilder();

                process.OutputDataReceived += (s, ea) =>
                {
                    if (!string.IsNullOrEmpty(ea.Data))
                    {
                        outputBuilder.AppendLine(ea.Data);
                    }
                };

                process.ErrorDataReceived += (s, ea) =>
                {
                    if (!string.IsNullOrEmpty(ea.Data))
                    {
                        outputBuilder.AppendLine(ea.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await Task.Run(() => process.WaitForExit());

                // 결과 텍스트 로드
                var transcriptPath = Path.ChangeExtension(audioPath, ".txt");
                if (File.Exists(transcriptPath))
                {
                    txtTranscript.Text = File.ReadAllText(transcriptPath);
                    MessageBox.Show("텍스트 추출 완료!", "완료",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    txtTranscript.Text = "텍스트 파일을 찾을 수 없습니다.\n\n" + outputBuilder.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"텍스트 추출 중 오류 발생: {ex.Message}", "오류",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnExtractText.IsEnabled = true;
            }
        }

        private void btnSaveText_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTranscript.Text))
            {
                MessageBox.Show("저장할 텍스트가 없습니다.", "알림",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "텍스트 저장",
                Filter = "텍스트 파일|*.txt|마크다운 파일|*.md|자막 파일 (SRT)|*.srt",
                FileName = "transcription"
            };

            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, txtTranscript.Text);
                MessageBox.Show("텍스트 저장 완료!", "완료",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion

        #region 새 기능 메뉴 이벤트

        private async void MenuCrop_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.CurrentImage == null)
            {
                MessageBox.Show("먼저 이미지를 선택하세요.", "알림",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var cropWindow = new CropWindow(_viewModel.CurrentImage);
            if (cropWindow.ShowDialog() == true && cropWindow.IsApplied)
            {
                try
                {
                    var cropArea = cropWindow.CropArea;
                    string currentPath = _viewModel.CurrentImageFile?.FilePath ?? "";

                    if (!string.IsNullOrEmpty(currentPath))
                    {
                        string outputPath = await _imageProcessor.CropImageAsync(currentPath,
                            (int)cropArea.X, (int)cropArea.Y, (int)cropArea.Width, (int)cropArea.Height);

                        MessageBox.Show($"자르기 완료!\n{Path.GetFileName(outputPath)}", "완료",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"자르기 중 오류 발생: {ex.Message}", "오류",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuSlideshow_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.ImageFiles.Count == 0)
            {
                MessageBox.Show("이미지가 없습니다.", "알림",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var slideshowWindow = new SlideshowWindow(
                _viewModel.ImageFiles.ToList(),
                _viewModel.CurrentImageIndex,
                3 // 3초 간격
            );
            slideshowWindow.ShowDialog();
        }

        private void MenuFullscreen_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized && WindowStyle == WindowStyle.None)
            {
                // 전체화면 해제
                WindowState = _previousWindowState;
                WindowStyle = _previousWindowStyle;
            }
            else
            {
                // 전체화면
                _previousWindowState = WindowState;
                _previousWindowStyle = WindowStyle;
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
        }

        private void MenuToggleThumbnails_Click(object sender, RoutedEventArgs e)
        {
            thumbnailPanel.Visibility = menuShowThumbnails.IsChecked ?
                Visibility.Visible : Visibility.Collapsed;
        }

        #endregion
    }
}
