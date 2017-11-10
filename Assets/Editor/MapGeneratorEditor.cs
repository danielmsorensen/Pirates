using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor {

    Texture2D mapTex;

    MapGenerator map;

    Rect texRect;

    public override void OnInspectorGUI() {
        map = target as MapGenerator;

        if (DrawDefaultInspector()) {
            map.redraw = true;
            map.Init();
        }

        if (map.redraw) {
            map.redraw = false;

            UpdateTexture();
        }

        float width = texRect.width;
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(width));
        if (Event.current.type == EventType.Repaint) {
            texRect = GUILayoutUtility.GetLastRect();
        }
        const int border = 5;
        if (mapTex != null) {
            GUI.DrawTexture(new Rect(texRect.position + Vector2.one * border, texRect.size - Vector2.one * border * 2), mapTex, ScaleMode.ScaleToFit, false, 0, Color.white, 0, 0);
        }
    }

    void UpdateTexture() {
        float[,] values = map.GetNoiseValues(map.previewSample, map.previewSample, map.previewOffset);

        mapTex = new Texture2D(map.previewSample, map.previewSample);
        Color[] colourMap = new Color[map.previewSample * map.previewSample];

        for (int x = 0; x < map.previewSample; x++) {
            for (int y = 0; y < map.previewSample; y++) {
                float value = values[x, y];

                Color colour = Color.Lerp(Color.black, Color.white, value);

                if (map.previewColourized) {
                    foreach(MapGenerator.Layer layer in map.layers) {
                        if(value >= layer.level) {
                            colour = layer.colour;
                            break;
                        }
                    }
                }

                colourMap[y * map.previewSample + x] = colour;

            }
        }

        mapTex.filterMode = FilterMode.Point;
        mapTex.wrapMode = TextureWrapMode.Clamp;
        mapTex.SetPixels(colourMap);
        mapTex.Apply();
    }
}
