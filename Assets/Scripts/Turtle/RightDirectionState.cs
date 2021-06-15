﻿using UnityEngine;

public class RightDirectionState : IDirectionState
{
    public void RotateLeft(Turtle turtle)
    {
        turtle.SetDirectionState(new UpDirectionState());
    }

    public void RotateRight(Turtle turtle)
    {
        turtle.SetDirectionState(new DownDirectionState());
    }

    public void Move(Turtle turtle)
    {
        Vector3 startPos = turtle.transform.position;

        float posX = startPos.x;
        float posY = startPos.y;
        turtle.Position.Y++;
        posX += turtle.offsetX;

        turtle.gameObject.transform.position = new Vector3(posX, posY, startPos.z);
    }

    public Vector2Int Move(Vector2Int position) => new Vector2Int(position.x, position.y + 1);

    public IDirectionState RotateLeft() => new UpDirectionState();

    public IDirectionState RotateRight() => new DownDirectionState();
}
