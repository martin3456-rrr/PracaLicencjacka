using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileX, int tileY)
    {
        List<Vector2Int> r = new List<Vector2Int>();

        //Right
        if (X + 1 < tileX)
        {
            //Right
            if (board[X + 1, Y] == null)
            {
                r.Add(new Vector2Int(X + 1, Y));
            }
            else if (board[X + 1, Y].team != team)
            {
                r.Add(new Vector2Int(X + 1, Y));
            }
            //Top Right
            if (Y + 1 < tileY)
            {
                if (board[X + 1, Y + 1] == null)
                {
                    r.Add(new Vector2Int(X + 1, Y + 1));
                }
                else if (board[X + 1, Y + 1].team != team)
                {
                    r.Add(new Vector2Int(X + 1, Y + 1));
                }
            }
            //Button Right
            if (Y - 1 >= 0)
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
        if (X - 1 >= 0)
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
        if (Y + 1 < tileY)
        {
            if (board[X, Y + 1] == null || board[X, Y + 1].team != team)
            {
                r.Add(new Vector2Int(X, Y + 1));
            }
        }

        //Down
        if (Y - 1 >= 0)
        {
            if (board[X, Y - 1] == null || board[X, Y - 1].team != team)
            {
                r.Add(new Vector2Int(X, Y - 1));
            }
        }

        return r;

    }

    public override SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        SpecialMove r = SpecialMove.None;

        var kingMove = moveList.Find(m => m[0].x == 4 && m[0].y == ((team == 0) ? 0 : 7));
        var leftRook = moveList.Find(m => m[0].x == 0 && m[0].y == ((team == 0) ? 0 : 7));
        var rightRook = moveList.Find(m => m[0].x == 4 && m[0].y == ((team == 0) ? 0 : 7));

        if (kingMove == null && X == 4)
        {
            //Whie team
            if (team == 0)
            {
                //Left Rook
                if (leftRook == null)
                {
                    if (board[0, 0].type == ChessPieceType.Rook)
                    {
                        if (board[0, 0].team == 0)
                        {
                            if (board[3, 0] == null)
                            {
                                if (board[2, 0] == null)
                                {
                                    if (board[1, 0] == null)
                                    {
                                        availableMoves.Add(new Vector2Int(2, 0));
                                        r = SpecialMove.Castling;
                                    }
                                }
                            }
                        }
                    }
                }
                //Right Rook
                if (rightRook == null)
                {
                    if (board[7, 0].type == ChessPieceType.Rook)
                    {
                        if (board[7, 0].team == 0)
                        {

                            if (board[5, 0] == null)
                            {
                                if (board[6, 0] == null)
                                {
                                    availableMoves.Add(new Vector2Int(6, 0));
                                    r = SpecialMove.Castling;
                                }
                            }

                        }
                    }
                }
            }
            //Black team
            else
            {
                //Left Rook
                if (leftRook == null)
                {
                    if (board[0, 7].type == ChessPieceType.Rook)
                    {
                        if (board[0, 7].team == 1)
                        {
                            if (board[3, 7] == null)
                            {
                                if (board[2, 7] == null)
                                {
                                    if (board[1, 7] == null)
                                    {
                                        availableMoves.Add(new Vector2Int(2, 7));
                                        r = SpecialMove.Castling;
                                    }
                                }
                            }
                        }
                    }
                }
                //Right Rook
                if (rightRook == null)
                {
                    if (board[7, 7].type == ChessPieceType.Rook)
                    {
                        if (board[7, 7].team == 1)
                        {
                            if (board[5, 7] == null)
                            {
                                if (board[6, 7] == null)
                                {
                                    availableMoves.Add(new Vector2Int(6, 7));
                                    r = SpecialMove.Castling;
                                }

                            }
                        }
                    }
                }

            }
        }
        return r;
    }
}