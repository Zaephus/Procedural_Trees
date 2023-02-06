using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TreeShape {
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
public class Tree : MonoBehaviour {

    public bool liveUpdate;
    [SerializeField]
    private bool flatShaded;

    [SerializeField, Range(3, 36)]
    private int radialVertexResolution;
    [SerializeField, Range(1, 16)]
    private int segmentVertexResolution;

    [SerializeField, Range(0, 0.999f)]
    private float startHeight;

    [SerializeField]
    private MeshFilter meshFilter;

    #region Main Parameters
    [Header("Main Parameters")]
    public TreeShape shape;
    public float baseSize;
    public float scale;
    public float scaleVariance;
    public float zScale;
    public float zScaleVariance;
    public int levels;
    public float ratio;
    public float ratioPower;
    #endregion

    #region Trunk and Branch Parameters
    [Header("Trunk and Branch Parameters")]
    public TrunkData trunkData;
    public BranchData firstBranchData;
    public BranchData secondBrachData;
    public BranchData thirdBranchData;
    #endregion

    #region Leaf Parameters
    [Header("Leaf Parameters")]
    public float leaves;
    public float leafScale;
    public float leafScaleX;
    #endregion
    
    public void Generate() {

        Trunk trunk = new Trunk(this, Vector3.zero, Vector3.zero, startHeight, radialVertexResolution, segmentVertexResolution, 0);
        Mesh[] stemMeshes = trunk.CreateTrunkMesh().ToArray();
        Mesh[] leafMeshes = trunk.GetLeafMeshes().ToArray();

        if(flatShaded) {
            for(int i = 0; i < stemMeshes.Length; i++) {
                stemMeshes[i] = TreeMeshBuilder.SetFlatShadedNormals(stemMeshes[i]);
                leafMeshes[i] = TreeMeshBuilder.SetFlatShadedNormals(leafMeshes[i]);
            }
        }

        CombineInstance[] stemCombines = new CombineInstance[stemMeshes.Length];

        for(int i = 0; i < stemMeshes.Length; i++) {
            stemCombines[i].mesh = stemMeshes[i];
            stemCombines[i].transform = Matrix4x4.identity;
        }

        Mesh stemMesh = new Mesh();
        stemMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        stemMesh.CombineMeshes(stemCombines);

        CombineInstance[] leafCombines = new CombineInstance[leafMeshes.Length];

        for(int i = 0; i < leafMeshes.Length; i++) {
            leafCombines[i].mesh = leafMeshes[i];
            leafCombines[i].transform = Matrix4x4.identity;
        }

        Mesh leafMesh = new Mesh();
        leafMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        leafMesh.CombineMeshes(leafCombines);

        CombineInstance[] combines = new CombineInstance[2];

        combines[0].mesh = stemMesh;
        combines[0].transform = Matrix4x4.identity;
        //combines[0].subMeshIndex = 0;

        combines[1].mesh = leafMesh;
        combines[1].transform = Matrix4x4.identity;
        //combines[1].subMeshIndex = 1;

        meshFilter.sharedMesh = new Mesh();
        meshFilter.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        meshFilter.sharedMesh.CombineMeshes(combines, false, true);

    }

}