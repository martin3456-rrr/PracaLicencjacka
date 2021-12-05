using System.Collections.Generic;
using UnityEngine;

public class Rook : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileX, int tileY)
    {
        List<Vector2Int> r = new List<Vector2Int>();
        //Down
        for (int i = Y - 1; i >= 0; i--)
        {
            if (board[X, i] == null)
            {
                r.Add(new Vector2Int(X, i));
            }
            if (board[X, i] != null)
            {
                if (board[X, i].team != team)
                {
                    r.Add(new Vector2Int(X, i));
                }
                break;
            }
        }
        //Up
        for (int i = Y + 1; i <tileY; i++)
        {
            if (board[X, i] == null)
            {
                r.Add(new Vector2Int(X, i));
            }
            if (board[X, i] != null)
            {
                if (board[X, i].team != team)
                {
                    r.Add(new Vector2Int(X, i));
                }
                break;
            }
        }
        //Left
        for (int i = X - 1; i >= 0; i--)
        {
            if (board[i, Y] == null)
            {
                r.Add(new Vector2Int(i, Y));
            }
            if (board[i,Y] != null)
            {
                if (board[X, i].team != team)
                {
                    r.Add(new Vector2Int(i,Y));
                }
                break;
            }
        }
        //Right
        for (int i = X + 1; i<tileX; i++)
        {
            if (board[i, Y] == null)
            {
                r.Add(new Vector2Int(i, Y));
            }
            if (board[i, Y] != null)
            {
                if (board[X, i].team != team)
                {
                    r.Add(new Vector2Int(i, Y));
                }
                break;
            }
        }
        return r;
    }
}
