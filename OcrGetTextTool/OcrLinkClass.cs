using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;

namespace OcrGetTextTool
{
    /// <summary>
    /// コイツらはOCRエンジンに関係してくる処理をまとめたクラス
    /// </summary>
    public class OcrLinkClass
    {

        /// <summary>
        /// OCRできるように画像データをSoftwareBitmap型に変換
        /// </summary>
        /// <param name="image">左枠に表示されてる画像</param>
        /// <returns></returns>
        public async Task<SoftwareBitmap> ConvertSoftwareBitmap(Image image)
        {
            try
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
            catch(Exception exe)
            {
                throw new Exception(exe.Message, exe);
            }
        }

        /// <summary>
        /// OCRを実行する
        /// </summary>
        /// <param name="sbitmap"></param>
        /// <returns></returns>
        public async Task<OcrResult> RunOcr(SoftwareBitmap sbitmap)
        {
            try
            {
                //OCRを実行する
                OcrEngine engine = OcrEngine.TryCreateFromLanguage(new Windows.Globalization.Language("ja-JP"));
                var result = await engine.RecognizeAsync(sbitmap);
                return result;
            }catch(Exception exe)
            {
                throw new Exception(exe.Message, exe);
            }
        }
    }
}
