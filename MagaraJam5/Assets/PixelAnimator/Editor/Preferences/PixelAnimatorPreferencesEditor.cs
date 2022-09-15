using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using binc.PixelAnimator.Preferences;
using binc.PixelAnimator.PropertyData;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.Experimental.GraphView;
using UnityEditorInternal;
using UnityEngine;
using binc.PixelAnimator.Utility;



namespace binc.PixelAnimator.Editor.Preferences{


    [CustomEditor(typeof(PixelAnimatorPreferences))]
    
    public class PixelAnimatorPreferencesEditor : UnityEditor.Editor, ISearchWindowProvider{

        #region Var
        private const string ResourcesPath = "Assets/SpriteAnimator/Editor Default Resources/";

        private string groupTypeName, spritePropertyName, hitBoxPropertyName;
        private Texture2D[] textures;

        private PixelAnimatorPreferences preferences;
        private SerializedObject so;
        private SerializedProperty propGroups, propSpriteProperties, propHitBoxProperties;
    
        
        
        private float lastTimeSinceStartup, animationTimer;
        private AnimBool alreadyExistGroupWarning;
        
        private List<SearchTreeEntry> searchList;
        private string[] packageDlls, generalDlls;
        private int activePropIndex;
        private DataType hitBoxProp, spriteProp;
        private PropertyType hitBoxPropType, spritePropType;

        private ReorderableList groupList;
        private ReorderableList hitBoxList;
        private ReorderableList spriteList;

        private PropertyWay lastInterectedProperty;

        #endregion


        #region  SearchWindow
        private void SetComponentSearchWindow(){

            searchList = new List<SearchTreeEntry>{ new SearchTreeGroupEntry( new GUIContent("Choose Component")) };

            
            foreach (var type in GetComponentType()) {
                searchList.Add( new SearchTreeEntry(new GUIContent( type.Name )){level = 1} );
            }
            
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) => searchList;

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context){

            SetSearchWindow(searchTreeEntry, (ComponentProperty)lastInterectedProperty.mainProperty);

            return true;
        }

        private void SetSearchWindow(SearchTreeEntry searchTreeEntry, ComponentProperty properties){

            foreach (var type in GetComponentType()) {
                if(searchTreeEntry.name == type.Name)
                    properties.selectedComponent = new SerializableSystemType(type);
            }
            
            // set property info
            var propertyInfos = properties.selectedComponent.SystemType.GetProperties
                                            ( BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty );
            
            properties.serializablePropertyInfo.CreateItem();

            foreach (var propertyInfo in propertyInfos) {
                if (!propertyInfo.CanWrite || !propertyInfo.SetMethod.IsPublic) continue;
                
                properties.serializablePropertyInfo.AddItem();
                var lastIndex = properties.serializablePropertyInfo.names.Count -1;
                    
                properties.serializablePropertyInfo.names[lastIndex] = propertyInfo.Name;
                properties.serializablePropertyInfo.serializableTypes[lastIndex] = new SerializableSystemType(propertyInfo.PropertyType);
            }
            

        }

        private IEnumerable<Type> GetComponentType(){
            
            packageDlls = Directory.GetFiles(Directory.GetCurrentDirectory() + "/Library/ScriptAssemblies", "*.dll");
            foreach(var dll in packageDlls){
                var packageComponent = Assembly.LoadFile(dll);
                foreach(var type in packageComponent.GetTypes()){
                    if(type.IsSubclassOf( typeof(Component) )) {
                        yield return type;

                    }
                }
            }
            
            generalDlls = Directory.GetFiles(EditorApplication.applicationContentsPath + @"/Managed/UnityEngine", "*dll");

            foreach(var dll in generalDlls){
                var generalComponent = Assembly.LoadFile(dll);
                foreach(var type in generalComponent.GetTypes()){
                    if(type.IsSubclassOf( typeof(Component) )) {
                        yield return type;

                    }
                }
            }
        }
        
        #endregion

        private void OnEnable() {


            textures = new Texture2D[4];
            textures[0] = (Texture2D)AssetDatabase.LoadAssetAtPath(ResourcesPath + "drop.png", typeof(Texture2D));
            textures[1] = (Texture2D)AssetDatabase.LoadAssetAtPath(ResourcesPath + "down.png", typeof(Texture2D));
            textures[2] = (Texture2D)AssetDatabase.LoadAssetAtPath(ResourcesPath + "-.png", typeof(Texture2D));
            textures[3] = (Texture2D)AssetDatabase.LoadAssetAtPath(ResourcesPath + "ok.png", typeof(Texture2D));

            preferences = (PixelAnimatorPreferences)target;
            
            so = serializedObject;

            propGroups = so.FindProperty("groups");
            propSpriteProperties = so.FindProperty("spriteProperties");
            propHitBoxProperties = so.FindProperty("hitBoxProperties");


            EditorApplication.update += SetEditorDeltaTime;

            alreadyExistGroupWarning = new AnimBool(false);
            alreadyExistGroupWarning.valueChanged.AddListener(Repaint);
            SetComponentSearchWindow();
            InitGroupList();
            InitPropertyList();

        }


        private void OnDisable() => EditorApplication.update -= SetEditorDeltaTime;

        public override void OnInspectorGUI(){
            base.OnInspectorGUI();
            so.Update();
            groupList?.DoLayoutList();
            so.ApplyModifiedProperties();
            spriteList?.DoLayoutList();
            so.ApplyModifiedProperties();
            hitBoxList?.DoLayoutList();
            so.ApplyModifiedProperties();


        }


        
        #region Groups

        private void InitGroupList(){
            groupList = new ReorderableList(serializedObject, propGroups,
                true, true, true, true){
                drawElementCallback = DrawGroups,
                elementHeight = EditorGUIUtility.singleLineHeight,
                drawHeaderCallback = (rect) => {EditorGUI.LabelField(rect, "Groups");},
                onAddCallback = (list => {
                    var index = list.serializedProperty.arraySize;
                    list.serializedProperty.arraySize ++;
                    list.index = index;
                    var element = list.serializedProperty.GetArrayElementAtIndex(index);
                    element.FindPropertyRelative("guid").stringValue = GUID.Generate().ToString();
                    element.serializedObject.ApplyModifiedProperties();
                })
            };
            

        }
        
        private void DrawGroups(Rect rect, int index, bool isActive, bool isFocused){
            var element = groupList.serializedProperty.GetArrayElementAtIndex(index);
            var eventCurrent = Event.current;
            so.Update();

            rect.y += 2;


            var color = element.FindPropertyRelative("color");
            var boxType = element.FindPropertyRelative("boxType");
            var activeLayer = element.FindPropertyRelative("activeLayer");
            var physicMaterial2D = element.FindPropertyRelative("physicMaterial");
            var rounded = element.FindPropertyRelative("rounded");
            var detection = element.FindPropertyRelative("colliderDetection");
            var collisionLayer = element.FindPropertyRelative("collisionLayer");

            var colorRect = new Rect(rect.x, rect.y, 140, EditorGUIUtility.singleLineHeight);
            var boxTypeRect = new Rect(colorRect.xMax + 10, rect.y, 100, EditorGUIUtility.singleLineHeight);
            var activeLayerRect = new Rect(boxTypeRect.xMax + 10, rect.y, 100, EditorGUIUtility.singleLineHeight);
            var physicMaterial2DRect = new Rect(activeLayerRect.xMax + 10, rect.y, 100, EditorGUIUtility.singleLineHeight);
            var roundedRect = new Rect(physicMaterial2DRect.xMax + 10, rect.y, 10, EditorGUIUtility.singleLineHeight);
            var detectionRect = new Rect(roundedRect.xMax + 10, rect.y, 100, EditorGUIUtility.singleLineHeight);
            var collisionLayerRect = new Rect(detectionRect.xMax + 10, rect.y, 100, EditorGUIUtility.singleLineHeight);
            
            EditorGUI.PropertyField(
                colorRect,
                color,
                GUIContent.none
                );

            EditorGUI.PropertyField(
                    boxTypeRect,
                    boxType,
                    GUIContent.none
                );

            EditorGUI.PropertyField(
                activeLayerRect,
                activeLayer,
                GUIContent.none
                );

            EditorGUI.PropertyField(
                physicMaterial2DRect,
                physicMaterial2D,
                GUIContent.none
                );

            EditorGUI.PropertyField(
                roundedRect,
                rounded,
                GUIContent.none
                );

            EditorGUI.PropertyField(
                detectionRect,
                detection,
                GUIContent.none
                );
            
            EditorGUI.PropertyField(
                collisionLayerRect,
                collisionLayer,
                GUIContent.none
            );

            

            //Setting Tool tips
            
            PixelAnimatorUtility.CreateTooltipForRect(activeLayerRect, LayerMask.LayerToName(activeLayer.intValue),
                eventCurrent.mousePosition);
            
            PixelAnimatorUtility.CreateTooltipForRect(collisionLayerRect,
                LayerMask.LayerToName(collisionLayer.intValue), eventCurrent.mousePosition);
            
            PixelAnimatorUtility.CreateTooltipForRect(roundedRect, "Rounded", eventCurrent.mousePosition);
            
            PixelAnimatorUtility.CreateTooltipForRect(boxTypeRect, "Name", eventCurrent.mousePosition);
            
            PixelAnimatorUtility.CreateTooltipForRect(detectionRect, "Detection Type", eventCurrent.mousePosition);
            
    
            element.serializedObject.ApplyModifiedProperties();


        }
        


        
        
        #endregion

        #region Properties
        
        private const string manuelComponentTip = "Type the name of the object from which you will receive the Component from the Animator.";

        private const string componentHelp =
            "Please choose only those components that will work for you. Otherwise, problems may arise.";
        private void InitPropertyList(){
            hitBoxList = new ReorderableList(serializedObject, propHitBoxProperties,
                true, true, true, true){
                drawElementCallback = DrawHitBoxProperties,
                elementHeight = EditorGUIUtility.singleLineHeight,
                drawHeaderCallback = rect => {EditorGUI.LabelField(rect, "Hit Box Properties!");},
                onAddCallback = _ => {
                    var index = hitBoxList.serializedProperty.arraySize;
                    hitBoxList.serializedProperty.arraySize ++;
                    hitBoxList.index = index;
                    var element = hitBoxList.serializedProperty.GetArrayElementAtIndex(index);
                    var mainProp = element.FindPropertyRelative("mainProperty");
                    mainProp.managedReferenceValue = PixelAnimatorUtility.CreateBlankPropertyData(PropertyType.Manuel);
                    mainProp.FindPropertyRelative("guid").stringValue = GUID.Generate().ToString();
                    element.serializedObject.ApplyModifiedProperties();
                }
                
            };
            
            
            
            spriteList = new ReorderableList(serializedObject, propSpriteProperties,
                true, true, true, true){
                drawElementCallback = DrawSpriteProperties,
                elementHeight = EditorGUIUtility.singleLineHeight,
                drawHeaderCallback = rect => {EditorGUI.LabelField(rect, "Sprite Properties!");},
                onAddCallback = _ => {
                    var index = spriteList.serializedProperty.arraySize;
                    spriteList.serializedProperty.arraySize ++;
                    spriteList.index = index;
                    var element = spriteList.serializedProperty.GetArrayElementAtIndex(index);
                    var mainProp = element.FindPropertyRelative("mainProperty");
                    mainProp.managedReferenceValue = PixelAnimatorUtility.CreateBlankPropertyData(PropertyType.Manuel);
                }
            };
        }



        private void DrawSpriteProperties(Rect rect, int index, bool isActive, bool isFocused){
            var element = spriteList.serializedProperty.GetArrayElementAtIndex(index);
            element.serializedObject.Update();
            rect.y += 2;
            
            var type = element.FindPropertyRelative("propertyType");

            var typeRect = new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight);
            
            // Draw type of property.
            using (var check = new EditorGUI.ChangeCheckScope()) {
                EditorGUI.PropertyField(
                    typeRect,
                    type,
                    GUIContent.none
                );
                if (check.changed) {
                    element.FindPropertyRelative("mainProperty").managedReferenceValue = PixelAnimatorUtility.
                        CreateBlankPropertyData((PropertyType)type.intValue);
                    element.serializedObject.ApplyModifiedProperties(); 
                }

            }
            
            switch ((PropertyType)type.intValue) {
                case PropertyType.Manuel:
                    DrawManuelProperty(rect, element);
                    break;
                case PropertyType.Component:
                    DrawComponentProperty(rect,element, preferences.SpriteProperties[index]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            element.serializedObject.ApplyModifiedProperties();

        }

        private void DrawHitBoxProperties(Rect rect, int index, bool isActive, bool isFocused){
            var element = hitBoxList.serializedProperty.GetArrayElementAtIndex(index);
            element.serializedObject.Update();
            rect.y += 2;

            var type = element.FindPropertyRelative("propertyType");
            var typeRect = new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight);
            
            // Draw type of property.
            using (var check = new EditorGUI.ChangeCheckScope()) {
                EditorGUI.PropertyField(
                    typeRect,
                    type,
                    GUIContent.none
                );
                if (check.changed) {
                    element.FindPropertyRelative("mainProperty").managedReferenceValue = PixelAnimatorUtility.
                        CreateBlankPropertyData((PropertyType)type.intValue);
                    element.serializedObject.ApplyModifiedProperties(); 
                }

            }

            switch ((PropertyType)type.intValue) {
                case PropertyType.Manuel:
                    DrawManuelProperty(rect, element);
                    break;
                case PropertyType.Component:
                    DrawComponentProperty(rect,element, preferences.HitBoxProperties[index]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }



            element.serializedObject.ApplyModifiedProperties();
        }
        
        
        
        private void DrawComponentProperty(Rect rect, SerializedProperty element, PropertyWay propertyWay){
            element.serializedObject.Update();
            
            var mainProperty = element.FindPropertyRelative("mainProperty");
            if (mainProperty.managedReferenceValue.GetType() != typeof(ComponentProperty)) return;
                
            
            
            var selectedComponent = mainProperty.FindPropertyRelative("selectedComponent");

            var componentName = selectedComponent.FindPropertyRelative("name").stringValue;
            var dataName = mainProperty.FindPropertyRelative("sourceName").stringValue;
            var fourTransactions = mainProperty.FindPropertyRelative("fourTransactions");
            var componentWay = mainProperty.FindPropertyRelative("componentWay");
            var gameObjectName = mainProperty.FindPropertyRelative("gameObjectName");
            
            var selectedDataSystemType = ((ComponentProperty)propertyWay.mainProperty).SelectedData.SystemType;
            

            var lastRect = DrawMainProperty(rect, element);
            element.serializedObject.Update();
            var componentSearchRect = new Rect(lastRect.xMax + 20, rect.y, 90, EditorGUIUtility.singleLineHeight);
            var propertySelectRect = new Rect(componentSearchRect.xMax + 20, rect.y, 90, EditorGUIUtility.singleLineHeight);
            var fourTransactionsRect = new Rect(propertySelectRect.xMax + 20, rect.y, 90, EditorGUIUtility.singleLineHeight);
            var componentWayRect = new Rect(fourTransactionsRect.xMax + 20, rect.y, 90, EditorGUIUtility.singleLineHeight);
            var gameObjectNameRect = new Rect(componentWayRect.xMax + 20, rect.y, 90, EditorGUIUtility.singleLineHeight);
            
            var componentSearchLabel = componentName == "" ? "Search" : componentName ;
            
            if (GUI.Button(componentSearchRect, componentSearchLabel, EditorStyles.popup)) {
                lastInterectedProperty = propertyWay;
                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),this);
            }
            
            if (componentName != "") {
                var dataLabel = dataName == "" ? "Select" : dataName;

                if (GUI.Button(propertySelectRect, dataLabel, EditorStyles.popup)) {
                    OpenSelectComponentProperty(propertyWay);
                }
                element.serializedObject.Update();
                element.serializedObject.ApplyModifiedProperties();
                
                
                
                if (selectedDataSystemType != null && dataName != "") {
                    EditorGUI.PropertyField(
                        fourTransactionsRect,
                        fourTransactions,
                        GUIContent.none
                    );

                    EditorGUI.PropertyField(
                        componentWayRect,
                        componentWay,
                        GUIContent.none
                        );

                    if ((ComponentWay)componentWay.intValue == ComponentWay.Manuel) {
                        EditorGUI.PropertyField(
                            gameObjectNameRect,
                            gameObjectName,
                            GUIContent.none
                        );
                    }
                }
                
                
            
                
            }

            element.serializedObject.ApplyModifiedProperties();
        }

        private void DrawManuelProperty(Rect rect, SerializedProperty element){
            element.serializedObject.Update();
            var lastRect = DrawMainProperty(rect, element);
            var mainProperty = element.FindPropertyRelative("mainProperty");
            if (mainProperty.managedReferenceValue.GetType() != typeof(ManuelProperty)) {
                return;
            }

            var dataType = mainProperty.FindPropertyRelative("dataType");

            var dataTypeRect = new Rect(lastRect.xMax + 20, lastRect.y, lastRect.width, lastRect.height);
            
            if (mainProperty.managedReferenceValue.GetType() != typeof(ManuelProperty)) return;

            using var check = new EditorGUI.ChangeCheckScope();
            EditorGUI.PropertyField(
                dataTypeRect,
                dataType,
                GUIContent.none
            );
            
            if (check.changed) {
                mainProperty.FindPropertyRelative("guid").stringValue = GUID.Generate().ToString();
            }
        }
        
        
        private Rect DrawMainProperty(Rect rect, SerializedProperty element){
            element.serializedObject.Update();
            
            var mainProperty = element.FindPropertyRelative("mainProperty");
            
            var nameProp = mainProperty.FindPropertyRelative("name");
            
            
            //Adjusted rect of property.
            var nameRect = new Rect(rect.x + 120, rect.y, 90, EditorGUIUtility.singleLineHeight);

            //Draw name of property
            EditorGUI.PropertyField(
                nameRect,
                nameProp,
                GUIContent.none
            );
            

            
            element.serializedObject.ApplyModifiedProperties();
            
            return nameRect;
        }
        
        private static void OpenSelectComponentProperty(PropertyWay propertyWay){
            var menu = new GenericMenu();
            var propInfo = ((ComponentProperty)propertyWay.mainProperty).serializablePropertyInfo;
            
            for(var i = 0; i < propInfo.names.Count; i++){
            
            
                var propType = propInfo.serializableTypes[i];
                var propName = propInfo.names[i];
                var itemName = propName + @"  \  " + propType.Name;
            
                menu.AddItem(new GUIContent(itemName), false, () => {
                    ((ComponentProperty)propertyWay.mainProperty).SetSelectedData(propType);
                    ((ComponentProperty)propertyWay.mainProperty).SetSourceName(propName);
            
                });
                        
            }
            menu.ShowAsContext();
        }

        
        #endregion 

        private void SetEditorDeltaTime(){

            if(lastTimeSinceStartup == 0f){
                lastTimeSinceStartup = (float)EditorApplication.timeSinceStartup;
            }

            lastTimeSinceStartup = (float)EditorApplication.timeSinceStartup;
            
            
        }




    }


}



