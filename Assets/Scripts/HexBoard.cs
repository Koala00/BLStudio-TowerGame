using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Builds the board mesh and handles mouse events.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class HexBoard : MonoBehaviour
{
    public const float TileScale = 1f;

    public GameObject Cursor;
    public GameObject Canvas;

    private GridPositionElements Positions;

    private int SizeX;
    private int SizeZ;
    private int TileWidth;
    private int TileHeight;

    // Use this for initialization
    void Awake()
    {
        Positions = GetComponent<GridPositionElements>();
        SizeX = GlobalSettings.Instance.gameRuleSettings.Board.Width;
        SizeZ = GlobalSettings.Instance.gameRuleSettings.Board.Height;
        BuildTexture();
        BuildMesh();
    }

    private void BuildTexture()
    {
        var renderer = GetComponent<MeshRenderer>();
        //var tileMaterial = Resources.Load("TileMat") as Material;
        //var oneTileTexture = tileMaterial.mainTexture as Texture2D;
        var oneTileTexture = renderer.sharedMaterial.mainTexture as Texture2D;
        TileWidth = oneTileTexture.width;
        TileHeight = oneTileTexture.height;
        if (!Application.isPlaying)
            return;
        // Create a new texture with a tile for each player plus the original tile for empty fields.
        // Each player tile is the empty field tile with the player color.
        var multiTileTexture = new Texture2D(TileWidth * (Player.Count + 1), TileHeight);
        Color gridColor;
        Color.TryParseHexString("3f3f3f", out gridColor);
        for (int player = 0; player <= Player.Count; player++)
        {
            var pixels = oneTileTexture.GetPixels();
            if (player < Player.Count) // The last tile keeps initial colors for empty fields.
                for (int i = 0; i < pixels.Length; i++)
                    if (pixels[i] != gridColor)
                      pixels[i] = Color.Lerp(pixels[i], GameRuleSettings.players_color[player], 0.5f);
            multiTileTexture.SetPixels(player * TileWidth, 0, TileWidth, TileHeight, pixels);
        }
        multiTileTexture.Apply();
        renderer.material.mainTexture = multiTileTexture;
    }

    public static readonly Vector2[] CornerToUV =
        {
            new Vector2(1f, 0.75f),
            new Vector2(0.5f, 1f),
            new Vector2(0f, 0.75f),
            new Vector2(0f, 0.25f),
            new Vector2(0.5f, 0f),
            new Vector2(1f, 0.25f)
    };

    private void BuildMesh()
    {
        /*
        1. Loop through hex grid in offset-coordinates (http://www.redblobgames.com/grids/hexagons/#map-storage)
        2. For each hex pos, get coordinates of each corner
        3. Use hash table to get vertex index of corner, store corner with index if not found.
        4. Create triangles based on vertex indices stored in hash table.
        */

        int numTiles = SizeX * SizeZ;
        int numTriangles = numTiles * 4;
        //var verticesMap = new Dictionary<Vector3, int>();
        var vertices = new Vector3[numTiles * 6];
        var triangles = new int[numTriangles * 3];
        var hexes = new HexCoord[numTiles];
        var uvMap = new Dictionary<int, Vector2>();

        int triIndex = 0;
        int vIndex = 0;
        int x, z, h = 0;
        for (x = 0; x < SizeX; x++)
            for (z = 0; z < SizeZ; z++)
            {
                hexes[h] = HexCoord.FromOffsetOddR(x, z);
                var corners = hexes[h].Corners().Select(p => new Vector3(p.x * TileScale, 0, p.y * TileScale)).ToArray();
                triangles[triIndex++] = vIndex + 0;
                triangles[triIndex++] = vIndex + 5;
                triangles[triIndex++] = vIndex + 4;

                triangles[triIndex++] = vIndex + 0;
                triangles[triIndex++] = vIndex + 4;
                triangles[triIndex++] = vIndex + 3;

                triangles[triIndex++] = vIndex + 0;
                triangles[triIndex++] = vIndex + 3;
                triangles[triIndex++] = vIndex + 1;

                triangles[triIndex++] = vIndex + 1;
                triangles[triIndex++] = vIndex + 3;
                triangles[triIndex++] = vIndex + 2;

                int cornerIndex = 0;
                foreach (var v in corners)
                {
                    vertices[vIndex] = v;
                    var uv = GetUvForCornerAndPlayer(cornerIndex, Player.NoPlayer);
                    uvMap.Add(vIndex, uv);
                    vIndex++;
                    cornerIndex++;
                }
                h++;
            }

        var mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.normals = vertices.Select(i => Vector3.up).ToArray();
        mesh.uv = uvMap.OrderBy(pair => pair.Key).Select(pair => pair.Value).ToArray();
        mesh.triangles = triangles;
        var meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        var meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
    }

    private Vector2 GetUvForCornerAndPlayer(int cornderIndex, int player)
    {
        var uv = CornerToUV[cornderIndex];
        if (Application.isPlaying)
        {
            // The tile of each player has the same position in the tile map as the player index. The NoPlayer tile is the last tile.
            int tileIndex = player == Player.NoPlayer ? Player.Count : player;
            // Do the UV mapping of the corner to the point in the tile of the given player.
            // First we scale the initial UV to the  width of the tile map.
            float numTiles = Player.Count + 1;
            uv.x /= numTiles;
            // Then we translate the position to the correct tile.
            uv.x += tileIndex / numTiles;
        }
        return uv;
    }

    public void SetTilePlayerColor(HexCoord position, int player)
    {
        var meshFilter = GetComponent<MeshFilter>();
        var mesh = meshFilter.sharedMesh;
        var vertices = mesh.vertices;
        var uv = mesh.uv;
        // Find the indices of the vertices of 
        var v = position.Corner3d(0) * TileScale;
        int i;
        for (i = 0; i < vertices.Length && vertices[i] != v; i++) ;
        if (i == vertices.Length)
            return; // vertex not found -> position probably not on board
        for (int corner = 0; corner < 6; corner++, i++)
        {
            uv[i] = GetUvForCornerAndPlayer(corner, player);            
        }
        mesh.uv = uv;
    }

    void Update()
    {
        HandleCursor();
    }

    private void HandleCursor()
    {
        if (!Input.mousePresent)
            return;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var collider = GetComponent<Collider>();
        RaycastHit hit;
        if (collider.Raycast(ray, out hit, Mathf.Infinity))
        {
            var p = transform.InverseTransformPoint(hit.point);
            float x = p.x / HexBoard.TileScale;
            float z = p.z / HexBoard.TileScale;
            var hex = HexCoord.AtPosition(new Vector2(x, z));
            var v = hex.Position3d() * HexBoard.TileScale;
            v = transform.TransformPoint(v);
            v.y = Cursor.transform.position.y;
            Cursor.transform.position = v;
            if (Input.GetMouseButtonDown(0))
                OnFieldSelected(hex);
        }
    }

    public bool IsPositionOnBoard(HexCoord position)
    {
        var point = position.OddRToOffset();
        return point.x >= 0 && point.x < SizeX && point.y >= 0 && point.y < SizeZ;
    }

    private void UpdateUi()
    {
        ExecuteEvents.Execute<IUpdateUi>(Canvas, null, (msg, data) => msg.SetCurrentPlayer());
        int[] scores = Positions.GetNumberOfControlledPositionsPerPlayer();
        ExecuteEvents.Execute<IUpdateUi>(Canvas, null, (msg, data) => msg.SetScores(scores));
    }

    private void OnFieldSelected(HexCoord position)
    {
        var gridTowers = GetComponent<GridTowers>();
        if (gridTowers.CreateTower(position, Player.Current))
            EndTurn();
    }

    private void EndTurn()
    {
        Positions.UpdateColors();
        var gridTowers = GetComponent<GridTowers>();
        gridTowers.EndTurn();
        Player.NextPlayer();
        UpdateUi();
        ExecuteEvents.Execute<IHandleEndTurn>(Canvas, null, (msg, data) => msg.EndTurn());
    }

}
