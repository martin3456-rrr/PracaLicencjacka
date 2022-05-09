using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AI_MoveGen
{
    public class Move
    {
        public Move(Vector2Int start, Vector2Int target, ChessPiece piece)
        {
            this.start = start;
            this.target = target;
            this.piece = piece;
        }
        public Vector2Int start;
        public Vector2Int target;
        public ChessPiece piece;
    };


    public static List<Move> GenerateMoves(ref ChessPiece[,] board, ref List<ChessPiece> list, bool whiteTeam)
    {
        List<Move> moves = new List<Move>();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                ChessPiece piece = board[i, j];
                if (piece)
                {
                    if (piece.team == (whiteTeam ? 0 : 1))
                    {
                        List<Vector2Int> listVec = piece.GetAvailableMoves(ref board, 8, 8);
                        foreach (Vector2Int itemVec in listVec)
                        {
                            moves.Add(new Move(new Vector2Int(i, j), itemVec, piece));
                        }
                    }
                }
            }
        }
        return moves;
    }
}