﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class SouthCohenController : MonoBehaviour
{
    private bool gameActive;
    private bool gameStarted;
    private int difficulty;
    private GridPixelScript[] borderPoints;
    private int linesQuantity;
    private GridPixelScript[,] lines;
    private int iteration;
    private List<int>[] lineZones;
    private int borderWidth;
    private int borderHeight;
    [SerializeField] private SpriteRenderer border;
    private int[,] gridCodes;
    private int gridCodesWidth;
    private int gridCodesHeight;
    private int maxLineLength;
    private int minLineLength;
    // Start is called before the first frame update
    void Start()
    {
        difficulty = GetComponent<GameField>().Difficulty;
        switch(difficulty)
        {
            case 0:
                linesQuantity = 5;
                borderWidth = 5;
                borderHeight = 5;
                maxLineLength = 8;
                minLineLength = 5;
                break;
            case 1:
                linesQuantity = 7;
                borderWidth = 7;
                borderHeight = 7;
                maxLineLength = 10;
                minLineLength = 8;
                break;
            case 2:
                linesQuantity = 10;
                borderWidth = 10;
                borderHeight = 10;
                maxLineLength = 11;
                minLineLength = 10;
                break;
            default:
                linesQuantity = 5;
                borderWidth = 5;
                borderHeight = 5;
                maxLineLength = 8;
                minLineLength = 5;
                break;

        }
        gameActive = false;
        gameStarted = false;
        //linesQuantity =1;
        lineZones = new List<int>[linesQuantity];
        for(int i = 0;i<linesQuantity;i++)
        {
            lineZones[i] = new List<int>();
        }
        lines = new GridPixelScript[2, linesQuantity];
        
        borderPoints = new GridPixelScript[2];
        GameField gameField = gameObject.GetComponent<GameField>();
        borderPoints[0] = gameField.grid[3, 3];
        borderPoints[1] = gameField.grid[7, 7];
        Vector3 pos = borderPoints[0].transform.position;
        pos.x += 12.5f;
        pos.y -= 12.5f;
        pos.z = border.transform.position.z;
        border.transform.position = pos;
        Vector3 scale = border.transform.localScale;
        scale.x = (scale.x) * 10;
        scale.y = (scale.y) * 10;
        border.transform.localScale = scale;
        GenerateLines();
        
        gridCodesWidth = gameField.GridCols;
        gridCodesHeight = gameField.GridRows;
        gridCodes = new int[gridCodesHeight, gridCodesWidth];
        for(int i=0;i<gridCodesHeight;i++)
        {
            for(int j=0;j<gridCodesWidth;j++)
            {
                gridCodes[i, j] = this.Code(gameField.grid[i, j], borderPoints[0], borderPoints[1]);
            }
        }
        //border.transform.localScale.Set(border.transform.localScale.x + borderWidth, border.transform.localScale.y + borderHeight, border.transform.localScale.z);
        //border.sprite.rect.size.Set(border.sprite.rect.size.x*borderWidth, border.sprite.rect.size.y*borderHeight);
        //gameObject.GetComponent<Algorithms>().southCohen(gameField.grid[0, 0], gameField.grid[9, 9],
        for (int i=0;i<linesQuantity;i++)
        {
            southCohen(lines[0, i], lines[1, i],
           borderPoints[0], borderPoints[1],i);
        }
        gameField.clearGrid();
        iteration = 0;
        GetComponent<Algorithms>().drawLine(lines[0, 0].Y,lines[0,0].X, lines[1, 0].Y,lines[1,0].X);
        Messenger<GridPixelScript>.AddListener(GameEvents.GAME_CHECK, gameCheck);
        Messenger.AddListener(GameEvents.TIMER_STOP, ChangeGameState);
        Messenger.AddListener(GameEvents.PAUSE_GAME, PauseGame);
        Messenger.AddListener(GameEvents.CONTINUE_GAME, ContinueGame);
        Messenger.AddListener(GameEvents.RESTART_GAME, RestartGame);
        
        
        GetComponent<GameplayTimer>().Format = GameplayTimer.TimerFormat.smms;

        if (PlayerPrefs.GetInt(GetComponent<ProfilesManager>().ActiveProfile.name + "_" + SceneManager.GetActiveScene().name + "_first_visit") == 0)
        {
            Messenger.Broadcast(GameEvents.START_GAME);
        }
        //Messenger.Broadcast(GameEvents.START_GAME);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnDestroy()
    {
        Messenger<GridPixelScript>.RemoveListener(GameEvents.GAME_CHECK, gameCheck);
        Messenger.RemoveListener(GameEvents.TIMER_STOP, ChangeGameState);
        Messenger.RemoveListener(GameEvents.PAUSE_GAME, PauseGame);
        Messenger.RemoveListener(GameEvents.CONTINUE_GAME, ContinueGame);
        Messenger.RemoveListener(GameEvents.RESTART_GAME, RestartGame);
    }
    public void gameCheck(GridPixelScript invoker)
    {
        if (!gameActive)
        {
            return;
        }
        if (!GetComponent<GameplayTimer>().Counting)
        {
            Debug.Log("Not Counting due to finish or no start");
            return;
        }
        
        if(iteration==linesQuantity)
        {
            Messenger.Broadcast(GameEvents.GAME_OVER);
            return;
        }
        bool check = false;
        int c=-1;
        Debug.Log("Invoker"+this.Code(invoker, borderPoints[0], borderPoints[1]));
        foreach (int a in lineZones[iteration])
        {
            if(a==this.Code(invoker, borderPoints[0], borderPoints[1]))
            {
                
                check = true;
                Debug.Log("Found Code:"+a);
                c = a;
                //lineZones[iteration].Remove(a);
                break;
            }
        }
        if(check)
        {
            Debug.Log("Correct");
            ClearZone(c);
            lineZones[iteration].Remove(c);
            Messenger<int>.Broadcast(GameEvents.ACTION_RIGHT_ANSWER, 100);
           
            if (lineZones[iteration].Count == 0)
            {
                iteration++;
            }
            else
            {
                return;
            }
            if (iteration == linesQuantity)
            {
                GetComponent<GameplayTimer>().StopTimer();
                Messenger.Broadcast(GameEvents.GAME_OVER);
            }
            else
            {
                GetComponent<GameField>().clearGrid();
                GetComponent<Algorithms>().drawLine(lines[0, iteration].Y, lines[0, iteration].X, 
                    lines[1, iteration].Y, lines[1, iteration].X);
            }
        }
        else
        {
            Debug.Log("Wrong");
            Messenger.Broadcast(GameEvents.ACTION_WRONG_ANSWER);
        }
       // GetComponent<Algorithms>().Code(invoker, borderPoints[0], borderPoints[1]);
        //invoker.setPixelState(true);
    }
    public void GenerateLines()
    {
        for(int i=0;i<linesQuantity;i++)
        {
            int firstX = UnityEngine.Random.Range(0, 9);
            int firstY = UnityEngine.Random.Range(0, 9);

            int secondX = UnityEngine.Random.Range(0, 9);
            int secondY = UnityEngine.Random.Range(0, 9);
             while ((Math.Sqrt((secondX - firstX) * (secondX - firstX) + (secondY - firstY) * (secondY - firstY)) > maxLineLength
               || Math.Sqrt((secondX - firstX) * (secondX - firstX) + (secondY - firstY) * (secondY - firstY)) < minLineLength)
               ||(!CheckIntersection(firstX,firstY,secondX,secondY)))
                    {
                        firstX = UnityEngine.Random.Range(0, 9);
                        firstY = UnityEngine.Random.Range(0, 9);

                        secondX = UnityEngine.Random.Range(0, 9);
                        secondY = UnityEngine.Random.Range(0, 9);
                    }
            lines[0, i] = GetComponent<GameField>().grid[firstY, firstX];
            lines[1, i] = GetComponent<GameField>().grid[secondY, secondX];
        }
    }
    public void southCohen(GridPixelScript nA, GridPixelScript nB, GridPixelScript rectLeft, GridPixelScript rectRight,int i)
    {
        //Point A = new Point();
        //A = nA;
        //Point B = new Point();
        //B = nB;
       
        GridPixelScript A = nA;
        GridPixelScript B = nB;
        int ax = A.X;
        int ay = A.Y;
        int bx = B.X;
        int by = B.Y;
       int code1 = this.Code(A, rectLeft, rectRight);
        int code2 = this.Code(B, rectLeft, rectRight);
        bool inside = (code1 | code2) == 0;
        bool outside = (code1 & code2) != 0;
        while (!inside && !outside)
        {
            if (code1 == 0)
            {
                //Swap;
                //GetComponent<Algorithms>().Swap(ref A, ref B);
                GetComponent<Algorithms>().Swap(ref ax,ref bx);
                GetComponent<Algorithms>().Swap(ref ay,ref by);
                int c = code1;
                code1 = code2;
                code2 = c;
            }
            if (Convert.ToBoolean(code1 & 0x01))
            {
                //A.Y += (rectLeft.X - A.X) * (B.Y - A.Y) / (B.X - A.X);
                //A.X = rectLeft.X;
                ay += (rectLeft.X - ax) * (by - ay) / (bx - ax);
                ax = rectLeft.X;
                if(!lineZones[i].Contains(code1))
                    lineZones[i].Add(code1);
            }
            if (Convert.ToBoolean(code1 & 0x02))
            {
                //A.X += (rectLeft.Y - A.Y) * (B.X - A.X) / (B.Y - A.Y);
                //A.Y = rectLeft.Y;
                ax += (rectLeft.Y - ay) * (bx - ax) / (by - ay);
                ay = rectLeft.Y;
                if (!lineZones[i].Contains(code1))
                    lineZones[i].Add(code1);
            }
            if (Convert.ToBoolean(code1 & 0x04))
            {
                //A.Y += (rectRight.X - A.X) * (B.Y - A.Y) / (B.X - A.X);
                //A.X = rectRight.X;
                ay += (rectRight.X - ax) * (by - ay) / (bx - ax);
                ax = rectRight.X;
                if (!lineZones[i].Contains(code1))
                    lineZones[i].Add(code1);
            }
            if (Convert.ToBoolean(code1 & 0x08))
            {
                //A.X += (rectRight.Y - A.Y) * (B.X - A.X) / (B.Y - A.Y);
                //A.Y = rectRight.Y;
                ax += (rectRight.Y - ay) * (bx - ax) / (by - ay);
                ay = rectRight.Y;
                if (!lineZones[i].Contains(code1))
                    lineZones[i].Add(code1);
            }

            //code1 = Code(A, rectLeft, rectRight);
            code1 = Code(GetComponent<GameField>().grid[ax,ay], rectLeft, rectRight);
            inside = (code1 | code2) == 0;
            outside = (code1 & code2) != 0;

        }
        if (!outside)
            //drawLine(A, B, Color.Blue);
            //GetComponent<Algorithms>().drawLine(A.X, A.Y, B.X, B.Y);
            GetComponent<Algorithms>().drawLine(ax, ay, bx, by);

    }
    public int Code(GridPixelScript point, GridPixelScript rectLeft, GridPixelScript rectRight)
    {
        int code = 0;
        if (point.X < rectLeft.X) code |= 0x01;//_ _ _ 1;
        if (point.X > rectRight.X) code |= 0x04;//_ 1 _ _;
        if (point.Y < rectLeft.Y) code |= 0x02;//_ _ 1 _;
        if (point.Y > rectRight.Y) code |= 0x08;//1 _ _ _;
        return code;
    }
    public void ClearZone(int code)
    {
        for(int i=0;i<gridCodesHeight;i++)
        {
            for(int j=0;j<gridCodesWidth;j++)
            {
                if(gridCodes[i,j]==code)
                {
                    if(!GetComponent<GameField>().grid[i,j].pixel_empty.activeSelf)
                    {
                        GetComponent<GameField>().grid[i, j].setPixelState(false);
                    }
                }
            }
        }
    }
    public void PauseGame()
    {
        if(gameStarted)
        {
            gameActive = false;
            GetComponent<GameplayTimer>().PauseTimer();
        }
        
    }
    public void ContinueGame()
    {
        if(gameStarted)
        {
            gameActive = true;
            GetComponent<GameplayTimer>().ResumeTimer();
        }
        
    }
    public void ChangeGameState()
    {
        if (!gameStarted)
        {
            gameActive = true;
            gameStarted = true;
            switch (difficulty)
            {
                case 0:
                    GetComponent<GameplayTimer>().StartTime = 60f;
                    break;
                case 1:
                    GetComponent<GameplayTimer>().StartTime = 80f;
                    break;
                case 2:
                    GetComponent<GameplayTimer>().StartTime = 120f;
                    break;
                default:
                    GetComponent<GameplayTimer>().StartTime = 60f;
                    break;
            }
            //GetComponent<GameplayTimer>().StartTime = 60f;
            GetComponent<GameplayTimer>().StartTimer();
        }
        else
        {
            gameActive = false;
        }
    }
    public void RestartGame()
    {
        gameActive = false;
        gameStarted = false;
        GetComponent<GameField>().clearGrid();
        for (int i = 0; i < linesQuantity; i++)
        {
            lineZones[i].Clear();
        }
        iteration = 0;
        GenerateLines();
        for (int i = 0; i < linesQuantity; i++)
        {
            southCohen(lines[0, i], lines[1, i],
           borderPoints[0], borderPoints[1], i);
        }
        GetComponent<GameField>().clearGrid();
        iteration = 0;
        GetComponent<Algorithms>().drawLine(lines[0, 0].Y, lines[0, 0].X, lines[1, 0].Y, lines[1, 0].X);

        GetComponent<GameplayTimer>().timerText.text = GameplayTimer.TimerFormat.smms_templater_timerText;
        Messenger.Broadcast(GameEvents.START_GAME);
    }
    public bool CheckIntersection(int Ax,int Ay,int Bx, int By)
    {
        int ax = Ax;
        int ay = Ay;
        int bx = Bx;
        int by = By;
        if(ax>bx)
        {
            GetComponent<Algorithms>().swap(ax, bx);
            GetComponent<Algorithms>().swap(ay, by);
        }
        int[,] matr = new int[2, 2];

        matr[0, 0] = ax.CompareTo(borderPoints[0].X)+ax.CompareTo(borderPoints[1].X);
        matr[0, 1] = ay.CompareTo(borderPoints[0].Y)+ay.CompareTo(borderPoints[1].Y);
        matr[1, 0] = bx.CompareTo(borderPoints[0].X)+bx.CompareTo(borderPoints[1].X);
        matr[1, 1] = by.CompareTo(borderPoints[0].Y)+by.CompareTo(borderPoints[1].Y);
        int checker = matr[0, 0];
        if((checker==matr[0,1])&&(checker==matr[1,0])&&(checker==matr[1,1]))
        {
            return false;
        }
        else
        {
            int res = (matr[0, 0] * matr[1, 1]) - (matr[1, 0] * matr[0, 1]);
            if(res==0)
            {
                Debug.Log("chosen opred: "+res);
                return true;
            }
            else
            {
                return false;
            }
        }
        
    }
    public void SendStartGameEvent()
    {
        if (PlayerPrefs.GetInt(GetComponent<ProfilesManager>().ActiveProfile.name + "_" + SceneManager.GetActiveScene().name + "_first_visit") == 1)
        {
            PlayerPrefs.SetInt(GetComponent<ProfilesManager>().ActiveProfile.name + "_" + SceneManager.GetActiveScene().name + "_first_visit", 0);
            PlayerPrefs.Save();
            Messenger.Broadcast(GameEvents.START_GAME);
        }

    }
}
