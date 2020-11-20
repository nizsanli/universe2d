using UnityEngine;
using System.Collections;
using System;

public class Noise : MonoBehaviour {

	public static float SCALE = 1f;

	public static void Perlin2D(float[,] noiseData, Vector2 noiseOrigin)
	{
		int width = noiseData.GetLength(0);
		int height = noiseData.GetLength(1);

		int offset = 99999;

		for (int y = 0; y < height; y++)
		{
			float yNoise = noiseOrigin.y + (float)y / height * SCALE + offset;

			for (int x = 0; x < width; x++)
			{
				float xNoise = noiseOrigin.x + (float)x / width * SCALE + offset;

				// calculate and put noise value into data table
				float noiseValue = Mathf.PerlinNoise(xNoise, yNoise);
				noiseData[x,y] = noiseValue;
			}
		}
	}

	public static float PerlinValue (float inputX, float inputY)
	{
		float offset = 100;
		return Mathf.PerlinNoise(inputX*SCALE + offset, inputY*SCALE + offset);
	}

	public static float PerlinChunkVal(int x, int y, float chunkSize, Vector2 noiseOrigin)
	{
		float offset = Int32.MaxValue;

		float xNoise = noiseOrigin.x/chunkSize*SCALE + (float)x / chunkSize;
		float yNoise = noiseOrigin.y/chunkSize*SCALE + (float)y / chunkSize;
		return Mathf.PerlinNoise(xNoise + 10000000, yNoise + 10000000);
	}

}