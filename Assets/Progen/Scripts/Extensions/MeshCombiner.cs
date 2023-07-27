using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

public class MeshCombiner : MonoBehaviour
{
    public Material[] materialsToCombine;

    [ContextMenu("CombineMeshes")]
    private void CombineMeshes()
    {
        List<MeshFilter> meshFilters = GetComponentsInChildren<MeshFilter>().ToList();

        // Create a dictionary to hold the combined meshes for each material
        Dictionary<Material, List<MeshFilter>> collectedMeshes = new Dictionary<Material, List<MeshFilter>>();

        foreach (MeshFilter meshFilter in meshFilters)
        {
            // Get the materials of the current mesh
            Material[] meshMaterials = meshFilter.GetComponent<MeshRenderer>().sharedMaterials;

            foreach (Material material in meshMaterials)
            {
                 // Check if the material is in the list of materials to combine
                if (Array.Exists(materialsToCombine, m => m == material))
                {
                    // Check if the material already has a collected mesh list
                    if (!collectedMeshes.ContainsKey(material))
                        collectedMeshes[material] = new List<MeshFilter>() { meshFilter };
                    else
                        collectedMeshes[material].Add(meshFilter);
                }
            }
        }

        GameObject buildingObject = new GameObject("Building");

        // Iterate through the combined meshes and create a single combined mesh for each material
        foreach (var kvp in collectedMeshes)
        {
            Material material = kvp.Key;
            List<MeshFilter> filters = kvp.Value;

            // Create a new combined mesh
            CombineInstance[] combineInstances = new CombineInstance[filters.Count];
            int i = 0;
            foreach (MeshFilter mesh in filters)
            {
                Material[] materialsOnMesh = mesh.GetComponent<MeshRenderer>().sharedMaterials;
                for (int m = 0; m < materialsOnMesh.Length; m++)
                {
                        if (materialsOnMesh[m] == material)
                            combineInstances[i].mesh = mesh.sharedMesh.GetSubmesh(m);
                }
                combineInstances[i].transform = mesh.transform.localToWorldMatrix;
                i++;
            }
            // Create a new GameObject to hold the combined mesh
            GameObject combinedMeshObject = new GameObject(material.name + "_CombinedMesh");
            combinedMeshObject.transform.SetParent(buildingObject.transform);
            combinedMeshObject.transform.localPosition = Vector3.zero;
            combinedMeshObject.transform.localRotation = Quaternion.identity;
            combinedMeshObject.transform.localScale = Vector3.one;

            // Add required components to the new GameObject
            MeshFilter combinedMeshFilter = combinedMeshObject.AddComponent<MeshFilter>();
            MeshRenderer combinedMeshRenderer = combinedMeshObject.AddComponent<MeshRenderer>();

            // Combine the meshes into a single mesh
            combinedMeshFilter.sharedMesh = new Mesh();
            combinedMeshFilter.sharedMesh.indexFormat = IndexFormat.UInt32;
            combinedMeshFilter.sharedMesh.CombineMeshes(combineInstances, true);

            // Set the material for the combined mesh
            combinedMeshRenderer.sharedMaterial = material;
        }
    }
}

public static class MeshExtension
 {
     private class Vertices
     {
         List<Vector3> verts = null;
         List<Vector2> uv1 = null;
         List<Vector2> uv2 = null;
         List<Vector2> uv3 = null;
         List<Vector2> uv4 = null;
         List<Vector3> normals = null;
         List<Vector4> tangents = null;
         List<Color32> colors = null;
         List<BoneWeight> boneWeights = null;
 
         public Vertices()
         {
             verts = new List<Vector3>();
         }
         public Vertices(Mesh aMesh)
         {
             verts = CreateList(aMesh.vertices);
             uv1 = CreateList(aMesh.uv);
             uv2 = CreateList(aMesh.uv2);
             uv3 = CreateList(aMesh.uv3);
             uv4 = CreateList(aMesh.uv4);
             normals = CreateList(aMesh.normals);
             tangents = CreateList(aMesh.tangents);
             colors = CreateList(aMesh.colors32);
             boneWeights = CreateList(aMesh.boneWeights);
         }
 
         private List<T> CreateList<T>(T[] aSource)
         {
             if (aSource == null || aSource.Length == 0)
                 return null;
             return new List<T>(aSource);
         }
         private void Copy<T>(ref List<T> aDest, List<T> aSource, int aIndex)
         {
             if (aSource == null)
                 return;
             if (aDest == null)
                 aDest = new List<T>();
             aDest.Add(aSource[aIndex]);
         }
         public int Add(Vertices aOther, int aIndex)
         {
             int i = verts.Count;
             Copy(ref verts, aOther.verts, aIndex);
             Copy(ref uv1, aOther.uv1, aIndex);
             Copy(ref uv2, aOther.uv2, aIndex);
             Copy(ref uv3, aOther.uv3, aIndex);
             Copy(ref uv4, aOther.uv4, aIndex);
             Copy(ref normals, aOther.normals, aIndex);
             Copy(ref tangents, aOther.tangents, aIndex);
             Copy(ref colors, aOther.colors, aIndex);
             Copy(ref boneWeights, aOther.boneWeights, aIndex);
             return i;
         }
         public void AssignTo(Mesh aTarget)
         {
             if (verts.Count > 65535)
                 aTarget.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
             aTarget.SetVertices(verts);
             if (uv1 != null) aTarget.SetUVs(0, uv1);
             if (uv2 != null) aTarget.SetUVs(1, uv2);
             if (uv3 != null) aTarget.SetUVs(2, uv3);
             if (uv4 != null) aTarget.SetUVs(3, uv4);
             if (normals != null) aTarget.SetNormals(normals);
             if (tangents != null) aTarget.SetTangents(tangents);
             if (colors != null) aTarget.SetColors(colors);
             if (boneWeights != null) aTarget.boneWeights = boneWeights.ToArray();
         }
     }
 
     public static Mesh GetSubmesh(this Mesh aMesh, int aSubMeshIndex)
     {
         if (aSubMeshIndex < 0 || aSubMeshIndex >= aMesh.subMeshCount)
             return null;
         int[] indices = aMesh.GetTriangles(aSubMeshIndex);
         Vertices source = new Vertices(aMesh);
         Vertices dest = new Vertices();
         Dictionary<int, int> map = new Dictionary<int, int>();
         int[] newIndices = new int[indices.Length];
         for (int i = 0; i < indices.Length; i++)
         {
             int o = indices[i];
             int n;
             if (!map.TryGetValue(o, out n))
             {
                 n = dest.Add(source, o);
                 map.Add(o,n);
             }
             newIndices[i] = n;
         }
         Mesh m = new Mesh();
         dest.AssignTo(m);
         m.triangles = newIndices;
         return m;
     }
 }