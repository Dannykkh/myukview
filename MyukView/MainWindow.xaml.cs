using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace MyukView
{
    public partial class MainWindow : Window
    {
        private string[] imageFilePaths;
        private string videoFilePath;
        ImageConv imageConv = new ImageConv();
        AudioConv audioConv = new AudioConv();

        public MainWindow()
        {
            InitializeComponent();
            InitFFmpeg(); // 실행 경로 설정
        }

        private async void InitFFmpeg()
        {
            txtStatus.Text = "FFmpeg 다운로드 확인 중...";
            try
            {
                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);
                FFmpeg.SetExecutablesPath(FFmpeg.ExecutablesPath);
                txtStatus.Text = "FFmpeg 준비 완료";
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"FFmpeg 오류: {ex.Message}";
                MessageBox.Show("FFmpeg 다운로드 또는 설정에 실패했습니다.\n인터넷 연결 확인 또는 수동 설정 필요", "FFmpeg 오류");
            }
        }

        #region 이미지 변환 관련

        private void btnLoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.avif;*.webp;*.tiff;*.bmp;*.jpg;*.jpeg;*.png;*.gif",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                imageFilePaths = openFileDialog.FileNames;
                txtImageStatus.Text = $"{imageFilePaths.Length}개의 파일이 로드되었습니다.";
            }
        }

        private void btnConvertImage_Click(object sender, RoutedEventArgs e)
        {
            if (imageFilePaths == null || imageFilePaths.Length == 0)
            {
                MessageBox.Show("먼저 이미지 파일을 불러오세요.");
                return;
            }

            try
            {
                foreach (var path in imageFilePaths)
                {
                    imageConv.ConvertAvifToPng(path);
                }
                string msg = $"{imageFilePaths.Length}개의 파일이 PNG로 변환되었습니다.";
                txtImageStatus.Text = msg;
                MessageBox.Show(msg);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"이미지 변환 중 오류 발생: {ex.Message}");
            }
        }

        #endregion

        #region 영상 처리 및 Whisper 관련

        private void btnLoadVideo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Video Files|*.mp4;*.mov;*.avi"
            };

            if (ofd.ShowDialog() == true)
            {
                videoFilePath = ofd.FileName;
                txtStatus.Text = $"영상 파일 선택됨: {videoFilePath}";
            }
        }

        private async void btnExtractAudio_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(videoFilePath))
            {
                MessageBox.Show("먼저 영상 파일을 불러오세요.");
                return;
            }

            txtStatus.Text = "M4A 추출 중...";

            try
            {
                progressBar.Value = 0;
                btnExtractAudio.IsEnabled = false;

                var codec = ((ComboBoxItem)cmbCodec.SelectedItem).Content.ToString();
                var ext = ((ComboBoxItem)cmbOutputExt.SelectedItem).Content.ToString();
                int bitrate = int.Parse(((ComboBoxItem)cmbBitrate.SelectedItem).Content.ToString());

                var settings = new AudioSettings
                {
                    Codec = codec,
                    Bitrate = bitrate,
                    Extension = ext
                };

                // 진행률 전달
                string output = await audioConv.ExtractAudioAsync(videoFilePath, settings, percent =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        progressBar.Value = percent;
                        txtStatus.Text = $"진행률: {percent:0.0}%";
                    });
                });

                txtStatus.Text = $"M4A 추출 완료: {output}";
                progressBar.Value = 100;

                // 변환 완료 후
                btnExtractAudio.IsEnabled = true;

                string folder = Path.GetDirectoryName(output);
                if (Directory.Exists(folder))
                {
                    Process.Start("explorer.exe", folder);
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = "오류 발생";
                MessageBox.Show($"오류: {ex.Message}");

                // 변환 완료 후
                btnExtractAudio.IsEnabled = true;
            }
        }

        private async void btnExtractText_Click(object sender, RoutedEventArgs e)
        {
            // 오디오 파일 경로 (M4A)가 존재해야 함.
            var audioPath = Path.ChangeExtension(videoFilePath, ".m4a");
            if (!File.Exists(audioPath))
            {
                MessageBox.Show("먼저 음성 추출을 진행하세요.");
                return;
            }

            string model = ((ComboBoxItem)cmbModel.SelectedItem).Content.ToString();
            bool timestamp = chkTimestamp.IsChecked == true;
            bool speaker = chkSpeaker.IsChecked == true;
            bool markdown = chkMarkdown.IsChecked == true;

            // 파이썬 스크립트의 인자 구성
            var argsBuilder = new StringBuilder();
            argsBuilder.Append($"\"scripts\\whisper_run.py\" \"{audioPath}\" {model}");
            if (timestamp) argsBuilder.Append(" --timestamp");
            if (speaker) argsBuilder.Append(" --speaker");
            if (markdown) argsBuilder.Append(" --markdown");

            string args = argsBuilder.ToString();
            string pythonExe = @"C:\Path\To\python.exe"; // 환경에 맞게 수정

            txtStatus.Text = "로컬 Whisper 실행 중...";
            progressBar.Value = 0;

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
                    Dispatcher.Invoke(() => txtStatus.Text = ea.Data);
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

            txtStatus.Text = "텍스트 추출 완료";

            var transcriptPath = Path.ChangeExtension(audioPath, ".txt");
            if (File.Exists(transcriptPath))
            {
                txtTranscript.Text = File.ReadAllText(transcriptPath);
            }
            else
            {
                txtTranscript.Text = "텍스트 파일을 찾을 수 없습니다.";
            }
        }

        private void btnSaveText_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTranscript.Text))
            {
                MessageBox.Show("저장할 텍스트가 없습니다.");
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "텍스트 파일|*.txt|마크다운 파일|*.md|자막 파일 (SRT)|*.srt",
                FileName = "transcription"
            };

            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, txtTranscript.Text);
                MessageBox.Show("텍스트 저장 완료");
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #endregion
    }
}
