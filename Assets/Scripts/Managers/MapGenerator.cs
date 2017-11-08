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
    [Space]
    public bool autoSpawn;
    [Space]
    [Space]
    [Tooltip("List of layers the map will be built from. It should be in descending order of levels")]
    public List<Layer> layers = new List<Layer>();

    [Header("Preview")]
    public int previewSample = 100;
    public bool previewColourized;

    #region Hidden Map Settings
    bool initialized;
    bool spawned;

    System.Random random;
    Vector2Int[] octaveOffsets;

    float amplitude;
    float frequency;

    float maxValue;
    #endregion

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

        value = Mathf.InverseLerp(0, maxValue, value);

        return value;
    }

    void Awake() {
        if (autoInit) {
            Init();
        }
    }

    void Start() {
        if (autoSpawn) {
            SpawnInitialTiles();
        }
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

    public void SpawnInitialTiles() {
        int tiles = Mathf.CeilToInt(targetRadius);
        for (int x = -tiles; x <= tiles; x++) {
            for (int y = -tiles; y <= tiles; y++) {
                SpawnTile(new Vector2Int(x, y));
            }
        }
    }

    public Transform SpawnTile(Vector2Int coord) {
        spawned = true;
        float value = GetNoiseValue(coord.x, coord.y);
        GameObject prefab = null;
        foreach (Layer layer in layers) {
            if (value >= layer.level) {
                prefab = layer.prefab;
                break;
            }
        }
        Vector3 position = new Vector3(coord.x, coord.y);
        if (prefab != null) {
            Transform tile = Instantiate(prefab, position, Quaternion.identity, tileHolder).transform;
            return tile;
        }
        return null;
    }

    public Vector2Int GetTile(Vector2 position) {
        return new Vector2Int(Mathf.FloorToInt((position.x + 0.5f)), Mathf.FloorToInt((position.y + 0.5f)));
    }

    void Update() {
        if (initialized && spawned && target != null && targetRadius > 0) {
            Vector2Int targetTile = GetTile(target.position);
            Rect targetRect = new Rect(targetTile + - Vector2.one * targetRadius, Vector2.one * targetRadius * 2);

            for (int i = 0; i < tileHolder.childCount; i++) {
                Transform tile = tileHolder.GetChild(i);
                
                Rect tileRect = new Rect(GetTile(tile.position) + -Vector2.one * 0.5f, Vector2.one);
                if (!targetRect.Overlaps(tileRect)) {
                    Vector2Int newPos = targetTile + lastTargetTile - GetTile(tile.position);
                    Destroy(tile.gameObject);
                    SpawnTile(newPos);
                }
            }
            lastTargetTile = targetTile;
            DebugTile(targetTile, Color.blue);
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
        public GameObject prefab;
    }
}
