using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
    public float initialWater = 100;
    private int simSteps = 0;
    public int maxSimSteps = 10;
    public int lookStep = 0;
    public float stepFlowFactor;
    private Tile currentSimTile = null;
    public Mesh planeMesh;
    public bool drawFlowDirection = true;
    [Range(0, 1.0f)]
    public float cutoff = 0.5f;
    public Material terrainMaterial;
    public Texture2D terrainTexture;
    private Texture2D terrainHeightTexture;
    private Texture2D terrainFlowDirectionTexture;
    private Texture2D terrainCurrentWaterTexture;
    public int completedStepIndex = 0;
    private Color[] currentWaters;
    public Text infoText;
    private DisplayMode currentDisplayMode = DisplayMode.None;

    // Use this for initialization
    private void Start() {
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

                newTile = new Tile(x, y, height0, height1, height2, height3, height4, stepFlowFactor);
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

        StartSim();

        InitTextures();
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
    private void Update() {
        if(Input.GetKeyDown(KeyCode.Z)) {
            SimStepNext();
        }
        if(Input.GetKeyDown(KeyCode.X)) {
            SimStepLast();
        }

        if(Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, 10000)) {
                Vector3 temp = new Vector3(hit.point.x / terrainSizeInMeters, hit.point.y / 50, hit.point.z / terrainSizeInMeters);
                temp = new Vector3((int)Mathf.Clamp(temp.x * subDivision, 0, subDivision), temp.y, (int)Mathf.Clamp(temp.z * subDivision, 0, subDivision));
                Tile tile = tiles.Find((X) => X.x == temp.z && X.y == temp.x);

                if(currentDisplayMode == DisplayMode.Height)
                    infoText.text = "Normalized Height : " + tile.averageHeight.ToString("0.00");

                if(currentDisplayMode == DisplayMode.Slope)
                    infoText.text = "Flow Direction : " + tile.flowDirection;

                if(currentDisplayMode == DisplayMode.CurrentWater)
                    infoText.text = "Runoff : " + tile.currentWaters[lookStep].ToString("0.00");
            }
        }

        if(Input.GetKeyDown(KeyCode.Alpha1)) {
            currentDisplayMode = DisplayMode.Height;
            ChangeDisplayMode(DisplayMode.Height);
        }
        if(Input.GetKeyDown(KeyCode.Alpha2)) {
            currentDisplayMode = DisplayMode.Slope;
            ChangeDisplayMode(DisplayMode.Slope);
        }
        if(Input.GetKeyDown(KeyCode.Alpha3)) {
            currentDisplayMode = DisplayMode.CurrentWater;
            ChangeDisplayMode(DisplayMode.CurrentWater);
        }

    }

    private void LoadHeightmapData() {
        int h = heightmapSize;
        int w = heightmapSize;
        heightData = new float[h, w];

#if UNITY_EDITOR
        using(var file = System.IO.File.OpenRead("Assets/Heightmaps/" + heightmapFileName + ".raw"))
#endif
#if !UNITY_EDITOR
        using(var file = System.IO.File.OpenRead("Heightmaps/" + heightmapFileName + ".raw"))
#endif

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

    private void StartSim() {
        for(int i = 0; i < tiles.Count; i++) {
            tiles[i].SimStart(initialWater);
        }
        completedStepIndex = 0;
    }

    private void SimStep() {
        for(int j = 0; j < tiles.Count; j++) {
            tiles[j].MoveWater();
        }

        for(int j = 0; j < tiles.Count; j++) {
            tiles[j].RecordWater();
        }

        completedStepIndex++;
    }

    private float FindMaxWater(int stepIndex) {
        return tiles.Max(t => t.currentWaters[stepIndex]);
    }

    private void InitTextures() {
        terrainHeightTexture = new Texture2D(subDivision, subDivision);
        terrainHeightTexture.filterMode = FilterMode.Point;
        for(int x = 0; x < subDivision; x++) {
            for(int y = 0; y < subDivision; y++) {
                //terrainHeightTexture.SetPixel(y, x, new Color(0, 0, tiles[x * subDivision + y].averageHeight, 1));    // x and y flipped to match the overlay texture with 3d terrain
                                                                                                                        // now just use same heightmaps in terrain and as height data
                terrainHeightTexture.SetPixel(y, x, Color.Lerp(Color.red, Color.blue, tiles[x * subDivision + y].averageHeight));
            }
        }
        terrainHeightTexture.Apply();

        terrainFlowDirectionTexture = new Texture2D(subDivision, subDivision);
        terrainFlowDirectionTexture.filterMode = FilterMode.Point;
        for(int x = 0; x < subDivision; x++) {
            for(int y = 0; y < subDivision; y++) {
                terrainFlowDirectionTexture.SetPixel(y, x, FlowDirectionToColor(tiles[x * subDivision + y].flowDirection));    // x and y flipped to match the overlay texture with 3d terrain
                                                                                                                               // now just use same heightmaps in terrain and as height data
            }
        }
        terrainFlowDirectionTexture.Apply();

        terrainCurrentWaterTexture = new Texture2D(subDivision, subDivision);
        currentWaters = new Color[subDivision * subDivision];
        terrainCurrentWaterTexture.filterMode = FilterMode.Point;
        UpdateCurrentWaterTexture();

        //ChangeDisplayMode(DisplayMode.CurrentWater);
    }

    private void ChangeDisplayMode(DisplayMode displayMode) {
        switch(displayMode) {
            case DisplayMode.CurrentWater:
                terrainMaterial.SetTexture("_TerrainOverlay", terrainCurrentWaterTexture);
                break;
            case DisplayMode.Height:
                terrainMaterial.SetTexture("_TerrainOverlay", terrainHeightTexture);
                break;
            case DisplayMode.Slope:
                terrainMaterial.SetTexture("_TerrainOverlay", terrainFlowDirectionTexture);
                break;
        }
    }

    private void UpdateCurrentWaterTexture() {
        float maxWater = FindMaxWater(lookStep);
        for(int x = 0; x < subDivision; x++) {
            for(int y = 0; y < subDivision; y++) {
                /*currentWaters[x * subDivision + y] = new Color(1 - tiles[x * subDivision + y].currentWaters[lookStep] / maxWater,
                                                                    1 - tiles[x * subDivision + y].currentWaters[lookStep] / maxWater,
                                                                    1,
                                                                    1
                                                                    );    // x and y flipped to match the overlay texture with 3d terrain
                                                                           // now just use same heightmaps in terrain and as height data
                                                                           */

                float temp = 1 - tiles[x * subDivision + y].currentWaters[lookStep] / maxWater;
                currentWaters[x * subDivision + y] = Color.Lerp(Color.blue, Color.white, temp);
            }
        }
        terrainCurrentWaterTexture.SetPixels(currentWaters);
        terrainCurrentWaterTexture.Apply();
    }

    private Color FlowDirectionToColor(TileFlowDirection flowDirection) {
        // using https://www.sessions.edu/color-calculator/
        switch(flowDirection) {
            case TileFlowDirection.None:
                return Color.black;
            case TileFlowDirection.Top:
                return new Color(0.466f, 1.0f, 0);
            case TileFlowDirection.Left:
                return new Color(1.0f, 0, 0);
            case TileFlowDirection.Right:
                return new Color(0, 1.0f, 1.0f);
            case TileFlowDirection.Bottom:
                return new Color(0.50f, 0, 1.0f);
            case TileFlowDirection.TopLeft:
                return new Color(1.0f, 0.713f, 0.0f);
            case TileFlowDirection.BottomRight:
                return new Color(0, 0.34f, 1.0f);
            case TileFlowDirection.TopRight:
                return new Color(0, 1.0f, 0.22f);
            case TileFlowDirection.BottomLeft:
                return new Color(1.0f, 0, 0.8f);
        }
        return new Color(1, 1, 1, 1);
    }

#region OnDrawGizmos
    /*
    private void OnDrawGizmos() {

        if (initDone) {
            for (int i = 0; i < tiles.Count; i++) {
                float mainColor = tiles[i].waterIn / maxWater;
                if (mainColor <= cutoff)
                    mainColor = 0;

                Gizmos.color = new Color(0, 0, mainColor, 1.0f);
                //Gizmos.color = new Color(0, 0, tiles[i].averageHeight, 1.0f);
                Vector3 origin = new Vector3(tiles[i].y * tileSizeInMeters + tileSizeInMeters / 2.0f, overlayHeightInMeters * 1.5f, (subDivision - tiles[i].x - 1) * tileSizeInMeters + tileSizeInMeters / 2.0f);
                Gizmos.DrawMesh(planeMesh, origin - new Vector3(0, 0, 0), Quaternion.Euler(-90, 0, 0), new Vector3(tileSizeInMeters / 2.0f, tileSizeInMeters/ 2.0f, 1));
                // z value changed because of 90 degree offset due to coordinate system change of raw data file and xyz coordinate system

                if (drawFlowDirection) {
                    Gizmos.color = new Color(1.0f, 0, 0, 1.0f);
                    if (tiles[i].flowDirection != TileFlowDirection.None) {
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
    }
    */
#endregion

    private void SimStepNext() {
        lookStep++;
        lookStep = Mathf.Clamp(lookStep, 0, maxSimSteps - 1);

        if(lookStep > completedStepIndex) {
            SimStep();
        }

        UpdateCurrentWaterTexture();
    }

    private void SimStepLast() {
        lookStep--;
        lookStep = Mathf.Clamp(lookStep, 0, maxSimSteps - 1);
        UpdateCurrentWaterTexture();
    }

}
