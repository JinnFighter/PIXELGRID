﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GameplayTimer : MonoBehaviour
{
    public class TimerFormat
    {
        public static string s = "{0:0}";
        public static string smms = "{0:00}:{1:00}:{2:000}";
    }
    private float startTime=10f;
    private float currentTime=0f;
    private bool counting;
    private float timeLeft;
    [SerializeField] public Text timerText;
    private string format;

    public bool Counting { get => counting; set => counting = value; }
    public float TimeLeft { get => timeLeft; set => timeLeft = value; }
    public float StartTime { get => startTime; set => startTime = value; }
    public string Format { get => format; set => format = value; }

    // Start is called before the first frame update
    void Start()
    {
        currentTime = StartTime;
        TimeLeft = StartTime;
        Counting = false;
        Messenger.AddListener(GameEvents.START_GAME,GameStarter);
    }

    // Update is called once per frame
    void Update()
    {
        if(Counting)
        {
            currentTime -= Time.deltaTime;
            if( Format == TimerFormat.s)
            {
                timerText.text = String.Format(Format,(int)(currentTime % 60));
            }
            else
            {
               // timerText.text = String.Format("{0:00}:{1:00}:{2:000}", (int)(currentTime / 60f) % 60,
                   // (int)(currentTime % 60), (int)(currentTime * 1000f) % 1000);
                timerText.text = String.Format(Format, (int)(currentTime / 60f) % 60, 
                    (int)(currentTime % 60), (int)(currentTime * 1000f) % 1000);
            }
            //timerText.text = currentTime.ToString("0");
            if (currentTime <= 0.0000f)
            {
                currentTime = 0.0000f;
                if (Format == TimerFormat.s)
                {
                    timerText.text = String.Format(Format, (int)(currentTime % 60));
                }
                else
                {
                    timerText.text = String.Format(Format, (int)(currentTime / 60f) % 60,
                        (int)(currentTime % 60), (int)(currentTime * 1000f) % 1000);
                }
                
                Counting = false;
                Messenger.Broadcast(GameEvents.TIMER_STOP);
            }
        }
        
    }
    public void StartTimer()
    {
        currentTime = StartTime;
        Counting = true;
    }
    public void StopTimer()
    {
        Counting = false;
        TimeLeft = currentTime;
    }
    public void PauseTimer()
    {
        Counting = false;
    }
    public void ResumeTimer()
    {
        Counting = true;
    }
    public void GameStarter()
    {
        GameField checker = GetComponent<GameField>();
        if(checker==null)
        {
            Destroy(checker);
            StartTimer();
        }
    }
}
