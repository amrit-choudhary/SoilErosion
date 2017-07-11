using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tile {

    /// <summary>
    /// X and Y coordinate index
    /// </summary>
    public int x, y;
    /// <summary>
    /// Height0 - top right
    /// Height1 - bottom right
    /// Height2 - bottom left
    /// Height3 - top left 
    /// </summary>
    public float height0 = 0, height1 = 0, height2 = 0, height3 = 0;
    private TileFlowDirection direction = TileFlowDirection.None;
    private float waterIn = 0;
    public bool isEdge = false;
    public float averageHeight;

    public Tile(int _x, int _y, bool _isEdge, float h0, float h1, float h2, float h3) {
        x = _x;
        y = _y;
        isEdge = _isEdge;
        height0 = h0;
        height1 = h1;
        height2 = h2;
        height3 = h3;
        averageHeight = (h0 + h1 + h2 + h3) / 4.0f;
    }


}
