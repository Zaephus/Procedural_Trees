using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TrunkSegment {

    public Vector3 midPoint;
    public List<Vector3> vertices = new List<Vector3>();

    public Vector3 rotation;

    public float radius;

    public TrunkSegment(Vector3 _pos, Vector3 _rot, float _rad, int _res) {

        midPoint = _pos;
        rotation = _rot;

        radius = _rad;

        vertices = SetVertices(_res);

    }

    private List<Vector3> SetVertices(int _res) {

        List<Vector3> vertexSet = new List<Vector3>();

        for(int i = 0; i < _res; i++) {
            float alpha = -((2*Mathf.PI)/_res)*i + rotation.y;
            float beta = rotation.x;
            Vector3 vertex = new Vector3(Mathf.Cos(alpha), Mathf.Sin(alpha) * Mathf.Sin(beta), -Mathf.Sin(alpha) * Mathf.Cos(beta));
            vertexSet.Add(midPoint + radius * vertex);
        }

        return vertexSet;
    }

}