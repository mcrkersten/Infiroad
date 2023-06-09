using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//[CustomEditor(typeof(Suspension))]
public class SuspensionInspector : Editor
{
 //   void OnEnable()
	//{
	//	SceneView.onSceneGUIDelegate += OnSceneGUI;
	//}

	//void OnDisable()
	//{
	//	SceneView.onSceneGUIDelegate -= OnSceneGUI;
	//}

    void OnSceneGUI(SceneView sceneView) 
    {
  //      EditorUtility.SetDirty(target);
		//Suspension t = target as Suspension;
		//DrawWheels(t);
    }

	private void DrawWheels(Suspension suspension)
	{
		Vector3 centre = suspension.transform.position - (suspension.wheel.transform.up * suspension.springLenght);
		Handles.DrawWireDisc(centre, Vector3.left, suspension.wheel.wheelRadius);
		Debug.DrawLine(suspension.transform.position, centre, Color.red);

		Vector3 offset_2 = centre - (suspension.wheel.transform.up * suspension.maxSpringStretch);
		Debug.DrawLine(centre, offset_2, Color.yellow);

		offset_2 = centre + (suspension.wheel.transform.up * suspension.maxSpringStretch);
		Debug.DrawLine(centre, offset_2, Color.magenta);
	}
}
