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
    
    public void Generate() {

        Trunk trunk = new Trunk(this, Vector3.zero, Vector3.zero, startHeight, radialVertexResolution, segmentVertexResolution, 0);
        Mesh[] meshes = trunk.CreateTrunkMesh().ToArray();

        if(flatShaded) {
            for(int i = 0; i < meshes.Length; i++) {
                meshes[i] = TreeMeshBuilder.SetFlatShadedNormals(meshes[i]);
            }
        }

        CombineInstance[] combines = new CombineInstance[meshes.Length];

        for(int i = 0; i < meshes.Length; i++) {
            combines[i].mesh = meshes[i];
            combines[i].transform = Matrix4x4.identity;
        }
        meshFilter.sharedMesh = new Mesh();
        meshFilter.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        meshFilter.sharedMesh.CombineMeshes(combines);

    }

}