using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [Header("Spawning")]
    public Transform target;
    public float targetRadius;
    public Transform tileHolder;
    public float tileSize;
    [Space]
    [Tooltip("List of layers the map will be built from. It should be in descending order of levels")]
    public List<Layer> layers = new List<Layer>();

    [Header("Preview")]
    public int previewSample = 100;
    public bool previewColourized;

    #region Hidden Map Settings
    bool initialized;

    System.Random random;
    Vector2Int[] octaveOffsets;

    float amplitude;
    float frequency;

    float maxValue;
    #endregion

    [HideInInspector]
    public bool redraw;

    int tilesPerUnit;
    Vector2 lastTargetTile;

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
        Init();
    }

    void Start() {
        SpawnInitialTiles();
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
        float tiles = Mathf.CeilToInt(targetRadius / tileSize);
        for (int x = 0; x < tiles; x++) {
            for (int y = 0; y < tiles; y++) {
                float value = GetNoiseValue(x, y);
                GameObject prefab = null;
                foreach (Layer layer in layers) {
                    if (value >= layer.level) {
                        prefab = layer.prefab;
                        break;
                    }
                }
                Vector3 position = new Vector3(x, y) * tileSize;
                if (prefab != null) {
                    Transform tile = Instantiate(prefab, position, Quaternion.identity, tileHolder).transform;
                }
            }
        }
    }

    public Vector2 GetTile(Vector2 position) {
        return new Vector2(Mathf.Floor((position.x + tileSize / 2) / tileSize), Mathf.Floor((position.y + tileSize / 2) / tileSize)) * tileSize;
    }

    void Update() {
        Vector2 targetTile = GetTile(target.position);
        DebugTile(targetTile, Color.blue);
        Vector2 positionVector = new Vector2(-1, 1);
        Rect targetRect = new Rect(targetTile + positionVector * targetRadius, Vector2.one * targetRadius * 2);
        if (target != null && targetRadius > 0) {
            for (int i = 0; i < tileHolder.childCount; i++) {
                Transform tile = tileHolder.GetChild(i);
                Rect tileRect = new Rect(GetTile(tile.position) + positionVector * tileSize / 2, Vector2.one * tileSize);
                if(!targetRect.Overlaps(tileRect)) {
                    DebugTile(tile.position, Color.red);
                }
            }
        }
    }

    public void DebugTile(Vector2 tile, Color colour) {
        Debug.DrawLine(tile + Vector2.up *tileSize / 2, tile - Vector2.up * tileSize / 2, colour);
        Debug.DrawLine(tile + Vector2.right * tileSize / 2, tile - Vector2.right * tileSize / 2, colour);
    }

    void OnValidate() {
        scale = Mathf.Max(scale, 0.01f);
        octaves = Mathf.Max(octaves, 1);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistance = Mathf.Clamp01(persistance);

        targetRadius = Mathf.Max(targetRadius, 0);

        previewSample = Mathf.Max(previewSample, 1);

        if(layers.Count > 0) {
            Layer layer = layers[layers.Count - 1];
            layer.level = 0;
            layers[layers.Count - 1] = layer;
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
