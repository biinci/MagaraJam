using System;
using System.Collections.Generic;
using binc.PixelAnimator.Elements;
using UnityEditor;
using UnityEngine;

namespace binc.PixelAnimator{
    [Serializable]
    public class Layer{

        
        public List<Frame> frames;
        
        [HideInInspector] public bool activeBox;
        [HideInInspector] public bool onHandleTopLeft;
        [HideInInspector] public bool onHandleTopCenter;
        [HideInInspector] public bool onHandleTopRight;
        [HideInInspector] public bool onHandleRightCenter;
        [HideInInspector] public bool onHandleBottomRight;
        [HideInInspector] public bool onHandleBottomCenter;
        [HideInInspector] public bool onHandleBottomLeft;
        [HideInInspector] public bool onHandleLeftCenter;
        [HideInInspector] public bool onMiddle;
        [ReadOnly, SerializeField]
        private string guid;

        public string Guid => guid;

        public Layer(string guid){
            frames = new List<Frame>();
            this.guid = guid;
        }

    }

    [Serializable]
    public class SerializablePropertyInfo{

        public List<string> names = new();
        public List<SerializableSystemType> serializableTypes = new();


        public void AddItem(){
            names.Add( null);
            serializableTypes.Add(null);

        }

        public void CreateItem(){
            names = new List<string>();
            serializableTypes = new List<SerializableSystemType>();

        }
    }
    public class ReadOnlyAttribute : PropertyAttribute{
        
        
    }
#if UNITY_EDITOR


    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyPropertyDrawer : PropertyDrawer{
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label){
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;
            
        }
    }
#endif
}






