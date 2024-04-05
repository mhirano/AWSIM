using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitAfterAWhile : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(DelayCoroutine());
    }

    private IEnumerator DelayCoroutine()
    {
        // 100フレーム待つ
        for (var i = 0; i < 400; i++)
        {
            yield return null;
        }
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
        #else
            Application.Quit();//ゲームプレイ終了
        #endif
    }

}
