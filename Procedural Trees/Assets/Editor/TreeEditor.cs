using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Tree))]
public class TreeEditor : Editor {

    private Tree tree;

    public override void OnInspectorGUI() {

        base.OnInspectorGUI();

        // using(var check = new EditorGUI.ChangeCheckScope()) {

        //     base.OnInspectorGUI();

        //     if(check.changed) {
        //         Generate();
        //     }

        // }

        if(GUILayout.Button("Generate")) {
            tree.Generate();
        }

    }

    private void OnEnable() {
        tree = (Tree)target;
    }


}