using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerlineSettings : MonoBehaviour
{
    public int powerlineResolution;
	public float lineShapeStrenght;
	public AnimationCurve lineShape;
    public List<PowerPole> instantiatedPowerpoles = new List<PowerPole>();


	public void CreatePowerlines()
	{

		int counter = 0;
		foreach (PowerPole pole in instantiatedPowerpoles)
		{
			if (counter < instantiatedPowerpoles.Count -1)
			{
				PowerPole connectingPole = instantiatedPowerpoles[counter + 1];
				pole.connectedPole = connectingPole;

				CreateLine(pole.frontBottomConnectors.L.GetComponent<LineRenderer>(), 
					pole.frontBottomConnectors.L.position, 
					connectingPole.rearBottomConnectors.L.position);

				CreateLine(pole.frontBottomConnectors.M.GetComponent<LineRenderer>(),
					pole.frontBottomConnectors.M.position,
					connectingPole.rearBottomConnectors.M.position);

				CreateLine(pole.frontBottomConnectors.R.GetComponent<LineRenderer>(),
					pole.frontBottomConnectors.R.position,
					connectingPole.rearBottomConnectors.R.position);


				CreateLine(pole.frontTopConnectors.L.GetComponent<LineRenderer>(),
					pole.frontTopConnectors.L.position,
					connectingPole.rearTopConnectors.L.position);

				CreateLine(pole.frontTopConnectors.M.GetComponent<LineRenderer>(),
					pole.frontTopConnectors.M.position,
					connectingPole.rearTopConnectors.M.position);

				CreateLine(pole.frontTopConnectors.R.GetComponent<LineRenderer>(),
					pole.frontTopConnectors.R.position,
					connectingPole.rearTopConnectors.R.position);
			}
			counter++;
		}
	}

	private void CreateLine(LineRenderer renderer, Vector3 start, Vector3 connectedPole)
    {
		renderer.positionCount = powerlineResolution + 1;
		for (int i = 0; i <= powerlineResolution; i++)
        {
			float step = (float)i / powerlineResolution;
			Vector3 lineShapeOffset = (Vector3.down * ((1f - lineShape.Evaluate(step)) *  lineShapeStrenght));
			renderer.SetPosition(i, Vector3.Lerp(start, connectedPole, step) + lineShapeOffset);
        }
	}

	public void Clear()
    {
		//Reset road to spawn new objects
		if (instantiatedPowerpoles.Count > 0)
			for (int i = instantiatedPowerpoles.Count -1 ; i >= 0; i--)
				DestroyImmediate(instantiatedPowerpoles[i].gameObject);
		instantiatedPowerpoles = new List<PowerPole>();
	}
}
