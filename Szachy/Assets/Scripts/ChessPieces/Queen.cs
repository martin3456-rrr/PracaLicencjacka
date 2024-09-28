using System.Collections.Generic;
using UnityEngine;

public class Queen : ChessPiece
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
        for (int i = Y + 1; i < tileY; i++)
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
            if (board[i, Y] != null)
            {
                if (board[i, Y].team != team)
                {
                    r.Add(new Vector2Int(i, Y));
                }
                break;
            }
        }
        //Right
        for (int i = X + 1; i < tileX; i++)
        {
            if (board[i, Y] == null)
            {
                r.Add(new Vector2Int(i, Y));
            }
            if (board[i, Y] != null)
            {
                if (board[i, Y].team != team)
                {
                    r.Add(new Vector2Int(i, Y));
                }
                break;
            }
        }

        //Top right
        for (int x = X + 1, y = Y + 1; x < tileX && y < tileY; x++, y++)
        {
            if (board[x, y] == null)
            {
                r.Add(new Vector2Int(x, y));
            }
            else
            {
                if (board[x, y].team != team)
                {
                    r.Add(new Vector2Int(x, y));
                }
                break;
            }
        }
        //Top Left
        for (int x = X - 1, y = Y + 1; x >= 0 && y < tileY; x--, y++)
        {
            if (board[x, y] == null)
            {
                r.Add(new Vector2Int(x, y));
            }
            else
            {
                if (board[x, y].team != team)
                {
                    r.Add(new Vector2Int(x, y));
                }
                break;
            }
        }
        //Button right
        for (int x = X + 1, y = Y - 1; x < tileX && y >= 0; x++, y--)
        {
            if (board[x, y] == null)
            {
                r.Add(new Vector2Int(x, y));
            }
            else
            {
                if (board[x, y].team != team)
                {
                    r.Add(new Vector2Int(x, y));
                }
                break;
            }
        }
        //Button Left
        for (int x = X - 1, y = Y - 1; x >= 0 && y >= 0; x--, y--)
        {
            if (board[x, y] == null)
            {
                r.Add(new Vector2Int(x, y));
            }
            else
            {
                if (board[x, y].team != team)
                {
                    r.Add(new Vector2Int(x, y));
                }
                break;
            }
        }
        return r;
    }
}