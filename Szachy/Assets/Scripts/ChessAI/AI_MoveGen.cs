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
    public static bool CanCapture(ref ChessPiece[,] chessPieces, ref List<ChessPiece> pieces, ChessPiece piece, int x, int y, bool isWhiteTeam)
    {
        // Sprawdzenie, czy dana figura mo¿e zaatakowaæ innego gracza w wybranym polu
        List<Move> moves = GenerateMoves(ref chessPieces, ref pieces, isWhiteTeam);

        // Sprawdzenie, czy figura, któr¹ chcemy zaatakowaæ, nale¿y do dru¿yny przeciwnej
        ChessPiece targetPiece = chessPieces[x, y];
        if (targetPiece == null || targetPiece.team == (isWhiteTeam ? 0 : 1))
            return false;

        foreach (Move move in moves)
        {
            if (piece.X == move.start.x && piece.Y == move.start.y && move.target.x == x && move.target.y == y)
            {
                return true;
            }
        }

        return false;
    }
    public static bool CanMove(ref ChessPiece[,] chessPieces, ref List<ChessPiece> pieces, ChessPiece piece, bool isWhiteTeam)
    {
        // Sprawdzanie, czy ruch jest dozwolony
        if (!MoveValid(ref chessPieces, piece.X, piece.Y, isWhiteTeam))
            return false;

        // Pêtla przez wszystkie figury
        foreach (ChessPiece otherPiece in pieces)
        {
            // Sprawdzenie, czy figura nale¿y do przeciwnika
            if (chessPieces[piece.X, piece.Y].team != (isWhiteTeam ? 0 : 1) && CanCapture(ref chessPieces, ref pieces, otherPiece, piece.X, piece.Y, isWhiteTeam))
            {
                // Sprawdzamy, czy mo¿emy zablokowaæ ruch
                if (CanBlock(ref chessPieces, ref pieces, otherPiece, piece.X, piece.Y, isWhiteTeam))
                    return true;
            }
            // Sprawdzamy, czy figura nale¿y do naszej dru¿yny
            else if (chessPieces[piece.X, piece.Y].team == (isWhiteTeam ? 0 : 1))
            {
                // Sprawdzamy, czy mo¿emy zablokowaæ ruch
                if (CanBlock(ref chessPieces, ref pieces, otherPiece, piece.X, piece.Y, isWhiteTeam))
                    return true;

                // Sprawdzamy, czy ruch jest poprawny i nie spowoduje szacha
                else if (MoveValid(ref chessPieces, piece.X, piece.Y, isWhiteTeam) && !WillCheckKing(ref chessPieces, piece.X, piece.Y, isWhiteTeam))
                    return true;
            }
        }
        return false;
    }
    public static bool MoveValid(ref ChessPiece[,] chessPieces, int x, int y, bool isWhiteTeam)
    {
        // Sprawdzamy, czy pole jest puste
        if (chessPieces[x, y] == null)
        {
            return true;
        }
        // Sprawdzamy, czy na polu znajduje siê figura przeciwnika
        else if (chessPieces[x, y].team != (isWhiteTeam ? 0 : 1))
        {
            return true;
        }
        return false;
    }
    public static bool CanBlock(ref ChessPiece[,] chessPieces, ref List<ChessPiece> pieces, ChessPiece otherPiece, int x, int y, bool isWhiteTeam)
    {
        int xDelta = 0;
        int yDelta = 0;

        // Sprawdzenie, czy figura przeciwnika mo¿e wykonaæ ruch blokuj¹cy szach
        if (otherPiece.X < x)
        {
            xDelta = -1;
        }
        else if (otherPiece.X > x)
        {
            xDelta = 1;
        }

        if (otherPiece.Y < y)
        {
            yDelta = -1;
        }
        else if (otherPiece.Y > y)
        {
            yDelta = 1;
        }

        int newX = otherPiece.X + xDelta;
        int newY = otherPiece.Y + yDelta;
        while (newX >= 0 && newX < 8 && newY >= 0 && newY < 8)
        {
            if (chessPieces[newX, newY] == null)
            {
                // Sprawdzenie, czy figura przeciwnika mo¿e wykonaæ ruch blokuj¹cy szach
                if (CanCapture(ref chessPieces, ref pieces, otherPiece, newX, newY, isWhiteTeam))
                {
                    return true;
                }
            }
            else
            {
                break;
            }
            newX += xDelta;
            newY += yDelta;
        }
        return false;
    }
    public static bool WillCheckKing(ref ChessPiece[,] chessPieces, int x, int y, bool isWhiteTeam)
    {
        int kingX = 0;
        int kingY = 0;

        // Znajdowanie pozycji króla
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (chessPieces[i, j] != null && chessPieces[i, j].team == (isWhiteTeam ? 0 : 1) && chessPieces[i, j].type == ChessPieceType.King)
                {
                    kingX = i;
                    kingY = j;
                }
            }
        }

        // Sprawdzanie, czy ruch powoduje szacha
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (chessPieces[i, j] != null && chessPieces[i, j].team != (isWhiteTeam ? 0 : 1) && MoveValid(ref chessPieces, i, j, !isWhiteTeam))
                {
                    if (i == kingX && j == kingY)
                        return true;
                }
            }
        }
        return false;
    }
}