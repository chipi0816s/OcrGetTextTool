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

        public MainWindow()
        {
            InitializeComponent();
            _serviceClass = new MainWindowServiceClass();
        }

        /// <summary>
        /// 「参照」ボタンを押したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnPath_Click(object sender, RoutedEventArgs e)
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
            }
        }

        /// <summary>
        ///     「OCR実行」ボタンを押したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnOcr_Click(object sender, RoutedEventArgs e)
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
            }catch(Exception exe)
            {
                MessageBox.Show("エラーが発生しました!"+"\r\n" + exe.Message+"\r\n" + exe.InnerException,
                                                "エラー", 
                                                MessageBoxButton.OK,
                                                MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 「クリップボードから貼付」を押したとき
        /// クリップボードの画像を表示する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScreenButton_Click(object sender, RoutedEventArgs e)
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
                txtPath.Text = "クリップボードから貼付";
            }
        }

        /// <summary>
        /// 「バージョン情報」を押したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void verWindow_Click(object sender, RoutedEventArgs e)
        {
            string verText = "[文字認識ソフト]かみまみた！\r\n\r\n" +
                                     "バージョン:   ";

            //自分自身のAssemblyを取得
            System.Reflection.Assembly asm =
                System.Reflection.Assembly.GetExecutingAssembly();
            //アセンブリバージョンの取得
            System.Version ver = asm.GetName().Version;
            verText += ver;


            MessageBox.Show(verText, "バージョン情報", MessageBoxButton.OK);
        }
    }
}
