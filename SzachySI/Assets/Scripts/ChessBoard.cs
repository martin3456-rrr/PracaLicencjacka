using System;
using System.Collections.Generic;
using UnityEngine;

public class ChessBoard : MonoBehaviour
{
    [Header("Art studd")]
    [SerializeField] private Material tileMaterial;
    [SerializeField] private float tileSize = 1.0f;
    [SerializeField] private float yOffset = 0.2f;
    [SerializeField] private Vector3 boardCenter = Vector3.zero;
    [SerializeField] private float deathSize = 0.3f;
    [SerializeField] private float deathSpacing = 0.3f;
    [SerializeField] private float dragOffset = 1.5f;

    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;

    //LOGIC
    private ChessPiece[,] chessPieces;
    private ChessPiece currentlyDragging;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private List<ChessPiece> deadWhites = new List<ChessPiece>();
    private List<ChessPiece> deadBlacks = new List<ChessPiece>();
    private const int Tile_X = 8;
    private const int Tile_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;
    private void Awake()
    {
        GenerateAllFiles(tileSize, Tile_X, Tile_Y);
        SpawnAllPieces();
        PositionAllPieces();
    }
    private void Update()
    {
        if (!currentCamera)
        {
            currentCamera = Camera.main;
            return;
        }

        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover","Highlight")))
        {
            //Get the indexes of the tile i've hit(Uzyskaj indeksy kafelka, w kt�ry trafi�em)
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            //if we're hovering a tile after not hovering any tiles(je�li naje�d�amy na kafelek po tym, jak nie naje�d�amy na �adne kafelki)
            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
            //If we were already hoevering a tile, change the preovious one(Je�li ju� unosili�my kafelek, zmie� poprzedni�)
            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }
            //If we press down on the mouse(Je�li naci�niemy myszk�)
            if (Input.GetMouseButtonDown(0))
            {
                if (chessPieces[hitPosition.x, hitPosition.y] != null)
                {
                    //Is it our turn?(Czy to nasza kolej?)
                    if (true)
                    {
                        currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];
                        //Get a list of where i can go, hightlight tiles as well
                        availableMoves = currentlyDragging.GetAvailableMoves(ref chessPieces, Tile_X, Tile_Y);
                        HighlightTiles();
                    }
                }
            }
            //If we are releasing the mouse button(Je�li zwalniamy przycisk myszy)
            if (currentlyDragging!=null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPosition = new Vector2Int(currentlyDragging.X,currentlyDragging.Y);
                
                bool validMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y);
                if(!validMove)
                {
                    currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                    currentlyDragging = null;
                }
                currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }
        else
        {
            
            if (currentHover != -Vector2Int.one)
            {
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves,currentHover))? LayerMask.NameToLayer("Hightlight") : LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }
            if(currentlyDragging && Input.GetMouseButtonUp(0))
            {
                currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.X, currentlyDragging.Y));
                currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }
        //If we're dragging a piece(Je�li przeci�gamy kawa�ek)
        if (currentlyDragging)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;
            if(horizontalPlane.Raycast(ray,out distance))
            {
                currentlyDragging.SetPosition(ray.GetPoint(distance)+Vector3.up*dragOffset);
            }    
        }
        
    }
    //Generate The Board
    private void GenerateAllFiles(float tileSize, int tileX, int tileY)
    {
        yOffset += transform.position.y;
        bounds = new Vector3((tileX / 2) * tileSize, 0, (tileY / 2) * tileSize) + boardCenter;
        tiles = new GameObject[tileX, tileY];
        for (int x = 0; x < tileX; x++)
        {
            for (int y = 0; y < tileY; y++)
            {
                tiles[x, y] = GenerateSingleTile(tileSize, x, y);
            }
        }

    }

    private GameObject GenerateSingleTile(float tileSize, int x, int y)
    {
        GameObject tileObject = new GameObject(string.Format("X:{0}, Y:{1}", x, y));
        tileObject.transform.parent = transform;

        Mesh mesh = new Mesh();
        tileObject.AddComponent<MeshFilter>().mesh = mesh;
        tileObject.AddComponent<MeshRenderer>().material = tileMaterial;

        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(x * tileSize, yOffset, y * tileSize) - bounds;
        vertices[1] = new Vector3(x * tileSize, yOffset, (y + 1) * tileSize) - bounds;
        vertices[2] = new Vector3((x + 1) * tileSize, yOffset, y * tileSize) - bounds;
        vertices[3] = new Vector3((x + 1) * tileSize, yOffset, (y + 1) * tileSize) - bounds;

        int[] tris = new int[] { 0, 1, 2, 1, 3, 2 };

        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        tileObject.layer = LayerMask.NameToLayer("Tile");
        tileObject.AddComponent<BoxCollider>();

        return tileObject;
    }

    //Spawing of the pieces(Tarcie kawa�k�w)
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[Tile_X, Tile_Y];
        int whiteTeam = 0, blackTeam = 1;
        //WhiteTeam
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Quenn, whiteTeam);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        for (int i = 0; i < Tile_X; i++)
        {
            chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);
        }
        //BlackTeam
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Quenn, blackTeam);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        for (int i = 0; i < Tile_X; i++)
        {
            chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);
        }
    }
    private ChessPiece SpawnSinglePiece(ChessPieceType type,int team)
    {
        ChessPiece cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPiece>();
        cp.type = type;
        cp.team = team;
        cp.GetComponent<MeshRenderer>().material = teamMaterials[team];
        return cp;
    }
    
    //Positioning
    private void PositionAllPieces()
    {
        for (int x = 0; x < Tile_X; x++)
        {
            for (int y = 0; y < Tile_Y; y++)
            {
                if(chessPieces[x,y]!=null)
                {
                    PositionSinglePiece(x, y, true);
                }
            }
        }
    }
    private void PositionSinglePiece(int x, int y, bool force=false)
    {
        chessPieces[x, y].X = x;
        chessPieces[x, y].Y = y;
        chessPieces[x, y].SetPosition(GetTileCenter(x,y),force); 

    }
    private Vector3 GetTileCenter(int x, int y)
    {
        return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }

    //Highlight Tiles
    private void HighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
        }
    }
    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
        {
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        }
        availableMoves.Clear();
    }

    //Operations

    private bool ContainsValidMove(ref List<Vector2Int> moves,Vector2 pos)
    {
        for (int i = 0; i < moves.Count; i++)
        {
            if(moves[i].x==pos.x && moves[i].y==pos.y)
            {
                return true;
            }
            
        }
        return false;
    }
    private bool MoveTo(ChessPiece cp, int x, int y)
    {
        if(!ContainsValidMove(ref availableMoves , new Vector2(x,y)))
        {
            return false;
        }
        Vector2Int previousPostiton = new Vector2Int(cp.X, cp.Y);

        // Is there another piece on the target position(Czy na pozycji docelowej jest inny kawa�ek)
        if (chessPieces[x, y] != null)
        {
            ChessPiece ocp = chessPieces[x, y];
            if (cp.team == ocp.team)
            {
                return false;
            }
            //If its the enemy team(Je�li to dru�yna wroga)
            if (ocp.team == 0)
            {
                deadWhites.Add(ocp);
                ocp.SetScale(Vector3.one * deathSize);
                ocp.SetPosition(
                    new Vector3(8 * tileSize, yOffset, -1 * tileSize) - bounds
                    + new Vector3(tileSize / 2, 0, tileSize / 2) +
                    (Vector3.forward * deathSpacing) * deadWhites.Count);
            }
            else
            {
                deadBlacks.Add(ocp);
                ocp.SetScale(Vector3.one * deathSize);
                ocp.SetPosition(
                   new Vector3(-1 * tileSize, yOffset, 8 * tileSize) - bounds
                   + new Vector3(tileSize / 2, 0, tileSize / 2) +
                   (Vector3.back * deathSpacing) * deadBlacks.Count);
            }
        }
        chessPieces[x, y] = cp;
        chessPieces[previousPostiton.x, previousPostiton.y] = null;
        PositionSinglePiece(x, y);
        return true;
    }
    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < Tile_X; x++)
        {
            for (int y = 0; y < Tile_Y; y++)
            {
                if (tiles[x, y] == hitInfo)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return -Vector2Int.one; //Invalid
    }


}