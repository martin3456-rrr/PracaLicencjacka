using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileX, int tileY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        //Top right
        int x = X + 1;
        int y = Y + 2;
        if(x<tileX && y<tileY)
        {
            if(board[x,y] ==null || board[x,y].team!=team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }

        x = X +2;
        y = Y + 1;
        if (x < tileX && y < tileY)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }

        //Top left
        x = X - 1;
        y = Y + 2;
        if(x>=0 && y<tileY)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }

        x = X - 2;
        y = Y + 1;
        if (x >= 0 && y < tileY)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }
        //Button Right
        x = X + 1;
        y = Y - 2;
        if(x<tileX && y>=0)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }

        x = X + 2;
        y = Y - 1;
        if (x < tileX && y >= 0)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }
        //Button Left
        x = X - 1;
        y = Y - 2;
        if (x >=0 && y >= 0)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }

        x = X - 2;
        y = Y - 1;
        if (x >= 0 && y >= 0)
        {
            if (board[x, y] == null || board[x, y].team != team)
            {
                r.Add(new Vector2Int(x, y));
            }
        }
        return r;
    }
}
