﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPixelScript : MonoBehaviour
{
    [SerializeField] public GameObject pixel_empty;
    [SerializeField] public GameObject gameController;
    // Start is called before the first frame update
    void Start()
    {
        //pixel_empty.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void setPixelState(bool state)
    {
        //if(painted!=state)
       // {
            pixel_empty.SetActive(!state);
        //}
        
    }
    public void OnMouseDown()
    {
        //gameController.GetComponent<BrezenheimGameController>().gameCheck(this);
    }
}
