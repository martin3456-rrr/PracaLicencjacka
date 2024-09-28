using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum SpecialMove
{
    None = 0,
    EnPassant,
    Castling,
    Promotion
}
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
    [SerializeField] private GameObject victoryScreen;
    [SerializeField] private TMPro.TMP_Text Diagnostic;
    [SerializeField] private TMPro.TMP_Text textButtonChange;
    [SerializeField] private GameObject board2D;
    [SerializeField] private GameObject board3D;

    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Material[] teamMaterials;
   
    //LOGIC
    public ChessPiece[,] chessPieces;
    private ChessPiece currentlyDragging;
    private List<Vector2Int> availableMoves = new List<Vector2Int>();
    private List<ChessPiece> deadWhites = new List<ChessPiece>();
    private List<ChessPiece> deadBlacks = new List<ChessPiece>();
    private List<Vector2Int[]> moveList = new List<Vector2Int[]>();
    private List<ChessPiece> allPieces = new List<ChessPiece>();
    private const int Tile_X = 8;
    private const int Tile_Y = 8;
    private GameObject[,] tiles;
    private Camera currentCamera;
    private Vector2Int currentHover;
    private Vector3 bounds;
    public bool IsWhiteTeam;
    private bool IsWin = false;
    private bool is3D = true;
    private SpecialMove specialMove;

    //AI LOGIC CONNECTION
    [SerializeField] private bool AI_Enable = false;
    [SerializeField] private bool AI_Only = false;
    public List<ChessPiece> aiPiecesBlack = new List<ChessPiece>();
    public List<ChessPiece> aiPiecesWhite = new List<ChessPiece>();
    [SerializeField] private float timeToWait = 1;
    [SerializeField] private float timeWait = 1;
    //AI LOGIC CONNECTION END

    private void Awake()
    {
        IsWhiteTeam = true;
        GenerateAllFiles(tileSize, Tile_X, Tile_Y);
        SpawnAllPieces();
        PositionAllPieces();
    }
    private void Start()
    {
        if (!currentCamera)
            currentCamera = Camera.main;
        timeWait = timeToWait;
        AI_System.chessBoard = this;
    }
    public void ChangeGraphicMode()
    {
        is3D = !is3D;
        if (is3D)
        {
            textButtonChange.text = "3D";
            Camera.main.transform.rotation = Quaternion.Euler(new Vector3(70, 0, 0));
            Camera.main.transform.position = new Vector3(0f, 4f, -5f);
            Camera.main.orthographic = false;
            board3D.SetActive(true);
            board2D.SetActive(false);
            for (int i = 0; i < allPieces.Count; i++)
            {
                if (allPieces[i] != null)
                {
                    allPieces[i].ChangeTo3D();
                }
            }
           
        }
        else
        {
            textButtonChange.text = "2D";
            Camera.main.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
            Camera.main.transform.position = new Vector3(0f, 1f, 0f);
            Camera.main.orthographic = true;
            board3D.SetActive(false);
            board2D.SetActive(true);
            for (int i = 0; i < allPieces.Count; i++)
            {
                if (allPieces[i] != null)
                {
                    allPieces[i].ChangeTo2D();
                }
            }
        }
    }
    private void Update()
    {
        if (IsWin)
            return;
        // Player vs AI
        if (AI_Enable && (!IsWhiteTeam || AI_Only || Config.isMultiplayer))
        {
            checkValid = false;
            //AI vs AI
            if (AI_Only || Config.isMultiplayer)
            {
                  timeWait -= Time.deltaTime;
                  if (timeWait > 0)
                    return;
                  timeWait = timeToWait;
            }
            // SIMPLE AI
            AI_MoveGen.Move bestMove = AI_System.BestMove();
            if (bestMove == null)
                Debug.LogError("NO MOVE!");
            string text = " ";
            //Debug.Log("MoveGent Test: " + AI_System.MoveGenerationTest(3).ToString());
            Debug.Log("AI " + (IsWhiteTeam ? "White" : "Black") + " turn.");
            Debug.Log("MOVE: " + bestMove.piece.type.ToString() + " | " + bestMove.start.ToString() + " -> " + bestMove.target.ToString());
            text+= ("AI " + (IsWhiteTeam ? "White" : "Black") + " turn.");
            text+= ("MOVE: " + bestMove.piece.type.ToString() + " | " + bestMove.start.ToString() + " -> " + bestMove.target.ToString());
            Diagnostic.text = text;
            availableMoves = bestMove.piece.GetAvailableMoves(ref chessPieces, Tile_X, Tile_Y);
            specialMove = bestMove.piece.GetSpecialMoves(ref chessPieces, ref moveList, ref availableMoves);
            PreventCheck();
            if (!MoveTo(bestMove.piece, bestMove.target.x, bestMove.target.y))
                  Debug.LogError("Fail move!");
            CheckForCheckmate();
            checkValid = true;
            return;
        }

        // Player vs Player
        RaycastHit info;
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100, LayerMask.GetMask("Tile", "Hover", "Highlight")))
        {
            //Get the indexes of the tile i've hit(Uzyskaj indeksy kafelka, w który trafi³em)
            Vector2Int hitPosition = LookupTileIndex(info.transform.gameObject);

            //if we're hovering a tile after not hovering any tiles(jeœli naje¿d¿amy na kafelek po tym, jak nie naje¿d¿amy na ¿adne kafelki)
            if (currentHover == -Vector2Int.one)
            {
                currentHover = hitPosition;
                if (hitPosition.x >= 0 && hitPosition.x < tiles.GetLength(0) && hitPosition.y >= 0 && hitPosition.y < tiles.GetLength(1))
                    tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
            }

            //If we were already hoevering a tile, change the preovious one(Jeœli ju¿ unosiliœmy kafelek, zmieñ poprzedni¹)
            if (currentHover != hitPosition)
            {
                tiles[currentHover.x, currentHover.y].layer = LayerMask.NameToLayer("Tile");
                currentHover = hitPosition;
                if (hitPosition.x >= 0 && hitPosition.x < tiles.GetLength(0) && hitPosition.y >= 0 && hitPosition.y < tiles.GetLength(1))
                {
                    tiles[hitPosition.x, hitPosition.y].layer = LayerMask.NameToLayer("Hover");
                }
            }

            //If we press down on the mouse(Jeœli naciœniemy myszkê)
            if (Input.GetMouseButtonDown(0))
            {
                if (hitPosition.x >= 0 && hitPosition.x < chessPieces.GetLength(0) && hitPosition.y >= 0 && hitPosition.y < chessPieces.GetLength(1) && chessPieces[hitPosition.x, hitPosition.y] != null)
                {
                    if (chessPieces[hitPosition.x, hitPosition.y] != null)
                    {
                        //Is it our turn?(Czy to nasza kolej?)
                        if ((chessPieces[hitPosition.x, hitPosition.y].team == 0 && IsWhiteTeam) || (chessPieces[hitPosition.x, hitPosition.y].team == 1 && !IsWhiteTeam))
                        {
                            currentlyDragging = chessPieces[hitPosition.x, hitPosition.y];
                            //Get a list of where i can go, hightlight tiles as well(Uzyskaj listê miejsc, do których mogê siê udaæ, zaznacz równie¿ kafelki)
                            availableMoves = currentlyDragging.GetAvailableMoves(ref chessPieces, Tile_X, Tile_Y);
                            //Get a list of special moves as well(Uzyskaj równie¿ listê ruchów specjalnych)
                            specialMove = currentlyDragging.GetSpecialMoves(ref chessPieces, ref moveList, ref availableMoves);
                            PreventCheck();
                            HighlightTiles();
                        }
                    }
                }
            }
            //If we are releasing the mouse button(Jeœli zwalniamy przycisk myszy)
            if (currentlyDragging != null && Input.GetMouseButtonUp(0))
            {
                Vector2Int previousPosition = new Vector2Int(currentlyDragging.X, currentlyDragging.Y);
                bool validMove = MoveTo(currentlyDragging, hitPosition.x, hitPosition.y);
                if (!validMove)
                {
                    currentlyDragging.SetPosition(GetTileCenter(previousPosition.x, previousPosition.y));
                    string text = " ";
                    Debug.Log("Player " + (IsWhiteTeam ? "White" : "Black") + " turn.");
                    Debug.Log("MOVE: " + currentlyDragging.type.ToString() + " | " + previousPosition.ToString() + " -> " + new Vector2Int(hitPosition.x, hitPosition.y).ToString());
                    text += ("Player " + (IsWhiteTeam ? "White" : "Black") + " turn.");
                    text += ("MOVE: " + currentlyDragging.type.ToString() + " | " + previousPosition.ToString() + " -> " + new Vector2Int(hitPosition.x, hitPosition.y).ToString());
                    Diagnostic.text = text;
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
                tiles[currentHover.x, currentHover.y].layer = (ContainsValidMove(ref availableMoves, currentHover)) ? LayerMask.NameToLayer("Hightlight") : LayerMask.NameToLayer("Tile");
                currentHover = -Vector2Int.one;
            }

            if (currentlyDragging && Input.GetMouseButtonUp(0))
            {
                currentlyDragging.SetPosition(GetTileCenter(currentlyDragging.X, currentlyDragging.Y));
                currentlyDragging = null;
                RemoveHighlightTiles();
            }
        }
        //If we're dragging a piece(Jeœli przeci¹gamy kawa³ek)
        if (currentlyDragging)
        {
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * yOffset);
            float distance = 0.0f;
            if (horizontalPlane.Raycast(ray, out distance))
            {
                currentlyDragging.SetPosition(ray.GetPoint(distance) + Vector3.up * dragOffset);
            }
        }
    }
    public bool IsCheck(bool IsWhite)
    {
        List<ChessPiece> pieces = (IsWhite ? aiPiecesWhite : aiPiecesBlack);
        foreach (ChessPiece piece in pieces)
        {
            // Sprawdzenie, czy dana figura mo¿e wykonaæ ruch, który spowoduje szach
            if (AI_MoveGen.CanMove(ref chessPieces, ref pieces, piece, isWhiteTeam: IsWhite))
            {
                return true;
            }
        }
        return false;
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
    //Spawing of the pieces(Tarcie kawa³ków)
    private void SpawnAllPieces()
    {
        chessPieces = new ChessPiece[Tile_X, Tile_Y];
        int whiteTeam = 0, blackTeam = 1;
        //WhiteTeam
        chessPieces[0, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        chessPieces[1, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[2, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[3, 0] = SpawnSinglePiece(ChessPieceType.Queen, whiteTeam);
        chessPieces[4, 0] = SpawnSinglePiece(ChessPieceType.King, whiteTeam);
        chessPieces[5, 0] = SpawnSinglePiece(ChessPieceType.Bishop, whiteTeam);
        chessPieces[6, 0] = SpawnSinglePiece(ChessPieceType.Knight, whiteTeam);
        chessPieces[7, 0] = SpawnSinglePiece(ChessPieceType.Rook, whiteTeam);
        for (int i = 0; i < Tile_X; i++)
            chessPieces[i, 1] = SpawnSinglePiece(ChessPieceType.Pawn, whiteTeam);

        //BlackTeam
        chessPieces[0, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        chessPieces[1, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[2, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[3, 7] = SpawnSinglePiece(ChessPieceType.Queen, blackTeam);
        chessPieces[4, 7] = SpawnSinglePiece(ChessPieceType.King, blackTeam);
        chessPieces[5, 7] = SpawnSinglePiece(ChessPieceType.Bishop, blackTeam);
        chessPieces[6, 7] = SpawnSinglePiece(ChessPieceType.Knight, blackTeam);
        chessPieces[7, 7] = SpawnSinglePiece(ChessPieceType.Rook, blackTeam);
        for (int i = 0; i < Tile_X; i++)
            chessPieces[i, 6] = SpawnSinglePiece(ChessPieceType.Pawn, blackTeam);

        // AI pieces lists
        aiPiecesWhite.Add(chessPieces[0, 0]);
        aiPiecesWhite.Add(chessPieces[1, 0]);
        aiPiecesWhite.Add(chessPieces[2, 0]);
        aiPiecesWhite.Add(chessPieces[3, 0]);
        aiPiecesWhite.Add(chessPieces[4, 0]);
        aiPiecesWhite.Add(chessPieces[5, 0]);
        aiPiecesWhite.Add(chessPieces[6, 0]);
        aiPiecesWhite.Add(chessPieces[7, 0]);
        for (int i = 0; i < Tile_X; i++)
            aiPiecesWhite.Add(chessPieces[i, 1]);

        aiPiecesBlack.Add(chessPieces[0, 7]);
        aiPiecesBlack.Add(chessPieces[1, 7]);
        aiPiecesBlack.Add(chessPieces[2, 7]);
        aiPiecesBlack.Add(chessPieces[3, 7]);
        aiPiecesBlack.Add(chessPieces[4, 7]);
        aiPiecesBlack.Add(chessPieces[5, 7]);
        aiPiecesBlack.Add(chessPieces[6, 7]);
        aiPiecesBlack.Add(chessPieces[7, 7]);
        for (int i = 0; i < Tile_X; i++)
            aiPiecesBlack.Add(chessPieces[i, 6]);

        for (int i = 0; i < aiPiecesWhite.Count; ++i) allPieces.Add(aiPiecesWhite[i]);
        for (int i = 0; i < aiPiecesBlack.Count; ++i) allPieces.Add(aiPiecesBlack[i]);
    }
    private ChessPiece SpawnSinglePiece(ChessPieceType type, int team)
    {
        ChessPiece cp = Instantiate(prefabs[(int)type - 1], transform).GetComponent<ChessPiece>();
        cp.type = type;
        cp.team = team;
        cp.GetComponent<MeshRenderer>().material = teamMaterials[team];
        cp.InitGraphics(team, teamMaterials[team]);
        return cp;
    }
    //Positioning
    private void PositionAllPieces()
    {
        for (int x = 0; x < Tile_X; x++)
        {
            for (int y = 0; y < Tile_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    PositionSinglePiece(x, y, true);
                }
            }
        }
    }
    private void PositionSinglePiece(int x, int y, bool force = false)
    {
        chessPieces[x, y].X = x;
        chessPieces[x, y].Y = y;
        chessPieces[x, y].SetPosition(GetTileCenter(x, y), force);
    }
    private Vector3 GetTileCenter(int x, int y)
    {
       return new Vector3(x * tileSize, yOffset, y * tileSize) - bounds + new Vector3(tileSize / 2, 0, tileSize / 2);
    }
    //Highlight Tiles
    private void HighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Highlight");
    }
    private void RemoveHighlightTiles()
    {
        for (int i = 0; i < availableMoves.Count; i++)
            tiles[availableMoves[i].x, availableMoves[i].y].layer = LayerMask.NameToLayer("Tile");
        availableMoves.Clear();
    }
    //Checkmate
    private void ChessMate(int team)
    {
        DisplayVicotry(team);
    }
    private void DisplayVicotry(int winnigTeam)
    {
        victoryScreen.SetActive(true);
        victoryScreen.transform.GetChild(winnigTeam).gameObject.SetActive(true);
        IsWin = true;
    }
    public void OnResetButton()
    {
        //UI
        victoryScreen.transform.GetChild(0).gameObject.SetActive(false);
        victoryScreen.transform.GetChild(1).gameObject.SetActive(false);
        victoryScreen.transform.GetChild(2).gameObject.SetActive(false);
        victoryScreen.SetActive(false);

        //Fields reset
        currentlyDragging = null;
        availableMoves.Clear();
        moveList.Clear();
  
        //Clean up
        for (int x = 0; x < Tile_X; x++)
        {
            for (int y = 0; y < Tile_Y; y++)
            {
                if (chessPieces[x, y] != null)
                    Destroy(chessPieces[x, y].gameObject);
                chessPieces[x, y] = null;
            }
        }

        for (int i = 0; i < deadWhites.Count; i++)
            Destroy(deadWhites[i].gameObject);

        for (int i = 0; i < deadBlacks.Count; i++)
            Destroy(deadBlacks[i].gameObject);
 
        deadWhites.Clear();
        deadBlacks.Clear();
        aiPiecesWhite.Clear();
        aiPiecesBlack.Clear();
        SpawnAllPieces();
        PositionAllPieces();
        IsWhiteTeam = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    //Special Moves
    private void ProcessSpecialMove()
    {
        if (specialMove == SpecialMove.EnPassant)
        {
            var newMove = moveList[moveList.Count - 1];
            ChessPiece myPawn = chessPieces[newMove[1].x, newMove[1].y];
            var targetPawnPosition = moveList[moveList.Count - 2];
            ChessPiece enemyPawn = chessPieces[targetPawnPosition[1].x, targetPawnPosition[1].y];

            if (myPawn.X == enemyPawn.X)
            {
                if (myPawn.Y == enemyPawn.Y - 1 || myPawn.Y == enemyPawn.Y + 1)
                {
                    if (enemyPawn.team == 0)
                    {
                        deadWhites.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deathSize);
                        enemyPawn.SetPosition(
                            new Vector3(8 * tileSize, yOffset, -1 * tileSize) - bounds
                            + new Vector3(tileSize / 2, 0, tileSize / 2) +
                            (Vector3.forward * deathSpacing) * deadWhites.Count);
                    }
                    else
                    {
                        deadBlacks.Add(enemyPawn);
                        enemyPawn.SetScale(Vector3.one * deathSize);
                        enemyPawn.SetPosition(
                           new Vector3(-1 * tileSize, yOffset, 8 * tileSize) - bounds
                           + new Vector3(tileSize / 2, 0, tileSize / 2) +
                           (Vector3.back * deathSpacing) * deadBlacks.Count);
                    }
                    chessPieces[enemyPawn.X, enemyPawn.Y] = null;
                }
            }
        }

        if (specialMove == SpecialMove.Promotion)
        {
            var lastMove = moveList[moveList.Count - 1];
            ChessPiece targetPawn = chessPieces[lastMove[1].x, lastMove[1].y];

            if (targetPawn.type == ChessPieceType.Pawn)
            {
                //White team
                if (targetPawn.team == 0 && lastMove[1].y == 7)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 0);
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }
                //Black team
                if (targetPawn.team == 1 && lastMove[1].y == 0)
                {
                    ChessPiece newQueen = SpawnSinglePiece(ChessPieceType.Queen, 1);
                    newQueen.transform.position = chessPieces[lastMove[1].x, lastMove[1].y].transform.position;
                    Destroy(chessPieces[lastMove[1].x, lastMove[1].y].gameObject);
                    chessPieces[lastMove[1].x, lastMove[1].y] = newQueen;
                    PositionSinglePiece(lastMove[1].x, lastMove[1].y);
                }
            }
        }
        if (specialMove == SpecialMove.Castling)
        {
            var lastMove = moveList[moveList.Count - 1];

            if (lastMove[1].x == 2)
            { // Left Rook
                if (lastMove[1].y == 0)
                { // White side
                    ChessPiece rook = chessPieces[0, 0];
                    chessPieces[3, 0] = rook;
                    PositionSinglePiece(3, 0);
                    chessPieces[0, 0] = null;
                }
                else if (lastMove[1].y == 7)
                { // Black side
                    ChessPiece rook = chessPieces[0, 7];
                    chessPieces[3, 7] = rook;
                    PositionSinglePiece(3, 7);
                    chessPieces[0, 7] = null;
                }
            }
            else if (lastMove[1].x == 6)
            { // Right Rook
                if (lastMove[1].y == 0)
                { // White side
                    ChessPiece rook = chessPieces[7, 0];
                    chessPieces[5, 0] = rook;
                    PositionSinglePiece(5, 0);
                    chessPieces[7, 0] = null;
                }
                else if (lastMove[1].y == 7)
                { // Black side
                    ChessPiece rook = chessPieces[7, 7];
                    chessPieces[5, 7] = rook;
                    PositionSinglePiece(5, 7);
                    chessPieces[7, 7] = null;
                }
            }
        }
        
    }
    private bool PreventCheck()
    {
        ChessPiece targetKing = null;
        bool isChecked = false;
        for (int x = 0; x < Tile_X; x++)
        {
            for (int y = 0; y < Tile_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    if (chessPieces[x, y].type == ChessPieceType.King)
                    {
                        if (chessPieces[x, y].team == currentlyDragging?.team)
                        {
                            targetKing = chessPieces[x, y];
                        }
                    }
                }

            }
        }
        for (int x = 0; x < Tile_X; x++)
        {
            for (int y = 0; y < Tile_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    if (chessPieces[x, y] != null && chessPieces[x, y].team != currentlyDragging?.team && chessPieces[x, y].type != ChessPieceType.King)
                    {
                        if (IsInCheck(chessPieces[x, y], targetKing))
                        {
                            isChecked = true;
                        }
                    }

                    // sprawdŸ, czy król obecnego zespo³u jest pod szachowaniem przez przeciwnika
                    if (chessPieces[x, y] != null && chessPieces[x, y].team != currentlyDragging?.team && chessPieces[x, y].type == ChessPieceType.King)
                    {
                        if (IsInCheck(chessPieces[x, y], targetKing))
                        {
                            isChecked = true;
                        }
                    }
                }
            }
        }
        return isChecked;

    }
    public bool IsInCheck(ChessPiece attacker, ChessPiece target)
    {
        bool canAttack = false;

        //sprawdŸ, czy pionek mo¿e atakowaæ w poziomie
        if (attacker != null && target != null && attacker.Y == target.Y)
        {
            if (attacker.X > target.X)
            {
                for (int x = attacker.X - 1; x > target.X; x--)
                {
                    if (chessPieces[x, attacker.Y] != null)
                    {
                        return false;
                    }
                }
                canAttack = true;
            }
            else
            {
                for (int x = attacker.X + 1; x < target.X; x++)
                {
                    if (chessPieces[x, attacker.Y] != null)
                    {
                        return false;
                    }
                }
                canAttack = true;
            }
        }

        //sprawdŸ, czy pionek mo¿e atakowaæ w pionie
        if (attacker != null && target != null && attacker.X == target.X)
        {
            if (attacker.Y > target.Y)
            {
                for (int y = attacker.Y - 1; y > target.Y; y--)
                {
                    if (chessPieces[attacker.X, y] != null)
                    {
                        return false;
                    }
                }
                canAttack = true;
            }
            else
            {
                for (int y = attacker.Y + 1; y < target.Y; y++)
                {
                    if (chessPieces[attacker.X, y] != null)
                    {
                        return false;
                    }
                }
                canAttack = true;
            }
        }

        //sprawdŸ, czy pionek mo¿e atakowaæ po skosie
        if (attacker != null && target != null && Math.Abs(attacker.X - target.X) == Math.Abs(attacker.Y - target.Y))
        {
            if (attacker.X > target.X && attacker.Y > target.Y)
            {
                int x = attacker.X - 1;
                int y = attacker.Y - 1;
                while (x > target.X && y > target.Y)
                {
                    if (chessPieces[x, y] != null)
                    {
                        return false;
                    }
                    x--;
                    y--;
                }
                // sprawdŸ, czy wszystkie pola pomiêdzy atakuj¹cym a celem s¹ puste
                if (CheckIfPathIsClear(attacker, target))
                    canAttack = true;
            }
            else if (attacker.X < target.X && attacker.Y > target.Y)
            {
                int x = attacker.X + 1;
                int y = attacker.Y - 1;
                while (x < target.X && y > target.Y)
                {
                    if (chessPieces[x, y] != null)
                    {
                        return false;
                    }
                    x++;
                    y--;
                }
                // sprawdŸ, czy wszystkie pola pomiêdzy atakuj¹cym a celem s¹ puste
                if (CheckIfPathIsClear(attacker, target))
                    canAttack = true;
            }
            else if (attacker.X > target.X && attacker.Y < target.Y)
            {
                int x = attacker.X - 1;
                int y = attacker.Y + 1;
                while (x > target.X && y < target.Y)
                {
                    if (chessPieces[x, y] != null)
                    {
                        return false;
                    }
                    x--;
                    y++;
                }
                // sprawdŸ, czy wszystkie pola pomiêdzy atakuj¹cym a celem s¹ puste
                if (CheckIfPathIsClear(attacker, target))
                    canAttack = true;
            }
            else if (attacker.X < target.X && attacker.Y < target.Y)
            {
                int x = attacker.X + 1;
                int y = attacker.Y + 1;
                while (x < target.X && y < target.Y)
                {
                    if (chessPieces[x, y] != null)
                    {
                        return false;
                    }
                    x++;
                    y++;
                }
                // sprawdŸ, czy wszystkie pola pomiêdzy atakuj¹cym a celem s¹ puste
                if (CheckIfPathIsClear(attacker, target))
                    canAttack = true;
            }
        }

        return canAttack;
    }
    private bool CheckIfPathIsClear(ChessPiece attacker, ChessPiece target)
    {
        if (attacker.X == target.X && attacker.Y == target.Y)
            return false;

        int x = attacker.X;
        int y = attacker.Y;
        int xDir = (target.X > attacker.X) ? 1 : -1;
        int yDir = (target.Y > attacker.Y) ? 1 : -1;

        while (x != target.X)
        {
            x += xDir;
            if (chessPieces[x, y] != null)
                return false;
        }
        while (y != target.Y)
        {
            y += yDir;
            if (chessPieces[x, y] != null)
                return false;
        }
        return true;
    }
    private void SimulateMoveForSinglePiece(ChessPiece cp, ref List<Vector2Int> moves, ChessPiece targetKing)
    {
        // Save the current values,to reset after the function call(Zapisz bie¿¹ce wartoœci, aby zresetowaæ po wywo³aniu funkcji!)
        int actualX = cp.X;
        int actulaY = cp.Y;
        List<Vector2Int> movesToRemove = new List<Vector2Int>();

        // Going through all the moves,simulate them and check if we're in check(Przechodz¹c przez wszystkie ruchy, zasymuluj je i sprawdŸ, czy jesteœmy w szachu)
        for (int i = 0; i < moves.Count; i++)
        {
            int simX = moves[i].x;
            int simY = moves[i].y;

            Vector2Int kingPositionThisSim = new Vector2Int(targetKing.X, targetKing.Y);
            //Did we simulate the king's move(Czy symulowaliœmy ruch króla?)
            if (cp.type == ChessPieceType.King)
                kingPositionThisSim = new Vector2Int(simX, simY);

            // Copy the[,] and not a reference(Skopiuj [,], a nie odniesienie)
            ChessPiece[,] simulation = new ChessPiece[Tile_X, Tile_Y];
            List<ChessPiece> simAttackingPieces = new List<ChessPiece>();
            for (int x = 0; x < Tile_X; x++)
            {
                for (int y = 0; y < Tile_Y; y++)
                {
                    if (chessPieces[x, y] != null)
                    {
                        simulation[x, y] = chessPieces[x, y];
                        if (simulation[x, y].team != cp.team)
                        {
                            simAttackingPieces.Add(simulation[x, y]);
                        }
                    }
                }
            }
            //Simulate that move(Symuluj ten ruch)
            simulation[actualX, actulaY] = null;
            cp.X = simX;
            cp.Y = simY;
            simulation[simX, simY] = cp;

            // Did one of the piece got taken down during our simulation(Czy jeden z elementów zosta³ usuniêty podczas naszej symulacji?)
            var deadPiece = simAttackingPieces.Find(c => c.X == simX && c.Y == simY);
            if (deadPiece != null)
                simAttackingPieces.Remove(deadPiece);

            // Get all the simulated attacking pieces moves(Zdob¹dŸ wszystkie symulowane ruchy pionków ataku)
            List<Vector2Int> simMoves = new List<Vector2Int>();
            for (int a = 0; a < simAttackingPieces.Count; a++)
            {
                var pieceMoves = simAttackingPieces[a].GetAvailableMoves(ref simulation, Tile_X, Tile_Y);
                for (int b = 0; b < pieceMoves.Count; b++)
                    simMoves.Add(pieceMoves[b]);
            }
            // Is the king in trouble? if so, remove the move(Czy król ma k³opoty? jeœli tak, usuñ ruch)
            if (ContainsValidMove(ref simMoves, kingPositionThisSim))
                movesToRemove.Add(moves[i]);

            // Restore the actual CP data(Przywróæ rzeczywiste dane CP)
            cp.X = actualX;
            cp.Y = actulaY;
        }

        // Remove from the current available move list(Usuñ z bie¿¹cej dostêpnej listy ruchów)
        for (int i = 0; i < movesToRemove.Count; i++)
            moves.Remove(movesToRemove[i]);
    }

    private bool CheckForCheckmate()
    {
        var lastMove = moveList[moveList.Count - 1];
        int targetTeam = (chessPieces[lastMove[1].x, lastMove[1].y].team == 0) ? 1 : 0;
        List<ChessPiece> attackingPieces = new List<ChessPiece>();
        List<ChessPiece> defendingPieces = new List<ChessPiece>();
        ChessPiece targetKing = null;
        for (int x = 0; x < Tile_X; x++)
        {
            for (int y = 0; y < Tile_Y; y++)
            {
                if (chessPieces[x, y] != null)
                {
                    if (chessPieces[x, y].team == targetTeam)
                    {
                        defendingPieces.Add(chessPieces[x, y]);
                        if (chessPieces[x, y].type == ChessPieceType.King)
                            targetKing = chessPieces[x, y];
                    }
                    else
                    {
                        attackingPieces.Add(chessPieces[x, y]);
                    }
                }
            }
        }

        // Is the king attacked right now(Czy król jest teraz atakowany?)
        List<Vector2Int> currentAvailabeMoves = new List<Vector2Int>();
        for (int i = 0; i < attackingPieces.Count; i++)
        {
            var pieceMoes = attackingPieces[i].GetAvailableMoves(ref chessPieces, Tile_X, Tile_Y);
            for (int b = 0; b < pieceMoes.Count; b++)
                currentAvailabeMoves.Add(pieceMoes[b]);
        }

        if (targetKing == null)
            return false;

        // Are we in check right now?
        if (ContainsValidMove(ref currentAvailabeMoves, new Vector2Int(targetKing.X, targetKing.Y)))
        {
            Debug.Log("Krol jest szachowany");
            // King is under attack, can we move something to help him?(King jest atakowany, czy mo¿emy coœ przesun¹æ, ¿eby mu pomóc?)
            for (int i = 0; i < defendingPieces.Count; i++)
            {
                List<Vector2Int> defendingMoves = defendingPieces[i].GetAvailableMoves(ref chessPieces, Tile_X, Tile_Y);
                // Since we're sending ref availableMoves, we will be deleting moves that are putting us in check(Poniewa¿ wysy³amy ref availableMoves, bêdziemy usuwaæ ruchy, które wystawiaj¹ nas na próbê)
                SimulateMoveForSinglePiece(defendingPieces[i], ref defendingMoves, targetKing);
                if (defendingMoves.Count != 0)
                    return false;
            }
            return true; //Checkmate exit
        }
        return false;
    }

    //Operations
    private bool ContainsValidMove(ref List<Vector2Int> moves, Vector2Int pos)
    {
        for (int i = 0; i < moves.Count; i++)
            if (moves[i].x == pos.x && moves[i].y == pos.y)
                return true;
        return false;
    }

    bool checkValid = true;
    private bool MoveTo(ChessPiece cp, int x, int y)
    {
        if (checkValid)
            if (!ContainsValidMove(ref availableMoves, new Vector2Int(x, y)))
                return false;
        Vector2Int previousPostiton = new Vector2Int(cp.X, cp.Y);
        // Is there another piece on the target position(Czy na pozycji docelowej jest inny kawa³ek)
        if (chessPieces[x, y] != null)
        {
            ChessPiece ocp = chessPieces[x, y];
            if (cp.team == ocp.team)
                return false;
            //If its the enemy team(Jeœli to dru¿yna wroga)
            if (ocp.team == 0)
            {
                if (ocp.type == ChessPieceType.King)
                    ChessMate(1);
                deadWhites.Add(ocp);
                aiPiecesWhite.Remove(ocp);
                ocp.SetScale(Vector3.one * deathSize);
                ocp.SetPosition(
                    new Vector3(8 * tileSize, yOffset, -1 * tileSize) - bounds
                    + new Vector3(tileSize / 2, 0, tileSize / 2) +
                    (Vector3.forward * deathSpacing) * deadWhites.Count);
            }
            else
            {
                if (ocp.type == ChessPieceType.King)
                    ChessMate(0);
                deadBlacks.Add(ocp);
                aiPiecesBlack.Remove(ocp);
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
        IsWhiteTeam = !IsWhiteTeam;
        moveList.Add(new Vector2Int[] { previousPostiton, new Vector2Int(x, y) });
        ProcessSpecialMove();
        if (CheckForCheckmate())
            ChessMate(cp.team);
        else if (CheckForStalemate())
            Stalemate();
        return true;
    }
    private bool CheckForStalemate()
    {
        int lastMoveCount = moveList.Count;
        // SprawdŸ zasadê 50 ruchów
        if (lastMoveCount >= 100)
        {
            bool allMovesPawnless = true;
            for (int i = lastMoveCount - 1; i >= lastMoveCount - 50; i--)
            {
                if (IsPawnMoveOrCapture(moveList[i]))
                {
                    allMovesPawnless = false;
                    break;
                }
            }

            if (allMovesPawnless)
            {
                Debug.Log("Zasada 50 ruchów zosta³a spe³niona. Remis przez pat!");
                Stalemate();
                return true;
            }
        }
        // SprawdŸ zasadê 75 ruchów narzucon¹ przez arbitra
        if (lastMoveCount >= 150 && lastMoveCount % 2 == 0)
        {
            bool noPawnMovesOrCaptures = true;
            for (int i = lastMoveCount - 1; i >= lastMoveCount - 75; i--)
            {
                if (IsPawnMoveOrCapture(moveList[i]))
                {
                    noPawnMovesOrCaptures = false;
                    break;
                }
            }
            if (noPawnMovesOrCaptures)
            {
                Debug.Log("Zasada 75 ruchów zosta³a spe³niona. Remis przez pat!");
                Stalemate();
                return true;
            }
        }
        return false;
    }
    private bool IsPawnMoveOrCapture(Vector2Int[] move)
	{
		ChessPiece piece = chessPieces[move[0].x, move[0].y];
		if (piece == null)
			return false;

		// Check if the piece is a pawn or if the move results in a capture
		if (piece.type == ChessPieceType.Pawn || chessPieces[move[1].x, move[1].y] != null)
			return true;

		return false;
	}
    private void Stalemate()
    {
        Debug.Log("Partia zakoñczona remisem - pat!");
        ChessMate(2);
    }
    private List<ChessPiece> backupList = new List<ChessPiece>();
    public void MakeMove(AI_MoveGen.Move move)
    {
        ChessPiece ocp = chessPieces[move.target.x, move.target.y];

        if (ocp != null)
        {
            if (ocp.team == 0)
                aiPiecesWhite.Remove(ocp);
            else
                aiPiecesBlack.Remove(ocp);
        }

        backupList.Add(ocp);
        chessPieces[move.target.x, move.target.y] = move.piece;
        chessPieces[move.start.x, move.start.y] = null;
        IsWhiteTeam = !IsWhiteTeam;
    }

    public void UnmakeMove(AI_MoveGen.Move move)
    {
        ChessPiece ocp = null;
        if (backupList.Count > 0)
        {
            ocp = backupList[backupList.Count - 1];
            backupList.RemoveAt(backupList.Count - 1);
        }

        chessPieces[move.target.x, move.target.y] = ocp;
        if (ocp)
        {
            if (ocp.team == 0)
                aiPiecesWhite.Add(ocp);
            else
                aiPiecesBlack.Add(ocp);
        }
        chessPieces[move.start.x, move.start.y] = move.piece;

        IsWhiteTeam = !IsWhiteTeam;
    }

    private Vector2Int LookupTileIndex(GameObject hitInfo)
    {
        for (int x = 0; x < Tile_X; x++)
            for (int y = 0; y < Tile_Y; y++)
                if (tiles[x, y] == hitInfo)
                    return new Vector2Int(x, y);
        return -Vector2Int.one; //Invalid
    }
}