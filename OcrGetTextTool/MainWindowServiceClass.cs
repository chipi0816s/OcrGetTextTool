using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace OcrGetTextTool
{
    /// <summary>
    /// こいつはMainWindowクラスに置いてても邪魔な処理を置いておく補助クラス！
    /// 主にクリップボード貼付、ダイアログから写真参照の処理を置いとく
    /// </summary>
    public class MainWindowServiceClass
    {

        

        /// <summary>
        /// ダイアログから画像を取得する処理
        /// </summary>
        /// <returns></returns>
        public string SelectPath()
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

        /// <summary>
        /// クリップボードからBitmapSourceを取り出して返す、PNG(アルファ値保持)形式に対応
        /// </summary>
        /// <returns></returns>
        public BitmapSource GetImageFromClipboardWithPNG()
        {
            BitmapSource source = null;
            //クリップボードにPNG形式のデータがあったら、それを使ってBitmapFrame作成して返す
            //なければ普通にClipboardのGetImage、それでもなければnullを返す
            using var ms = (MemoryStream)Clipboard.GetData("PNG");
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

        /// <summary>
        /// ぐりぐりと動かした画像を初期値に戻す
        /// </summary>
        /// <param name="ImgTarget">初期値に戻したい画像</param>
        public void ImgePositionReset(System.Windows.Controls.Image ImgTarget)
        {
            var matrix = ImgTarget.RenderTransform.Value;
            matrix.M11 = 1.0;
            matrix.M12 = 0.0;
            matrix.M21 = 0.0;
            matrix.M22 = 1.0;
            matrix.OffsetX = 0.0;
            matrix.OffsetY = 0.0;
            ImgTarget.RenderTransform = new System.Windows.Media.MatrixTransform(matrix);
        }


        /// <summary>
        /// バージョン情報ポップアップWindowに表示するテキストを作成する
        /// </summary>
        /// <returns>バージョン情報テキスト</returns>
        public string verWindowText()
        {
            string verText = "[文字認識ソフト]かみまみた！\r\n\r\n" +
                         "バージョン:   ";

            //自分自身のAssemblyを取得
            System.Reflection.Assembly asm =
                System.Reflection.Assembly.GetExecutingAssembly();
            //アセンブリバージョンの取得
            System.Version ver = asm.GetName().Version;
            verText += ver;

            return verText;
        }
    }
}
