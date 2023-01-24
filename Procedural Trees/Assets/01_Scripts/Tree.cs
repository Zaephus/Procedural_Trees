using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tree : MonoBehaviour {

    [SerializeField]
    private Material treeMaterial;

    [SerializeField]
    private MeshFilter meshFilter;

    [SerializeField]
    private float trunkSize;
    [SerializeField]
    private float height;
    
    public void Generate() {
        
        meshFilter.sharedMesh = new Mesh();
        meshFilter.GetComponent<MeshRenderer>().sharedMaterial = treeMaterial;

        Vector3[] points = {
            transform.position,
            transform.position + new Vector3(Random.Range(-trunkSize, trunkSize), height/2, Random.Range(-trunkSize, trunkSize)),
            transform.position + new Vector3(Random.Range(-trunkSize, trunkSize), height, Random.Range(-trunkSize, trunkSize))
        };

        Vector3[] vertices = new Vector3[points.Length * 4];
        int[] triangles = new int[(points.Length * 8)*3];

        for(int i = 0; i < points.Length; i++) {

            int inverseIndex = points.Length - i;

            vertices[i*4] = points[i] + new Vector3(inverseIndex * trunkSize, 0, 0);
            vertices[i*4 + 1] = points[i] + new Vector3(0, 0, inverseIndex * trunkSize);
            vertices[i*4 + 2] = points[i] + new Vector3(-inverseIndex * trunkSize, 0, 0);
            vertices[i*4 + 3] = points[i] + new Vector3(0, 0, -inverseIndex * trunkSize);

        }

        //vertices[vertices.Length-1] = points[points.Length-1];

        for (int i = 0; i < vertices.Length; i++) {
            triangles[i * 3 + 0] = i;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
 
            // triangles[i * 3 + 3] = i * 2 + 2;
            // triangles[i * 3 + 4] = i * 2 + 1;
            // triangles[i * 3 + 5] = i * 2 + 3;
        }

        Debug.Log(vertices.Length);
        Debug.Log(triangles.Length);

        Vector3[] finalVertices = new Vector3[triangles.Length];
        for (int i = 0; i < triangles.Length; i++) {
            Debug.Log(triangles[i]);
            finalVertices[i] = vertices[triangles[i]];
            triangles[i] = i;
        }

        meshFilter.sharedMesh.Clear();
        meshFilter.sharedMesh.vertices = finalVertices;
        meshFilter.sharedMesh.triangles = triangles;
        meshFilter.sharedMesh.RecalculateNormals();
        meshFilter.sharedMesh.RecalculateBounds();

    }
}
