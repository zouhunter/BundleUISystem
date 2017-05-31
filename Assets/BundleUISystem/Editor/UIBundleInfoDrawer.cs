﻿using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using BundleUISystem.Internal;
using BundleUISystem;

[CustomPropertyDrawer(typeof(UIBundleInfo))]
public class RtABInfoDrawer : PropertyDrawer
{
    const float widthBt = 20;
    const int ht = 6;
    List<GameObject> created = new List<GameObject>();
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;
        var typeProp = property.FindPropertyRelative("type");
        var buttonListProp = property.FindPropertyRelative("button");
        var toggleListProp = property.FindPropertyRelative("toggle");
        switch (typeProp.enumValueIndex)
        {
            case 0:
                return ht * EditorGUIUtility.singleLineHeight + EditorGUI.GetPropertyHeight(buttonListProp);
            case 1:
                return ht * EditorGUIUtility.singleLineHeight + EditorGUI.GetPropertyHeight(toggleListProp);
            case 2:
            case 3:
            default:
                return ht * EditorGUIUtility.singleLineHeight;
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var prefab = property.FindPropertyRelative("prefab");
        var assetName = property.FindPropertyRelative("assetName");
        var bundleName = property.FindPropertyRelative("bundleName");
        var typeProp = property.FindPropertyRelative("type"); ;
        var parentLayerProp = property.FindPropertyRelative("parentLayer");
        var boolProp = property.FindPropertyRelative("reset");
        var buttonProp = property.FindPropertyRelative("button");
        var toggleProp = property.FindPropertyRelative("toggle");
        float height = EditorGUIUtility.singleLineHeight;

        Rect rect = new Rect(position.xMin, position.yMin, position.width, height);

        rect.width -= widthBt * 8;
        rect.width /= 1.5f;
        if (GUI.Button(rect, assetName.stringValue))
        {
            property.isExpanded = !property.isExpanded;
            var instence = created.Find(x => x.name == assetName.stringValue);
            if (instence != null)
            {
                created.Remove(instence);
                Object.DestroyImmediate(instence);
            }
            if (property.isExpanded)
            {
                string[] paths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(bundleName.stringValue, assetName.stringValue);
                if (paths != null && paths.Length > 0)
                {
                    GameObject gopfb = AssetDatabase.LoadAssetAtPath<GameObject>(paths[0]);
                    prefab.objectReferenceValue = gopfb;
                    GameObject go = PrefabUtility.InstantiatePrefab(gopfb) as GameObject;
                    var uigroup = GameObject.FindObjectOfType<UIGroup>();
                    bool isworld = !uigroup.transform.GetComponent<RectTransform>();
                    go.transform.SetParent(uigroup.transform, isworld);
                    if (boolProp.boolValue)
                    {
                        go.transform.position = Vector3.zero;
                        go.transform.localRotation = Quaternion.identity;
                    }
                    created.Add(go);
                }
            }
        }
        rect.width = widthBt * 7;
        rect.x = position.xMax - widthBt * 8;

        prefab.objectReferenceValue = EditorGUI.ObjectField(rect, new GUIContent(""), prefab.objectReferenceValue, typeof(GameObject), false);

        //rect = new Rect(position.xMin, position.yMin, position.width, height);
        if (!property.isExpanded)
        {
            var width = position.width - widthBt * 8;
            width /= 1.5f;
            Rect draggableRect = new Rect(width + position.x, position.y, position.width - width - widthBt * 8, position.height);
            EditorGUI.Toggle(draggableRect, false, EditorStyles.toolbarButton);

            //    rect = new Rect(position.xMin, position.yMin, position.width, height);
            //    rect.width -= widthBt * 8;
            //    rect.x += rect.width / 1.2f;
            //    rect.width = widthBt * 1.5f;
            //    if (GUI.Button(rect, "[-]"))
            //    {
            //        Object pfbItem = prefab.objectReferenceValue;
            //        if (pfbItem != null)
            //        {
            //            bool find = false;
            //            MonoBehaviour[] scripts = ((GameObject)pfbItem).GetComponents<MonoBehaviour>();
            //            for (int i = 0; i < scripts.Length && !find; i++)
            //            {
            //                MonoBehaviour item = scripts[i];
            //                if (item is IPanelButton || item is IPanelEnable || item is IPanelName || item is IPanelToggle)
            //                {
            //                    find = true;
            //                    Selection.activeObject = MonoScript.FromMonoBehaviour(item);
            //                }
            //            }
            //        }
            //    }
            return;
        }

        EditorGUI.BeginDisabledGroup(true);
        rect = new Rect(position.xMin, position.yMin + height, position.width, height);
        EditorGUI.PropertyField(rect, assetName, new GUIContent("name"));

        rect.y += height;
        EditorGUI.PropertyField(rect, bundleName, new GUIContent("bundle"));
        EditorGUI.EndDisabledGroup();

        rect.y += height;
        EditorGUI.PropertyField(rect, typeProp, new GUIContent("type"));

        switch (typeProp.enumValueIndex)
        {
            case 0:
                rect.y += height;
                EditorGUI.PropertyField(rect, buttonProp, new GUIContent("Button"));
                break;
            case 1:
                rect.y += height;
                EditorGUI.PropertyField(rect, toggleProp, new GUIContent("Toggle"));
                break;
            case 2:
            case 3:
                break;
            default:
                break;
        }

        rect.y += height;
        EditorGUI.PropertyField(rect, parentLayerProp, new GUIContent("parentLayer"));

        rect.y += height;
        EditorGUI.PropertyField(rect, boolProp, new GUIContent("reset"));


    }
}
