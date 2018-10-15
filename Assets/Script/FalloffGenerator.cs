using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffGenerator {

	public static float[,] GeneratoFallofMap(int width, int height)
    {
        float[,] map = new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float valY = y / (float)height * 2 - 1;
                float valX = x / (float)width * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(valX), Mathf.Abs(valY));
                map[x, y] = Evaluate(value);
            }
        }

        return map;
    }

    static float Evaluate(float value)
    {
        float a = 3;
        float b = 5f;
        return Mathf.Pow(value,a) / (Mathf.Pow(value,a) + Mathf.Pow(b - b * value, a));
    }
}
