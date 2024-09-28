using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AI_System
{
    public static ChessBoard chessBoard;

    public static int MoveGenerationTest(int depth)
    {
        if (depth == 0)
            return 1;

        List<ChessPiece> list = (chessBoard.IsWhiteTeam ? chessBoard.aiPiecesWhite : chessBoard.aiPiecesBlack);
        List<AI_MoveGen.Move> moves = AI_MoveGen.GenerateMoves(ref chessBoard.chessPieces, ref list, chessBoard.IsWhiteTeam);

        int numPositions = 0;
        foreach (AI_MoveGen.Move move in moves)
        {
            chessBoard.MakeMove(move);
            numPositions += MoveGenerationTest(depth - 1);
            chessBoard.UnmakeMove(move);
        }
        return numPositions;
    }


    const int pawnValue = 100;
    const int knightValue = 300;
    const int bishopValue = 300;
    const int rookValue = 500;
    const int queenValue = 900;
    const int kingValue = 1000000000;

    public static int Evaluate()
    {
        int whiteEval = CountMaterial(true);
        int blackEval = CountMaterial(false);
        int evaluation = whiteEval - blackEval;

        int perspective = (chessBoard.IsWhiteTeam) ? 1 : -1;
        return evaluation * perspective;
    }

    static int CountMaterial(bool isWhite)
    {
        int material = 0;
        foreach (ChessPiece item in (isWhite ? chessBoard.aiPiecesWhite : chessBoard.aiPiecesBlack))
        {
            switch (item.type)
            {
                case ChessPieceType.Pawn:
                    material += pawnValue;
                    break;
                case ChessPieceType.Rook:
                    material += rookValue;
                    break;
                case ChessPieceType.Knight:
                    material += knightValue;
                    break;
                case ChessPieceType.Bishop:
                    material += bishopValue;
                    break;
                case ChessPieceType.Queen:
                    material += queenValue;
                    break;
                case ChessPieceType.King:
                    material += kingValue;
                    break;
            }
        }
        return material;
    }

    static int Search(int depth,int alpha, int beta)
    {
        if (depth == 0)
            return Quiesce(alpha, beta);
        //return Evaluate();
        List<ChessPiece> list = (chessBoard.IsWhiteTeam ? chessBoard.aiPiecesWhite : chessBoard.aiPiecesBlack);
        List<AI_MoveGen.Move> moves = AI_MoveGen.GenerateMoves(ref chessBoard.chessPieces, ref list, chessBoard.IsWhiteTeam);
        OrderMoves(ref moves);
        if (moves.Count == 0)
        {
            if (chessBoard.IsCheck(chessBoard.IsWhiteTeam))
            {
                return int.MinValue;
            }
            else
            {
                return 0;
            }
        }
        foreach (AI_MoveGen.Move move in moves)
        {
            chessBoard.MakeMove(move);
            int evaluation = -Search(depth - 1, -beta, -alpha);
            chessBoard.UnmakeMove(move);
            if (evaluation >= beta)
                return beta;
            alpha = (evaluation > alpha ? evaluation : alpha);
        }
        return alpha;
    }
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    public static AI_MoveGen.Move BestMove()
    {
        List<ChessPiece> list = (chessBoard.IsWhiteTeam ? chessBoard.aiPiecesWhite : chessBoard.aiPiecesBlack);
        List<AI_MoveGen.Move> moves = AI_MoveGen.GenerateMoves(ref chessBoard.chessPieces, ref list, chessBoard.IsWhiteTeam);
        moves.Shuffle();
        int bestScore = int.MinValue+1;
        AI_MoveGen.Move bestMove = moves[Random.Range(0, moves.Count)];
        foreach (AI_MoveGen.Move move in moves)
        {
            chessBoard.MakeMove(move);
            int newScore = -Search(1, int.MinValue+1, int.MaxValue-1);
            if (newScore >= bestScore)
            {
                bestScore = newScore;
                bestMove = move;
            }
            chessBoard.UnmakeMove(move);
        }
        return bestMove;
    }

    static int GetPieceValue(ChessPieceType type)
    {
        switch (type)
        {
            case ChessPieceType.Pawn:
                return pawnValue;
            case ChessPieceType.Rook:
                return rookValue;
            case ChessPieceType.Knight:
                return knightValue;
            case ChessPieceType.Bishop:
                return bishopValue;
            case ChessPieceType.Queen:
                return queenValue;
            default:
                return 0;
        }
    }

    static void OrderMoves(ref List<AI_MoveGen.Move> moves)
    {
        int[] moveScores = new int[moves.Count];
        for (int i = 0; i < moves.Count; i++)
        {
            AI_MoveGen.Move move = moves[i];
            int moveScoreGuess = 0;
            ChessPieceType movePieceType = move.piece.type;
            ChessPieceType capturePieceType = ChessPieceType.None;
            if (chessBoard.chessPieces[move.target.x, move.target.y])
                capturePieceType = chessBoard.chessPieces[move.target.x, move.target.y].type;
            if (capturePieceType != ChessPieceType.None)
                moveScoreGuess = 10 * GetPieceValue(capturePieceType) - GetPieceValue(movePieceType);
            moveScores[i] = moveScoreGuess;
        }
        for (int i = 0; i < moves.Count; i++)
        {
            int j = i;
            AI_MoveGen.Move move = moves[i];
            int moveScoreGuess = moveScores[i];
            while (j > 0 && moveScores[j - 1] < moveScoreGuess)
            {
                moves[j] = moves[j - 1];
                moveScores[j] = moveScores[j - 1];
                j--;
            }
            moves[j] = move;
            moveScores[j] = moveScoreGuess;
        }
    }

    static int Quiesce(int alpha, int beta)
    {
        int evaluation = Evaluate();

        if (evaluation >= beta)
            return beta;

        if (evaluation >= alpha)
            alpha = evaluation;

        List<ChessPiece> list = (chessBoard.IsWhiteTeam ? chessBoard.aiPiecesWhite : chessBoard.aiPiecesBlack);
        // ONLY ATTACK
        List<AI_MoveGen.Move> moves = AI_MoveGen.GenerateMoves(ref chessBoard.chessPieces, ref list, chessBoard.IsWhiteTeam);
        OrderMoves(ref moves);
        foreach (AI_MoveGen.Move move in moves)
        {
            chessBoard.MakeMove(move);
            evaluation = -Quiesce(-beta, -alpha);
            chessBoard.UnmakeMove(move);
            if (evaluation >= beta)
                return beta;
            alpha = (evaluation > alpha ? evaluation : alpha);
        }
        return alpha;
    }
}