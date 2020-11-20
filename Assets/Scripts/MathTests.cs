using UnityEngine;
using System.Collections;

public class MathTests {

	public static Rect GetOverlappingBox(Rect moving, Rect stationary)
	{
		if ((stationary.x + stationary.width - moving.x) > 0 && (stationary.y + stationary.height - moving.y) > 0)
		{
			if ((moving.x + moving.width - stationary.x) > 0 && (moving.y + moving.height - stationary.y) > 0)
			{
				// something is overlapping
				
				float minX = Mathf.Max(moving.x, stationary.x);
				float minY = Mathf.Max(moving.y, stationary.y);
				
				float maxX = Mathf.Min(moving.x + moving.width, stationary.x + stationary.width);
				float maxY = Mathf.Min(moving.y + moving.height, stationary.y + stationary.height);

				return new Rect(minX, minY, maxX-minX, maxY-minY);
			}
			else
			{
				return new Rect(0f, 0f, 0f, 0f);
			}
		}
		else
		{
			return new Rect(0f, 0f, 0f, 0f);
		}
	}

	public static Rect BoxChunkUnits(Rect box)
	{
		float newX = box.x /= GameManager.CHUNK_SIZE;
		float newY = box.y /= GameManager.CHUNK_SIZE;
		float newWidth = box.width /= GameManager.CHUNK_SIZE;
		float newHeight = box.height /= GameManager.CHUNK_SIZE;

		return new Rect(newX, newY, newWidth, newHeight);
	}

	public static Rect BoxToGrid(Rect box)
	{
		Vector2 gridPointMin = new Vector2(Mathf.Floor(box.x), Mathf.Floor(box.y));
		Vector2 gridPointMax = new Vector2(Mathf.Ceil((box.x+box.width)), Mathf.Ceil(box.y+box.height));

		return new Rect(gridPointMin.x, gridPointMin.y, gridPointMax.x-gridPointMin.x, gridPointMax.y-gridPointMin.y);
	}

}
