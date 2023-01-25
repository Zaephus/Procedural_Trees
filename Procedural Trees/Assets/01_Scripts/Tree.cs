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

    //private List<Vector3> points = new Vector3();

    private List<Vector3> vertices = new List<Vector3>();

    [Range(0,1)]
    public int k = 0;
    [Range(0,7)]
    public int l = 3;

    public int[] triangles;
    
    public void Generate() {

        vertices.Clear();
        
        meshFilter.sharedMesh = new Mesh();
        meshFilter.GetComponent<MeshRenderer>().sharedMaterial = treeMaterial;

        Vector3[] points = {
            transform.position,
            transform.position + new Vector3(Random.Range(-trunkSize, trunkSize), height/2, Random.Range(-trunkSize, trunkSize)),
            transform.position + new Vector3(Random.Range(-trunkSize, trunkSize), height, Random.Range(-trunkSize, trunkSize))
        };

        //Vector3[] vertices = new Vector3[points.Length * 4];
        triangles = new int[points.Length * 16];

        for(int i = 0; i < points.Length; i++) {

            int inverseIndex = points.Length - i;

            // vertices[i*4] = points[i] + new Vector3(inverseIndex * trunkSize, 0, 0);
            // vertices[i*4 + 1] = points[i] + new Vector3(0, 0, inverseIndex * trunkSize);
            // vertices[i*4 + 2] = points[i] + new Vector3(-inverseIndex * trunkSize, 0, 0);
            // vertices[i*4 + 3] = points[i] + new Vector3(0, 0, -inverseIndex * trunkSize);

            vertices.Add(points[i] + new Vector3(inverseIndex * trunkSize, 0, 0));
            vertices.Add(points[i] + new Vector3(0, 0, inverseIndex * trunkSize));
            vertices.Add(points[i] + new Vector3(-inverseIndex * trunkSize, 0, 0));
            vertices.Add(points[i] + new Vector3(0, 0, -inverseIndex * trunkSize));

        }

        // for(int i = 1; i < points.Length; i++) {
        //     for(int j = 0; j < 4; j++) {

        //         triangles[((i-1) * 6) + 0 + j] = i * 4 + j;
        //         triangles[((i-1) * 6) + 1 + j] = i * 4 + (1 + j)%4;
        //         triangles[((i-1) * 6) + 2 + j] = (i-1) * 4 + j;

        //         // triangles[((i-1) * 6) + 3 + j] = i * 4 + (1 + j)%4;
        //         // triangles[((i-1) * 6) + 4 + j] = (i-1) * 4 + j;
        //         // triangles[((i-1) * 6) + 5 + j] = (i-1) * 4 + j;

        //     }
        // }
        // triangles[k * 6 + 0] = l + (k + 1) * 4;
        // triangles[k * 6 + 1] = l + (k + 1) * 4 - 3;
        // triangles[k * 6 + 2] = l + (k + 1) * 4 - 4;

        // triangles[k * 6 + 3] = l + (k + 1) * 4;
        // triangles[k * 6 + 4] = l + (k + 1) * 4 + 1;
        // triangles[k * 6 + 5] = l + (k + 1) * 4 - 3;


        // triangles[k * 6 + 0] = l + ((k + 1) * 4) - 1;
        // triangles[k * 6 + 1] = l + ((k + 1) * 4);
        // triangles[k * 6 + 2] = l + ((k + 1) * 4) - 4;

        // triangles[k * 6 + 3] = l + ((k + 1) * 4);
        // triangles[k * 6 + 4] = l + ((k + 1) * 4) - 3;
        // triangles[k * 6 + 5] = l + ((k + 1) * 4) - 4;

        // triangles[l * 6 + 0] = l + 4 - 1;
        // triangles[l * 6 + 1] = l + 4;
        // triangles[l * 6 + 2] = l + 4 - 4;

        // triangles[l * 6 + 3] = l + 4;
        // triangles[l * 6 + 4] = l + 4 - 3;
        // triangles[l * 6 + 5] = l + 4 - 4;

        // Debug.Log(l + (k + 1) * 4);
        // Debug.Log(l + (k + 1) * 4 + 1);
        // Debug.Log(l + (k + 1) * 4 - 3);

        // for(int i = 0; i < points.Length-1; i++) {
        //     for(int j = 0; j < 4; j++) {
        //         triangles[i * j * 6 + 0] = j + (i + 1) * 4 - 1;
        //         triangles[i * j * 6 + 1] = j + (i + 1) * 4;
        //         triangles[i * j * 6 + 2] = j + (i + 1) * 4 - 4;
    
        //         triangles[i * j * 6 + 3] = j + (i + 1) * 4;
        //         triangles[i * j * 6 + 4] = j + (i + 1) * 4 - 3;
        //         triangles[i * j * 6 + 5] = j + (i + 1) * 4 - 4;
        //     }
        // // }

        for(int i = 0; i < triangles.Length/6; i++) {
            triangles[i * 6 + 0] = i + 4 - 1;
            triangles[i * 6 + 1] = i + 4;
            triangles[i * 6 + 2] = i + 4 - 4;

            triangles[i * 6 + 3] = i + 4;
            triangles[i * 6 + 4] = i + 4 - 3;
            triangles[i * 6 + 5] = i + 4 - 4;
        }

        // Vector3[] finalVertices = new Vector3[triangles.Length];
        // for (int i = 0; i < triangles.Length; i++) {
        //     finalVertices[i] = vertices[triangles[i]];
        //     triangles[i] = i;
        // }

        meshFilter.sharedMesh.Clear();
        meshFilter.sharedMesh.vertices = vertices.ToArray();
        meshFilter.sharedMesh.triangles = triangles;
        meshFilter.sharedMesh.RecalculateNormals();
        meshFilter.sharedMesh.RecalculateBounds();

    }

    private void OnDrawGizmos() {
        
        Gizmos.color = Color.blue;

        foreach(Vector3 vec in vertices) {
            Gizmos.DrawSphere(vec, 0.2f);
        }
    }

}
