using System;

[Serializable]
public struct TrunkData {

    public float flare;
    public float scale;
    public float scaleVariance;
    public float length;
    public float lengthVariance;
    public float taper;
    public int baseSplits;
    public float segmentSplits;
    public float splitAngle;
    public float splitAngleVariance;
    public int curveResolution;
    public float curve;
    public float curveBack;
    public float curveVariance;
    
}

[Serializable]
public struct BranchData {

    public float downAngle;
    public float downAngleVariance;
    public float rotate;
    public float rotateVariance;
    public int branches;
    public float length;
    public float lengthVariance;
    public float taper;
    public float segmentSplits;
    public float splitAngle;
    public float splitAngleVariance;
    public int curveResolution;
    public float curve;
    public float curveBack;
    public float curveVariance;

}