﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class SouthCohenGameMode : GameMode
{
    private Pixel[] borderPoints;
    private int linesQuantity;
    private Pixel[,] lines;
    private int iteration;
    private List<int>[] lineZones;
    private SpriteRenderer border;
    private int[,] gridCodes;
    private int gridCodesWidth;
    private int gridCodesHeight;
    private int maxLineLength;
    private int minLineLength;
    private GameField _gameField;

    public SouthCohenGameMode(GameplayTimer timer, SpriteRenderer border, GameField field, int difficulty) : base(difficulty)
    {
        _gameField = field;
        difficulty = _gameField.Difficulty;
        this.border = border;
        borderPoints = new Pixel[2];
        Vector3 pos;
        Vector3 scale;
        switch (difficulty)
        {
            case 0:
                linesQuantity = 5;
                maxLineLength = 8;
                minLineLength = 5;
                borderPoints[0] = _gameField.grid[3, 3];
                borderPoints[1] = _gameField.grid[7, 7];
                pos = borderPoints[0].transform.position;
                pos.x += 12.5f;
                pos.y -= 12.5f;
                pos.z = border.transform.position.z;
                border.transform.position = pos;
                scale = border.transform.localScale;
                scale.x *= 10;
                scale.y *= 10;
                border.transform.localScale = scale;
                break;
            case 1:
                linesQuantity = 7;
                maxLineLength = 10;
                minLineLength = 8;
                borderPoints[0] = _gameField.grid[2, 2];
                borderPoints[1] = _gameField.grid[8, 8];
                pos = borderPoints[0].transform.position;
                pos.x += 9.5f;
                pos.y -= 9.5f;
                pos.z = border.transform.position.z;
                border.transform.position = pos;
                scale = border.transform.localScale;
                scale.x *= 7.5f;
                scale.y *= 7.5f;
                border.transform.localScale = scale;
                break;
            case 2:
                linesQuantity = 10;
                maxLineLength = 11;
                minLineLength = 10;
                borderPoints[0] = _gameField.grid[2, 2];
                borderPoints[1] = _gameField.grid[11, 11];
                pos = borderPoints[0].transform.position;
                pos.x += 14.5f;
                pos.y -= 14.5f;
                pos.z = border.transform.position.z;
                border.transform.position = pos;
                scale = border.transform.localScale;
                scale.x *= 10;
                scale.y *= 10;
                border.transform.localScale = scale;
                break;
            default:
                linesQuantity = 5;
                maxLineLength = 8;
                minLineLength = 5;
                borderPoints[0] = _gameField.grid[3, 3];
                borderPoints[1] = _gameField.grid[7, 7];
                pos = borderPoints[0].transform.position;
                pos.x += 12.5f;
                pos.y -= 12.5f;
                pos.z = border.transform.position.z;
                border.transform.position = pos;
                scale = border.transform.localScale;
                scale.x *= 10;
                scale.y *= 10;
                border.transform.localScale = scale;
                break;

        }
        gameActive = false;
        gameStarted = false;
        lineZones = new List<int>[linesQuantity];
        for (var i = 0; i < linesQuantity; i++)
        {
            lineZones[i] = new List<int>();
        }
        lines = new Pixel[2, linesQuantity];

        GenerateLines();

        gridCodesWidth = _gameField.Width;
        gridCodesHeight = _gameField.Height;
        gridCodes = new int[gridCodesHeight, gridCodesWidth];
        for (var i = 0; i < gridCodesHeight; i++)
        {
            for (var j = 0; j < gridCodesWidth; j++)
            {
                gridCodes[i, j] = this.Code(_gameField.grid[i, j], borderPoints[0], borderPoints[1]);
            }
        }

        for (var i = 0; i < linesQuantity; i++)
        {
            southCohen(lines[0, i], lines[1, i],
           borderPoints[0], borderPoints[1], i);
        }

        iteration = 0;

        var linePts = Algorithms.GetBrezenheimLineData(new Line(new Position(lines[0, 0].Y, lines[0, 0].X), new Position(lines[1, 0].Y, lines[1, 0].X)), out _);
        _gameField.Draw(linePts);

        eventReactor = new DefaultReactor(timer, difficulty);

        Messenger.Broadcast(GameEvents.START_GAME);
    }

    ~SouthCohenGameMode()
    {
        Messenger<Pixel>.RemoveListener(GameEvents.GAME_CHECK, CheckAction);
        Messenger.RemoveListener(GameEvents.TIMER_STOP, ChangeGameState);
        Messenger.RemoveListener(GameEvents.PAUSE_GAME, Pause);
        Messenger.RemoveListener(GameEvents.CONTINUE_GAME, Continue);
        Messenger.RemoveListener(GameEvents.RESTART_GAME, Restart);
    }

    public void southCohen(Pixel nA, Pixel nB, Pixel rectLeft, Pixel rectRight, int i)
    {
        var A = nA;
        var B = nB;
        var ax = A.X;
        var ay = A.Y;
        var bx = B.X;
        var by = B.Y;
        var code1 = Code(A, rectLeft, rectRight);
        var code2 = Code(B, rectLeft, rectRight);
        var inside = (code1 | code2) == 0;
        var outside = (code1 & code2) != 0;
        while (!inside && !outside)
        {
            if (code1 == 0)
            {
                Algorithms.Swap(ref ax, ref bx);
                Algorithms.Swap(ref ay, ref by);
                int c = code1;
                code1 = code2;
                code2 = c;
            }

            if (Convert.ToBoolean(code1 & 0x01))
            {
                ay += (rectLeft.X - ax) * (by - ay) / (bx - ax);
                ax = rectLeft.X;
                if (!lineZones[i].Contains(code1))
                    lineZones[i].Add(code1);
            }

            if (Convert.ToBoolean(code1 & 0x02))
            {
                ax += (rectLeft.Y - ay) * (bx - ax) / (by - ay);
                ay = rectLeft.Y;
                if (!lineZones[i].Contains(code1))
                    lineZones[i].Add(code1);
            }

            if (Convert.ToBoolean(code1 & 0x04))
            {
                ay += (rectRight.X - ax) * (by - ay) / (bx - ax);
                ax = rectRight.X;
                if (!lineZones[i].Contains(code1))
                    lineZones[i].Add(code1);
            }

            if (Convert.ToBoolean(code1 & 0x08))
            {
                ax += (rectRight.Y - ay) * (bx - ax) / (by - ay);
                ay = rectRight.Y;
                if (!lineZones[i].Contains(code1))
                    lineZones[i].Add(code1);
            }

            code1 = Code(_gameField.grid[ax, ay], rectLeft, rectRight);
            inside = (code1 | code2) == 0;
            outside = (code1 & code2) != 0;
        }
    }

    public int Code(Pixel point, Pixel rectLeft, Pixel rectRight)
    {
        var code = 0;
        if (point.X < rectLeft.X) code |= 0x01;//_ _ _ 1;
        if (point.X > rectRight.X) code |= 0x04;//_ 1 _ _;
        if (point.Y < rectLeft.Y) code |= 0x02;//_ _ 1 _;
        if (point.Y > rectRight.Y) code |= 0x08;//1 _ _ _;
        return code;
    }

    public void GenerateLines()
    {
        int b;
        switch (difficulty)
        {
            case 0:
                b = 9;
                break;
            case 1:
                b = 14;
                break;
            case 2:
                b = 19;
                break;
            default:
                b = 9;
                break;
        }

        for (var i = 0; i < linesQuantity; i++)
        {
            var firstX = UnityEngine.Random.Range(0, b);
            var firstY = UnityEngine.Random.Range(0, b);

            var secondX = UnityEngine.Random.Range(0, b);
            var secondY = UnityEngine.Random.Range(0, b);

            while ((Math.Sqrt((secondX - firstX) * (secondX - firstX) + (secondY - firstY) * (secondY - firstY)) > maxLineLength
              || Math.Sqrt((secondX - firstX) * (secondX - firstX) + (secondY - firstY) * (secondY - firstY)) < minLineLength)
              || (!CheckIntersection(firstX, firstY, secondX, secondY)))
            {
                firstX = UnityEngine.Random.Range(0, b);
                firstY = UnityEngine.Random.Range(0, b);

                secondX = UnityEngine.Random.Range(0, b);
                secondY = UnityEngine.Random.Range(0, b);
            }
            lines[0, i] = _gameField.grid[firstY, firstX];
            lines[1, i] = _gameField.grid[secondY, secondX];
        }
    }

    public bool CheckIntersection(int Ax, int Ay, int Bx, int By)
    {
        var ax = Ax;
        var ay = Ay;
        var bx = Bx;
        var by = By;
        if (ax > bx)
        {
            Algorithms.swap(ax, bx);
            Algorithms.swap(ay, by);
        }
        int[,] matr = new int[2, 2];

        matr[0, 0] = ax.CompareTo(borderPoints[0].X) + ax.CompareTo(borderPoints[1].X);
        matr[0, 1] = ay.CompareTo(borderPoints[0].Y) + ay.CompareTo(borderPoints[1].Y);
        matr[1, 0] = bx.CompareTo(borderPoints[0].X) + bx.CompareTo(borderPoints[1].X);
        matr[1, 1] = by.CompareTo(borderPoints[0].Y) + by.CompareTo(borderPoints[1].Y);
        int checker = matr[0, 0];
        if ((checker == matr[0, 1]) && (checker == matr[1, 0]) && (checker == matr[1, 1]))
            return false;
        else
        {
            var res = (matr[0, 0] * matr[1, 1]) - (matr[1, 0] * matr[0, 1]);
            if (res == 0)
                return true;
            else
                return false;
        }
    }

    public override void CheckAction(Pixel invoker)
    {
        if (!gameActive) return;

        //if (!GetComponent<GameplayTimer>().Counting) return;

        if (iteration == linesQuantity)
        {
            Messenger.Broadcast(GameEvents.GAME_OVER);
            return;
        }
        var check = false;
        var c = -1;

        foreach (var a in lineZones[iteration])
        {
            if (a == this.Code(invoker, borderPoints[0], borderPoints[1]))
            {
                check = true;
                c = a;
                break;
            }
        }
        if (check)
        {
            ClearZone(c);
            lineZones[iteration].Remove(c);
            Messenger<int>.Broadcast(GameEvents.ACTION_RIGHT_ANSWER, 100);

            if (lineZones[iteration].Count == 0)
                iteration++;
            else
                return;

            if (iteration == linesQuantity)
            {
                eventReactor.OnGameOver();
            }
            else
            {
                _gameField.ClearGrid();

                var linePts = Algorithms.GetBrezenheimLineData(new Line(new Position(lines[0, iteration].Y, lines[0, iteration].X), new Position(lines[1, iteration].Y, lines[1, iteration].X)), out _);
                _gameField.Draw(linePts);
            }
        }
        else
            Messenger.Broadcast(GameEvents.ACTION_WRONG_ANSWER);
    }

    public void ClearZone(int code)
    {
        for (var i = 0; i < gridCodesHeight; i++)
        {
            for (var j = 0; j < gridCodesWidth; j++)
            {
                if (gridCodes[i, j] == code)
                {
                    if (!_gameField.grid[i, j].pixel_empty.activeSelf)
                        _gameField.grid[i, j].setPixelState(false);
                }
            }
        }
    }

    public override void Restart()
    {
        gameActive = false;
        gameStarted = false;
        _gameField.ClearGrid();
        for (var i = 0; i < linesQuantity; i++)
            lineZones[i].Clear();

        iteration = 0;
        GenerateLines();

        for (var i = 0; i < linesQuantity; i++)
        {
            southCohen(lines[0, i], lines[1, i],
           borderPoints[0], borderPoints[1], i);
        }
        iteration = 0;

        var linePts = Algorithms.GetBrezenheimLineData(new Line(new Position(lines[0, 0].Y, lines[0, 0].X), new Position(lines[1, 0].Y, lines[1, 0].X)), out _);
        _gameField.Draw(linePts);

        eventReactor.OnRestart();

        Messenger.Broadcast(GameEvents.START_GAME);
    }
}
