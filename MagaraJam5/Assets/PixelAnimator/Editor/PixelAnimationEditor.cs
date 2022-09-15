using binc.PixelAnimator.Elements;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;


namespace binc.PixelAnimator.Editor{
    

    [CustomEditor(typeof(PixelAnimation))]
    public class PixelAnimationEditor : UnityEditor.Editor{

        private PixelAnimation pixelAnimation;

        private int spriteIndex;
        private ReorderableList pixelSpriteList;
        private bool pixelSpriteFoldout;
        private GUIStyle customFoldoutStyle;
        private Rect lastRect;
        private SerializedProperty layerProps;
        private void OnEnable(){
            
            pixelAnimation = target as PixelAnimation;

            InitPixelSpriteList();
    


        }

        private void InitPixelSpriteList(){
            
            pixelSpriteList = new ReorderableList(serializedObject, serializedObject.FindProperty("pixelSprites"),
                true, true, true, true)
            {
                drawElementCallback = DrawPixelSprite,
                elementHeight = EditorGUIUtility.singleLineHeight * 4
            };


            layerProps = serializedObject.FindProperty("layers");
            
            pixelSpriteList.onAddCallback = (reorderableList) => {
                var index = reorderableList.serializedProperty.arraySize;
                reorderableList.serializedProperty.arraySize ++;
                reorderableList.index = index;
                var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("SpriteId").stringValue = GUID.Generate().ToString();
                
                if(pixelAnimation.Layers == null) return;
                
                for (var i = 0; i < layerProps.arraySize; i ++) {
                    var framesProp = layerProps.GetArrayElementAtIndex(i).FindPropertyRelative("frames");
                    var frameIndex = framesProp.arraySize; 
                    framesProp.arraySize++;
                    var frame = framesProp.GetArrayElementAtIndex(frameIndex);
                    frame.FindPropertyRelative("SpriteId").stringValue =
                        element.FindPropertyRelative("SpriteId").stringValue;
                    var hitBoxRectProp = frame.FindPropertyRelative("hitBoxRect");
                    hitBoxRectProp.FindPropertyRelative("x").floatValue = 16;
                    hitBoxRectProp.FindPropertyRelative("y").floatValue = 16;
                    hitBoxRectProp.FindPropertyRelative("width").floatValue = 16;
                    hitBoxRectProp.FindPropertyRelative("height").floatValue = 16;
                }
                
            };
            
            pixelSpriteList.onRemoveCallback = (reorderableList) => {
                reorderableList.serializedProperty.DeleteArrayElementAtIndex(reorderableList.index);
                if(pixelAnimation.Layers == null) return;
                
                for (var i = 0; i < layerProps.arraySize; i ++) {
                    var frameProp = layerProps.GetArrayElementAtIndex(i).FindPropertyRelative("frames");
                    frameProp.DeleteArrayElementAtIndex(reorderableList.index);
                }
            };


            pixelSpriteList.onReorderCallbackWithDetails = (_, index, newIndex) => {
                for (var i = 0; i < layerProps.arraySize; i++) {
                    var frameProp = layerProps.GetArrayElementAtIndex(i).FindPropertyRelative("frames");
                    frameProp.MoveArrayElement(index, newIndex);
                }
            };

            pixelSpriteList.drawHeaderCallback = (rect) => {
                EditorGUI.LabelField(rect, "Sprites! ");
            };

        }



        public override void OnInspectorGUI(){

            customFoldoutStyle = EditorStyles.foldout;
            customFoldoutStyle.fontSize = 12;
            customFoldoutStyle.normal.textColor = Color.white;
            
            serializedObject.Update();
            
            DrawPropertiesExcluding(serializedObject, new []{"pixelSprites", "m_Script"});
            SetPixelSpriteFoldout();

            GUILayout.Space(10);
            if(pixelSpriteFoldout) pixelSpriteList.DoLayoutList();
            DropAreaGUI();
            serializedObject.ApplyModifiedProperties();
            
        }



        private void DropAreaGUI (){ 
        var evt = Event.current;
            var dropArea = lastRect;
        
            switch (evt.type) {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (dropArea.Contains(evt.mousePosition)) {
                    
                
                    UnityEditor.DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    
                    if (evt.type == EventType.DragPerform) {
                        UnityEditor.DragAndDrop.AcceptDrag ();
                    
                        foreach (var draggedObject in UnityEditor.DragAndDrop.objectReferences) {
                            if(draggedObject is Sprite sprite){
                                pixelSpriteList.onAddCallback.Invoke(pixelSpriteList);
                                pixelSpriteList.serializedProperty
                                    .GetArrayElementAtIndex(pixelSpriteList.serializedProperty.arraySize - 1)
                                    .FindPropertyRelative("sprite").objectReferenceValue = sprite;
                            }
                        }
                    }
                }
                break;
            }
        }


        private void SetPixelSpriteFoldout(){
            
            pixelSpriteFoldout = EditorGUILayout.Foldout(pixelSpriteFoldout, "Pixel Sprites", true, customFoldoutStyle);
            lastRect = GUILayoutUtility.GetLastRect();
            Repaint();

            if(lastRect.Contains(Event.current.mousePosition)){

                lastRect.position = new Vector2(lastRect.position.x - 15, lastRect.position.y);
                lastRect.width += 15;
                EditorGUI.DrawRect(lastRect, new Color(1, 1, 1, 0.08f));
                
            }

                
        }
        
        private void DrawPixelSprite(Rect rect, int index, bool isActive, bool isFocused){

            var element = pixelSpriteList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, 210, 
                EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("sprite"),
                GUIContent.none
            
            );
            EditorGUI.PropertyField(
                new Rect(rect.x + 220, rect.y, 120, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("SpriteId"),
                GUIContent.none
            );

            EditorGUI.BeginChangeCheck();
            var sprite = (Sprite)EditorGUI.ObjectField(
                new Rect(rect.x + 450, rect.y, 48, 48), 
                pixelAnimation.pixelSprites[index].sprite, typeof(Sprite), true
            );
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(target, "Changed Sprite");
                pixelAnimation.pixelSprites[index].sprite = sprite;
            }


        }

        
        
        }
}
