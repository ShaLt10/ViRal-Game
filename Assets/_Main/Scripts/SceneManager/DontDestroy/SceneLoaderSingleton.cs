using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoaderSingleton : Singleton<SceneLoaderSingleton>
{
    public void LoadSceneMode(int scene)
    {
        SceneFlow.LoadScene(scene);
    }
}
