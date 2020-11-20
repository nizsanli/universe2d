using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChunkTexture : MonoBehaviour {

	private MeshRenderer meshRenderer;
	private MeshFilter meshFilter;
	private Mesh mesh;
	
	public Texture2D chunkTexture;

	private byte[] chunkData;
	private CircleCollider2D[] colliders;

	private ChunkTexture left;
	private ChunkTexture up;
	private ChunkTexture right;
	private ChunkTexture down;

	public CircleCollider2D colliderPrefab;

	public static class Tile {
		public static byte SNOW = 2;
		public static byte GRASS = 3;
		public static byte SAND = 4;
		public static byte STONE = 5;
		public static byte MAGMA = 6;
		public static byte DIRT = 7;

	}

	public void Init()
	{
		left = null;
		up = null;
		right = null;
		down = null;

		chunkData = new byte[GameManager.CHUNK_SIZE * GameManager.CHUNK_SIZE];
		colliders = new CircleCollider2D[GameManager.CHUNK_SIZE * GameManager.CHUNK_SIZE];

		chunkTexture = new Texture2D(GameManager.CHUNK_SIZE, GameManager.CHUNK_SIZE);
		chunkTexture.filterMode = FilterMode.Point;
		
		meshRenderer = (MeshRenderer) gameObject.AddComponent<MeshRenderer>();
		meshRenderer.material = new Material(Shader.Find("Unlit/Transparent"));
		meshRenderer.material.mainTexture = chunkTexture;
		
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

	/*
	public void FillTextureTrans()
	{
		Color[] pixels = new Color[(int)(GameManager.CHUNK_SIZE * GameManager.CHUNK_SIZE)];
		meshRenderer.material.mainTexture = chunkTexture;
		chunkTexture.filterMode = FilterMode.Point;
	}
	*/

	public void ApplyRadial(Vector3 point, float radius, Rect frame)
	{
		for (float y = frame.y; y < frame.y+frame.height; y++)
		{
			for (float x = frame.x; x < frame.x+frame.width; x++)
			{
				Vector2 vecToCenter = new Vector2(point.x, point.y) - new Vector2(transform.position.x + x, transform.position.y + y);
				float sqrDist = vecToCenter.x * vecToCenter.x + vecToCenter.y * vecToCenter.y;
				if (sqrDist <= radius*radius*0.9f)
				{
					Color oldColor = chunkTexture.GetPixel((int)x, (int)y);
					chunkTexture.SetPixel((int)x, (int)y, Color.clear);
					chunkData[(int)(y*GameManager.CHUNK_SIZE+x)] = 1;
				}
			}
		}

		chunkTexture.Apply();
	}

	public void UpdateColliders(Rect frame)
	{
		Planet planet = transform.parent.GetComponent<Planet>();

		Vector2 chunkCoor = new Vector2(transform.position.x/GameManager.CHUNK_SIZE + planet.radiusChunks, transform.position.y/GameManager.CHUNK_SIZE + planet.radiusChunks);

		if (chunkCoor.x > 0)
		{
			left = planet.chunks[(int)(chunkCoor.y*planet.radiusChunks*2 + (chunkCoor.x - 1))];
		}
		if (chunkCoor.x < planet.radiusChunks*2 - 1)
		{
			right = planet.chunks[(int)(chunkCoor.y*planet.radiusChunks*2 + (chunkCoor.x + 1))];
		}
		if (chunkCoor.y > 0)
		{
			down = planet.chunks[(int)((chunkCoor.y - 1)*planet.radiusChunks*2 + (chunkCoor.x))];
		}
		if (chunkCoor.y < planet.radiusChunks*2 - 1)
		{
			up = planet.chunks[(int)((chunkCoor.y + 1)*planet.radiusChunks*2 + (chunkCoor.x))];
		}


		for (float y = frame.y; y < frame.y+frame.height; y++)
		{
			for (float x = frame.x; x < frame.x+frame.width; x++)
			{
				int index = (int)(y * GameManager.CHUNK_SIZE + x);

				if (chunkData[index] > 1 && colliders[index] == null)
				{
					ColliderAt(x, y);
				}
				else if (chunkData[index] <= 1 && colliders[index] != null)
				{
					Destroy(colliders[index]);
					colliders[index] = null;
				}

			}
		}
	}

	private void AddCollider(float x, float y)
	{
		int index = (int)(y * GameManager.CHUNK_SIZE + x);

		CircleCollider2D collider = (CircleCollider2D) Instantiate(colliderPrefab, new Vector3(transform.position.x + x + 0.5f, transform.position.y + y + 0.5f), Quaternion.identity);
		collider.transform.parent = gameObject.transform;
		colliders[index] = collider;
	}

	private void ColliderAt(float x, float y)
	{
		int index = (int)(y * GameManager.CHUNK_SIZE + x);

		if (x == 0)
		{
			if (left == null)
			{
				AddCollider(x, y);
				return;
			}
			else if (left.chunkData[(int)(y*GameManager.CHUNK_SIZE + GameManager.CHUNK_SIZE-1)] == 1)
			{
				// check left chunk
				AddCollider(x, y);
				return;
			}
		}
		else if (chunkData[(int)(index-1)] == 1)
		{
			// left
			AddCollider(x, y);
			return;
		}

		if (x == GameManager.CHUNK_SIZE-1)
		{
			if (right == null)
			{
				AddCollider(x, y);
				return;
			}
			else if (right.chunkData[(int)(y*GameManager.CHUNK_SIZE)] == 1)
			{
				// check right chunk
				AddCollider(x, y);
				return;
			}
		}
		else if (chunkData[(int)(index+1)] == 1)
		{
			// right
			AddCollider(x, y);
			return;
		}

		if (y == 0)
		{
			if (down == null)
			{
				AddCollider(x, y);
				return;
			}
			else if (down.chunkData[(int)((GameManager.CHUNK_SIZE-1)*GameManager.CHUNK_SIZE + x)] == 1)
			{
				// check down chunk
				AddCollider(x, y);
				return;
			}
		}
		else if (chunkData[(int)(index-GameManager.CHUNK_SIZE)] == 1)
		{
			// down
			AddCollider(x, y);
			return;
		}

		if (y == GameManager.CHUNK_SIZE-1)
		{
			if (up == null)
			{
				AddCollider(x, y);
				return;
			}
			else if (up.chunkData[(int)x] == 1)
			{
				// check up chunk
				AddCollider(x, y);
				return;
			}
		}
		else if (chunkData[(int)(index+GameManager.CHUNK_SIZE)] == 1)
		{
			// up
			AddCollider(x, y);
			return;
		}
	}

	public void FillTexturePlanet(Rect frame)
	{
		Planet planet = transform.parent.GetComponent<Planet>();
		float radius = planet.radiusChunks * GameManager.CHUNK_SIZE;

		float a = 0.6f;
		float baseLev = radius * a;
		float core = radius * 0.3f;

		Color tileColor;

		float offset = 2.5f;
		float radScale = 1f;
		float noiseScale = 120f;
		float baseVar = radius * a * 0.2f;
		float topVar = baseVar * 0.01f;
		float snowVar = baseVar * 0.5f;

		float hillRange1 = radius * a * 0.5f;
		float hillRange2 = hillRange1 * 0.2f;
		
		for (float y = frame.y; y < frame.y+frame.height; y++)
		{
			for (float x = frame.x; x < frame.x+frame.width; x++)
			{
				int dataIndex = (int)(y*GameManager.CHUNK_SIZE + x);

				Vector2 vecToCenter = new Vector2(transform.position.x + x, transform.position.y + y) - new Vector2(planet.transform.position.x, planet.transform.position.y);
				float distSqrd = vecToCenter.x * vecToCenter.x + vecToCenter.y * vecToCenter.y;
				float radiusSqrd = radius*radius;

				byte type = 1;

				if (distSqrd < radiusSqrd)
				{
					float noiseVal = Mathf.PerlinNoise((transform.position.x + x + radius)*0.01f*noiseScale, (transform.position.y + y + radius)*0.01f*noiseScale);

					// optimize!
					float dist = Mathf.Sqrt(distSqrd);
					Vector2 vecNorm = vecToCenter.normalized;


					float radNoise = Mathf.PerlinNoise((vecNorm.x + offset)*radScale, (vecNorm.y + offset)*radScale);
					float radNoise2 = Mathf.PerlinNoise((vecNorm.x + offset)*radScale*3f, (vecNorm.y + offset)*radScale*3f);

					float radVariance = radNoise*hillRange1 + radNoise2*hillRange2;

					float baseLev2 = baseLev + radVariance;

					float snowLev = baseLev + (hillRange1 + hillRange2)*0.88f;
					float seaLev = baseLev + (hillRange1 + hillRange2)*0.45f;

					if (dist < baseLev2)
					{
						if (dist > snowLev - baseVar*radNoise2 - 20f*noiseVal)
						{
							// snow
							noiseVal *= 1.5f;
							tileColor = Color.white;
							
							type = (byte)Tile.SNOW;
						}
						else if (dist > seaLev && dist > (baseLev2 - topVar*radNoise2 - 8f*noiseVal))
						{
							// grass
							noiseVal *= 1.5f;
							tileColor = Color.green;
							
							type = (byte)Tile.GRASS;
						}
						else if (dist < seaLev && dist > (baseLev2 - 10f*noiseVal))
						{
							// sand
							noiseVal *= 1.1f;
							tileColor = Color.yellow;
							
							type = (byte)Tile.SAND;
						}
						else if (dist > (baseLev + baseVar*radNoise + 50f*noiseVal))
						{
							// dirt
							noiseVal *= 1.5f;
							tileColor = TileColors.DIRT;
							
							type = (byte)Tile.DIRT;
						}
						else if (dist > core - 10f*noiseVal)
						{
							// stone
							tileColor = Color.gray;
							noiseVal *= 0.8f;

							type = (byte)Tile.STONE;
						}
						else
						{
							tileColor = Color.red;
							noiseVal *= 0.5f;

							type = (byte)Tile.MAGMA;
						}

						chunkTexture.SetPixel((int)x, (int)y, new Color(tileColor.r*noiseVal, tileColor.g*noiseVal, tileColor.b*noiseVal, 1f));
					}
				}

				if (type == 1)
				{
					chunkTexture.SetPixel((int)x, (int)y, new Color(0f, 0f, 0f, 0f));
				}

				chunkData[dataIndex] = type;
			}
		}

		// chunkTexture.Apply();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
