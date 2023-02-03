using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TreeMeshBuilder {

    public static List<int> SetTriangles(int _amount, int _radialResolution) {

        List<int> triangleSet = new List<int>();

        for(int i = 0; i < _amount - _radialResolution; i++) {
            triangleSet.Add(i + _radialResolution - 1);
            triangleSet.Add(i + _radialResolution);
            triangleSet.Add(i + _radialResolution - _radialResolution);

            triangleSet.Add(i + _radialResolution);
            triangleSet.Add(i + _radialResolution - _radialResolution + 1);
            triangleSet.Add(i + _radialResolution - _radialResolution);
        }

        return triangleSet;

    }

    public static float CalculateTaper(float _height, float _taper, float _length, float _baseRadius) {

        float radiusZ = _baseRadius;

        float unitTaper;

        if(_taper >= 0 && _taper < 1) {
            unitTaper = _taper;
        }
        else if(_taper >= 1 && _taper < 2) {
            unitTaper = 2 - _taper;
        }
        else {
            unitTaper = 0;
        }

        float taperZ = _baseRadius * (1 - unitTaper * _height);

        float zTwo;
        float zThree;

        float depth;

        if(_taper >= 0 && _taper < 1) {
            radiusZ = taperZ;
        }
        else if(_taper >= 1 && _taper <= 3) {
            zTwo = (1 - _height) * _length;

            if(_taper < 2 || zTwo < taperZ) {
                depth = 1;
            }
            else {
                depth = _taper - 2;
            }

            if(_taper < 2) {
                zThree = zTwo;
            }
            else {
                zThree = Mathf.Abs(zTwo - (2 * taperZ * ((int)(zTwo / (2 * taperZ) + 0.5f))));
            }

            if(_taper < 2 && zThree >= taperZ) {
                radiusZ = taperZ;
            }
            else {
                radiusZ = (1 - depth) * taperZ + depth * Mathf.Sqrt((taperZ * taperZ) - ((zThree - taperZ)*(zThree - taperZ)));
            }
            
        }

        return radiusZ;

    }

    public static List<Vector3> SetFlatShadedNormals(List<Vector3> _vertices, List<int> _triangles) {

        List<Vector3> vertexSet = new List<Vector3>();

        for(int i = 0; i < _triangles.Count; i++) {
            vertexSet.Add(_vertices[_triangles[i]]);
            _triangles[i] = i;
        }

        return vertexSet;

    }

    public static Mesh SetFlatShadedNormals(Mesh _mesh) {

        Mesh mesh = new Mesh();

        List<Vector3> vertexSet = new List<Vector3>();
        List<int> triangleSet = new List<int>();

        for(int i = 0; i < _mesh.triangles.Length; i++) {
            vertexSet.Add(_mesh.vertices[_mesh.triangles[i]]);
            triangleSet.Add(i);
        }

        mesh.vertices = vertexSet.ToArray();
        mesh.triangles = triangleSet.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
        
    }

    public static Mesh CreateMesh(List<Vector3> _vertices, List<int> _triangles) {

        Mesh mesh = new Mesh();

        mesh.vertices = _vertices.ToArray();
        mesh.triangles = _triangles.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

}