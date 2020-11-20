using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {
	
	public static int CHUNK_SIZE = 1024;
	public Planet planetPrefab;

	public static Planet testPlanet;

	public Rigidbody2D ship;

	public float viewWidth;
	public float viewHeight;

	public Vector2 lastGenCent;

	public int radiusChunks;

	void Awake()
	{
		radiusChunks = 1;
		ship.transform.position = new Vector3(0f, (radiusChunks-4)*GameManager.CHUNK_SIZE, 0f);
	}

	// Use this for initialization
	void Start () {
		testPlanet = (Planet) Instantiate(planetPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity);
		Camera.main.orthographicSize = 64f;

		viewHeight = Camera.main.orthographicSize * 2f;
		// viewWidth = Camera.main.aspect * viewHeight;
		viewWidth = viewHeight*2f;

		testPlanet.Init(this);
	}
	
	// Update is called once per frame
	void Update () {
		float distSqrd = Vector2.SqrMagnitude(new Vector2(ship.transform.position.x, ship.transform.position.y) - new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.y));
		float snapDist = 16f;
		float snapDistSqrd = snapDist * snapDist;
		float damp = 0.95f;
		if (distSqrd > snapDistSqrd)
		{
			Vector2 vecSnap = (new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.y) - new Vector2(ship.transform.position.x, ship.transform.position.y)).normalized * snapDist;

			Vector2 newCamPos = new Vector2(ship.transform.position.x, ship.transform.position.y) + vecSnap;
			Camera.main.transform.position = new Vector3(newCamPos.x, newCamPos.y, Camera.main.transform.position.z);
			// Camera.main.transform.position = new Vector3(ship.transform.position.x, ship.transform.position.y, Camera.main.transform.position.z);
		}

		Vector3 from = new Vector3(0f, 1f, 0f);
		Vector3 to = new Vector3(ship.transform.position.x, ship.transform.position.y, 0f) - testPlanet.transform.position;

		Camera.main.transform.localRotation = Quaternion.FromToRotation(from, to);


		if (Input.GetMouseButton(0))
		{
			testPlanet.ApplyRadial(Input.mousePosition, 32f);
		}

		if (Input.GetMouseButton(1))
		{
			float multiplier = 20f;
			Camera.main.transform.Translate(new Vector3(-Input.GetAxis("Mouse X")*multiplier, -Input.GetAxis("Mouse Y")*multiplier, 0f));
		}

		float mult = Camera.main.orthographicSize*250f / (GameManager.CHUNK_SIZE);
		Camera.main.orthographicSize += -Input.GetAxis("Mouse ScrollWheel") * mult;
	}



}
