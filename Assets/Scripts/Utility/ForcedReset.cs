using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Luminosity.IO;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ForcedReset : MonoBehaviour {
    private void Update() {
        // if we have forced a reset ...
        if (InputManager.GetButtonDown("ResetObject")) {
            //... reload the scene
            SceneManager.LoadScene(SceneManager.GetSceneAt(0).name);
        }
    }
}
