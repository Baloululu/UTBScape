using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGenerator
{

    private GameObject tile;
    private Mesh originale;

    private float minHeight;

    public TileGenerator(GameObject tile)
    {
        this.tile = tile;
        originale = tile.GetComponent<MeshFilter>().sharedMesh;
        minHeight = FindMinHeight(originale.vertices);
    }

    private float FindMinHeight(Vector3[] vertices)
    {
        float min = float.MaxValue;
        for (int i = 0; i < vertices.Length; i++)
        {
            if (min > vertices[i].y)
                min = vertices[i].y;
        }

        return min;
    }

    public GameObject GenerateTile(Vector3 pos)
    {
        GameObject instance = GameObject.Instantiate(tile, pos, Quaternion.identity) as GameObject;
        instance.transform.position = pos;
        instance.transform.rotation = Quaternion.identity;
        Mesh mesh = GameObject.Instantiate(originale) as Mesh;

        Vector3[] vertices = originale.vertices;
        float height = pos.y;

        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].y == minHeight)
                vertices[i].y -= height;
        }

        mesh.vertices = vertices;

        instance.GetComponent<MeshFilter>().sharedMesh = mesh;

        return instance;
    }
}
