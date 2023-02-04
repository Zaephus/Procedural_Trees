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
    [SerializeField]
    private TreeShape shape;
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
    private int levels;
    [SerializeField]
    private float ratio;
    [SerializeField]
    private float ratioPower;
    #endregion

    #region Trunk and Branch Parameters
    [Header("Trunk and Branch Parameters")]
    [SerializeField]
    private TrunkData trunkData;
    [SerializeField]
    private BranchData[] branchDatas;
    #endregion
    
    [SerializeField]
    private List<Mesh> meshes = new List<Mesh>();
    
    public void Generate() {

        meshes.Clear();

        Trunk trunk = new Trunk(Vector3.zero, Vector3.zero, startHeight, radialVertexResolution, segmentVertexResolution, scale, scaleVariance, zScale, zScaleVariance, levels, 0, ratio, trunkData);
        meshes.AddRange(trunk.CreateTrunkMesh());

        if(flatShaded) {
            for(int i = 0; i < meshes.Count; i++) {
                meshes[i] = TreeMeshBuilder.SetFlatShadedNormals(meshes[i]);
            }
        }

        CombineInstance[] combines = new CombineInstance[meshes.Count];

        for(int i = 0; i < meshes.Count; i++) {
            combines[i].mesh = meshes[i];
            combines[i].transform = Matrix4x4.identity;
        }
        meshFilter.sharedMesh = new Mesh();
        meshFilter.sharedMesh.CombineMeshes(combines);

    }

}