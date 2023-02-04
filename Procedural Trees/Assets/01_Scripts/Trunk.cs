using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trunk {

    private Vector3 startPoint;
    private Vector3 startRotation;

    private float currentHeight;

    private int currentLevel;

    private int radialResolution;
    private int segmentResolution;
    private int vertexSegmentAmount;

    #region Parameters

    private float scale;
    private float scaleVariance;
    private float zScale;
    private float zScaleVariance;
    private int levels;
    private float ratio;

    private TrunkData data;

    #endregion

    private float length;
    private float lengthLeft;
    private float segmentLength;
    private float baseRadius;

    private int startIndex;

    private List<VertexSegment> vertexSegments = new List<VertexSegment>();

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    private List<Mesh> meshes = new List<Mesh>();

    public Trunk(Vector3 _startPoint, Vector3 _startRotation, float _currentHeight, int _radRes, int _segRes, float _scale, float _scaleV, float _zScale, float _zScaleV, int _levels, int _currentLevel, float _ratio, TrunkData _data) {
        
        startPoint = _startPoint;
        startRotation = _startRotation;

        currentHeight = _currentHeight;

        radialResolution = _radRes;
        segmentResolution = _segRes;

        scale = _scale;
        scaleVariance = _scaleV;
        zScale = _zScale;
        zScaleVariance = _zScaleV;

        levels = _levels;
        currentLevel = _currentLevel;

        ratio = _ratio;

        data = _data;

        length = (scale + scaleVariance) * (data.length + data.lengthVariance);
        lengthLeft = (1 - currentHeight) * length;

        vertexSegmentAmount = data.curveResolution * segmentResolution + 1;

        segmentLength = length/(vertexSegmentAmount - 1);
        baseRadius = length * ratio * (data.scale + data.scaleVariance);

        startIndex = Mathf.RoundToInt((length - lengthLeft) / segmentLength);

    }

    public List<Mesh> CreateTrunkMesh() {

        meshes.Clear();

        vertexSegments = SetSegments();
        vertices = SetVertices();
        triangles = TreeMeshBuilder.SetTriangles(vertices.Count, radialResolution);

        meshes.Add(TreeMeshBuilder.CreateMesh(vertices, triangles));

        return meshes;

    }

    private List<VertexSegment> SetSegments() {

        List<VertexSegment> vertexSegmentSet = new List<VertexSegment>();

        Vector3 midPoint = startPoint;
        Vector3 rotation = startRotation;

        int vertexSegmentIndex = 0;

        float error = 0;
        float segmentSplitsEffective = 0;

        float angleSplit = 0;

        for(int i = 0; i < vertexSegmentAmount - startIndex; i++) {

            float height = i * segmentLength + (length - lengthLeft);

            float radius = TreeMeshBuilder.CalculateTaper(height/length, data.taper, length, baseRadius);
            float flare = CalculateFlare(height/length);

            if(i > 0) {

                Vector3 PQ = vertexSegmentSet[i-1].midPoint - vertexSegmentSet[i-1].vertices[0];
                Vector3 PR = vertexSegmentSet[i-1].midPoint - vertexSegmentSet[i-1].vertices[1];
                Vector3 normal = -Vector3.Cross(PQ, PR);
                normal.Normalize();

                Debug.Log(i + " - Normal.y: " + normal.y);
                Debug.Log(i + " - Declination: " + Mathf.Acos(normal.y));

                midPoint = vertexSegmentSet[i-1].midPoint + normal * segmentLength;

                rotation = vertexSegmentSet[i-1].rotation;

                if(vertexSegmentIndex == 0) {

                    #region Rotation
                    if(angleSplit != 0) {
                        rotation += new Vector3(
                            Mathf.Deg2Rad * -angleSplit,
                            0,
                            0
                        );
                        angleSplit = 0;
                    }

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
                    #endregion

                    #region Splits
                    if(currentLevel <= levels) {
                        segmentSplitsEffective = Mathf.Round(data.segmentSplits + error);
                        //split in the amount of segmentSplitsEffective
                        //Debug.Log(segmentSplitsEffective);

                        if(segmentSplitsEffective > 0) {

                            float declination = Mathf.Rad2Deg * Mathf.Acos(normal.y);
                            angleSplit = (data.splitAngle + data.splitAngleVariance) - declination;
                                
                            float divergence = 20 + 0.75f * (30 + Mathf.Abs(declination - 90)) * (Random.Range(0f, 1f) * Random.Range(0f, 1f));

                            rotation += new Vector3(
                                Mathf.Deg2Rad * RandomSign() * angleSplit,
                                0,
                                0
                            );

                            for(int j = 0; j < segmentSplitsEffective; j++) {

                                Vector3 splitRotation = new Vector3(
                                    Mathf.Deg2Rad * RandomSign() * angleSplit,
                                    Mathf.Deg2Rad * RandomSign() * divergence,
                                    0
                                );

                                Trunk trunk = new Trunk(midPoint, splitRotation, height/length, radialResolution, segmentResolution, scale, scaleVariance, zScale, zScaleVariance, levels, currentLevel+1, ratio, data);
                                meshes.AddRange(trunk.CreateTrunkMesh());

                            }

                        }

                        error -= segmentSplitsEffective - data.segmentSplits;
                    }
                    #endregion

                }

            }

            vertexSegmentSet.Add(new VertexSegment(midPoint, rotation, radius * flare, radialResolution));

            if(vertexSegmentIndex == segmentResolution - 1) {
                vertexSegmentIndex = 0;
            }
            else {
                vertexSegmentIndex++;
            }

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

    private float CalculateFlare(float _height) {

        float y = 1 - 8 * _height;

        float flareZ = data.flare * (Mathf.Pow(100, y) - 1) / 100 + 1;

        return flareZ;

    }

    private int RandomSign() {
        return Random.Range(0, 2) * 2 - 1;
    }

}