using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods {

    public static float Map(this float _value, float _inputMin, float _inputMax, float _outputMin, float _outputMax) {
        return (_value - _inputMin) / (_inputMax - _inputMin) * (_outputMax - _outputMin) + _outputMin;
    }
    
}