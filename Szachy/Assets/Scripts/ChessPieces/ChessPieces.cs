using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public enum ChessPieceType
{
    None = 0,
    Pawn = 1,
    Rook = 2,
    Knight = 3,
    Bishop = 4,
    Queen = 5,
    King = 6
}

public class ChessPiece : MonoBehaviour
{
    public int team; // 0 - Bia³e / 1 - Czarne
    public int X;
    public int Y;
    public ChessPieceType type;
    public Sprite spriteWhite, spriteBlack;

    private Vector3 desiredPosition;
    private Vector3 desiredScale = Vector3.one;
    private SpriteRenderer spriteRend;
    [SerializeField] private GameObject child3D, child2D;

    private void Start()
    {
        transform.rotation = Quaternion.Euler((team == 1) ? Vector3.zero : new Vector3(0, 180, 0));
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);
    }
    public virtual List<Vector2Int> GetAvailableMoves(ref ChessPiece[,] board, int tileX, int tileY)
    {
        List<Vector2Int> r = new List<Vector2Int>();
        r.Add(new Vector2Int(3, 3));
        r.Add(new Vector2Int(3, 4));
        r.Add(new Vector2Int(4, 3));
        r.Add(new Vector2Int(4, 4));
        return r;
    }
    public virtual SpecialMove GetSpecialMoves(ref ChessPiece[,] board, ref List<Vector2Int[]> moveList, ref List<Vector2Int> availableMoves)
    {
        return SpecialMove.None;
    }
    public void InitGraphics(int team, Material material)
    {
        child3D = this.transform.GetChild(0).gameObject;
        child3D.GetComponent<MeshRenderer>().material = material;
        child2D = this.transform.GetChild(1).gameObject;
        spriteRend = child2D.GetComponent<SpriteRenderer>();
        spriteRend.sprite = team == 0 ? spriteWhite : spriteBlack;
        spriteRend.size = new Vector2(100.0f, 100.0f);
        child2D.SetActive(false);
        child2D.transform.localScale = new Vector3(20, 20, 20);
    }
    public void ChangeTo3D()
    {
        transform.rotation = Quaternion.Euler((team == 1) ? Vector3.zero : new Vector3(0, 180, 0));
        child3D.SetActive(true);
        child2D.SetActive(false);
    }
    public void ChangeTo2D()
    {
        child2D.SetActive(true);
        child3D.SetActive(false);
        transform.rotation = Quaternion.Euler(new Vector3(270, 180, 0));
    }
    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        desiredPosition = position;
        if (force)
            transform.position = desiredPosition;
    }
    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        desiredScale = scale;
        if (force)
            transform.position = desiredScale;
    }
}