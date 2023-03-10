using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VertexSegment {

    public Vector3 midPoint;
    public List<Vector3> vertices = new List<Vector3>();

    public Vector3 rotation;

    public float radius;

    public VertexSegment(Vector3 _pos, Vector3 _rot, float _rad, int _res) {

        midPoint = _pos;
        rotation = _rot;

        radius = _rad;

        vertices = SetVertices(_res);

    }

    private List<Vector3> SetVertices(int _res) {

        List<Vector3> vertexSet = new List<Vector3>();

        float alpha = 0;
        float beta = rotation.x;
        float gamma = rotation.y;

        for(int i = 0; i < _res; i++) {

            alpha = -((2*Mathf.PI)/_res)*i;

            Vector3 vertex = new Vector3(
                Mathf.Cos(alpha),
                Mathf.Sin(alpha) * Mathf.Sin(beta),
                -Mathf.Sin(alpha) * Mathf.Cos(beta)
            );

            vertex = new Vector3(
                vertex.x * Mathf.Cos(gamma) + vertex.z * Mathf.Sin(gamma),
                vertex.y,
                -vertex.x * Mathf.Sin(gamma) + vertex.z * Mathf.Cos(gamma)
            );

            vertexSet.Add(midPoint + radius * vertex);

        }

        return vertexSet;

    }

}