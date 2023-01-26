using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tree : MonoBehaviour {

    [SerializeField]
    private bool drawGizmos;

    [SerializeField, Range(3, 36)]
    private int vertexResolution;
    [SerializeField]
    private float trunkSize;
    [SerializeField]
    private float height;
    [SerializeField, Range(2, 36)]
    private int heightResolution;

    #region Dependencies
    [Header("Dependencies")]
    [SerializeField]
    private Material treeMaterial;
    [SerializeField]
    private MeshFilter meshFilter;
    #endregion

    #region Main Parameters
    [Header("Main Parameters")]
    [SerializeField]
    private Shape shape;
    [SerializeField]
    private float baseSize;
    [SerializeField]
    private float scale;
    [SerializeField]
    private float scaleVariance;
    [SerializeField]
    private float zScale;
    [SerializeField]
    private float zScaleVariance;
    [SerializeField]
    private float ratio;
    [SerializeField]
    private float ratioPower;
    [SerializeField]
    private int lobes;
    [SerializeField]
    private float lobeDepth;
    [SerializeField]
    private float flare;
    #endregion

    #region Base Parameters
    [Header("Base Parameters")]
    [SerializeField]
    private float baseScale;
    [SerializeField]
    private float baseScaleVariance;
    [SerializeField]
    private float baseLength;
    [SerializeField]
    private float baseLengthVariance;
    [SerializeField]
    private float baseTaper;
    [SerializeField]
    private int baseSplits;
    [SerializeField]
    private int basesegmentSplits;
    [SerializeField]
    private float basesplitAngle;
    [SerializeField]
    private float baseSplitAngleVariance;
    [SerializeField]
    private float baseCurveResolution;
    [SerializeField]
    private float baseCurve;
    [SerializeField]
    private float baseCurveBack;
    [SerializeField]
    private float baseCurveVariance;
    #endregion

    #region Branch Parameters
    [Header("Branch Parameters")]
    [SerializeField]
    private BranchData[] branchDatas;
    #endregion
    private List<Vector3> points = new List<Vector3>();
    private List<Vector3> vertices = new List<Vector3>();

    private enum Shape {
        Conical = 0,
        Spherical = 1,
        HemiSpherical = 2,
        Cylindrical = 3,
        TaperedCylindrical = 4,
        Flame = 5,
        InverseConical = 6,
        TendFlame = 7
    }

    [System.Serializable]
    private class BranchData {

        public float downAngle;
        public float downAngleVariance;
        public float rotate;
        public float rotateVariance;
        public int branches;
        public float length;
        public float lengthVariance;
        public float taper;
        public int segmentSplits;
        public float splitAngle;
        public float splitAngleVariance;
        public float curveResolution;
        public float curve;
        public float curveBack;
        public float curveVariation;

    }
    
    public void Generate() {

        points.Clear();
        vertices.Clear();
        
        meshFilter.sharedMesh = new Mesh();
        meshFilter.GetComponent<MeshRenderer>().sharedMaterial = treeMaterial;

        // points =  new List<Vector3> {
        //     transform.position,
        //     transform.position + new Vector3(Random.Range(-trunkSize, trunkSize), height/2, Random.Range(-trunkSize, trunkSize)),
        //     transform.position + new Vector3(Random.Range(-trunkSize, trunkSize), height, Random.Range(-trunkSize, trunkSize))
        // };

        points.Add(transform.position);
        for(int i = 1; i < heightResolution; i++) {
            int inverseIndex = heightResolution-i;
            points.Add(transform.position + new Vector3(Random.Range(-trunkSize, trunkSize) / inverseIndex, 
                                                        i * (height/heightResolution),
                                                        Random.Range(-trunkSize, trunkSize) / inverseIndex));
        }

        //Vector3[] vertices = new Vector3[points.Length * vertexResolution];

        for(int i = 0; i < points.Count; i++) {

            int inverseIndex = points.Count - i;

            for(int j = 0; j < vertexResolution; j++) {
                float currentAngle = ((2*Mathf.PI)/vertexResolution)*j;
                vertices.Add(points[i] + (trunkSize / (i+2)) * new Vector3(Mathf.Cos(currentAngle), 0, Mathf.Sin(currentAngle)));
            }

        }

        
        int[] triangles = new int[(vertices.Count - vertexResolution) * 6];

        for(int i = 0; i < triangles.Length/6; i++) {
            triangles[i * 6 + 0] = i + vertexResolution - 1;
            triangles[i * 6 + 1] = i + vertexResolution;
            triangles[i * 6 + 2] = i + vertexResolution - vertexResolution;

            triangles[i * 6 + 3] = i + vertexResolution;
            triangles[i * 6 + 4] = i + vertexResolution - vertexResolution + 1;
            triangles[i * 6 + 5] = i + vertexResolution - vertexResolution;
        }

        Vector3[] finalVertices = new Vector3[triangles.Length];
        for (int i = 0; i < triangles.Length; i++) {
            finalVertices[i] = vertices[triangles[i]];
            triangles[i] = i;
        }

        meshFilter.sharedMesh.Clear();
        meshFilter.sharedMesh.vertices = finalVertices;
        meshFilter.sharedMesh.triangles = triangles;
        meshFilter.sharedMesh.RecalculateNormals();
        meshFilter.sharedMesh.RecalculateBounds();

    }

    private void OnDrawGizmos() {
        
        if(drawGizmos) {
            Gizmos.color = Color.blue;

            foreach(Vector3 vert in vertices) {
                Gizmos.DrawSphere(vert, 0.2f);
            }

            Gizmos.color = Color.green;
            for(int i = 1; i < points.Count; i++) {
                Gizmos.DrawLine(points[i-1], points[i]);
            }
        }

    }

}
