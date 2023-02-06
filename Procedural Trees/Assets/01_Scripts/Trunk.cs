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

    private Tree tree;

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

    private int branchAmount;

    private int startIndex;

    private List<VertexSegment> vertexSegments = new List<VertexSegment>();

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    private List<Mesh> stemMeshes = new List<Mesh>();
    private List<Mesh> leafMeshes = new List<Mesh>();

    public Trunk(Tree _tree, Vector3 _startPoint, Vector3 _startRotation, float _currentHeight, int _radRes, int _segRes, int _currentLevel) {
        
        tree = _tree;

        startPoint = _startPoint;
        startRotation = _startRotation;

        currentHeight = _currentHeight;

        radialResolution = _radRes;
        segmentResolution = _segRes;

        scale = tree.scale;
        scaleVariance = tree.scaleVariance;
        zScale = tree.zScale;
        zScaleVariance = tree.zScaleVariance;

        levels = tree.levels;
        currentLevel = _currentLevel;

        ratio = tree.ratio;

        data = tree.trunkData;

        length = (scale + scaleVariance) * (data.length + data.lengthVariance);
        lengthLeft = (1 - currentHeight) * length;

        vertexSegmentAmount = data.curveResolution * segmentResolution + 1;

        segmentLength = length/(vertexSegmentAmount - 1);
        baseRadius = length * ratio * (data.scale + data.scaleVariance);

        branchAmount = tree.firstBranchData.branches;

        startIndex = Mathf.RoundToInt((length - lengthLeft) / segmentLength);

    }

    public List<Mesh> CreateTrunkMesh() {

        stemMeshes.Clear();

        vertexSegments = SetSegments();
        vertices = SetVertices();
        triangles = TreeMeshBuilder.SetTriangles(vertices.Count, radialResolution);

        stemMeshes.Add(TreeMeshBuilder.CreateMesh(vertices, triangles));

        return stemMeshes;

    }

    public List<Mesh> GetLeafMeshes() {
        return leafMeshes;
    }

    private List<VertexSegment> SetSegments() {

        List<VertexSegment> vertexSegmentSet = new List<VertexSegment>();

        Vector3 midPoint = startPoint;
        Vector3 rotation = startRotation;
        Vector3 normal = Vector3.zero;

        int vertexSegmentIndex = 0;

        float error = 0;
        float segmentSplitsEffective = 0;

        float angleSplit = 0;

        int branchesLeft = branchAmount;

        for(int i = 0; i < vertexSegmentAmount - startIndex; i++) {

            float height = i * segmentLength + (length - lengthLeft);

            float radius = TreeMeshBuilder.CalculateTaper(height/length, data.taper, length, baseRadius);
            float flare = CalculateFlare(height/length);

            if(i > 0) {

                Vector3 PQ = vertexSegmentSet[i-1].midPoint - vertexSegmentSet[i-1].vertices[0];
                Vector3 PR = vertexSegmentSet[i-1].midPoint - vertexSegmentSet[i-1].vertices[1];
                normal = -Vector3.Cross(PQ, PR);
                normal.Normalize();

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
                                Mathf.Deg2Rad * TreeMeshBuilder.RandomSign() * angleSplit,
                                0,
                                0
                            );

                            for(int j = 0; j < segmentSplitsEffective; j++) {

                                Vector3 splitRotation = new Vector3(
                                    Mathf.Deg2Rad * TreeMeshBuilder.RandomSign() * angleSplit,
                                    Mathf.Deg2Rad * TreeMeshBuilder.RandomSign() * divergence,
                                    0
                                );

                                Trunk trunk = new Trunk(tree, midPoint, splitRotation, height/length, radialResolution, segmentResolution, currentLevel);
                                stemMeshes.AddRange(trunk.CreateTrunkMesh());
                                leafMeshes.AddRange(trunk.GetLeafMeshes());

                            }

                        }

                        error -= segmentSplitsEffective - data.segmentSplits;
                    }
                    #endregion

                }

            }

            vertexSegmentSet.Add(new VertexSegment(midPoint, rotation, radius * flare, radialResolution));

            #region Branches
            if(height / length >= tree.baseSize) {

                float lengthBase = tree.baseSize * (scale + scaleVariance);

                Vector3 lastRot = Vector3.zero;
                BranchData nextData = tree.firstBranchData;

                Vector3 branchRotation = Vector3.zero;

                int vertexSegmentBranches = branchesLeft / (vertexSegmentAmount - i);
                float heightBetweenBranches = segmentLength / vertexSegmentBranches;
                
                for(int j = 0; j < vertexSegmentBranches; j++) {

                    float branchHeight = height - heightBetweenBranches * j;

                    if(nextData.downAngleVariance >= 0) {
                        branchRotation = new Vector3(
                            Mathf.Deg2Rad * (nextData.downAngle + nextData.downAngleVariance),
                            0,
                            0
                        );
                    }
                    else {
                        branchRotation = new Vector3(
                            Mathf.Deg2Rad * (nextData.downAngle + (nextData.downAngleVariance * (1 - 2 * Branch.ShapeRatio(0, (length - branchHeight) / (length - lengthBase))))),
                            0,
                            0
                        );
                    }

                    if(nextData.rotate >= 0) {
                        branchRotation += new Vector3(
                            0,
                            lastRot.y + Mathf.Deg2Rad * (nextData.rotate + nextData.rotateVariance),
                            0
                        );
                    }
                    else {
                        branchRotation += new Vector3(
                            0,
                            lastRot.y + Mathf.Deg2Rad * (180 + nextData.rotate + nextData.rotateVariance),
                            0
                        );
                    }

                    lastRot = branchRotation;

                    Branch branch = new Branch(tree, length, baseRadius, vertexSegmentSet[i].midPoint - heightBetweenBranches * j * normal, branchRotation, branchHeight, radialResolution, segmentResolution, currentLevel + 1);
                    stemMeshes.AddRange(branch.CreateBranchMesh());
                    leafMeshes.AddRange(branch.GetLeafMeshes());

                }

                branchesLeft -= vertexSegmentBranches;

            }
            #endregion

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

}