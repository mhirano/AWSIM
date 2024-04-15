using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitAfterAWhile : MonoBehaviour
{
    void Update()
    {
        if (Time.frameCount > 400){
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
        #else
            Application.Quit();//ゲームプレイ終了
        #endif
        }
    }

}
