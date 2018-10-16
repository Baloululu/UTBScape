using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

    public enum DrawMode { ColourMap, FalloffMap, Mesh};
    public DrawMode drawMode;

    public GameObject map;

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
    public bool combineMesh;

    public TerrainType[] regions;

    float[,] falloffMap;

    void Awake()
    {
        falloffMap = FalloffGenerator.GeneratoFallofMap(mapWidth, mapHeight);
    }

    public void GenerateMap()
    {
        map.transform.localScale = Vector3.one;
        int[,] regionMap = Noise.GenerateRegionMap(mapWidth, mapHeight, noiseScale, seed, octaves, persistance, lacunarity, useFalloff, falloffMap, regions);

        Color[] colourMap = new Color[mapWidth * mapHeight];

        TileGenerator[] tilesGenerator = new TileGenerator[regions.Length];
        Bounds bounds = regions[0].tile.GetComponent<MeshFilter>().sharedMesh.bounds;

        Material mat = regions[0].material;

        int halfWidth = mapWidth / 2;
        int halfHeight = mapHeight / 2;

        float displacementX = bounds.size.x;
        float displacementY = (bounds.size.z - (bounds.size.x / 4f)) -0.1f;

        Dictionary<int, List<GameObject>> dictionaryMap= new Dictionary<int, List<GameObject>>();

        for (int i = 0; i < regions.Length; i++)
        {
            dictionaryMap[i] = new List<GameObject>();
            tilesGenerator[i] = new TileGenerator(regions[i].tile);
        }

        if (drawMode == DrawMode.Mesh)
        {
            int i = 0;

            GameObject[] allChildren = new GameObject[map.transform.childCount];

            foreach (Transform child in map.transform)
                allChildren[i++] = child.gameObject;

            foreach (GameObject child in allChildren)
                DestroyImmediate(child.gameObject);
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                Vector3 pos = new Vector3(-halfWidth + (x * displacementX), 0, -halfHeight + (y * displacementY));

                if (y % 2 == 1)
                    pos.x += bounds.size.x / 2;

                int region = regionMap[x, y];

                colourMap[y * mapWidth + x] = regions[region].colour;
                mat = regions[region].material;
                pos.y = region;

                if (drawMode == DrawMode.Mesh)
                {
                    bool sameLevel = true;

                    if (x > 0 && y > 0 && x < mapWidth - 1 && y < mapHeight - 1)
                    {
                        for (int i = -1; i < 2; i++)
                        {
                            for (int j = -1; j < 2; j++)
                            {
                                if (regionMap[x + i, y + j] < regionMap[x, y])
                                {
                                    sameLevel = false;
                                    break;
                                }
                            }
                        }
                    }
                    else
                        sameLevel = false;

                    GameObject tilePointer;

                    if (sameLevel)
                        tilePointer = GameObject.Instantiate(regions[region].tileTop, pos, Quaternion.identity) as GameObject;
                    else
                        tilePointer = tilesGenerator[region].GenerateTile(pos);

                    tilePointer.GetComponent<Renderer>().material = mat;
                    tilePointer.transform.parent = map.transform;
                    dictionaryMap[region].Add(tilePointer);
                }
            }
        }

        if (drawMode == DrawMode.Mesh)
        {
            if (combineMesh)
            {
                for (int i = 0; i < regions.Length; i++)
                {
                    int maxTilesPerMesh = 65535 / regions[i].tile.GetComponent<MeshFilter>().sharedMesh.vertexCount;

                    List<List<GameObject>> combineMeshes = SplitList<GameObject>(dictionaryMap[i], maxTilesPerMesh);

                    for (int j = 0; j < combineMeshes.Count; j++)
                    {
                        CombineMeshFromList(combineMeshes[j], regions[i].name + j, regions[i].material);
                    }
                }
            }
            map.transform.localScale = Vector3.one * mapScale;
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();

        if (drawMode == DrawMode.ColourMap)
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

    private void CombineMeshFromList(List<GameObject> gameObjects, string name, Material mat)
    {
        CombineInstance[] combine = new CombineInstance[gameObjects.Count];

        for (int i = 0; i < gameObjects.Count; i++)
        {
            combine[i].mesh = gameObjects[i].GetComponent<MeshFilter>().sharedMesh;
            combine[i].transform = gameObjects[i].transform.localToWorldMatrix;
            DestroyImmediate(gameObjects[i]);
        }

        Mesh mapMesh = new Mesh();
        mapMesh.CombineMeshes(combine);

        GameObject combineTiles = new GameObject(name);
        MeshFilter meshFilter = combineTiles.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = combineTiles.AddComponent<MeshRenderer>();

        meshFilter.sharedMesh = mapMesh;
        meshRenderer.material = mat;
        combineTiles.transform.parent = map.transform;
    }

    public static List<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
    {
        var list = new List<List<T>>();
        for (int i = 0; i < locations.Count; i += nSize)
        {
            list.Add(locations.GetRange(i, Mathf.Min(nSize, locations.Count - i)));
        }
        return list;
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public GameObject tile;
    public GameObject tileTop;
    public Color colour;
    public Material material;
}