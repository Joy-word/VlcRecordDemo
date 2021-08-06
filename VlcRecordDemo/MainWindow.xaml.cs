using Meta.Vlc.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VlcRecordDemo {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private bool isRecord;

        #region --- Cleanup ---

        protected override void OnClosing(CancelEventArgs e) {
            Player.Dispose();
            ApiManager.ReleaseAll();
            base.OnClosing(e);
        }

        #endregion --- Cleanup ---

        #region --- Events ---

        private void Load_Click(object sender, RoutedEventArgs e) {
            var openfiles = new OpenFileDialog();
            if (openfiles.ShowDialog() == true) {
                Player.Stop();
                Player.LoadMedia(openfiles.FileName);
                Player.Play();
            }
            return;

            String pathString = path.Text;

            Uri uri = null;
            if (!Uri.TryCreate(pathString, UriKind.Absolute, out uri)) return;

            Player.Stop();
            Player.LoadMedia(uri);
            //if you pass a string instead of a Uri, LoadMedia will see if it is an absolute Uri, else will treat it as a file path
            Player.Play();
        }

        private void Play_Click(object sender, RoutedEventArgs e) {
            Thread.Sleep(10000);
            Player.Play();
        }

        private void Pause_Click(object sender, RoutedEventArgs e) {
            Player.PauseOrResume();
        }

        private void Stop_Click(object sender, RoutedEventArgs e) {
            Player.Stop();
        }

        private void Exit_Click(object sender, RoutedEventArgs e) {
            Close(); //closing the main window will also terminate the application
        }

        private void AspectRatio_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (Player == null) return;
            switch ((sender as ComboBox).SelectedIndex) {
                case 0:
                    Player.AspectRatio = AspectRatio.Default;
                    break;

                case 1:
                    Player.AspectRatio = AspectRatio._16_9;
                    break;

                case 2:
                    Player.AspectRatio = AspectRatio._4_3;
                    break;
            }
        }

        private void ProgressBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var value = (float)(e.GetPosition(ProgressBar).X / ProgressBar.ActualWidth);
            ProgressBar.Value = value;
        }

        #endregion --- Events ---

        WriteableBitmap writableBitmap = null;
        private void Player_VideoDisplaying(object sender, VideoDisplayEventArgs e) {

            playImage.Dispatcher.BeginInvoke((Action)(() => {
                var bitmap = (InteropBitmap)Imaging.CreateBitmapSourceFromMemorySection(Player.Context.FileMapping, videoWidth, videoHeight, PixelFormats.Bgr32, videoWidth * PixelFormats.Bgr32.BitsPerPixel / 8, 0);
                playImage.Source = bitmap;
            }));

            playImage2.Dispatcher.BeginInvoke((Action)(() => {
                if (writableBitmap == null || writableBitmap.Width != videoWidth || writableBitmap.Height != videoHeight) {
                    writableBitmap = new System.Windows.Media.Imaging.WriteableBitmap(videoWidth, videoHeight,
                         96, 96, System.Windows.Media.PixelFormats.Bgra32,
                        null);
                }
                //writableBitmap.WritePixels(new System.Windows.Int32Rect(videoWidth / 2 -1, 0, videoWidth / 2, videoHeight / 2), e.Picture, videoWidth * videoHeight * 2, writableBitmap.BackBufferStride, 0, 0);
                writableBitmap.WritePixels(new System.Windows.Int32Rect(0, 0, videoWidth, videoHeight), e.Picture, videoWidth * videoHeight * 4, writableBitmap.BackBufferStride);

                //writableBitmap.Lock();
                //writableBitmap.AddDirtyRect(new System.Windows.Int32Rect(0, 0, videoWidth / 2, videoHeight / 2));
                //writableBitmap.Unlock();
                playImage2.Source = writableBitmap;
            }));

        }

        private void Record_Click(object sender, RoutedEventArgs e) {
            if(Player.State == Meta.Vlc.Interop.Media.MediaState.Playing ) {
                if (isRecord) {
                    isRecord = false;
                    MessageBox.Show("录制结束！");
                    //todo
                }
                else {
                    isRecord = true;
                    MessageBox.Show("开始录制！");
                    //todo
                }
            }
            else {
                MessageBox.Show("没有视频在播放！");
            }
        }

        private int videoWidth;
        private int videoHeight;
        private void Player_VideoFormatChanging(object sender, VideoFormatChangingEventArgs e) {
            videoWidth = (int)e.Width;
            videoHeight = (int)e.Height;

        }

        InteropBitmap interopImage;
    }
}
