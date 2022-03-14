using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FenceSettings : MonoBehaviour
{
    public List<FencePart> instantiatedFenceParts = new List<FencePart>();

    public void CreateFenceBars()
    {
		int counter = 0;
		foreach (FencePart fence in instantiatedFenceParts)
		{
			if (counter < instantiatedFenceParts.Count - 1)
			{
				FencePart connectingFence = instantiatedFenceParts[counter + 1];

				fence.SetFenceBars(fence.transform.position, connectingFence.transform.position);
			}
			counter++;
		}
	}

	public void Clear()
	{
		//Reset road to spawn new objects
		if (instantiatedFenceParts.Count > 0)
			for (int i = instantiatedFenceParts.Count - 1; i >= 0; i--)
				DestroyImmediate(instantiatedFenceParts[i].gameObject);
		instantiatedFenceParts = new List<FencePart>();
	}
}
