using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Branch {

    private Vector3 startPoint;
    private Vector3 startRotation;

    private int currentLevel;

    private int radialResolution;
    private int segmentResolution;
    private int vertexSegmentAmount;

    #region Parameters;

    private Tree tree;

    private TreeShape shape;
    private float baseSize;
    private float scale;
    private float scaleVariance;
    private float zScale;
    private float zScaleVariance;
    private int levels;
    private float ratio;

    private BranchData data;
    private BranchData nextData;

    #endregion

    private float offset;

    private float parentLength;
    private float length;
    private float segmentLength;
    private float baseRadius;

    private int branchAmount;
    private int leafAmount;

    private List<VertexSegment> vertexSegments = new List<VertexSegment>();

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    private List<Mesh> stemMeshes = new List<Mesh>();
    private List<Mesh> leafMeshes = new List<Mesh>();

    public Branch(Tree _tree, float _parentLength, float _parentRadius, Vector3 _startPoint, Vector3 _startRotation, float _offset, int _radRes, int _segRes, int _currentLevel) {

        tree = _tree;

        parentLength = _parentLength;

        startPoint = _startPoint;
        startRotation = _startRotation;

        radialResolution = (int)Mathf.Clamp(_radRes/1.5f, 3, 32);
        segmentResolution = (int)Mathf.Clamp(_segRes/1.5f, 1, 16);

        shape = tree.shape;
        baseSize = tree.baseSize;
        scale = tree.scale;
        scaleVariance = tree.scaleVariance;
        zScale = tree.zScale;

        levels = tree.levels;
        currentLevel = _currentLevel;

        ratio = tree.ratio;

        offset = _offset;

        switch(currentLevel) {
            case 1:
                data = tree.firstBranchData;
                nextData = tree.secondBrachData;
                break;
            case 2:
                data = tree.secondBrachData;
                nextData = tree.thirdBranchData;
                break;
            case 3:
                data = tree.thirdBranchData;
                nextData = tree.thirdBranchData;
                break;
        }

        float lengthBase = baseSize * (scale + scaleVariance);

        if(currentLevel == 1) {
            length = parentLength * (data.length + data.lengthVariance) * ShapeRatio(shape, (parentLength - offset) / (parentLength - lengthBase));
            branchAmount = Mathf.RoundToInt(nextData.branches * (0.2f + 0.8f * (length / parentLength) / (data.length + data.length)));
        }
        else {
            length = (data.length + data.lengthVariance) * (parentLength - 0.6f * offset);
            branchAmount = Mathf.RoundToInt(nextData.branches * (1.0f -0.5f * offset / parentLength));
        }

        leafAmount = Mathf.RoundToInt(tree.leaves * ShapeRatio(TreeShape.TaperedCylindrical, offset/parentLength));

        vertexSegmentAmount = data.curveResolution * segmentResolution + 1;

        segmentLength = length/(vertexSegmentAmount - 1);

        baseRadius = _parentRadius * Mathf.Pow(length / parentLength, tree.ratioPower);

    }

    public List<Mesh> CreateBranchMesh() {

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
        int leavesLeft = leafAmount;

        for(int i = 0; i < vertexSegmentAmount; i++) {

            float height = i * segmentLength;

            float radius = TreeMeshBuilder.CalculateTaper(height/length, data.taper, length, baseRadius);
            
            if( i > 0) {

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

                                Branch branch = new Branch(tree,length, baseRadius, midPoint, splitRotation, height/length, radialResolution, segmentResolution, currentLevel + 1);
                                stemMeshes.AddRange(branch.CreateBranchMesh());
                                leafMeshes.AddRange(branch.GetLeafMeshes());

                            }

                        }

                        error -= segmentSplitsEffective - data.segmentSplits;
                    }
                    #endregion

                }

            }

            vertexSegmentSet.Add(new VertexSegment(midPoint, rotation, radius, radialResolution));

            if(currentLevel <= levels) {

                float lengthBase = tree.baseSize * (scale + scaleVariance);

                if(currentLevel == levels-1) {

                    int vertexSegmentLeaves = leavesLeft / (vertexSegmentAmount - i);
                    float heightBetweenLeaves = segmentLength / vertexSegmentLeaves;

                    Vector3 leafRotation = Vector3.zero;
                    Vector3 lastRotation = Vector3.zero;

                    for(int j = 0; j < vertexSegmentLeaves; j++) {

                        float leafHeight = height - heightBetweenLeaves * j;

                        if(nextData.downAngleVariance >= 0) {
                            leafRotation = new Vector3(
                                Mathf.Deg2Rad * (nextData.downAngle + nextData.downAngleVariance),
                                0,
                                0
                            );
                        }
                        else {
                            leafRotation = new Vector3(
                                Mathf.Deg2Rad * (nextData.downAngle + (nextData.downAngleVariance * (1 - 2 * Branch.ShapeRatio(0, (length - leafHeight) / (length - lengthBase))))),
                                0,
                                0
                            );
                        }

                        if(nextData.rotate >= 0) {
                            leafRotation += new Vector3(
                                0,
                                lastRotation.y + Mathf.Deg2Rad * (nextData.rotate + nextData.rotateVariance),
                                0
                            );
                        }
                        else {
                            leafRotation += new Vector3(
                                0,
                                lastRotation.y + Mathf.Deg2Rad * (180 + nextData.rotate + nextData.rotateVariance),
                                0
                            );
                        }

                        lastRotation = leafRotation;

                        Leaf leaf = new Leaf(tree, vertexSegmentSet[i].midPoint - heightBetweenLeaves * j * normal, leafRotation);
                        leafMeshes.Add(leaf.CreateLeafMesh());

                    }

                    leavesLeft -= vertexSegmentLeaves;
                    
                }
                else {
                    if(height / length >= tree.baseSize) {

                        BranchData nextData = tree.firstBranchData;

                        Vector3 branchRotation = Vector3.zero;
                        Vector3 lastRotation = Vector3.zero;

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
                                    lastRotation.y + Mathf.Deg2Rad * (nextData.rotate + nextData.rotateVariance),
                                    0
                                );
                            }
                            else {
                                branchRotation += new Vector3(
                                    0,
                                    lastRotation.y + Mathf.Deg2Rad * (180 + nextData.rotate + nextData.rotateVariance),
                                    0
                                );
                            }

                            lastRotation = branchRotation;

                            Branch branch = new Branch(tree, length, baseRadius, vertexSegmentSet[i].midPoint - heightBetweenBranches * j * normal, branchRotation, branchHeight, radialResolution, segmentResolution, currentLevel + 1);
                            stemMeshes.AddRange(branch.CreateBranchMesh());
                            leafMeshes.AddRange(branch.GetLeafMeshes());

                        }

                        branchesLeft -= vertexSegmentBranches;

                    }
                }
            }   

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

    public static float ShapeRatio(TreeShape _shape, float _ratio) {

        float branchRatio = Mathf.Clamp(_ratio, 0.01f, 0.99f);

        switch(_shape) {

            case TreeShape.Conical:
                return 0.2f + 0.8f * branchRatio;
            
            case TreeShape.Spherical:
                return 0.2f + 0.8f * Mathf.Sin(Mathf.PI * branchRatio);

            case TreeShape.HemiSpherical:
                return 0.2f + 0.8f * Mathf.Sin(0.5f * Mathf.PI * branchRatio);

            case TreeShape.Cylindrical:
                return 1.0f;

            case TreeShape.TaperedCylindrical:
                return 0.5f + 0.5f + branchRatio;

            case TreeShape.Flame:
                if(branchRatio <= 0.7f) {
                    return branchRatio / 0.7f;
                }
                else {
                    return (1.0f - branchRatio) / 0.3f;
                }

            case TreeShape.InverseConical:
                return 1.0f - 0.8f * branchRatio;

            case TreeShape.TendFlame:
                if(branchRatio <= 0.7f) {
                    return 0.5f + 0.5f * (branchRatio / 0.7f);
                }
                else {
                    return 0.5f + 0.5f * ((1.0f - branchRatio) / 0.3f);
                }

            default:
                return 1.0f;

        }

    }

}