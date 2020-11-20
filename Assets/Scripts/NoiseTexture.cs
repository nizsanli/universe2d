using UnityEngine;
using System.Collections;

public class NoiseTexture : MonoBehaviour {

	private Texture2D noiseTex;
	private Color[] pixels;

	[Range(0f, 15f)]
	public float scale;
	private float lastScale;
	[Range(0f, 10000f)]
	public float noiseXOrigin;
	private float lastNoiseXOrigin;
	[Range(0f, 10000f)]
	public float noiseYOrigin;
	private float lastNoiseYOrigin;
	
	public int picWidth;
	public int picHeight;

	[Range (0f, 1f)]
	public float isoValue;
	private float lastIsoValue;

	private Sprite sprite;

	// Use this for initialization
	void Start () {
		scale = 2;
		lastScale = scale;
		noiseXOrigin = 0f;
		noiseYOrigin = 0f;
		lastNoiseXOrigin = noiseXOrigin;
		lastNoiseYOrigin = noiseYOrigin;
		picWidth = 256;
		picHeight = 256;
		noiseTex = new Texture2D(picWidth, picHeight);
		pixels = new Color[picWidth * picHeight];

		isoValue = .65f;
		lastIsoValue = isoValue;
		applyNoise();

		Vector3 bottomLeft = new Vector3(-picWidth * 0.5f, -picHeight * 0.5f, 0f);
		transform.position = bottomLeft;
		Camera.main.orthographicSize = Screen.height * 0.5f;
	}
	
	// Update is called once per frame
	void Update () {
		// applyNoise();
		float speedScroll = 100f;
		Camera.main.orthographicSize += Input.GetAxis("Mouse ScrollWheel") * speedScroll;

		float speedTranslate = 10f;
		if (Input.GetMouseButton(0))
		{
			Vector3 translateVec = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0f) * speedTranslate;
			Camera.main.transform.Translate(translateVec);		
		}

		if (lastScale != scale || lastNoiseXOrigin != noiseXOrigin || lastNoiseYOrigin != noiseYOrigin || lastIsoValue != isoValue)
		{
			applyNoise();
			lastScale = scale;
			lastNoiseXOrigin = noiseXOrigin;
			lastNoiseYOrigin = noiseYOrigin;
			lastIsoValue = isoValue;
		}
	}

	private void applyNoise()
	{
		float[,] noiseData = new float[picWidth, picHeight];
		Noise.SCALE = scale;
		Noise.Perlin2D(noiseData, new Vector2(noiseXOrigin, noiseYOrigin));

		for (int y = 0; y < picHeight; y++)
		{
			for (int x = 0; x < picWidth; x++)
			{


				if (noiseData[x,y] >= isoValue)
				{
					pixels[y * picWidth + x] = new Color(1f, 1f, 1f);
				}
				else
				{
					pixels[y * picWidth + x] = 
						new Color(Camera.main.backgroundColor.r, Camera.main.backgroundColor.g, Camera.main.backgroundColor.b);
				}


				//pixels[y * picWidth + x] = new Color(noiseData[x,y], noiseData[x,y], noiseData[x,y]);
			}
		}

		noiseTex.SetPixels(pixels);
		noiseTex.filterMode = FilterMode.Point;
		noiseTex.Apply();

		sprite = null;
		sprite = Sprite.Create(noiseTex, new Rect(0f, 0f, picWidth, picHeight), new Vector2(0f, 0f), 1f);
		gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
	}
}
