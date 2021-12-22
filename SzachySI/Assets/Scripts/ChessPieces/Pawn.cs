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

    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        int direction = (team == 0) ? 1 : -1;
        if((team==0 && Y==6) || (team==1 && Y==1))
        {
            return SpecialMove.Promotion;
        }

        //En Passant
        if(moveList.Count>0)
        {
            Vector2Int[] lastMove = moveList[moveList.Count - 1];
            if(board[lastMove[1].x,lastMove[1].y].type==ChessPieceType.Pawn)// If the last piece moved a pawn
            {
                if(Mathf.Abs(lastMove[0].y-lastMove[1].y)==2)//If the last move was a +2 in either direction
                {
                    if(board[lastMove[1].x,lastMove[1].y].team!=team)//If the move was from the other team
                    {
                        if(lastMove[1].y==Y)//If both pawns are on the same Y
                        {
                            if(lastMove[1].x==X-1)//Landed Left
                            {
                                availableMoves.Add(new Vector2Int(X - 1, Y + direction));
                                return SpecialMove.EnPassant;
                            }
                            if (lastMove[1].x == X + 1)//Landed right
                            {
                                availableMoves.Add(new Vector2Int(X + 1, Y + direction));
                                return SpecialMove.EnPassant;
                            }

                        }
                    }
                }
            }

        }
        return SpecialMove.None;
    }
}
