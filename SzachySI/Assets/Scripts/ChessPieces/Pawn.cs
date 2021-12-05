using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board,int tileX,int tileY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        int direction = (team == 0) ? 1 : -1;
        // One in fornt
        if(board[X,Y+direction]==null)
        {
            r.Add(new Vector2Int(X, Y + direction));
        }
        //Two in front
        if (board[X, Y + direction] == null)
        {
            if (team == 0 && Y == 1 && board[X, Y + direction * 2] == null)
            {
                r.Add(new Vector2Int(X, Y + (direction * 2)));
            }
            if (team == 1 && Y == 6 && board[X, Y + direction * 2] == null)
            {
                r.Add(new Vector2Int(X, Y + (direction * 2)));
            }
        }
        //kill move
        if(X!=tileX-1)
        {
            if(board[X+1,Y+direction]!=null && board[X + 1, Y + direction].team!=team)
            {
                r.Add(new Vector2Int(X + 1, Y + direction));
            }
        }
        if (X !=0)
        {
            if (board[X - 1, Y + direction] != null && board[X - 1, Y + direction].team != team)
            {
                r.Add(new Vector2Int(X - 1, Y + direction));
            }
        }
        return r;
    }


}
