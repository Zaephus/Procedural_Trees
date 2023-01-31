using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseTree : MonoBehaviour {

    [SerializeField]
    private bool drawGizmos;
    [SerializeField]
    private float gizmoSize;
    [SerializeField]
    private bool flatShaded;

    [SerializeField, Range(3, 36)]
    private int vertexResolution;

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
    private List<int> triangles = new List<int>();

    public List<TrunkSegment> segments = new List<TrunkSegment>();

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
    private struct BranchData {

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

        points = SetTrunkPoints();
        //vertices = SetTrunkVertices();

        float length = (scale + scaleVariance) * (baseLength + baseLengthVariance);
        float baseRadius = length  * ratio * (baseScale + baseScaleVariance);

        for(int i = 0; i < points.Count; i++) {
            Vector3 rot = Vector3.zero;
            if(i > 0) {
                rot = new Vector3(Mathf.Cos(baseCurveVariance/baseCurveResolution), 0, Mathf.Sin(baseCurveVariance/baseCurveResolution));
            }
            
            segments.Add(new TrunkSegment(points[i], rot, baseRadius, vertexResolution));
            vertices.AddRange(segments[i].vertices);
        }
        triangles = SetTrunkTriangles();

        if(flatShaded) {
            vertices = SetFlatShadedNormals();
        }

        SetMesh();

    }

    private List<Vector3> SetTrunkPoints() {

        List<Vector3> pointSet = new List<Vector3>();

        float length = (scale + scaleVariance) * (baseLength + baseLengthVariance);
        float baseRadius = length  * ratio * (baseScale + baseScaleVariance);

        pointSet.Add(transform.position);
        for(int i = 1; i < baseCurveResolution; i++) {
            int inverseIndex = (int)baseCurveResolution-i;
            pointSet.Add(transform.position + new Vector3(Random.Range(-baseRadius, baseRadius) / inverseIndex, 
                                                        i * (baseLength/baseCurveResolution),
                                                        Random.Range(-baseRadius, baseRadius) / inverseIndex));
        }

        return pointSet;

    }

    private List<Vector3> SetTrunkVertices() {

        List<Vector3> vertexSet = new List<Vector3>();
        
        float length = (scale + scaleVariance) * (baseLength + baseLengthVariance);
        float baseRadius = length  * ratio * (baseScale + baseScaleVariance);

        for(int i = 0; i < points.Count; i++) {
            for(int j = 0; j < vertexResolution; j++) {
                float currentAngle = ((2*Mathf.PI)/vertexResolution)*j + (Mathf.Deg2Rad * (baseCurveVariance/baseCurveResolution));
                Vector3 rot = new Vector3(Mathf.Cos(currentAngle), 0, Mathf.Sin(currentAngle));
                vertexSet.Add(points[i] + baseRadius * rot);
            }

        }

        return vertexSet;

    }

    private List<int> SetTrunkTriangles() {

        List<int> triangleSet = new List<int>();

        for(int i = 0; i < vertices.Count - vertexResolution; i++) {
            triangleSet.Add(i + vertexResolution - 1);
            triangleSet.Add(i + vertexResolution);
            triangleSet.Add(i + vertexResolution - vertexResolution);

            triangleSet.Add(i + vertexResolution);
            triangleSet.Add(i + vertexResolution - vertexResolution + 1);
            triangleSet.Add(i + vertexResolution - vertexResolution);
        }

        return triangleSet;

    }

    private List<Vector3> SetFlatShadedNormals() {

        List<Vector3> vertexSet = new List<Vector3>();

        for (int i = 0; i < triangles.Count; i++) {
            vertexSet.Add(vertices[triangles[i]]);
            triangles[i] = i;
        }

        return vertexSet;

    }

    private void SetMesh() {

        meshFilter.sharedMesh = new Mesh();
        meshFilter.GetComponent<MeshRenderer>().sharedMaterial = treeMaterial;
        
        meshFilter.sharedMesh.Clear();
        meshFilter.sharedMesh.vertices = vertices.ToArray();
        meshFilter.sharedMesh.triangles = triangles.ToArray();
        meshFilter.sharedMesh.RecalculateNormals();
        meshFilter.sharedMesh.RecalculateBounds();

    }

    private void OnDrawGizmos() {
        
        if(drawGizmos) {
            Gizmos.color = Color.blue;

            foreach(Vector3 vert in vertices) {
                Gizmos.DrawSphere(vert, gizmoSize);
            }

            Gizmos.color = Color.green;
            for(int i = 1; i < points.Count; i++) {
                Gizmos.DrawLine(points[i-1], points[i]);
            }
        }

    }

}
