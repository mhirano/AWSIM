using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using System.Collections;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class VehicleTest
{

    AsyncOperation sceneLoader;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        yield return EditorSceneManager.LoadSceneAsync("NPCDriveTest", LoadSceneMode.Additive);
    }

    [UnityTest]
    public IEnumerator Movement()
    {
        yield return new WaitForSeconds(10);

        LogAssert.Expect(LogType.Log, "Straight");
        
        yield return null;
    }
}
