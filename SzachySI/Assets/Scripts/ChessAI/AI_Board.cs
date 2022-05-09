using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AI_Board
{
    public static int[] Square;

    static AI_Board()
    {
        Square = new int[64];
        for (int i = 0; i < 64; i++)
            Square[i] = AI_Piece.None;
    }

    //rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
    public const string startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
    public static void LoadFEN(string FEN_String)
    {
        int position = 0;

        // Sprawdza ka¿dy znak
        foreach (char ch in FEN_String)
        {
            if (position >= 64) // Zabezpieczenie przed wypadniêciem
                break;

            // Je¿eli to jest numer, to przesuñ o N pól
            if (char.IsNumber(ch))
            {
                position += (int)(ch - 48);
                continue;
            }

            // Je¿eli '/' idŸ do kolejnej linii
            if (ch == '/')
            {
                if ((position % 8) != 0)
                    position += 8 - (position % 8);
                continue;
            }

            // Ustaw aktualne pole na nic
            int currentPiece = AI_Piece.None;

            // A = 0000
            // B = 0110
            // A | B == 0110

            // Je¿eli jest to ma³a litera to jest czarna figura, je¿eli du¿a to bia³a 
            if (char.IsLower(ch))
                currentPiece |= AI_Piece.Black; // currentPiece = currentPiece | AI_Piece.Black;
            else
                currentPiece |= AI_Piece.White;

            switch (char.ToLower(ch))
            {
                //pawn = "P", knight = "N", bishop = "B", rook = "R", queen = "Q" and king = "K"
                //PNBRQK white
                //pnbrqk black
                case 'p':
                    currentPiece |= AI_Piece.Pawn;
                    break;
                case 'n':
                    currentPiece |= AI_Piece.Knight;
                    break;
                case 'b':
                    currentPiece |= AI_Piece.Bishop;
                    break;
                case 'r':
                    currentPiece |= AI_Piece.Rook;
                    break;
                case 'q':
                    currentPiece |= AI_Piece.Queen;
                    break;
                case 'k':
                    currentPiece |= AI_Piece.King;
                    break;
                default:
                    break;
            }

            // Zapisz figurê i idŸ dalej
            Square[position++] = currentPiece;
        }
    }

    public static int lastX = 0;
    public static int lastY = 0;

    public static void Move()
    {

        int tmp = Square[0];
        Square[0] = Square[63];
        Square[63] = tmp;
    }
}