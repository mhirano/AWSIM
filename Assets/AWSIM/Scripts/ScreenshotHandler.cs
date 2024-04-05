using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class ScreenshotHandler : MonoBehaviour
{
    // カメラの参照
    [SerializeField] Camera cameraToCapture;
    // 保存するスクリーンショットの上限数
    const int UPPER_LIMIT_SAVE_PICTURE = 100;
    // スクリーンショットのファイル形式
    const string PNG = ".png";
    // static int fileInd_camera1 = 0;
    // static int fileInd_camera2 = 0;
    // static int fileInd_camera3 = 0;
    int fileInd = 0;
    
    string saveDirectoryRootPath;

    void Awake(){
        // string directoryPath = Application.persistentDataPath + "/" + folderName + "/" + DateTime.Now.ToString("yyyyMMddHHmmss") + "/" + cameraToCapture.name +"/";  // e.g. /home/mhirano/.config/unity3d/TIERIV/AWSIM/AutowareSimulation/
        saveDirectoryRootPath = Application.persistentDataPath + "/" + SceneManager.GetActiveScene().name + "/" + DateTime.Now.ToString("yyyyMMddHHmmss");  // e.g. /home/mhirano/.config/unity3d/TIERIV/AWSIM/AutowareSimulation/
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

                // // 保存ディレクトリからファイルパスのリストを取得
                // List<string> imageFilePaths = GetAllFileFromDirectory(GetSaveDirectoryPath(path));

                // // ファイル数が上限に達していた場合、最も古いファイルを削除
                // if (imageFilePaths.Count >= UPPER_LIMIT_SAVE_PICTURE)
                // {
                //     File.Delete(imageFilePaths[0]);
                // }

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

    // "ディレクトリ配下のファイル"が全て入ったリストを返す
    // 最も古いファイルが[0]番目
    List<string> GetAllFileFromDirectory(string directoryName)
    {
        //古いものが先頭にくるようにファイルをソート
        List<string> imageFilePathList = Directory
                                            //Imageディレクトリ内の全ファイルを取得
                                            .GetFiles(directoryName, "*", SearchOption.AllDirectories)
                                            //.DS_Storeは除く
                                            .Where(filePath => Path.GetFileName(filePath) != ".DS_Store")
                                            //日付順に降順でソート
                                            .OrderBy(filePath => File.GetLastWriteTime(filePath).Date)
                                            //同じ日付内で時刻順に降順でソート
                                            .ThenBy(filePath => File.GetLastWriteTime(filePath).TimeOfDay)
                                            .ToList();

        return imageFilePathList;
    }

    // 保存ディレクトリのパスを取得するメソッド
    string GetSaveDirectoryPath(string folderName)
    {
        // string directoryPath = Application.persistentDataPath + "/" + folderName + "/" + DateTime.Now.ToString("yyyyMMddHHmmss") + "/" + cameraToCapture.name +"/";  // e.g. /home/mhirano/.config/unity3d/TIERIV/AWSIM/AutowareSimulation/
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
        // if(cameraToCapture.name.Equals("CameraToCapture1")){
        //     return GetSaveDirectoryPath(folderName) + (fileInd_camera1++).ToString() + fileType;
        // } else if (cameraToCapture.name.Equals("CameraToCapture2")){
        //     return GetSaveDirectoryPath(folderName) + (fileInd_camera2++).ToString() + fileType;
        // } else if (cameraToCapture.name.Equals("CameraToCapture3")) {
        //     return GetSaveDirectoryPath(folderName) + (fileInd_camera3++).ToString() + fileType;
        // }
        // return GetSaveDirectoryPath(folderName) + "" + fileType;
            return GetSaveDirectoryPath(folderName) + (fileInd++).ToString() + fileType;
    }
}
