using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AI_Piece
{
    public const int None = 0;      // **000
    public const int King = 1;      // --001
    public const int Pawn = 2;      // --010
    public const int Knight = 3;    // --011
    public const int Bishop = 5;    // --100
    public const int Rook = 6;      // --101
    public const int Queen = 7;     // --110

    public const int White = 8;     // 01---
    public const int Black = 16;    // 10---

    const int typeMask = 0b00111;   // Maska typu figury
    const int blackMask = 0b10000;  // Maska koloru czarnego
    const int whiteMask = 0b01000;  // Maska koloru bia³ego
    const int colourMask = whiteMask | blackMask; // Maska kolru

    // Czy figura [piece] ma dany kolor [colour]
    public static bool IsColour(int piece, int colour)
    {
        return (piece & colourMask) == colour;
    }

    // Jaki kolor ma figura
    public static int Colour(int piece)
    {
        return piece & colourMask;
    }

    // Jaki typ figury
    public static int PieceType(int piece)
    {
        return piece & typeMask;
    }

    // Czy to jest Wierza lub Królowa
    public static bool IsRookOrQueen(int piece)
    {
        return (piece & 0b110) == 0b110;
    }

    // Czy to jest Goniec lub Królowa
    public static bool IsBishopOrQueen(int piece)
    {
        return (piece & 0b101) == 0b101;
    }

    // ???
    public static bool IsSlidingPiece(int piece)
    {
        return (piece & 0b100) != 0;
    }
}