using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public string heightmapFileName;
    public int heightmapSize;
    public int terrainSizeInMeters;
    public int subDivision;
    private int tileSizeInPixels;
    private float tileSizeInMeters;
    public List<Tile> tiles = new List<Tile>();
    private float[,] heightData;
    private bool initDone = false;
    public int overlayHeightInMeters;

    // Use this for initialization
    void Start() {
        LoadHeightmapData();

        #region SetHeight

        tileSizeInPixels = (heightmapSize - 1) / subDivision;
        tileSizeInMeters = (float)terrainSizeInMeters / subDivision;
        Tile newTile;
        /// <summary>
        /// For an axis aligned square with top left at origin
        /// going right +Y
        /// goint bottom is +X
        /// 
        /// Height0 - center
        /// Height1 - top left
        /// Height2 - top right
        /// Height3 - bottom right
        /// Height4 - bottom left
        /// 
        /// tiles are getting filled first horizontally +Y
        /// then vertically +X
        /// </summary>
        float height0, height1, height2, height3, height4;
        for(int x = 0; x < subDivision; x++) {
            for(int y = 0; y < subDivision; y++) {
                height0 = heightData[(int)((x + 0.5f) * tileSizeInPixels), (int)((y + 0.5f) * tileSizeInPixels)];
                height1 = heightData[(x + 0) * tileSizeInPixels, (y + 0) * tileSizeInPixels];
                height2 = heightData[(x + 0) * tileSizeInPixels, (y + 1) * tileSizeInPixels];
                height3 = heightData[(x + 1) * tileSizeInPixels, (y + 1) * tileSizeInPixels];
                height4 = heightData[(x + 1) * tileSizeInPixels, (y + 0) * tileSizeInPixels];

                newTile = new Tile(x, y, height0, height1, height2, height3, height4);
                tiles.Add(newTile);
            }
        }
        #endregion

        #region SetNeighbour
        // neighbours counted in this order
        // 0 1 2
        // 3 x 4
        // 5 6 7

        List<Tile> neighbours = new List<Tile>();
        for(int i = 0; i < tiles.Count; i++) {
            neighbours.Clear();

            AddNeighbour(-1, -1, tiles[i], neighbours);    // top left
            AddNeighbour(-1, 0, tiles[i], neighbours);     // top
            AddNeighbour(-1, 1, tiles[i], neighbours);     // top right
            AddNeighbour(0, -1, tiles[i], neighbours);     // left
            AddNeighbour(0, 1, tiles[i], neighbours);      // right
            AddNeighbour(1, -1, tiles[i], neighbours);     // bottom left
            AddNeighbour(1, 0, tiles[i], neighbours);      // bottom
            AddNeighbour(1, 1, tiles[i], neighbours);      // bottom right

            tiles[i].SetNeighbours(neighbours);
        }
        #endregion
        initDone = true;
    }

    private void AddNeighbour(int deltaX, int deltaY, Tile currentTile, List<Tile> neighbourTiles) {
        int newX = currentTile.x + deltaX;
        int newY = currentTile.y + deltaY;

        if(newX < 0 || newX >= subDivision || newY < 0 || newY >= subDivision) {
            neighbourTiles.Add(new Tile(true));
        } else {
            int neighbourIndex = newX * subDivision + newY;
            neighbourTiles.Add(tiles[neighbourIndex]);
        }
    }

    // Update is called once per frame
    void Update() {
        if(Input.GetKeyDown(KeyCode.X)) {

        }
    }

    private void LoadHeightmapData() {
        int h = heightmapSize;
        int w = heightmapSize;
        heightData = new float[h, w];

        using(var file = System.IO.File.OpenRead("Assets/Heightmaps/" + heightmapFileName + ".raw"))
        using(var reader = new System.IO.BinaryReader(file)) {
            for(int x = 0; x < w; x++) {
                for(int y = 0; y < h; y++) {
                    float v = (float)reader.ReadUInt16() / 0xffff;
                    heightData[x, y] = v;

                    // top left is origin
                    // going right is +X
                    // going bottom is +Y
                    // v = 0 = black
                    // v = 1 = white
                }
            }
        }

    }

    private void OnDrawGizmos() {

        if(initDone) {
            for(int i = 0; i < tiles.Count; i++) {
                Gizmos.color = new Color(0, 0, tiles[i].height0, 0.0f);
                Vector3 origin = new Vector3(tiles[i].y * tileSizeInMeters + tileSizeInMeters / 2.0f, overlayHeightInMeters * 1.5f, (subDivision - tiles[i].x - 1) * tileSizeInMeters + tileSizeInMeters / 2.0f);
                Gizmos.DrawCube(origin, new Vector3(tileSizeInMeters, 100, tileSizeInMeters));
                // z value changed because of 90 degree offset due to coordinate system change of raw data file and xyz coordinate system

                Gizmos.color = new Color(1.0f, 0, 0, 1.0f);
                if(tiles[i].flowDirection != TileFlowDirection.None) {
                    int flowAngle = DirectionToAngle(tiles[i].flowDirection);
                    Vector3 directionVector = new Vector3(0, 0, tileSizeInMeters * 0.4f);
                    directionVector = Quaternion.Euler(0, flowAngle, 0) * directionVector;
                    Vector3 target = origin + directionVector;
                    Gizmos.DrawWireCube(origin, Vector3.one * tileSizeInMeters * 0.15f);
                    Gizmos.DrawLine(origin, target);
                }
            }
        }
    }

    private int DirectionToAngle(TileFlowDirection flowDirection) {
        if(flowDirection == TileFlowDirection.TopLeft)
            return -45;

        if(flowDirection == TileFlowDirection.Top)
            return 0;

        if(flowDirection == TileFlowDirection.TopRight)
            return 45;

        if(flowDirection == TileFlowDirection.Left)
            return -90;

        if(flowDirection == TileFlowDirection.Right)
            return 90;

        if(flowDirection == TileFlowDirection.BottomLeft)
            return -135;

        if(flowDirection == TileFlowDirection.Bottom)
            return 180;

        if(flowDirection == TileFlowDirection.BottomRight)
            return 135;

        return 0;
    }
}
