using UnityEngine;
using System.Collections;

public class BoxScript : MonoBehaviour {

	private bool touched = false;

	private bool up = false;
	private bool left = false;
	private bool right = false;

    public float gravityAmount;

	// Use this for initialization
	void Start () {
		Rigidbody2D body = gameObject.GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
		Rigidbody2D body = gameObject.GetComponent<Rigidbody2D>();
		// body.AddForce(new Vector2 (200f, 0f));

		if (Input.GetKey(KeyCode.DownArrow))
		{
			GameManager.testPlanet.ApplyRadial(new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z), 32f);
		}
	}

	void FixedUpdate()
	{
		Vector2 vecToCenter = new Vector2(transform.position.x, transform.position.y);
		Vector2 vecToCenterNorm = vecToCenter.normalized;

		Physics2D.gravity = -vecToCenter * gravityAmount;

		Rigidbody2D body = gameObject.GetComponent<Rigidbody2D>();

		float upForce = 300f;
		Vector2 up = vecToCenterNorm * upForce;

		float sideForce = 300f;
		Vector3 right = Camera.main.transform.right * sideForce;
		
		body.AddForce(Input.GetAxis("Vertical") * up);
		body.AddForce(Input.GetAxis("Horizontal") * right);

	}
}
