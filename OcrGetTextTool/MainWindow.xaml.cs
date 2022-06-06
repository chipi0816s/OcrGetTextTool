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
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void btnPath_Click(object sender, RoutedEventArgs e)
        {
            ImgTarget.Source = null;
            txtPath.Text = "";

            await Task.Delay(10);

            //画像ファイルのパスを取得
            txtPath.Text = SelectPath();

            if (txtPath.Text != "")
            {
                //画像ファイルの読み込み
                ImgTarget.Source = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri(txtPath.Text, UriKind.Absolute), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
        }

        private string SelectPath()
        {
            var path = "";

            // ダイアログのインスタンスを生成
            var dialog = new OpenFileDialog();

            // ファイルの種類を設定
            dialog.Filter = "Image File(*.bmp, *.jpg, *.png, *.tif) | *.bmp; *.jpg; *.png; *.tif | Bitmap(*.bmp) | *.bmp | Jpeg(*.jpg) | *.jpg | PNG(*.png) | *.png";

            // ダイアログを表示する
            if (dialog.ShowDialog() == true)
            {
                // 選択されたファイル名を取得
                path = dialog.FileName;
            }

            return path;
        }

        private async void btnOcr_Click(object sender, RoutedEventArgs e)
        {
            //OCRの実行処理
            var sbitmap = await ConvertSoftwareBitmap(ImgTarget);
            txtOcrResult.Text = (await RunOcr(sbitmap)).Text;
        }

        /// <summary>
        /// クリップボードにある画像を表示させる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScreenButton_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource source = GetImageFromClipboardWithPNG();
            if (source == null)
            {
                MessageBox.Show("クリップボード上に画像ねーじゃんか！\r\nよく確認しろや");
            }
            else
            {

                //画像ファイルの読み込み
                ImgTarget.Source = BitmapFrame.Create(source);

            }

        }

        /// <summary>
        /// クリップボードからBitmapSourceを取り出して返す、PNG(アルファ値保持)形式に対応
        /// </summary>
        /// <returns></returns>
        private BitmapSource GetImageFromClipboardWithPNG()
        {
            BitmapSource source = null;
            //クリップボードにPNG形式のデータがあったら、それを使ってBitmapFrame作成して返す
            //なければ普通にClipboardのGetImage、それでもなければnullを返す
            using var ms = (System.IO.MemoryStream)Clipboard.GetData("PNG");
            if (ms != null)
            {
                //source = BitmapFrame.Create(ms);//これだと取得できない
                source = BitmapFrame.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
            else if (Clipboard.ContainsImage())
            {
                source = Clipboard.GetImage();
            }
            return source;
        }

        #region いずれここの部分は外部dllに移動させたい
        private async Task<SoftwareBitmap> ConvertSoftwareBitmap(Image image)
        {
            SoftwareBitmap sbitmap = null;

            using (MemoryStream stream = new MemoryStream())
            {
                //BmpBitmapEncoderに画像を書きこむ
                var encoder = new BmpBitmapEncoder();
                encoder.Frames.Add((System.Windows.Media.Imaging.BitmapFrame)image.Source);
                encoder.Save(stream);

                //メモリストリームを変換
                var irstream = WindowsRuntimeStreamExtensions.AsRandomAccessStream(stream);

                //画像データをSoftwareBitmapに変換
                var decorder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(irstream);
                sbitmap = await decorder.GetSoftwareBitmapAsync();
            }

            return sbitmap;
        }

        private async Task<OcrResult> RunOcr(SoftwareBitmap sbitmap)
        {
            //OCRを実行する
            OcrEngine engine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("ja-JP"));
            var result = await engine.RecognizeAsync(sbitmap);
            return result;
        }
        #endregion  
    }
}
