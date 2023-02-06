using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaf {

    private Vector3 startPoint;
    private Vector3 startRotation;

    #region Parameters

    private Tree tree;

    private float leafScale;
    private float leafScaleX;

    private BranchData data;

    #endregion

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    public Leaf(Tree _tree, Vector3 _startPoint, Vector3 _startRotation) {

        tree = _tree;

        startPoint = _startPoint;
        startRotation = _startRotation;

        leafScale = tree.leafScale;
        leafScaleX = tree.leafScaleX;

        data = tree.thirdBranchData;

    }

    public Mesh CreateLeafMesh() {

        vertices = SetVertices();
        triangles = SetTriangles();

        return TreeMeshBuilder.CreateMesh(vertices, triangles);

    }

    private List<Vector3> SetVertices() {

        float scaleX = leafScale * leafScaleX;
        float scaleY = leafScale;

        List<Vector3> vertexSet = new List<Vector3> {
            new Vector3(0             , 0            , 0),
            new Vector3(0.3f * scaleX , 0.3f * scaleY, 0),
            new Vector3(0             , 0.3f * scaleY, 0),
            new Vector3(-0.3f * scaleX, 0.3f * scaleY, 0),
            new Vector3(0.3f * scaleX , 0.7f * scaleY, 0),
            new Vector3(0             , 0.7f * scaleY, 0),
            new Vector3(-0.3f * scaleX, 0.7f * scaleY, 0),
            new Vector3(0             , 1.0f * scaleY, 0)
        };

        for(int i = 0; i < vertexSet.Count; i++) {
            vertexSet[i] = new Vector3(
                vertexSet[i].x,
                vertexSet[i].y * Mathf.Cos(startRotation.x) - vertexSet[i].z * Mathf.Sin(startRotation.x),
                vertexSet[i].y * Mathf.Sin(startRotation.x) + vertexSet[i].z * Mathf.Cos(startRotation.x)
            );

            vertexSet[i] = new Vector3(
                vertexSet[i].x * Mathf.Cos(startRotation.y) + vertexSet[i].z * Mathf.Sin(startRotation.y),
                vertexSet[i].y,
                -vertexSet[i].x * Mathf.Sin(startRotation.y) + vertexSet[i].z * Mathf.Cos(startRotation.y)
            );

            vertexSet[i] += startPoint;
        }

        return vertexSet;

    }

    private List<int> SetTriangles() {

        List<int> triangleSet = new List<int> {
            0,2,1,
            0,3,2,
            1,2,4,
            2,5,4,
            2,3,5,
            3,6,5,
            4,5,7,
            5,6,7
        };

        return triangleSet;

    }

}