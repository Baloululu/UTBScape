using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

    public enum DrawMode { NoiseMap, ColourMap, FalloffMap, Mesh};
    public DrawMode drawMode;

    public GameObject map;
    public GameObject tile;

    public float mapScale;

    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    public int seed;

    public bool useFalloff;

    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;

    public bool autoUpdate;

    public TerrainType[] regions;

    float[,] falloffMap;

    void Awake()
    {
        falloffMap = FalloffGenerator.GeneratoFallofMap(mapWidth, mapHeight);
    }

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale, seed, octaves, persistance, lacunarity);

        Color[] colourMap = new Color[mapWidth * mapHeight];

        TileGenerator tileGenerator = new TileGenerator(tile);
        Bounds bounds = tile.GetComponent<MeshFilter>().sharedMesh.bounds;

        Material mat = regions[0].material;

        int halfWidth = mapWidth / 2;
        int halfHeight = mapHeight / 2;

        float displacementX = bounds.size.x;
        float displacementY = (bounds.size.z - (bounds.size.x / 4f)) -0.1f;

        if (drawMode == DrawMode.Mesh)
        {
            int i = 0;

            GameObject[] allChildren = new GameObject[map.transform.childCount];

            foreach (Transform child in map.transform)
            {
                allChildren[i] = child.gameObject;
                i++;
            }

            foreach (GameObject child in allChildren)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                if (useFalloff)
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);

                float currentHeight = noiseMap[x, y];

                Vector3 pos = new Vector3(-halfWidth + (x * displacementX), 0, -halfHeight + (y * displacementY));

                if (y % 2 == 1)
                    pos.x += bounds.size.x / 2;

                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colourMap[y * mapWidth + x] = regions[i].colour;
                        mat = regions[i].material;
                        pos.y = i;
                        break;
                    }
                }

                if (drawMode == DrawMode.Mesh)
                {
                    GameObject tilePointer = tileGenerator.GenerateTile(pos);
                    tilePointer.GetComponent<Renderer>().material = mat;
                    tilePointer.transform.parent = map.transform;
                }
            }
        }

        if (drawMode == DrawMode.Mesh)
            map.transform.localScale *= mapScale;

        MapDisplay display = FindObjectOfType<MapDisplay>();

        if (drawMode == DrawMode.NoiseMap)
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        else if (drawMode == DrawMode.ColourMap)
            display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
        else if (drawMode == DrawMode.FalloffMap)
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(falloffMap));
    }

    void OnValidate()
    {
        if (mapWidth < 1)
            mapWidth = 1;
        if (mapHeight < 1)
            mapHeight = 1;
        if (lacunarity < 1)
            lacunarity = 1;
        if (octaves < 0)
            octaves = 0;

        falloffMap = FalloffGenerator.GeneratoFallofMap(mapWidth, mapHeight);
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;
    public Material material;
}