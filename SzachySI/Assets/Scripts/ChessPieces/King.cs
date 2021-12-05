using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileX, int tileY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        //Right
        if(X+1<tileX)
        {
            //Right
            if (board[X + 1, Y] == null)
            {
                r.Add(new Vector2Int(X + 1, Y));
            }
            else if(board[X+1,Y].team!=team)
            {
                r.Add(new Vector2Int(X + 1, Y));
            }
            //Top Right
            if(Y+1<tileY)
            {
                if (board[X + 1, Y+1] == null)
                {
                    r.Add(new Vector2Int(X + 1, Y+1));
                }
                else if (board[X + 1, Y+1].team != team)
                {
                    r.Add(new Vector2Int(X + 1, Y+1));
                } 
            }
            //Button Right
            if (Y - 1 >=0)
            {
                if (board[X + 1, Y - 1] == null)
                {
                    r.Add(new Vector2Int(X + 1, Y - 1));
                }
                else if (board[X + 1, Y - 1].team != team)
                {
                    r.Add(new Vector2Int(X + 1, Y - 1));
                }
            }
        }

        //Left
        if (X - 1 >=0)
        {
            //Left
            if (board[X - 1, Y] == null)
            {
                r.Add(new Vector2Int(X - 1, Y));
            }
            else if (board[X - 1, Y].team != team)
            {
                r.Add(new Vector2Int(X - 1, Y));
            }
            //Top Left
            if (Y + 1 < tileY)
            {
                if (board[X - 1, Y + 1] == null)
                {
                    r.Add(new Vector2Int(X - 1, Y + 1));
                }
                else if (board[X - 1, Y + 1].team != team)
                {
                    r.Add(new Vector2Int(X - 1, Y + 1));
                }
            }
            //Button Left
            if (Y - 1 >= 0)
            {
                if (board[X - 1, Y - 1] == null)
                {
                    r.Add(new Vector2Int(X - 1, Y - 1));
                }
                else if (board[X - 1, Y - 1].team != team)
                {
                    r.Add(new Vector2Int(X - 1, Y - 1));
                }
            }
        }

        //Up
        if(Y+1<tileY)
        {
            if(board[X,Y+1]==null || board[X,Y+1].team!=team)
            {
                r.Add(new Vector2Int(X, Y + 1));
            }
        }

        //Down
        if (Y - 1 >=0)
        {
            if (board[X, Y - 1] == null || board[X, Y - 1].team != team)
            {
                r.Add(new Vector2Int(X, Y - 1));
            }
        }

        return r;

    }
}
