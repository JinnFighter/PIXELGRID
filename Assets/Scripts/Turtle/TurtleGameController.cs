﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class TurtleGameController : MonoBehaviour
{
    [SerializeField] private Pixel originalPixel;
    [SerializeField] private Turtle turtle;
    [SerializeField] private InputField routeInputField;

    private int pathsQuantity;
    private string[] paths;
    private int pathsLength;
    private string route;
    private int x;
    private int y;
    private enum directionEnum { UP, LEFT, DOWN, RIGHT };
    private enum commandsEnum { FORWARD, ROTATE_LEFT, ROTATE_RIGHT };
    private readonly Dictionary<int, char> commands = new Dictionary<int, char>
    {
        {(int)commandsEnum.FORWARD, 'F' },
        {(int)commandsEnum.ROTATE_LEFT, '+' },
        {(int)commandsEnum.ROTATE_RIGHT, '-' }
    };
    private List<int>[] commands_history;
    private int look;
    private int cur_action;
    private bool finished;
    private int last_action;
    private int iteration;
    private Vector3 turtle_start_pos;
    private Quaternion turtle_start_rotation;
    private bool gameActive;
    private bool gameStarted;
    private int difficulty;

    // Start is called before the first frame update
    void Start()
    {
        difficulty = GetComponent<GameField>().Difficulty;
        switch(difficulty)
        {
            case 0:
                pathsQuantity = 5;
                pathsLength = 5;
                x = 4;
                y = 4;
                break;
            case 1:
                pathsQuantity = 7;
                pathsLength = 7;
                x = 6;
                y = 6;
                break;
            case 2:
                pathsQuantity = 10;
                pathsLength = 10;
                x = 7;
                y = 7;
                break;
            default:
                pathsQuantity = 5;
                pathsLength = 5;
                x = 4;
                y = 4;
                break;
        }
        paths = new string[pathsQuantity];
        route = "";
        commands_history = new List<int>[pathsQuantity];
        for (var i = 0; i < pathsQuantity; i++)
            commands_history[i] = new List<int>();
        
        look = (int)directionEnum.RIGHT;
        turtle.gameObject.transform.Rotate(0, 0, -90);
        turtle.transform.position = new Vector3(GetComponent<GameField>().grid[x, y].transform.position.x, GetComponent<GameField>().grid[x, y].transform.position.y, turtle.transform.position.z);
        turtle_start_pos = turtle.transform.position;
        turtle_start_rotation = turtle.transform.rotation;
        cur_action = 0;
        last_action = -1;
        finished = false;
        generateStringPaths();

        Vector3 startPos = turtle_start_pos;
        routeInputField.text = paths[iteration];
        Messenger.AddListener(GameEvents.TIMER_STOP, ChangeGameState);
        Messenger.AddListener(GameEvents.PAUSE_GAME, PauseGame);
        Messenger.AddListener(GameEvents.CONTINUE_GAME, ContinueGame);
        Messenger.AddListener(GameEvents.RESTART_GAME, RestartGame);
        GetComponent<GameplayTimer>().Format = GameplayTimer.TimerFormat.smms;
        Messenger.Broadcast(GameEvents.START_GAME);
    }

    void OnDestroy()
    {
        Messenger.RemoveListener(GameEvents.TIMER_STOP, ChangeGameState);
        Messenger.RemoveListener(GameEvents.PAUSE_GAME, PauseGame);
        Messenger.RemoveListener(GameEvents.CONTINUE_GAME, ContinueGame);
        Messenger.RemoveListener(GameEvents.RESTART_GAME, RestartGame);
    }

    public void rotateLeft()
    {
        turtle.transform.Rotate(0, 0, 90);
        switch (look)
        {
            case (int)directionEnum.UP:
                look = (int)directionEnum.LEFT;
                break;
            case (int)directionEnum.LEFT:
                look = (int)directionEnum.DOWN;
                break;
            case (int)directionEnum.DOWN:
                look = (int)directionEnum.RIGHT;
                break;
            case (int)directionEnum.RIGHT:
                look = (int)directionEnum.UP;
                break;
        }
    }
    public void rotateRight()
    {
        turtle.transform.Rotate(0, 0, -90);
        switch (look)
        {
            case (int)directionEnum.UP:
                look = (int)directionEnum.RIGHT;
                break;
            case (int)directionEnum.RIGHT:
                look = (int)directionEnum.DOWN;
                break;
            case (int)directionEnum.DOWN:
                look = (int)directionEnum.LEFT;
                break;
            case (int)directionEnum.LEFT:
                look = (int)directionEnum.UP;
                break;
        }
    }

    public void moveForward()
    {
        var allowMove = true;
        var startPos = turtle.transform.position;
        var posX = startPos.x;
        var posY = startPos.y;
        switch (look)
        {
            case (int)directionEnum.UP:
                if(x == 0)
                {
                    allowMove = false;
                    break;
                }
                x--;
                posY += GetComponent<GameField>().OffsetY;
                break;
            case (int)directionEnum.RIGHT:
                if (y == GetComponent<GameField>().GridCols - 1)
                {
                    allowMove = false;
                    break;
                }
                y++;
                posX += GetComponent<GameField>().OffsetX;
                break;
            case (int)directionEnum.DOWN:
                if (x == GetComponent<GameField>().GridRows - 1)
                {
                    allowMove = false;
                    break;
                }
                x++;
                posY -= GetComponent<GameField>().OffsetY;
                break;
            case (int)directionEnum.LEFT:
                if (y == 0)
                {
                    allowMove = false;
                    break;
                }
                y--;
                posX -= GetComponent<GameField>().OffsetX;
                break;
        }
        if(allowMove)
            turtle.transform.position = new Vector3(posX, posY, startPos.z);
    }

    void executeMoveSequence()
    {
        for(var i = 0;i < route.Length; i++)
        {
           switch(route[i])
            {
                case 'F':
                    moveForward();
                    commands_history[iteration].Add((int)commandsEnum.FORWARD);
                    break;
                case '+':
                    rotateLeft();
                    commands_history[iteration].Add((int)commandsEnum.ROTATE_LEFT);
                    break;
                case '-':
                    rotateRight();
                    commands_history[iteration].Add((int)commandsEnum.ROTATE_RIGHT);
                    break;
            }
        }
        turtle.transform.position = turtle_start_pos;
        turtle.transform.rotation = turtle_start_rotation;
        look = (int)directionEnum.RIGHT;
        switch(difficulty)
        {
            case 0:
                x = 4;
                y = 4;
                break;
            case 1:
                x = 6;
                y = 6;
                break;
            case 2:
                x = 7;
                y = 7;
                break;
            default:
                x = 4;
                y = 4;
                break;
        } 
    }
    public void GameCheck(int action)
    {
        if (!gameActive) return;

        if (!GetComponent<GameplayTimer>().Counting) return;

        if (finished) return;

        last_action = action;
        if(last_action == commands_history[iteration][cur_action])
        {
            Messenger<int>.Broadcast(GameEvents.ACTION_RIGHT_ANSWER,100);
            switch (last_action)
            {
                case (int)commandsEnum.FORWARD:
                    GetComponent<GameField>().grid[x, y].setPixelState(true);
                    moveForward();
                    break;
                case (int)commandsEnum.ROTATE_LEFT:
                    rotateLeft();
                    break;
                case (int)commandsEnum.ROTATE_RIGHT:
                    rotateRight();
                    break;
            }
            cur_action++;
            if (cur_action == commands_history[iteration].Count)
            {
                cur_action = 0;
                iteration++;
                if (iteration == pathsQuantity)
                {
                    GetComponent<GameplayTimer>().StopTimer();
                    Messenger.Broadcast(GameEvents.GAME_OVER);
                }
                else
                    routeInputField.text = paths[iteration];
            }
        }
        else
            Messenger.Broadcast(GameEvents.ACTION_WRONG_ANSWER);
    }

    public void generateStringPaths()
    {
        for(var i = 0;i < pathsQuantity; i++)
        {
            iteration = i;
            for(var j = 0; j < pathsLength; j++)
            { 
                var c = commands[UnityEngine.Random.Range(0, 2)];
                while (j == 0 && c != commands[0])
                   c = commands[UnityEngine.Random.Range(0, 2)];

                route = String.Concat(route, c);
            }
            paths[i] = route;
            executeMoveSequence();
            route = "";
        }
        iteration = 0;
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
            GameplayTimer timer = GetComponent<GameplayTimer>();
            switch (difficulty)
            {
                case 0:
                    timer.StartTime = 60f;
                    break;
                case 1:
                    timer.StartTime = 80f;
                    break;
                case 2:
                    timer.StartTime = 120f;
                    break;
                default:
                    timer.StartTime = 60f;
                    break;
            }
            timer.StartTimer();
        }
        else
            gameActive = false;
    }

    public void RestartGame()
    {
        gameActive = false;
        gameStarted = false;
        GetComponent<GameField>().clearGrid();
        route = "";
        for (var i = 0; i < pathsQuantity; i++)
        {
            commands_history[i].Clear();
            paths[i] = "";
        }
        switch (difficulty)
        {
            case 0:
                x = 4;
                y = 4;
                break;
            case 1:
                x = 6;
                y = 6;
                break;
            case 2:
                x = 7;
                y = 7;
                break;
            default:
                x = 4;
                y = 4;
                break;
        }
        cur_action = 0;
        last_action = -1;
        finished = false;
        iteration = 0;
        look = (int)directionEnum.RIGHT;
        generateStringPaths();
        switch (difficulty)
        {
            case 0:
                x = 4;
                y = 4;
                break;
            case 1:
                x = 6;
                y = 6;
                break;
            case 2:
                x = 7;
                y = 7;
                break;
            default:
                x = 4;
                y = 4;
                break;
        }
        iteration = 0;
        cur_action = 0;
        last_action = -1;
        finished = false;
        look = (int)directionEnum.RIGHT;
        turtle.transform.position = turtle_start_pos;
        turtle.transform.rotation = turtle_start_rotation;
        routeInputField.text = paths[iteration];
        GetComponent<GameplayTimer>().timerText.text = GameplayTimer.TimerFormat.smms_templater_timerText;
        Messenger.Broadcast(GameEvents.START_GAME);
    }

    public void SendStartGameEvent()
    {
        var pfManager = GetComponent<ProfilesManager>();
        if (PlayerPrefs.GetInt(pfManager.ActiveProfile.name + "_" + SceneManager.GetActiveScene().name + "_first_visit") == 1)
        {
            PlayerPrefs.SetInt(pfManager.ActiveProfile.name + "_" + SceneManager.GetActiveScene().name + "_first_visit", 0);
            PlayerPrefs.Save();
            Messenger.Broadcast(GameEvents.START_GAME);
        }
    }
}