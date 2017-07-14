using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public const int CLIFF = -100;

    /// <summary>
    /// X and Y coordinate index
    /// </summary>
    public int x, y;
    /// <summary>
    /// Height0 - center
    /// Height1 - top left
    /// Height2 - top right
    /// Height3 - bottom right
    /// Height4 - bottom left
    /// </summary>
    public float height0 = 0, height1 = 0, height2 = 0, height3 = 0, height4 = 0;
    public TileFlowDirection flowDirection = TileFlowDirection.None;
    private float waterIn = 0;
    public bool isCliff = false;
    public float averageHeight;
    public List<Tile> neighbourTiles;
    public Tile flowTile;

    public Tile(int _x, int _y, float h0, float h1, float h2, float h3, float h4) {
        x = _x;
        y = _y;
        height0 = h0;
        height1 = h1;
        height2 = h2;
        height3 = h3;
        height4 = h4;
        averageHeight = (h0 + h1 + h2 + h3) / 4.0f;
    }

    /// <summary>
    /// Off map tiles, which will indicate flow end
    /// Used to mark neighbour as off map
    /// </summary>
    /// <param name="isCliff"></param>
    public Tile(bool _isCliff) {
        averageHeight = CLIFF;
        isCliff = true;
    }

    /// <summary>
    /// Array of neightbour tiles where
    /// neighbours counted in this order
    /// 0 1 2
    /// 3 x 4
    /// 5 6 7
    /// <param name="t0"> top left </param>
    /// <param name="t1"> top </param>
    /// <param name="t2"> top right </param>
    /// <param name="t3"> left </param>
    /// <param name="t4"> right </param>
    /// <param name="t5"> bottom left </param>
    /// <param name="t6"> bottom </param>
    /// <param name="t7"> bottom right </param>
    public void SetNeighbours(List<Tile> _neighbourTiles) {
        neighbourTiles = _neighbourTiles;
        List<Tile> unsortedNeighbourTiles = new List<Tile>(neighbourTiles);
        neighbourTiles.Sort((x, y) => x.averageHeight.CompareTo(y.averageHeight));
        flowTile = neighbourTiles[0];

        int flowIndex = unsortedNeighbourTiles.IndexOf(flowTile);
        if(flowTile.isCliff) {
            flowDirection = (TileFlowDirection)(- 1);
        } else {
            flowDirection = (TileFlowDirection)flowIndex;
        }
        Debug.Log(flowDirection);
    }

    public override string ToString() {
        if(isCliff) {
            return "Cliff Tile";
        } else {
            return "Tile(" + x + ", " + y + ")";
        }
    }


}
