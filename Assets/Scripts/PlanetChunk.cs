using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
public class PlanetChunk : MonoBehaviour {

	private MeshRenderer meshRenderer;
	private MeshFilter meshFilter;
	private Mesh mesh;

	private Texture2D chunkTexture;
	byte[] data;

	public CircleCollider2D colliderPrefab;

	// Use this for initialization
	void Start () {

	}

	public void Init()
	{
		data = new byte[GameManager.CHUNK_SIZE * GameManager.CHUNK_SIZE];

		chunkTexture = new Texture2D(GameManager.CHUNK_SIZE, GameManager.CHUNK_SIZE);

		meshRenderer = (MeshRenderer) gameObject.AddComponent<MeshRenderer>();
		meshRenderer.material = new Material(Shader.Find("Unlit/Transparent"));
		
		meshFilter = (MeshFilter) gameObject.AddComponent<MeshFilter>();
		mesh = new Mesh();
		
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector2> uvs = new List<Vector2>();
		
		vertices.Add(new Vector3(0f, 0f, 0f));
		vertices.Add(new Vector3(0f, GameManager.CHUNK_SIZE, 0f));
		vertices.Add(new Vector3(GameManager.CHUNK_SIZE, GameManager.CHUNK_SIZE, 0f));
		vertices.Add(new Vector3(GameManager.CHUNK_SIZE, 0f, 0f));
		
		triangles.Add(0);
		triangles.Add(1);
		triangles.Add(2);
		triangles.Add(2);
		triangles.Add(3);
		triangles.Add(0);
		
		uvs.Add(new Vector2(0f, 0f));
		uvs.Add(new Vector2(0f, 1f));
		uvs.Add(new Vector2(1f, 1f));
		uvs.Add(new Vector2(1f, 0f));
		
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uvs.ToArray();
		
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		mesh.Optimize();
		
		meshFilter.sharedMesh = mesh;
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void DestroyPlanetChunk()
	{
		Color[] pixels = new Color[GameManager.CHUNK_SIZE * GameManager.CHUNK_SIZE];
		for (int i = 0; i < pixels.Length; i++)
		{
			pixels[i] = new Color(0f, 0f, 0f, 0f);
		}

		chunkTexture.SetPixels(pixels);
		chunkTexture.filterMode = FilterMode.Point;
		chunkTexture.Apply();
		
		meshRenderer.material.mainTexture = chunkTexture;

		Debug.Log(transform.position);
	}

	public void RadialDamage(Vector2 center, float radius)
	{
		for (int y = 0; y < GameManager.CHUNK_SIZE; y++)
		{
			for (int x = 0; x < GameManager.CHUNK_SIZE; x++)
			{
				Vector2 vecToCenter = new Vector2(transform.position.x + x, transform.position.y + y) - center;
				float distToCenter = Mathf.Sqrt(vecToCenter.x * vecToCenter.x + vecToCenter.y * vecToCenter.y);

				if (distToCenter < radius)
				{
					chunkTexture.SetPixel(x, y, new Color(0f, 0f, 0f, 0f));
					chunkTexture.Apply();
				}
			}
		}
	}

	public void GeneratePlanetChunk()
	{
		Planet planetScript = transform.parent.GetComponent<Planet>();
		Vector2 planetCent = new Vector2(transform.parent.position.x, transform.parent.position.y);

		Color[] pixels = new Color[GameManager.CHUNK_SIZE * GameManager.CHUNK_SIZE];

		float smallVariance = 64f;
		float mediumVariance = 64f;
		float largeVariance = 64f;
		
		float noiseScale = 1f;

		for (int y = 0; y < GameManager.CHUNK_SIZE; y++)
		{
			for (int x = 0; x < GameManager.CHUNK_SIZE; x++)
			{
				Vector2 vecToCenter = new Vector2(transform.position.x + x, transform.position.y + y) - planetCent;
				float distToCenter = Mathf.Sqrt(vecToCenter.x * vecToCenter.x + vecToCenter.y * vecToCenter.y);
				Vector2 vecToCenterNorm = vecToCenter * 1/distToCenter;

				float rangeScale = 2f;
				float offset = 1f;

				float factor = 2f;
				float factor2 = 6f;
				float heightNoiseVal = Mathf.PerlinNoise((vecToCenterNorm.x + 1f) * rangeScale, (vecToCenterNorm.y + 1f) * rangeScale);
				float heightNoiseVal2 = Mathf.PerlinNoise((vecToCenterNorm.x + 1f) * rangeScale*factor, (vecToCenterNorm.y + 1f) * rangeScale*factor);
				float heightNoiseVal3 = Mathf.PerlinNoise((vecToCenterNorm.x + 1f) * rangeScale*factor2, (vecToCenterNorm.y + 1f) * rangeScale*factor2);

				float marginbase = 512f;
				float margin = 256f;
				float margin2 = 64f;
				float margin3 = 32f;

				float distVariance = marginbase - margin*heightNoiseVal - margin2*heightNoiseVal2 - margin3*heightNoiseVal3;

				if (distToCenter < planetScript.radius - distVariance || distToCenter == 0)
				{
					pixels[y * GameManager.CHUNK_SIZE + x] = new Color(1f * (distToCenter/planetScript.radius) * 2f, 1f * (distToCenter/planetScript.radius) * 2f, 1f * (distToCenter/planetScript.radius) * 2f, 1f);
					data[y * GameManager.CHUNK_SIZE + x] = 1;
				}
			}
		}

		chunkTexture.SetPixels(pixels);
		chunkTexture.filterMode = FilterMode.Point;
		chunkTexture.Apply();

		meshRenderer.material.mainTexture = chunkTexture;
	}

	public void CalculateCollider(int numChunksLength)
	{
		Planet planet = transform.parent.GetComponent<Planet>();
		
		int xPos = (int)(transform.position.x - (transform.parent.transform.position.x - planet.radius))/GameManager.CHUNK_SIZE;
		int yPos = (int)(transform.position.y - (transform.parent.transform.position.y - planet.radius))/GameManager.CHUNK_SIZE;
		
		// get neighbors
		PlanetChunk leftChunk = null;
		PlanetChunk rightChunk = null;
		PlanetChunk botChunk = null;
		PlanetChunk topChunk = null;
		if (xPos > 0)
		{
			leftChunk = planet.generatedChunks[yPos * numChunksLength + xPos-1];
		}
		if (xPos < numChunksLength-1)
		{
			rightChunk = planet.generatedChunks[yPos * numChunksLength + xPos+1];
		}
		if (yPos > 0)
		{
			botChunk = planet.generatedChunks[(yPos-1) * numChunksLength + xPos];
		}
		if (yPos < numChunksLength-1)
		{
			topChunk = planet.generatedChunks[(yPos+1) * numChunksLength + xPos];
		}

		for (int y = 0; y < GameManager.CHUNK_SIZE; y++)
		{
			for (int x = 0; x < GameManager.CHUNK_SIZE; x++)
			{
				if (data[y * GameManager.CHUNK_SIZE + x] > 0)
				{
					int index = 0;

					if (x == 0)
					{
						if (leftChunk != null && leftChunk.data[y * GameManager.CHUNK_SIZE + GameManager.CHUNK_SIZE-1] > 0)
						{
							index |= 8;
						}
					}
					else
					{
						if (data[y * GameManager.CHUNK_SIZE + (x-1)] > 0)
						{
							index |= 8;
						}
					}
					
					// right edge
					if (x == GameManager.CHUNK_SIZE-1)
					{
						if (rightChunk != null && rightChunk.data[y * GameManager.CHUNK_SIZE + 0] > 0)
						{
							index |= 2;
						}
					}
					else
					{
						if (data[y * GameManager.CHUNK_SIZE + x+1] > 0)
						{
							index |= 2;
						}
					}
					
					// bot edge
					if (y == 0)
					{
						if (botChunk != null && botChunk.data[(GameManager.CHUNK_SIZE-1) * GameManager.CHUNK_SIZE + x] > 0)
						{
							index |= 1;
						}
					}
					else
					{
						if (data[(y-1) * GameManager.CHUNK_SIZE + x] > 0)
						{
							index |= 1;
						}
					}
					
					// top edge
					if (y == GameManager.CHUNK_SIZE-1)
					{
						if (topChunk != null && topChunk.data[x] > 0)
						{
							index |= 4;
						}
					}
					else
					{
						if (data[(y+1) * GameManager.CHUNK_SIZE + x] > 0)
						{
							index |= 4;
						}
					}

					if (index != 0xF)
					{
						CircleCollider2D collider = (CircleCollider2D) Instantiate(colliderPrefab, new Vector3(transform.position.x + x + 0.5f, transform.position.y + y + 0.5f, 0f), Quaternion.identity);
						collider.transform.parent = gameObject.transform;
					}
				}
			}
		}


		Planet planet = transform.parent.GetComponent<Planet>();

		int xPos = (int)(transform.position.x - (transform.parent.transform.position.x - planet.radius))/GameManager.CHUNK_SIZE;
		int yPos = (int)(transform.position.y - (transform.parent.transform.position.y - planet.radius))/GameManager.CHUNK_SIZE;

		// get neighbors
		PlanetChunk leftChunk = null;
		PlanetChunk rightChunk = null;
		PlanetChunk botChunk = null;
		PlanetChunk topChunk = null;
		if (xPos > 0)
		{
			leftChunk = planet.generatedChunks[yPos * numChunksLength + xPos-1];
		}
		if (xPos < numChunksLength-1)
		{
			rightChunk = planet.generatedChunks[yPos * numChunksLength + xPos+1];
		}
		if (yPos > 0)
		{
			botChunk = planet.generatedChunks[(yPos-1) * numChunksLength + xPos];
		}
		if (yPos < numChunksLength-1)
		{
			topChunk = planet.generatedChunks[(yPos+1) * numChunksLength + xPos];
		}

		for (int y = 0; y < GameManager.CHUNK_SIZE; y++)
		{
			for (int x = 0; x < GameManager.CHUNK_SIZE; x++)
			{
				int index = 0;

				// left edge
				if (x == 0)
				{
					if (leftChunk != null && leftChunk.data[y * GameManager.CHUNK_SIZE + GameManager.CHUNK_SIZE-1] > 0)
					{
						index |= 8;
					}
				}
				else
				{
					if (data[y * GameManager.CHUNK_SIZE + (x-1)] > 0)
					{
						index |= 8;
					}
				}

				// right edge
				if (x == GameManager.CHUNK_SIZE-1)
				{
					if (rightChunk != null && rightChunk.data[y * GameManager.CHUNK_SIZE + 0] > 0)
					{
						index |= 2;
					}
				}
				else
				{
					if (data[y * GameManager.CHUNK_SIZE + x+1] > 0)
					{
						index |= 2;
					}
				}

				// bot edge
				if (y == 0)
				{
					if (botChunk != null && botChunk.data[(GameManager.CHUNK_SIZE-1) * GameManager.CHUNK_SIZE + x] > 0)
					{
						index |= 1;
					}
				}
				else
				{
					if (data[(y-1) * GameManager.CHUNK_SIZE + x] > 0)
					{
						index |= 1;
					}
				}

				// top edge
				if (y == GameManager.CHUNK_SIZE-1)
				{
					if (topChunk != null && topChunk.data[x] > 0)
					{
						index |= 4;
					}
				}
				else
				{
					if (data[(y+1) * GameManager.CHUNK_SIZE + x] > 0)
					{
						index |= 4;
					}
				}

				// find edge colliders using index
			}
		}

	}


	public void applyNoise(float scale, float isoValue, float hillHeight)
	{
		float[,] noiseData = new float[GameManager.CHUNK_SIZE, GameManager.CHUNK_SIZE];
		Noise.SCALE = scale;
		Noise.Perlin2D(noiseData, new Vector2(transform.position.x/GameManager.CHUNK_SIZE * scale, transform.position.y/GameManager.CHUNK_SIZE * scale));

		Color[] pixels = new Color[GameManager.CHUNK_SIZE * GameManager.CHUNK_SIZE];

		Noise.SCALE = scale * 0.25f;
		for (int y = 0; y < GameManager.CHUNK_SIZE; y++)
		{
			for (int x = 0; x < GameManager.CHUNK_SIZE; x++)
			{
				Vector2 vec = new Vector2(transform.position.x + x, transform.position.y + y) - transform.parent.GetComponent<Planet>().center;
				vec.Normalize();

				float heightVal = Noise.PerlinValue(vec.x, vec.y);

				float distFromCenter = Vector2.Distance(new Vector2(transform.position.x + x, transform.position.y + y), transform.parent.GetComponent<Planet>().center);

				if (distFromCenter < transform.parent.GetComponent<Planet>().coreRadius)
				{
					pixels[y * GameManager.CHUNK_SIZE + x] = TileColors.MANTLE_BASE;
				}
				else if (distFromCenter < transform.parent.GetComponent<Planet>().radius - hillHeight*heightVal)
				{
					if (distFromCenter > transform.parent.GetComponent<Planet>().radius - hillHeight - 16f)
					{
						pixels[y * GameManager.CHUNK_SIZE + x] = TileColors.CRUST_BASE;
					}
					else
					{
						if (noiseData[x, y] >= isoValue)
						{
							pixels[y * GameManager.CHUNK_SIZE + x] = TileColors.CRUST_BASE;
						}
						else
						{
							pixels[y * GameManager.CHUNK_SIZE + x] = Color.clear;
						}
					}
				}
			}
		}

		chunkTexture.SetPixels(pixels);
		chunkTexture.filterMode = FilterMode.Point;
		chunkTexture.Apply();

		meshRenderer.material.mainTexture = chunkTexture;
	}
}
*/