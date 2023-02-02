using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trunk {

    private Vector3 startPoint;

    private int radialResolution;
    private int segmentResolution;

    #region Parameters

    private float scale;
    private float scaleVariance;
    private float zScale;
    private float zScaleVariance;
    private float ratio;

    private TrunkData data;

    #endregion

    private float length;
    private float segmentLength;
    private float baseRadius;

    private List<VertexSegment> vertexSegments = new List<VertexSegment>();

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    public Trunk(Vector3 _startPoint, int _radRes, int _segRes, float _scale, float _scaleV, float _zScale, float _zScaleV, float _ratio, TrunkData _data) {
        
        startPoint = _startPoint;

        radialResolution = _radRes;
        segmentResolution = _segRes;

        scale = _scale;
        scaleVariance = _scaleV;
        zScale = _zScale;
        zScaleVariance = _zScaleV;
        ratio = _ratio;

        data = _data;

        length = (scale - scaleVariance) * (data.length - data.lengthVariance);
        segmentLength = length/data.curveResolution;
        baseRadius = length * ratio * (data.scale - data.scaleVariance);

    }

    public Mesh CreateTrunkMesh() {

        vertexSegments = SetSegments();
        vertices = SetVertices();
        triangles = TreeMeshBuilder.SetTriangles(vertices.Count, radialResolution);

        return TreeMeshBuilder.CreateMesh(vertices, triangles);

    }

    private List<VertexSegment> SetSegments() {

        List<VertexSegment> vertexSegmentSet = new List<VertexSegment>();

        Vector3 midPoint = startPoint;
        Vector3 rotation = Vector3.zero;

        for(int i = 0; i < data.curveResolution; i++) {
            if(i > 0) {

                rotation = vertexSegmentSet[i-1].rotation;
                if(data.curveBack == 0) {
                    rotation += new Vector3(
                        Mathf.Deg2Rad * (data.curve/data.curveResolution),
                        Mathf.Deg2Rad * Random.Range(-data.curveVariance/data.curveResolution, data.curveVariance/data.curveResolution),
                        0
                    );
                }
                else {
                    if(i < data.curveResolution/2) {
                        rotation += new Vector3(
                            Mathf.Deg2Rad * (data.curve/(data.curveResolution/2)),
                            Mathf.Deg2Rad * Random.Range(-data.curveVariance/data.curveResolution, data.curveVariance/data.curveResolution),
                            0
                        );
                    }
                    else {
                        rotation += new Vector3(
                            Mathf.Deg2Rad * (data.curveBack/(data.curveResolution/2)),
                            Mathf.Deg2Rad * Random.Range(-data.curveVariance/data.curveResolution, data.curveVariance/data.curveResolution),
                            0
                        );
                    }
                }

                Vector3 PQ = vertexSegmentSet[i-1].midPoint - vertexSegmentSet[i-1].vertices[0];
                Vector3 PR = vertexSegmentSet[i-1].midPoint - vertexSegmentSet[i-1].vertices[1];
                Vector3 normal = -Vector3.Cross(PQ, PR);
                normal.Normalize();

                midPoint = vertexSegmentSet[i-1].midPoint + normal * segmentLength;

            }

            float height = (i+1) * length / data.curveResolution;
            float radius = TreeMeshBuilder.CalculateTaper(height/length, data.taper, length, baseRadius);

            vertexSegmentSet.Add(new VertexSegment(midPoint, rotation, radius, radialResolution));

        }

        return vertexSegmentSet;

    }

    private List<Vector3> SetVertices() {

        List<Vector3> vertexSet = new List<Vector3>();

        foreach(VertexSegment vs in vertexSegments) {
            vertexSet.AddRange(vs.vertices);
        }

        return vertexSet;

    }

}