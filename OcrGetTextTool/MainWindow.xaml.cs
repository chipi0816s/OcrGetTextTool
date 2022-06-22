using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using BitmapFrame = System.Windows.Media.Imaging.BitmapFrame;

namespace OcrGetTextTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowServiceClass _serviceClass;
        private MainWindwSystem _mainWindwSystem;

        public MainWindow()
        {
            InitializeComponent();
            _serviceClass = new MainWindowServiceClass();
            _mainWindwSystem = new MainWindwSystem();
        }

        /// <summary>
        /// 「参照」ボタンを押したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPath_Click(object sender, RoutedEventArgs e)
        {
            _mainWindwSystem.btnPath(ImgTarget, txtPath);
        }

        /// <summary>
        ///     「OCR実行」ボタンを押したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnOcr_Click(object sender, RoutedEventArgs e)
        {
            _mainWindwSystem.btnOcr(ImgTarget, txtOcrResult);
        }

        /// <summary>
        /// 「クリップボードから貼付」を押したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScreenButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindwSystem.ScreenButton(ImgTarget, txtPath);
        }

        #region 画像拡大。並行移動処理
        // ここは以下サイトをパk...参考にしますた
        //https://ni4muraano.hatenablog.com/entry/2017/10/21/135713

        /// <summary>
        /// マウスホイールでマウスのある位置で画像を拡大
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // スケールの値を変えることでホイールを動かした時の拡大率を制御できます
            const double scale = 1.2;

            var matrix = ImgTarget.RenderTransform.Value;
            if (e.Delta > 0)
            {
                // 拡大処理
                matrix.ScaleAt(scale, scale, e.GetPosition(this).X, e.GetPosition(this).Y);
            }
            else
            {
                // 縮小処理
                matrix.ScaleAt(1.0 / scale, 1.0 / scale, e.GetPosition(this).X, e.GetPosition(this).Y);
            }

            ImgTarget.RenderTransform = new MatrixTransform(matrix);
        }

        // 以下、マウスをドラッグして画像を平行移動させる
        private Point _start;

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ImgTarget.CaptureMouse();
            _start = e.GetPosition(Border1);
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            if (ImgTarget.IsMouseCaptured)
            {
                var matrix = ImgTarget.RenderTransform.Value;

                Vector v = _start - e.GetPosition(Border1);
                matrix.Translate(-v.X, -v.Y);
                ImgTarget.RenderTransform = new MatrixTransform(matrix);
                _start = e.GetPosition(Border1);
            }
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ImgTarget.ReleaseMouseCapture();
        }
        #endregion


        /// <summary>
        /// キーボードショートカットを呼び出す
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //Controlキー押してる最中
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                //Ctrl + O　ファイルから参照
                if (e.Key == Key.O)
                    _mainWindwSystem.btnPath(ImgTarget, txtPath);

                //Ctrl + V　クリップボードから貼付け
                if (e.Key == Key.V)
                    _mainWindwSystem.ScreenButton(ImgTarget, txtPath);

                //Ctrl + Enter   OCR実行
                if (e.Key == Key.Enter)
                    _mainWindwSystem.btnOcr(ImgTarget, txtOcrResult);

                //Ctrl + R　画像の位置を元に戻す
                if (e.Key == Key.R)
                    _serviceClass.ImgePositionReset(ImgTarget);
            }
        }

        /// <summary>
        /// 「画像の位置を元に戻す」を押したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ImgTargetReset_Click(object sender, RoutedEventArgs e)
        {
            _serviceClass. ImgePositionReset(ImgTarget);
        }

        /// <summary>
        /// 「バージョン情報」を押したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void verWindow_Click(object sender, RoutedEventArgs e)
        {
            string verText = _serviceClass.verWindowText();
            MessageBox.Show(verText, "バージョン情報", MessageBoxButton.OK);
        }
    }

    /// <summary>
    /// メインウインドウで呼び出されたイベントから処理を行うクラス
    /// やべぇ！いい感じに各場所がない！！
    /// (汚くなるの覚悟でここに書くしか無い)
    /// </summary>
    public class MainWindwSystem
    {
        private MainWindowServiceClass _serviceClass;
        public MainWindwSystem()
        {
            _serviceClass = new MainWindowServiceClass();
        }

        /// <summary>
        /// エクスプローラを開き、ファイルから画像を開く処理
        /// </summary>
        /// <param name="ImgTarget">OCR実行する画像枠</param>
        /// <param name="txtPath">パス表示欄</param>
        public async void btnPath(System.Windows.Controls.Image ImgTarget, System.Windows.Controls.TextBox txtPath)
        {
            ImgTarget.Source = null;
            txtPath.Text = "";

            await Task.Delay(10);

            //画像ファイルのパスを取得
            txtPath.Text = _serviceClass.SelectPath();

            if (txtPath.Text != "")
            {
                //画像ファイルの読み込み
                ImgTarget.Source = BitmapFrame.Create(new Uri(txtPath.Text, UriKind.Absolute),
                                                                                  BitmapCreateOptions.None,
                                                                                  BitmapCacheOption.OnLoad);
                //位置は初期化する
                _serviceClass.ImgePositionReset(ImgTarget);
            }
        }

        /// <summary>
        /// OCRを実行する
        /// </summary>
        /// <param name="ImgTarget">OCR実行する画像枠</param>
        /// <param name="txtOcrResult">実行結果テキスト欄</param>
        public async void btnOcr(System.Windows.Controls.Image ImgTarget, System.Windows.Controls.TextBox txtOcrResult)
        {
            //左枠の画像がなければOCRは実行しない
            if (ImgTarget.Source == null)
                return;

            OcrLinkClass textTool = new OcrLinkClass();
            try
            {
                //OCRの実行処理
                var sbitmap = await textTool.ConvertSoftwareBitmap(ImgTarget);
                txtOcrResult.Text = (await textTool.RunOcr(sbitmap)).Text;
            }
            catch (Exception exe)
            {
                MessageBox.Show("エラーが発生しました!" + "\r\n" + exe.Message + "\r\n" + exe.InnerException,
                                                "エラー",
                                                MessageBoxButton.OK,
                                                MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// クリップボードの画像を表示する
        /// </summary>
        /// <param name="ImgTarget">OCR実行する画像枠</param>
        /// <param name="txtPath">パス表示欄</param>
        public void ScreenButton(System.Windows.Controls.Image ImgTarget, System.Windows.Controls.TextBox txtPath)
        {
            BitmapSource source = _serviceClass.GetImageFromClipboardWithPNG();
            if (source == null)
            {
                MessageBox.Show("クリップボード上に画像ねーじゃんか！\r\nよく確認しろや",
                                               "警告！！！",
                                               MessageBoxButton.OK,
                                               MessageBoxImage.Exclamation);
            }
            else
            {
                //画像ファイルの読み込み
                ImgTarget.Source = BitmapFrame.Create(source);
                //位置は初期化する
                _serviceClass.ImgePositionReset(ImgTarget);
                txtPath.Text = "クリップボードから貼付";
            }
        }
    }
}
