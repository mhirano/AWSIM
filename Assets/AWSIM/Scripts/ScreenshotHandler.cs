using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(1)]
public class ScreenshotHandler : MonoBehaviour
{
    // カメラの参照
    [SerializeField] Camera cameraToCapture;
    // スクリーンショットのファイル形式
    const string PNG = ".png";
    int fileInd = 0;
    
    string saveDirectoryRootPath;

    void Awake(){
        saveDirectoryRootPath = Application.persistentDataPath + "/" + SceneManager.GetActiveScene().name + "/" + "DEFAULT_DIRECTORY"; /*RENDERING_EXPORT_DIRECTORY*/
        // e.g. /home/mhirano/.config/unity3d/TIERIV/AWSIM/AutowareSimulation/DEFAULT_DIRECTORY 
    }    
    void Update()
    {
        CaptureScreenshotWithAsyncGPUReadback();
    }

    // スクリーンショットを撮影し、保存するメソッド
    public void CaptureScreenshotWithAsyncGPUReadback(int width = 1280, int height = 720)
    {
        // カメラの描画結果を一時的に保存するためのRenderTextureを作成
        var rt = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
        var oldTarget = cameraToCapture.targetTexture;

        // カメラの描画先を一時的に作成したRenderTextureに変更して、レンダリング
        cameraToCapture.targetTexture = rt;
        cameraToCapture.Render();

        // カメラの描画先を元に戻す
        cameraToCapture.targetTexture = oldTarget;

        // GPUからピクセルデータを非同期で読み取る
        AsyncGPUReadback.Request(rt, 0, async request =>
        {
            if (request.hasError)
            {
                // 読み取りにエラーがあった場合はログを出力
                Debug.LogError("AsyncGPUReadbackにエラーが発生しました。");
            }
            else
            {
                // 現在のシーン名を使用してファイルパスを生成
                string path = SceneManager.GetActiveScene().name;

                // リクエストから生のピクセルデータを取得
                var data = request.GetData<Color32>();
                var format = rt.graphicsFormat;

                // 画像を保存するための完全なファイルパスを生成
                var saveFilePath = GetSaveFilePath(path, PNG);

                // 別のスレッドでピクセルデータをPNGにエンコード
                var bytes = await UniTask.RunOnThreadPool(() =>
                {
                    var bytes = ImageConversion.EncodeNativeArrayToPNG(data, format, (uint)width, (uint)height);
                    return bytes;
                });

                // エンコードされたバイトを配列に変換
                var pngBytes = bytes.ToArray();

                // 別のスレッドでPNGデータをファイルに書き込む
                await UniTask.RunOnThreadPool(async () =>
                {
                    using var fs = new FileStream(saveFilePath, FileMode.Create, FileAccess.Write);
                    {
                        await fs.WriteAsync(pngBytes, 0, pngBytes.Length);
                    }
                });

                // 必要ない一時的なRenderTextureを解放
                RenderTexture.ReleaseTemporary(rt);
            }
        });
    }

    // 保存ディレクトリのパスを取得するメソッド
    string GetSaveDirectoryPath(string folderName)
    {
        string directoryPath = saveDirectoryRootPath + "/" + cameraToCapture.name +"/";  // e.g. /home/mhirano/.config/unity3d/TIERIV/AWSIM/AutowareSimulation/

        if (!Directory.Exists(directoryPath))
        {
            //まだ存在してなかったら作成
            Directory.CreateDirectory(directoryPath);
            return directoryPath;
        }

        return directoryPath;
    }

    // 保存先のファイルのパス取得
    string GetSaveFilePath(string folderName, string fileType)
    {
            return GetSaveDirectoryPath(folderName) + (fileInd++).ToString() + fileType;
    }
}
