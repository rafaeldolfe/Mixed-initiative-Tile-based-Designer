using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Tilemaps;

public class SetTileRequest
{
    public Vector3Int position;
    public Tile tile;
}
public class SetTileDelayManager : MonoBehaviour
{
    public static SetTileDelayManager _instance;
    public static SetTileDelayManager Instance 
    { 
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SetTileDelayManager>();
            }
            return _instance;
        } 
    }

    public LevelGenerator lg;

    private bool hasSetTilesThisFrame = false;
    private int frameCounter;
    private Queue<SetTileRequest> requests = new Queue<SetTileRequest>();

    public void Enqueue(Vector3Int position, Tile tile)
    {
        requests.Enqueue(new SetTileRequest { position = position, tile = tile });
    }

    public void Update()
    {
        hasSetTilesThisFrame = false;
        Tilemap tilemap = lg.tilemap;
        SetTileRequest request = null;
        int i = 1000000;
        while (requests.Count != 0 && i > 0)
        {
            i--;
            Profiler.BeginSample("while (requests.Count != 0)");
            hasSetTilesThisFrame = true;

            request = requests.Dequeue();

            tilemap.SetTile(request.position, request.tile);

            Level currentLevel = lg.GetLevel();
            if (currentLevel != null)
            {
                Profiler.BeginSample("currentLevel.tilePosToTile[request.position]");
                currentLevel.tilePosToTile[request.position] = request.tile;
                Profiler.EndSample();
            }
            Profiler.EndSample();
        }
        if (hasSetTilesThisFrame)
        {
            frameCounter++;
        }
    }

    public bool Finished()
    {
        return !hasSetTilesThisFrame;
    }
}
