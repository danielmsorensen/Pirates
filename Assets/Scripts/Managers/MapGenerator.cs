using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour {

    [Header("Noise Settings")]
    public float scale = 50;
    [Space]
    public int octaves = 6;
    [Range(0, 1)]
    public float persistance = .6f;
    public float lacunarity = 2;
    [Space]
    public int seed;
    public Vector2Int offset;
    [Space]
    public bool autoInit;
    [Header("Spawning")]
    public Transform target;
    public float targetRadius;
    public Transform tileHolder;
    public Tilemap[] tilemaps;
    [Space]
    public bool autoSpawn;
    public bool useTilemapper;
    [Space]
    [Space]
    [Tooltip("List of layers the map will be built from. It should be in descending order of levels")]
    public List<Layer> layers = new List<Layer>();

    [Header("Preview")]
    public int previewSample = 100;
    public Vector2Int previewOffset;
    public bool previewColourized;

    bool initialized;
    bool spawned;

    System.Random random;
    Vector2Int[] octaveOffsets;

    float amplitude;
    float frequency;

    float maxValue;
    int tiles;

    [HideInInspector]
    public bool redraw;
    
    Vector2Int lastTargetTile;

    [ContextMenu("Init")]
    public void Init() {
        initialized = true;

        random = new System.Random(seed);
        octaveOffsets = new Vector2Int[octaves];

        amplitude = 1;
        frequency = 1;

        maxValue = 0;
        tiles = Mathf.CeilToInt(targetRadius);

        if (tilemaps != null) {
            foreach (Tilemap tilemap in tilemaps) {
                tilemap.gameObject.SetActive(useTilemapper);
            }
        }
        if(tileHolder != null) {
            tileHolder.gameObject.SetActive(!useTilemapper);
        }

        for (int i = 0; i < octaves; i++) {
            int offsetX = random.Next(-100000, 100000) + offset.x;
            int offsetY = random.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2Int(offsetX, offsetY);

            maxValue += amplitude;
            amplitude *= persistance;
        }
    }

    [ContextMenu("Redraw")]
    public void ReDraw() {
        redraw = true;
    }

    public float GetNoiseValue(int x, int y) {
        if(!initialized) {
            Init();
        }

        float value = 0;
        amplitude = 1;
        frequency = 1;

        for (int i = 0; i < octaves; i++) {
            float X = (x + octaveOffsets[i].x) / scale * frequency;
            float Y = (y + octaveOffsets[i].y) / scale * frequency;

            float perlinValue = Mathf.PerlinNoise(X, Y) * 2 - 1;
            value += perlinValue * amplitude;

            amplitude *= persistance;
            frequency *= lacunarity;
        }

        value = Mathf.Clamp01(Mathf.InverseLerp(0, maxValue, value));

        return value;
    }

    public float[,] GetNoiseValues(int width, int height, Vector2Int centre) {
        float[,] values = new float[width, height];

        Vector2Int halfSize = new Vector2Int(Mathf.FloorToInt(width / 2f), Mathf.FloorToInt(height / 2f));

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                int X = x - halfSize.x + centre.x;
                int Y = y - halfSize.y - centre.y;

                values[x, y] = GetNoiseValue(X, Y);
            }
        }

        return values;
    }
    
    void Awake() {
        if (autoInit) {
            Init();
        }
    }

    void Start() {
        if (autoSpawn) {
            SpawnInitialTiles(Vector2Int.zero);
        }
    }

    public void SpawnInitialTiles(Vector2Int position) {
        spawned = true;
        for (int x = -tiles; x <= tiles; x++) {
            for (int y = -tiles; y <= tiles; y++) {
                Vector2Int tile = new Vector2Int(x, y) + position;
                if (useTilemapper) {
                    SpawnTileBase(tile);
                }
                else {
                    SpawnTile(tile);
                }
            }
        }
    }

    public Transform SpawnTile(Vector2Int position) {
        position = GetTile(position);
        float value = GetNoiseValue(position.x, position.y);
        GameObject prefab = null;
        foreach (Layer layer in layers) {
            if (value >= layer.level) {
                prefab = layer.prefab;
                break;
            }
        }
        if (prefab != null) {
            Transform tile = Instantiate(prefab, new Vector3(position.x, position.y), Quaternion.identity, tileHolder).transform;
            tile.position = new Vector3(position.x, position.y);
            return tile;
        }
        return null;
    }

    public TileBase SpawnTileBase(Vector2Int position) {
        position = GetTile(position);
        float value = GetNoiseValue(position.x, position.y);
        List<Layer> layersReversed = new List<Layer>(layers);
        layersReversed.Reverse();
        TileBase tile = null;
        foreach(Layer layer in layersReversed) {
            if(value >= layer.level) {
                tilemaps[layer.tilemapLayerIndex].SetTile(new Vector3Int(position.x, position.y, 0), layer.tile);
                tile = layer.tile;
            }
        }
        
        return tile;
    }

    public Vector2Int GetTile(Vector2 position) {
        if (useTilemapper) {
            Vector3Int tile = tilemaps[0].WorldToCell(new Vector3(position.x, position.y, 0));
            return new Vector2Int(tile.x, tile.y);
        }
        else {
            return new Vector2Int(Mathf.FloorToInt(position.x + 0.5f), Mathf.FloorToInt(position.y + 0.5f));
        }
    }

    void Update() {
        if (initialized && spawned && target != null && targetRadius > 0) {
            Vector2Int targetTile = GetTile(target.position);
            Rect targetRect = new Rect(targetTile - Vector2.one * targetRadius, Vector2.one * targetRadius * 2);

            if (targetTile != lastTargetTile) {
                if (useTilemapper) {
                    foreach(Tilemap tilemap in tilemaps) {
                        tilemap.ClearAllTiles();
                    }
                    SpawnInitialTiles(targetTile);
                }
                else {
                    for (int i = 0; i < tileHolder.childCount; i++) {
                        Transform tile = tileHolder.GetChild(i);
                        Vector2Int tilePosition = GetTile(tile.position);
                        Rect tileRect = new Rect(tilePosition - Vector2.one * 0.5f, Vector2.one);
                        if (!targetRect.Overlaps(tileRect)) {
                            Vector2Int newPos = targetTile + lastTargetTile - tilePosition;
                            Destroy(tile.gameObject);
                            SpawnTile(newPos);
                        }
                    }
                    lastTargetTile = targetTile;
                    DebugTile(targetTile, Color.blue);
                }
            }
        }
    }

    public void DebugTile(Vector2 tile, Color colour) {
        Vector2 pn = new Vector2(1, -1) * 0.5f;
        Vector2 pp = new Vector2(1, 1) * 0.5f;

        Debug.DrawLine(tile + pp, tile - pn, colour);
        Debug.DrawLine(tile - pn, tile - pp, colour);
        Debug.DrawLine(tile - pp, tile + pn, colour);
        Debug.DrawLine(tile + pn, tile + pp, colour);

        Debug.DrawLine(tile + pp, tile - pp, colour);
        Debug.DrawLine(tile - pn, tile + pn, colour);
    }

    void OnValidate() {
        scale = Mathf.Max(scale, 0.01f);
        octaves = Mathf.Max(octaves, 1);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistance = Mathf.Clamp01(persistance);

        targetRadius = Mathf.Max(targetRadius, 0);

        previewSample = Mathf.Max(previewSample, 1);

        if(autoSpawn) {
            autoInit = true;
        }

        if(layers.Count > 0) {
            Layer layer = layers[layers.Count - 1];
            layer.level = 0;
            layers[layers.Count - 1] = layer;
        }
    }

    void OnDrawGizmos() {
        if (target != null && targetRadius > 0) {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(target.position, Vector2.one * targetRadius * 2);
        }
    }

    [System.Serializable]
    public struct Layer {
        public string name;
        [Range(0, 1)]
        public float level;
        public Color colour;
        [Space]
        public GameObject prefab;
        public TileBase tile;
        public int tilemapLayerIndex;
    }
}
