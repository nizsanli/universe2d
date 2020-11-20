using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Planet : MonoBehaviour {

	// center is at transform.position
	public int radiusChunks;

	// square that contains planet
	public Rect planetFrame;

	public ChunkTexture[] chunks;

	public ChunkTexture chunkTexture;

	private GameManager gameManager;
	private float viewFactor;

	public Queue<DictionaryEntry> query;

	public byte[] genChunks;

	public float div;

	public void Init(GameManager gameManager)
	{
		radiusChunks = gameManager.radiusChunks;
		float radius = radiusChunks * GameManager.CHUNK_SIZE;

		chunks = new ChunkTexture[(radiusChunks*2)*(radiusChunks*2)];
		div = 8;
		genChunks = new byte[(int)((radiusChunks*2*div)*(radiusChunks*2*div))];

		query = new Queue<DictionaryEntry>();

		this.gameManager = gameManager;
		gameManager.lastGenCent = new Vector2(gameManager.ship.transform.position.x, gameManager.ship.transform.position.y);

		planetFrame = new Rect(-radius, -radius, radius*2f, radius*2f);

		Load(2f);

		/*
		float factor = 1f;
		Rect innerFrame = new Rect(gameManager.ship.transform.position.x - gameManager.viewWidth*factor*3f, gameManager.ship.transform.position.y - gameManager.viewHeight*factor*3f, gameManager.viewWidth*factor*2f*3f, gameManager.viewHeight*factor*2f*3f);
		
		Rect loadBox = MathTests.GetOverlappingBox(planetFrame, innerFrame);
		Rect loadBoxGrid = MathTests.BoxChunkUnits(loadBox);
		Rect chunkGenFrame = MathTests.BoxToGrid(loadBoxGrid);
		
		GenerateChunksWithin(chunkGenFrame, loadBoxGrid);
		*/
	}

	public void GenerateChunksWithin(Rect frameDisc, Rect frameCont)
	{
		for (float y = frameDisc.y; y < frameDisc.y+frameDisc.height; y++)
		{
			for (float x = frameDisc.x; x < frameDisc.x+frameDisc.width; x++)
			{
				ChunkTexture chunk = null;
				if (chunks[((int)y+radiusChunks) * radiusChunks*2 + ((int)x+radiusChunks)] != null)
				{
					chunk = chunks[((int)y+radiusChunks) * radiusChunks*2 + ((int)x+radiusChunks)];
				}
				else
				{
					Vector3 chunkPos = new Vector3(x*GameManager.CHUNK_SIZE, y*GameManager.CHUNK_SIZE, 0f);

					chunk = (ChunkTexture) Instantiate(chunkTexture, chunkPos, Quaternion.identity);
					chunk.Init();
					
					chunk.transform.parent = gameObject.transform;

					// chunk.FillTextureTrans();

					chunks[((int)y+radiusChunks) * radiusChunks*2 + ((int)x+radiusChunks)] = chunk;
				}
				
				Rect chunkBox = new Rect(x, y, 1f, 1f);
				Rect overlapBox = MathTests.GetOverlappingBox(frameCont, chunkBox);
				
				Rect chunkDivBox = GetChunkDivisionBox(chunkBox, overlapBox, div);

				GenParts(chunk, chunkDivBox);
			}
		}
	}

	private void GenParts(ChunkTexture chunk, Rect genBox)
	{
		int xPos = (int)(chunk.transform.position.x/(GameManager.CHUNK_SIZE/div)+radiusChunks*div);
		int yPos = (int)(chunk.transform.position.y/(GameManager.CHUNK_SIZE/div)+radiusChunks*div);
		
		int pixelsPart = (int)(GameManager.CHUNK_SIZE/div);

		for (float y = genBox.y; y < genBox.y+genBox.height; y++)
		{
			for (float x = genBox.x; x < genBox.x+genBox.width; x++)
			{
				int index = (int)((yPos+y)*(radiusChunks*div*2) + (xPos+x));

				if (genChunks[index] == 0)
				{
					Rect partPixelsBox = new Rect(x*pixelsPart, y*pixelsPart, pixelsPart, pixelsPart);

					query.Enqueue(new DictionaryEntry(chunk, partPixelsBox));

					genChunks[index] = 1;
				}
			}
		}
	}

	public Rect GetChunkDivisionBox(Rect chunkBox, Rect overlapBox, float div)
	{
		float xIndex = Mathf.Floor((overlapBox.x - chunkBox.x)*div);
		float yIndex = Mathf.Floor((overlapBox.y - chunkBox.y)*div);
		float xIndex2 = Mathf.Ceil((overlapBox.x + overlapBox.width - chunkBox.x)*div);
		float yIndex2 = Mathf.Ceil((overlapBox.y + overlapBox.height - chunkBox.y)*div);
		
		return new Rect(xIndex, yIndex, xIndex2-xIndex, yIndex2-yIndex);
	}

	public Rect GetPixelsBox(Rect chunkBox, Rect overlapBox)
	{
		float xIndex = Mathf.Floor((overlapBox.x - chunkBox.x)*GameManager.CHUNK_SIZE);
		float yIndex = Mathf.Floor((overlapBox.y - chunkBox.y)*GameManager.CHUNK_SIZE);
		float xIndex2 = Mathf.Ceil((overlapBox.x + overlapBox.width - chunkBox.x)*GameManager.CHUNK_SIZE);
		float yIndex2 = Mathf.Ceil((overlapBox.y + overlapBox.height - chunkBox.y)*GameManager.CHUNK_SIZE);
		
		return new Rect(xIndex, yIndex, xIndex2-xIndex, yIndex2-yIndex);
	}

	public void ApplyRadial(Vector3 point, float radius)
	{
		point = Camera.main.ScreenToWorldPoint(point);
		Rect dmgBox = new Rect(point.x-radius, point.y-radius, radius*2, radius*2f);

		Rect chunkRange = MathTests.GetOverlappingBox(planetFrame, dmgBox);
		Rect chunkRangeUnits = MathTests.BoxChunkUnits(chunkRange);
		Rect chunkRangeUnitsDisc = MathTests.BoxToGrid(chunkRangeUnits);

		for (float y = chunkRangeUnitsDisc.y; y < chunkRangeUnitsDisc.y+chunkRangeUnitsDisc.height; y++)
		{
			for (float x = chunkRangeUnitsDisc.x; x < chunkRangeUnitsDisc.x+chunkRangeUnitsDisc.width; x++)
			{
				ChunkTexture chunk = chunks[((int)y+radiusChunks)*radiusChunks*2 + ((int)x+radiusChunks)];

				Rect chunkBox = new Rect(x, y, 1f, 1f);
				Rect overlapBox = MathTests.GetOverlappingBox(chunkRangeUnits, chunkBox);

				Rect pixelsBox = GetPixelsBox(chunkBox, overlapBox);

				chunk.ApplyRadial(point, radius, pixelsBox);
				chunk.UpdateColliders(pixelsBox);
			}
		}

	}

	// Use this for initialization
	void Start () {
		
	}

	private void Load(float factor)
	{
		Rect innerFrame = new Rect(gameManager.ship.transform.position.x - gameManager.viewWidth*factor*0.5f, gameManager.ship.transform.position.y - gameManager.viewHeight*factor*0.5f, gameManager.viewWidth*factor, gameManager.viewHeight*factor);
		
		Rect loadBox = MathTests.GetOverlappingBox(planetFrame, innerFrame);
		Rect loadBoxGrid = MathTests.BoxChunkUnits(loadBox);
		Rect chunkGenFrame = MathTests.BoxToGrid(loadBoxGrid);
		
		GenerateChunksWithin(chunkGenFrame, loadBoxGrid);
		
		gameManager.lastGenCent = new Vector2(gameManager.ship.transform.position.x, gameManager.ship.transform.position.y);
	}

	// Update is called once per frame
	void Update () {

		float dist = Vector2.Distance(new Vector2(gameManager.ship.transform.position.x, gameManager.ship.transform.position.y), gameManager.lastGenCent);
		if (dist > 32f)
		{
			Load(4f);
		}


		if (query.Count > 0)
		{
			GenerateBatch();
		}
	}

	private void GenerateBatch()
	{
		int length = 2;
		length = (int) Mathf.Min(length, query.Count);

		List<ChunkTexture> chunkList = new List<ChunkTexture>();
		
		for (int i = 0; i < length; i++)
		{
			DictionaryEntry entry = query.Dequeue();

			ChunkTexture chunk = (ChunkTexture) entry.Key;
			Rect pixelsBox = (Rect) entry.Value;

			chunk.FillTexturePlanet(pixelsBox);
			chunk.UpdateColliders(pixelsBox);

			if (!chunkList.Contains(chunk))
			{
				chunkList.Add(chunk);
			}
		}

		foreach (ChunkTexture chunk in chunkList)
		{
			chunk.chunkTexture.Apply();
		}

	}
}
