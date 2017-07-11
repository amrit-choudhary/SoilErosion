using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public string heightmapFileName;
    public int heightmapSize;
    public int subDivision;
    private int tileSize;
    public List<Tile> tiles;
    private float[,] heightData;
    public int testX, testY;

    // Use this for initialization
    void Start() {
        LoadHeightmapData();

        tileSize = (heightmapSize - 1) / subDivision;
        Tile newTile;
        /// <summary>
        /// For an axis aligned square with top left at origin
        /// going right +Y
        /// goint bottom is +X
        /// 
        /// Height0 - top left
        /// Height1 - top right
        /// Height2 - bottom right
        /// Height3 - bottom left
        /// 
        /// tiles are getting filled first horizontally +Y
        /// then vertically +X
        /// </summary>
        float height0, height1, height2, height3;
        for(int x = 0; x < subDivision; x++) {
            for(int y = 0; y < subDivision; y++) {

                height0 = heightData[(x + 0) * tileSize, (y + 0) * tileSize];
                height1 = heightData[(x + 0) * tileSize, (y + 1) * tileSize];
                height2 = heightData[(x + 1) * tileSize, (y + 1) * tileSize];
                height3 = heightData[(x + 1) * tileSize, (y + 0) * tileSize];

                if(x == 0 || y == 0 || x == subDivision - 1 || y == subDivision - 1)
                    newTile = new Tile(x, y, true, height0, height1, height2, height3);
                else
                    newTile = new Tile(x, y, false, height0, height1, height2, height3);


                tiles.Add(newTile);
            }
        }
    }

    // Update is called once per frame
    void Update() {
        if(Input.GetKeyDown(KeyCode.X)) {
            Debug.Log(heightData[testX, testY]);
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

}
