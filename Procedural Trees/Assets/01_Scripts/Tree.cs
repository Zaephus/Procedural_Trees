using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tree : MonoBehaviour {

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
    private int baseSegmentSplits;
    [SerializeField]
    private float baseSplitAngle;
    [SerializeField]
    private float baseSplitAngleVariance;
    [SerializeField, Range(2, 16)]
    private int baseCurveResolution;
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
        public int curveResolution;
        public float curve;
        public float curveBack;
        public float curveVariation;

    }
    
    public void Generate() {

        SetTrunkSegments();        

        triangles = SetTrunkTriangles();

        if(flatShaded) {
            vertices = SetFlatShadedNormals();
        }

        SetMesh();

    }

    private void SetTrunkSegments() {

        segments.Clear();
        vertices.Clear();

        float length = (scale - scaleVariance) * (baseLength - baseLengthVariance);
        float baseRadius = length * ratio * (baseScale - baseScaleVariance);
        float segmentLength = length/baseCurveResolution;
        
        Vector3 midPoint = transform.position;

        for(int i = 0; i < baseCurveResolution; i++) {
            Vector3 rot = Vector3.zero;
            if(i > 0) {
                rot = segments[i-1].rotation;
                rot += new Vector3(
                    Mathf.Deg2Rad * (baseCurve/baseCurveResolution),
                    Mathf.Deg2Rad * Random.Range(-baseCurveVariance/baseCurveResolution, baseCurveVariance/baseCurveResolution),
                    0
                );

                Vector3 PQ = segments[i-1].midPoint - segments[i-1].vertices[0];
                Vector3 PR = segments[i-1].midPoint - segments[i-1].vertices[1];
                Vector3 normal = -Vector3.Cross(PQ, PR);
                normal.Normalize();
                
                midPoint = segments[i-1].midPoint + normal * (segmentLength);

            }

            float z = (i+1) * length/baseCurveResolution;
            float rad = CalculateTaper(z/length);
            
            segments.Add(new TrunkSegment(midPoint, rot, rad, vertexResolution));
            vertices.AddRange(segments[i].vertices);

        }

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

    private float CalculateTaper(float _z) {
        
        float length = (scale - scaleVariance) * (baseLength - baseLengthVariance);
        float baseRadius = length * ratio * (baseScale - baseScaleVariance);

        float radius = baseRadius;

        float unitTaper;

        if(baseTaper >= 0 && baseTaper < 1) {
            unitTaper = baseTaper;
        }
        else if(baseTaper >= 1 && baseTaper < 2) {
            unitTaper = 2 - baseTaper;
        }
        else {
            unitTaper = 0;
        }

        float taper = baseRadius * (1 - unitTaper * _z);

        float zTwo;
        float zThree;
        
        float depth;

        if(baseTaper >= 0 && baseTaper < 1) {
            radius = taper;
        }
        else if(baseTaper >= 1 && baseTaper <= 3) {
            zTwo = (1 - _z) * length;

            if(baseTaper < 2 || zTwo < taper) {
                depth = 1;
            }
            else {
                depth = baseTaper - 2;
            }

            if(baseTaper < 2) {
                zThree = zTwo;
            }
            else {
                zThree = Mathf.Abs(zTwo - (2 * taper * ((int)(zTwo / (2 * taper)))) + 0.5f);
            }

            if(baseTaper < 2 && zThree >= taper) {
                radius = taper;
            }
            else {
                radius = (1 - depth) * taper + depth * Mathf.Sqrt((taper * taper) - Mathf.Pow((zThree - taper), 2));
            }

        }

        return radius;

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

        }

    }

}