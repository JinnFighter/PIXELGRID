﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class SceneLoader : MonoBehaviour
{
    [SerializeField] public GameObject loadingImageObject;
    [SerializeField] public Animation loadingAnim;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator loadSceneRoutine(string name)
    {
        loadingImageObject.SetActive(true);
        loadingAnim.Play("loadingPixels");
        string curScene = SceneManager.GetActiveScene().name;
        AsyncOperation sceneLoader = SceneManager.LoadSceneAsync(name);
        sceneLoader.allowSceneActivation = false;
        while(!sceneLoader.isDone)
        {
            
        
            if (sceneLoader.progress == 0.9f)
            {
                loadingAnim.Stop("loadingPixels");
                //loadingImageObject.SetActive(false);
                sceneLoader.allowSceneActivation = true;
            }
            yield return null;
        }
        AsyncOperation sceneUnloader = SceneManager.UnloadSceneAsync(curScene);
        while (!sceneUnloader.isDone)
        {
            yield return null;
        }
        //loadingImageObject.SetActive(false);
        //yield return sceneLoader;
    }
    public void LoadScene(string name)
    {
        // loadingImage.SetActive(true);
        
        
        StartCoroutine(loadSceneRoutine(name));
        //loadingImageObject.SetActive(false);
        // loadingImage.SetActive(false);
    }
}
