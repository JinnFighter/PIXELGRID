﻿using System;
using System.Collections.Generic;

public class BezierGameMode : GameMode
{
    private LinesModeData _linesData;
    private ILineDataGenerator _generator;
    private GameField _gameField;

    public BezierGameMode(GameplayTimer timer, int difficulty, GameField field) : base(difficulty)
    {
        _gameField = field;
        _linesData = new LinesModeData();

        Generate();

        eventReactor = new DefaultReactor(timer, difficulty);

        Messenger.Broadcast(GameEvents.START_GAME);
    }

    public override void CheckAction(Pixel invoker)
    {
        if (!CanCheckAction()) return;

        if (_linesData.IsCurrentLast())
            return;
        else
        {
            if (invoker.X == _linesData.GetCurrentPoint().X && invoker.Y == _linesData.GetCurrentPoint().Y)
            {
                Messenger<int>.Broadcast(GameEvents.ACTION_RIGHT_ANSWER, 100);
                _linesData.NextPoint();
                if (_linesData.IsCurrentLast())
                {
                    eventReactor.OnGameOver();
                }
            }
            else
            {
                Messenger.Broadcast(GameEvents.ACTION_WRONG_ANSWER);
            }
        }
    }

    public override void Restart()
    {
        gameActive = false;
        gameStarted = false;
        _gameField.ClearGrid();

        Generate();

        eventReactor.OnRestart();
        Messenger.Broadcast(GameEvents.START_GAME);
    }

    private void DrawBezier(GameField field, List<Position> curvePoints)
    {
        double t, sx, sy, oldx, oldy, ax, ay, tau;
        oldx = curvePoints[0].X;
        oldy = curvePoints[0].Y;
        var counter = curvePoints.Count;

        for (t = 0; t <= 0.5; t += 0.005)
        {
            sx = curvePoints[0].X;
            sy = curvePoints[0].Y;
            ax = 1.0;
            ay = 1.0;
            tau = 1.0;
            for (int i = 1; i < counter; i++)//counter;
            {
                tau *= (1 - t);
                ax = ax * t * (counter - i) / (i * (1 - t));
                ay = ay * t * (counter - i) / (i * (1 - t));
                sx += ax * curvePoints[i].X;
                sy += ay * curvePoints[i].Y;
            }
            sx *= tau;
            sy *= tau;

            var linePts = Algorithms.GetBrezenheimLineData(new Line(new Position(oldx, oldy), new Position(sx, sy)), out _);
            field.Draw(linePts);

            oldx = sx;
            oldy = sy;
        }
        oldx = curvePoints[counter - 1].X;
        oldy = curvePoints[counter - 1].Y;
        for (t = 1.0; t >= 0.5; t -= 0.005)
        {
            sx = curvePoints[counter - 1].X;
            sy = curvePoints[counter - 1].Y;
            ax = 1.0;
            ay = 1.0;
            tau = 1.0;
            for (var i = counter - 2; i >= 0; i--)
            {
                tau *= t;
                ax = ax * (1 - t) * (i + 1) / (t * (counter - 1 - i));
                ay = ay * (1 - t) * (i + 1) / (t * (counter - 1 - i));
                sx += ax * curvePoints[i].X;
                sy += ay * curvePoints[i].Y;
            }
            sx *= tau;
            sy *= tau;

            var linePts = Algorithms.GetBrezenheimLineData(new Line(new Position(oldx, oldy), new Position(sx, sy)), out _);
            field.Draw(linePts);

            oldx = sx;
            oldy = sy;
        }
    }

    private void Generate()
    {
        int pointsCount;
        int minLength;
        int maxLength;
        switch (difficulty)
        {
            case 1:
                pointsCount = 5;
                minLength = 4;
                maxLength = 6;
                break;
            case 2:
                pointsCount = 7;
                minLength = 5;
                maxLength = 7;
                break;
            default:
                pointsCount = 3;
                minLength = 3;
                maxLength = 5;
                break;
        }

        _generator = new BezierLineDataGenerator(pointsCount);
        _linesData = _generator.GenerateData(minLength, maxLength);
        var curvePoints = new List<Position>();
        for(var i = 0; i < _linesData.GetPointsCount(); i++)
        {
            curvePoints.Add(_linesData.GetPoint(i));
        }
        DrawBezier(_gameField, curvePoints);
    }
}
