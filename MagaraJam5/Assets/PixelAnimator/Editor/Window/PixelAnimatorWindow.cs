using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using binc.PixelAnimator.PropertyData;
using UnityEngine.Events;
using binc.PixelAnimator.Elements;
using binc.PixelAnimator.Preferences;
using binc.PixelAnimator.Utility;
using Codice.CM.SEIDInfo;
using Component = System.ComponentModel.Component;


namespace binc.PixelAnimator.Editor.Window{ 

    [Serializable]
    
    public class PixelAnimatorWindow : EditorWindow{

        
        #region Variable

        private const string PixelAnimatorPath = "Assets/PixelAnimator/";
        private const string ResourcesPath = PixelAnimatorPath + "Editor Default Resources/";
        private Rect timelineRect, dragTimelineRect, topTimelineRect, addGroupsRect, backRect, playRect, frontRect, yLayoutLine, xLayoutLine;

        private Rect[] yLineRect;
        private Rect[,] selectFrameRect;

        private List<GroupRects> groupRects;
        private PixelAnimation selectedAnimation; 

        private float scrollWhellZoom = 1, layoutZoomFactor;
        
        private List<Rect> frameRects;
        private int activeFrame, activeLayerIndex;
        
        //Editor Delta Time
        private float timer, editorDeltaTime;
        private float lastTimeSinceStartup;
        
        private Color blackColor = new Color(0.15f, 0.15f, 0.15f, 1);
        
        private bool isPlaying, draggableTimeline;
        private Vector2 timeLinePosition;
        private PixelAnimatorPreferences preferences;
        
        private Texture2D backT, playT, frontT, defAddGroupsT, onMouseAddGroupsT, singleFrameT;
        
        private SerializedObject targetAnimation;


        private int frameCount;

        private float thumbNailScale;

        private Vector2 greatestSpriteSize;
        private Vector2 windowMatrixScale;
        private GenericMenu boxTypePopup, settingsPopup;
        private Vector2 delta;
        [SerializeField] private Rect spriteWindowRect;
        private bool drag;
        private Vector2 propertyXScrollPos = Vector2.one;
        private Vector2 propertyYScrollPos = Vector2.one;
        private bool reSizingBox;
        private Rect propertyWindowRect = new(10, 6, 120, 20);
        private bool methodNameFoldout;
        private enum PropertyFocus{HitBox, Sprite}

        private enum WindowFocus{
            Property,
            TimeLine,
            SpriteWindow
        }

        private WindowFocus windowFocus;
        private PropertyFocus propertyFocus;
        private Layer ActiveLayer{
            get{
                if (selectedAnimation != null && selectedAnimation.Layers is{ Count: > 0 }) {
                    return selectedAnimation.Layers[activeLayerIndex];
                    
                }

                return null;

            } 
        }
        #endregion




        [MenuItem("Window/Animator")]
        private static void Init() => GetWindow<PixelAnimatorWindow>("Pixel Animator");
        
    
        
        private void OnEnable(){

            //Set textures
            LoadInitResources();
            
            const float buttonSize = 32;// button rect set
            addGroupsRect = new Rect(15, 15, 48, 48);
            backRect = new Rect(200, 20, buttonSize, buttonSize);
            playRect = new Rect((backRect.width + backRect.xMin) + 2, backRect.yMin, buttonSize, buttonSize); 
            frontRect = new Rect((playRect.width + playRect.xMin) + 2, backRect.yMin, buttonSize, buttonSize);

            lastTimeSinceStartup = 0f;
            groupRects = new List<GroupRects>();
            frameRects = new List<Rect>();
            // minSize = new Vector2( 700, 400 );
            
            
        }


        private void LoadInitResources(){
            preferences = (PixelAnimatorPreferences)AssetDatabase.LoadAssetAtPath(
                PixelAnimatorPath + "Preferences/PixelAnimatorPreferences.asset", typeof(PixelAnimatorPreferences));
            backT = (Texture2D)AssetDatabase.LoadAssetAtPath(ResourcesPath + "Back.png", typeof(Texture2D)) ;
            frontT = (Texture2D)AssetDatabase.LoadAssetAtPath(ResourcesPath + "Front.png", typeof(Texture2D));
            defAddGroupsT = (Texture2D)AssetDatabase.LoadAssetAtPath(ResourcesPath + "AddBoxes.png", typeof(Texture2D));
            onMouseAddGroupsT = (Texture2D)AssetDatabase.LoadAssetAtPath(ResourcesPath + "AddBoxes2.png", typeof(Texture2D));
            singleFrameT = (Texture2D)AssetDatabase.LoadAssetAtPath(ResourcesPath + "frame.png", typeof(Texture2D));
        }
        
        
        
        private void OnGUI(){
            
            SelectedObject();
            SetWindows();
            
            if (selectedAnimation == null) return;
            

            if (selectedAnimation.Layers != null && activeLayerIndex >= selectedAnimation.Layers.Count || activeLayerIndex < 0) {
                activeLayerIndex = 0;    
            }
            
            if (activeFrame >= selectedAnimation.GetSpriteList().Count || activeFrame < 0) {
                activeFrame = 0;
            }
            if(selectedAnimation.Layers.Count > 0)
                reSizingBox = PixelAnimatorUtility.CheckLayerBool(selectedAnimation.Layers[activeLayerIndex]);
            

            SetAddLayerPopup();

            SetFrameCopyPaste();

          

        }


        private void SetWindows(){

            BeginWindows();
            CreateTimeline();
            DrawPropertyWindow();
            DrawSpriteWindow();
            EndWindows();
            
            switch (windowFocus) {
                case WindowFocus.TimeLine or WindowFocus.SpriteWindow :
                    if(Event.current.type == EventType.MouseDown){ 
                        GUI.FocusControl(null);    
                    }
                    break;
            }
            
            GUI.BringWindowToFront(4);
            GUI.BringWindowToFront(5);
            GUI.BringWindowToFront(2);
            GUI.BringWindowToBack(1);

        }

        private void DrawSpriteWindow(){
            SetZoom();
            if (targetAnimation == null || selectedAnimation == null) return;
            var tempMatrix = GUI.matrix;
            var zoom = scrollWhellZoom * layoutZoomFactor * Vector2.one;
            var spriteList = selectedAnimation.GetSpriteList();
            GUIUtility.ScaleAroundPivot( zoom,  spriteWindowRect.position);

            var eventCurrent = Event.current;
            if(eventCurrent.type == EventType.MouseDrag && eventCurrent.button == 2)
                drag = true;
            if(drag)
                delta = eventCurrent.delta;
            if(eventCurrent.type == EventType.MouseUp){
                delta = Vector2.zero;
                drag = false;
            }
            
            spriteWindowRect.size = new Vector2(spriteList[activeFrame].rect.width, spriteList[activeFrame].rect.height);
            spriteWindowRect.position += delta * scrollWhellZoom;
            if (spriteWindowRect.Contains(eventCurrent.mousePosition) && eventCurrent.type == EventType.MouseDown) {
                windowFocus = WindowFocus.SpriteWindow;
            }
                
            GUI.Window(1, spriteWindowRect, CreateSpriteTexture, GUIContent.none);
            windowMatrixScale = GUI.matrix.lossyScale;
            GUI.matrix = tempMatrix;
            SetMouseCursor();
        }
        
        private void CreateSpriteTexture(int windowID){
            
            var activeSprite = selectedAnimation.GetSpriteList()[activeFrame];
            if (activeSprite == null) return;
            EditorGUI.DrawTextureTransparent(new Rect(0, 0, activeSprite.rect.width, activeSprite.rect.height), 
                                            AssetPreview.GetAssetPreview(activeSprite), ScaleMode.ScaleToFit);
            AssetPreview.GetAssetPreview(activeSprite).filterMode = FilterMode.Point;

            
            SetPlayKeys();
            SetLayerKeys();
            SetBox();
        }

        private void SetBox(){

            var eventCurrent = Event.current;
                targetAnimation.Update();
                if(selectedAnimation.Layers.Count > 0){


                    for(var i = 0; i < selectedAnimation.Layers.Count; i++){
                        var layer = selectedAnimation.Layers[i];
                        var frame = layer.frames[activeFrame];

                        frame.hitBoxRect.width  = Mathf.Clamp( frame.hitBoxRect.width, 0, float.MaxValue );
                        frame.hitBoxRect.height = Mathf.Clamp( frame.hitBoxRect.height, 0, float.MaxValue);


                        if (frame.hitBoxRect.Contains(eventCurrent.mousePosition) && eventCurrent.button == 0
                            && eventCurrent.type == EventType.MouseDown && !reSizingBox && !ActiveLayer
                                .frames[activeFrame].hitBoxRect.Contains(eventCurrent.mousePosition)) {
                            activeLayerIndex = i;
                            layer.activeBox = true;
                        }
                        else if (activeLayerIndex != i)
                            layer.activeBox = false;

                        
                        PixelAnimatorUtility.DrawBox(layer, preferences.GetGroup(layer.Guid),
                             activeFrame, windowMatrixScale);
                    }

                    ActiveLayer.activeBox = true;

                }
                targetAnimation.ApplyModifiedProperties();
            
        }
        
        private void SetMouseCursor(){
            if (selectedAnimation.Layers.Count <= 0) return;

            var spriteWindowPos = spriteWindowRect.position;
            
            var rect = selectedAnimation.Layers[activeLayerIndex].frames[activeFrame].hitBoxRect;
            var size = PixelAnimatorUtility.BoxSize / windowMatrixScale.x;
            var rTopLeft = new Rect(rect.xMin - size/2, rect.yMin - size/2, size, size);
            var rTopCenter = new Rect((rect.xMin + rect.width/2) - size/2, rect.yMin - size/2, size, size);
            var rTopRight = new Rect(rect.xMax - size/2, rect.yMin - size/2, size, size);
            var rRightCenter = new Rect(rect.xMax - size/2, rect.yMin + (rect.yMax - rect.yMin)/2 - size/2, size, size);
            var rBottomCenter = new Rect((rect.xMin + rect.width/2) - size/2, rect.yMax - size/2, size, size);
            var rBottomLeft = new Rect(rect.xMin - size/2, rect.yMax - size/2, size, size);
            var rBottomRight = new Rect(rect.xMax - size/2, rect.yMax - size/2, size, size);
            var rLeftCenter = new Rect(rect.xMin - size/2, rect.yMin + (rect.yMax - rect.yMin)/2 - size/2, size, size);
            
            var smallMiddleRect = new Rect(rect.xMin + 0.5f , rect.yMin + 0.5f
                , rect.size.x - 0.5f, rect.size.y - 0.5f);
            var adjustedMiddleRect = new Rect((smallMiddleRect.x * windowMatrixScale.x) + spriteWindowPos.x, (smallMiddleRect.y * windowMatrixScale.y) 
                + spriteWindowPos.y, smallMiddleRect.width * windowMatrixScale.x, smallMiddleRect.height * windowMatrixScale.y);
                
                
            EditorGUIUtility.AddCursorRect(PixelAnimatorUtility.
                GetAdjustedBoxHandleRect(rTopLeft, windowMatrixScale, spriteWindowPos), MouseCursor.ResizeUpLeft);
                
            EditorGUIUtility.AddCursorRect(PixelAnimatorUtility.
                GetAdjustedBoxHandleRect(rTopCenter, windowMatrixScale, spriteWindowPos), MouseCursor.ResizeVertical);

            EditorGUIUtility.AddCursorRect(PixelAnimatorUtility.
                GetAdjustedBoxHandleRect(rTopRight, windowMatrixScale, spriteWindowPos), MouseCursor.ResizeUpRight);
                
            EditorGUIUtility.AddCursorRect(PixelAnimatorUtility.
                GetAdjustedBoxHandleRect(rRightCenter, windowMatrixScale, spriteWindowPos), MouseCursor.ResizeHorizontal);

            EditorGUIUtility.AddCursorRect(PixelAnimatorUtility.
                GetAdjustedBoxHandleRect(rBottomRight, windowMatrixScale, spriteWindowPos), MouseCursor.ResizeUpLeft);

            EditorGUIUtility.AddCursorRect(PixelAnimatorUtility.
                GetAdjustedBoxHandleRect(rBottomCenter, windowMatrixScale, spriteWindowPos), MouseCursor.ResizeVertical);

            EditorGUIUtility.AddCursorRect(PixelAnimatorUtility.
                GetAdjustedBoxHandleRect(rBottomLeft, windowMatrixScale, spriteWindowPos), MouseCursor.ResizeUpLeft);

            EditorGUIUtility.AddCursorRect(PixelAnimatorUtility.
                GetAdjustedBoxHandleRect(rLeftCenter, windowMatrixScale, spriteWindowPos), MouseCursor.ResizeHorizontal);

            EditorGUIUtility.AddCursorRect(adjustedMiddleRect, MouseCursor.MoveArrow);

        }
        

        #region Property
        
        private void DrawHitBoxProperties(int windowID){
            if (targetAnimation == null || selectedAnimation == null || selectedAnimation.Layers.Count <= 0) return;
            
            using var yScroll = new EditorGUILayout.ScrollViewScope(propertyYScrollPos);
            using var xScroll = new EditorGUILayout.ScrollViewScope(propertyXScrollPos);
            targetAnimation.Update();
            EditorGUI.LabelField(propertyWindowRect, "HitBox Properties", EditorStyles.boldLabel);
            EditorGUI.DrawRect( new Rect(7, 30, 300, 2f), new Color(0.3f, 0.3f, 0.3f, 0.6f) );
            GUILayout.Space(30);
            propertyXScrollPos = xScroll.scrollPosition;
            
            using(new GUILayout.HorizontalScope()){        
                GUILayout.Space(20);
                using(new GUILayout.VerticalScope()){
                            
                    var frame = targetAnimation.FindProperty("layers").GetArrayElementAtIndex(activeLayerIndex)
                        .FindPropertyRelative("frames").GetArrayElementAtIndex(activeFrame);
                    if (frame == null) return;
                    GUILayout.Space(20);
                    EditorGUILayout.PropertyField(frame.FindPropertyRelative("colliderType"));
                            

                    using(new GUILayout.HorizontalScope()){

                        EditorGUILayout.LabelField("Box", GUILayout.Width(70));
                        EditorGUILayout.PropertyField(frame.FindPropertyRelative("hitBoxRect"), GUIContent.none, 
                            GUILayout.Width(140), GUILayout.MaxHeight(60));
                    }
                    targetAnimation.ApplyModifiedProperties();
                    
                    var propListenerMethodNames = frame.FindPropertyRelative("hitBoxMethodNames");
                    var propHitBoxData = frame.FindPropertyRelative("hitBoxData");
                    var hitBoxData = selectedAnimation.Layers[activeLayerIndex].frames[activeFrame].hitBoxData;
                    
                    foreach (var prop in preferences.HitBoxProperties) {
                        var single = hitBoxData.FirstOrDefault(x => x.baseData.Guid == prop.mainProperty.Guid).baseData;
                        var selectedIndex = single == null ? -1 : hitBoxData.FindIndex(x => x.baseData == single); 
                        SetPropertyField(prop, propHitBoxData, single, selectedIndex);
                    }

                    SetMethodField(propListenerMethodNames);

                    
                    targetAnimation.ApplyModifiedProperties();
                }
            }
            
        }
        

        private void DrawSpriteProperties(int windowID){
            if (targetAnimation == null || selectedAnimation == null || selectedAnimation.Layers.Count <= 0) return;

            using var yScroll = new EditorGUILayout.ScrollViewScope(propertyYScrollPos);
            using var xScroll = new EditorGUILayout.ScrollViewScope(propertyXScrollPos);
            
            EditorGUI.LabelField(propertyWindowRect, "Sprite Properties", EditorStyles.boldLabel);
            EditorGUI.DrawRect( new Rect(7, 30, 300, 2f), new Color(0.3f, 0.3f, 0.3f, 0.6f) );
            GUILayout.Space(30);
            propertyXScrollPos = xScroll.scrollPosition;
            
            using(new GUILayout.HorizontalScope()){        
                GUILayout.Space(20);
                using(new GUILayout.VerticalScope()) {
                    var frame = targetAnimation.FindProperty("layers").GetArrayElementAtIndex(activeLayerIndex)
                        .FindPropertyRelative("frames").GetArrayElementAtIndex(activeFrame);
                    if (frame == null) return;
                    GUILayout.Space(20);

                    var propListenerMethodNames = frame.FindPropertyRelative("spriteMethodNames");
                    var propSpriteData = frame.FindPropertyRelative("spriteData");
                    var spriteData = selectedAnimation.Layers[activeLayerIndex].frames[activeFrame].spriteData;
                    foreach (var prop in preferences.SpriteProperties) {
                        var single = spriteData.FirstOrDefault(x => x.baseData.Guid == prop.mainProperty.Guid).baseData;
                        var selectedIndex = single == null ? -1 : spriteData.FindIndex(x => x.baseData == single); 
                        SetPropertyField(prop, propSpriteData, single, selectedIndex);
                    }
                    SetMethodField(propListenerMethodNames);
                    targetAnimation.ApplyModifiedProperties();
                }
            }
            
        }
        
        private void DrawPropertyWindow(){
            var tempColor = GUI.color;
            GUI.color = new Color(0, 0, 0, 0.2f);
            switch (propertyFocus) {
                case PropertyFocus.HitBox:
                    GUI.Window(4, new Rect(10, 10, 360, 280), DrawHitBoxProperties, GUIContent.none);
                    break;
                case PropertyFocus.Sprite:
                    GUI.Window(5, new Rect(10, 10, 360, 280), DrawSpriteProperties,
                        GUIContent.none);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            if (propertyWindowRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown) {
                windowFocus = WindowFocus.Property;
            }
            
            GUI.color = tempColor;

        }

        
        private void AddPropertyValue(SerializedProperty propertyValues, PropertyWay propWay){
            propertyValues.InsertArrayElementAtIndex(propertyValues.arraySize);
            propertyValues.serializedObject.ApplyModifiedProperties();
            propertyValues.serializedObject.Update();
            var propertyValue = propertyValues.GetArrayElementAtIndex(propertyValues.arraySize - 1);
            var baseDataProp = propertyValue.FindPropertyRelative("baseData");
            var dataType = propertyValue.FindPropertyRelative("dataType");
            switch (propWay.mainProperty) {
                case ComponentProperty property:
                    dataType.intValue = (int)PixelAnimatorUtility.ToDataType(property.SelectedData.SystemType);
                    baseDataProp.managedReferenceValue =
                        PixelAnimatorUtility.CreateBlankBaseData(
                            PixelAnimatorUtility.ToDataType(property.SelectedData.SystemType));
                    break;
                case ManuelProperty property:
                    dataType.intValue = (int)property.dataType;
                    baseDataProp.managedReferenceValue =
                        PixelAnimatorUtility.CreateBlankBaseData(property.dataType);
                    break;

            }
            baseDataProp.FindPropertyRelative("guid").stringValue = propWay.mainProperty.Guid;
            baseDataProp.FindPropertyRelative("name").stringValue = propWay.mainProperty.Name;
        }
        
        
        private void SetPropertyField(PropertyWay propWay, SerializedProperty propertyValues, BaseData baseData, int index){
            propertyValues.serializedObject.Update();
            var alreadyExist = baseData != null;
            if (alreadyExist) {
                var propertyValue = propertyValues.GetArrayElementAtIndex(index);
                var propBaseData = propertyValue.FindPropertyRelative("baseData");
            }
            using(new GUILayout.HorizontalScope()){
                EditorGUILayout.LabelField(propWay.mainProperty.Name, GUILayout.MaxWidth(100));
                

                if (alreadyExist) {
                    propertyValues.serializedObject.ApplyModifiedProperties();
                }
                else {
                    switch (propWay.mainProperty) {
                        case ComponentProperty property:
                            PixelAnimatorUtility.SystemObjectPreviewField(PixelAnimatorUtility.CreateObject(property.SelectedData.SystemType));        
                            break;
                        case ManuelProperty property:
                            PixelAnimatorUtility.SystemObjectPreviewField(PixelAnimatorUtility.DataTypeToSystemObject(property.dataType));
                            break;
                    }

                    
                }
                
                
                GUILayout.Space(10);
                if(GUILayout.Button("X", GUILayout.MaxWidth(15), GUILayout.MaxHeight(15))){
                    if (alreadyExist) {
                        propertyValues.DeleteArrayElementAtIndex(index);
                    }
                    else {
                        AddPropertyValue(propertyValues, propWay);
                        
                    }

                    propertyValues.serializedObject.ApplyModifiedProperties();


                }
            }

        }

        private void SetMethodField(SerializedProperty listenerMethodNames){
            GUILayout.Space(20);

            methodNameFoldout = EditorGUILayout.Foldout(methodNameFoldout, "Listener Method Names", true);
            if (methodNameFoldout == false) return;
            for (var i = 0; i < listenerMethodNames.arraySize; i ++) {
                var methodName = listenerMethodNames.GetArrayElementAtIndex(i);
                using (new GUILayout.HorizontalScope()) {
                    EditorGUILayout.PropertyField(methodName, GUIContent.none, GUILayout.MaxWidth(140));
                    if (GUILayout.Button("X", GUILayout.MaxWidth(15), GUILayout.MaxHeight(15))) {
                        listenerMethodNames.DeleteArrayElementAtIndex(i);
                        listenerMethodNames.serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            if (GUILayout.Button("Add Method")) {
                listenerMethodNames.arraySize++;
                listenerMethodNames.serializedObject.ApplyModifiedProperties();
            }
            listenerMethodNames.serializedObject.ApplyModifiedProperties();

        }
        
        #endregion

        #region Timeline

        private void SetTimelineRect(){
            var eventCurrent = Event.current;
            
            topTimelineRect = new Rect( timelineRect.xMin, timelineRect.y - 10, timelineRect.xMax - timelineRect.xMin, 10 );
            dragTimelineRect = new Rect(0, topTimelineRect.y + 5, topTimelineRect.width, 10);

            timelineRect.size = new Vector2(position.width + 10, position.height + 100 );
            var clampYPosition = Mathf.Clamp(timelineRect.position.y, 200, position.height - 200);
            timelineRect.position = new Vector2(0, clampYPosition);
            
            EditorGUIUtility.AddCursorRect( dragTimelineRect, MouseCursor.ResizeVertical );
            


            if(draggableTimeline && eventCurrent.type == EventType.MouseDrag &&  !reSizingBox){
                
                if(eventCurrent.mousePosition.y < timelineRect.position.y && Math.Sign(eventCurrent.delta.y) == -1 ) 
                    timeLinePosition = new Vector2(timelineRect.x, clampYPosition + eventCurrent.delta.y);
                
                if(eventCurrent.mousePosition.y > timelineRect.position.y && Mathf.Sign(eventCurrent.delta.y) == 1)
                    timeLinePosition = new Vector2(timelineRect.x, clampYPosition + eventCurrent.delta.y);
                
                timelineRect.position = timeLinePosition;

            }
            
            
            switch (eventCurrent.type) {
                // drag timeline
                case EventType.MouseDrag:{
                    if(dragTimelineRect.Contains(eventCurrent.mousePosition)) draggableTimeline = true;
                    Repaint();
                    break;
                }
                case EventType.MouseUp:
                    draggableTimeline = false;
                    break;
            }

        }
        
        private void CreateTimeline(){
            
            SetTimelineRect();
            timelineRect = GUILayout.Window( 2, timelineRect, TimelineFunction, GUIContent.none);
            if (timelineRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown) {
                windowFocus = WindowFocus.TimeLine;
            }
            
        }

        private void TimelineFunction(int windowID){

            yLayoutLine = new Rect(frontRect.xMax + 15, 10, 5, timelineRect.height);
            
            EditorGUI.DrawRect(new Rect(0,0, timelineRect.width, timelineRect.height), new Color(0.1f,0.1f,0.1f,1));
            EditorGUI.DrawRect(new Rect(0,0, timelineRect.width, 10), blackColor);
            EditorGUI.DrawRect(new Rect(yLayoutLine.xMax, xLayoutLine.yMax, timelineRect.width, timelineRect.height), 
                new Color(0, 0, 0.03f, 1)
                );

            
            // Thumbnail Scale 
            using(var change = new EditorGUI.ChangeCheckScope()){

                thumbNailScale = EditorGUI.FloatField(new Rect(10, 250, 120, 30),thumbNailScale);
                if(change.changed){
                    while (thumbNailScale * greatestSpriteSize.y < 64) {
                        thumbNailScale += 0.1f;
                    }
                    greatestSpriteSize *= thumbNailScale;
                    
                }

            }
            
            xLayoutLine = new Rect(0, greatestSpriteSize.y + 10, timelineRect.width, 8);

            EditorGUI.DrawRect(xLayoutLine, blackColor );
            EditorGUI.DrawRect(yLayoutLine, blackColor);
            

            DrawTimelineButtons();
            if (selectedAnimation == null) return;
            if (selectedAnimation.GetSpriteList().Count > 0) {
                SetFrameThumbnail();
                if (selectedAnimation.Layers.Count > 0 ) {
                    DrawLayer();
                    SetFrames();
                    SetLayerMenu();
                    // SetFrameCopyPaste();
                        
                }
            }

            SetPlayKeys();
            SetLayerKeys();
        }

        private void DrawTimelineButtons(){
            if(!isPlaying)playT = (Texture2D)AssetDatabase.LoadAssetAtPath(ResourcesPath + "Play.png", typeof(Texture2D)) ;
            else playT = (Texture2D)AssetDatabase.LoadAssetAtPath(ResourcesPath + "Playing.png", typeof(Texture2D));
            using(new GUILayout.HorizontalScope()){

                if(PixelAnimatorUtility.Button(defAddGroupsT, onMouseAddGroupsT, addGroupsRect )){
                    if(boxTypePopup != null) boxTypePopup.ShowAsContext();
                }
                else if(PixelAnimatorUtility.Button(backT, backRect)) {
                    if (selectedAnimation == null) return;
                        switch (activeFrame) {
                            case 0:
                                var index = selectedAnimation.GetSpriteList().Count - 1; 
                                activeFrame = index;
                                break;
                            case (>0):
                                activeFrame--;
                                break;
                        }
                        Repaint();
                }
                else if(PixelAnimatorUtility.Button(playT, playRect)){
                    if (selectedAnimation == null) return;
                    isPlaying = !isPlaying;
                    playT = (Texture2D)AssetDatabase.LoadAssetAtPath(ResourcesPath + "Playing.png", typeof(Texture2D));
                    Repaint();
                }
                else if(PixelAnimatorUtility.Button(frontT, frontRect)){
                    if (selectedAnimation == null) return;
                    activeFrame = (activeFrame + 1) % selectedAnimation.GetSpriteList().Count;
                    Repaint();
                }

            }
        } 

        
        private void SetAddLayerPopup(){
            settingsPopup = new GenericMenu(){allowDuplicateNames = true};
            boxTypePopup = new GenericMenu(){allowDuplicateNames = true};

            
            boxTypePopup.AddItem(new GUIContent("Go to Preferences"), false, () => {
                Selection.activeObject = preferences;

            });
            
            boxTypePopup.AddItem(new GUIContent("Update Animation"), false, () => {
                foreach (var frame in selectedAnimation.Layers.SelectMany(layer => layer.frames)) {
                    for (var i = 0; i < frame.hitBoxData.Count; i++) {
                        var myGuid = frame.hitBoxData[i].baseData.Guid;
                        var exist = preferences.HitBoxProperties.Any(x => x.mainProperty.Guid == myGuid);
                        if (exist) continue;
                        frame.hitBoxData.Remove(frame.hitBoxData[i]);
                        i = -1;
                    }
                }
            });
            
            boxTypePopup.AddSeparator("");
            var groups = preferences.Groups;


            settingsPopup.AddItem(new GUIContent("Settings/Delete"), false, () => {
                    targetAnimation.Update();
                    targetAnimation.FindProperty("layers").DeleteArrayElementAtIndex(activeLayerIndex);
                    targetAnimation.ApplyModifiedProperties();
                });

            for(var i = 0; i < preferences.Groups.Count; i ++) {


                boxTypePopup.AddItem(new GUIContent(groups[i].boxType), false, (userData) => {
                    targetAnimation.Update();
                    var group = (Group)userData;

                    if (selectedAnimation.Layers.Any(x => x.Guid == group.Guid)) {
                        Debug.LogError("This group has already been added! Please add another group.");
                        return;
                    }
                    selectedAnimation.AddLayer(group.Guid);

                }, groups[i]);
                

            }

        }


        private void SetFrameThumbnail(){

            var eventCurrent = Event.current;
            
            greatestSpriteSize = GetGreatestSpriteSize();

            // First y axis line rect
            yLineRect[0] = new Rect(yLayoutLine.xMax + greatestSpriteSize.x, 10, 5, greatestSpriteSize.y + xLayoutLine.height);
            for(var i = 0; i < selectedAnimation.GetSpriteList().Count; i++){
                if(i>0 && i != yLineRect.Length-1)
                    yLineRect[i] = new Rect(yLineRect[i-1].xMax + greatestSpriteSize.x, 10, 5, greatestSpriteSize.y + xLayoutLine.height);

                if (i == yLineRect.Length - 1)
                    yLineRect[i] = new Rect(yLineRect[i - 1].xMax + greatestSpriteSize.x, 10, 5,
                        greatestSpriteSize.y + xLayoutLine.height + (selectedAnimation.Layers.Count * 45));
                    
                if(i != selectedAnimation.GetSpriteList().Count){
                    var sprite = selectedAnimation.GetSpriteList()[i];

                    var width = sprite.rect.width;
                    var height = sprite.rect.height;
                    var adjustedSpriteWidth  = width* thumbNailScale;
                    var adjustedSpriteHeight = height * thumbNailScale ;
                    var adjustedSpriteXPos   = (yLineRect[i].xMin - greatestSpriteSize.x) + (greatestSpriteSize.x/2 - adjustedSpriteWidth/2) ;
                    var adjustedSpriteYPos   = ( greatestSpriteSize.x/2 - adjustedSpriteHeight/2 ) + 10;

                    

                    // Set sprite x pos
                    adjustedSpriteXPos = (yLineRect[i].xMin - greatestSpriteSize.x) + (greatestSpriteSize.x/2 - adjustedSpriteWidth/2); 

                    // Set sprite y pos
                    adjustedSpriteYPos = ( greatestSpriteSize.y/2 - adjustedSpriteHeight/2 ) + 10;

                        
                    var spriteRect = new Rect(adjustedSpriteXPos, adjustedSpriteYPos, adjustedSpriteWidth, adjustedSpriteHeight);
                    var transparentRect = new Rect( yLineRect[i].xMin - greatestSpriteSize.x , yLineRect[i].yMin, greatestSpriteSize.x, timelineRect.height);
                    var spriteSelectableRect = new Rect(adjustedSpriteXPos, adjustedSpriteYPos, greatestSpriteSize.x, greatestSpriteSize.y);


                
                    GUI.DrawTexture(spriteRect, AssetPreview.GetAssetPreview(sprite));
                    
                    if(activeFrame == i){
                        EditorGUI.DrawRect(transparentRect, new Color(255, 255, 255, 0.2f));
                        Repaint();
                    }
                    if (spriteSelectableRect.Contains(eventCurrent.mousePosition) && eventCurrent.button == 0 &&
                        eventCurrent.type == EventType.MouseDown) {
                        propertyFocus = PropertyFocus.Sprite;
                        activeFrame = i;
                        Repaint();
                    } 
                    
                }
                EditorGUI.DrawRect(yLineRect[i], blackColor);
            }

        }
        
        
        private void DrawLayer(){
            
            var width = yLayoutLine.xMin - xLayoutLine.xMin;
            const int height = 45;
            const int lineHeight = 5;
            
            var layers = selectedAnimation.Layers;
            
            
            for(var i = 0; i < layers.Count; i++) {
                var group = preferences.GetGroup(layers[i].Guid);

                if (i == groupRects.Count) {
                    groupRects.Add(new GroupRects());
                }

                var offColor = group.color * Color.gray;

                var moreOffColor = new Color(group.color.r/3, group.color.g/3, group.color.b/3 , 1f);

                
                var yPos = i == 0 ? xLayoutLine.yMax : xLayoutLine.yMax + ( i * 45 );
                var bodyRect = new Rect(xLayoutLine.xMin, yPos, width, height - 5);
                var settingsRect = new Rect(xLayoutLine.xMin, yPos, height - 5, height - 5);
                var bottomLine = new Rect(yLayoutLine.xMax, yPos + height - 5, yLineRect[^1].xMin - yLayoutLine.xMax, lineHeight);
                groupRects[i] = new GroupRects(bodyRect, settingsRect, bottomLine);


                EditorGUI.DrawRect(bodyRect, offColor);
                EditorGUI.DrawRect(settingsRect, group.color);
                EditorGUI.DrawRect(bottomLine, blackColor);
                EditorGUI.DrawRect(groupRects[i].Parting, moreOffColor);
                var settingsTexture =
                    (Texture2D)AssetDatabase.LoadAssetAtPath(ResourcesPath + "settings.png", typeof(Texture2D));
                
                GUI.DrawTexture(settingsRect, (Texture2D)AssetDatabase.LoadAssetAtPath( ResourcesPath + "settings.png", typeof(Texture2D)) );
                var evtCurrent = Event.current;
                if (evtCurrent.button is 0 or 1 && evtCurrent.type is EventType.MouseDown) {
                    if (groupRects[i].bodyRect.Contains(evtCurrent.mousePosition)) {
                        activeLayerIndex = i;
                        Repaint();
                    }
                    
                }
                
                var tempColor = GUI.color;
                GUI.color = group.color * 1.5f; 
                EditorGUI.LabelField(new Rect(groupRects[i].settingsRect.xMax + 10, groupRects[i].settingsRect.yMin + lineHeight/2 + 5, width, 30), group.boxType);
                GUI.color = tempColor;



            }

        }

        
        private void SetLayerMenu(){
            var eventCurrent = Event.current;
            var layers = selectedAnimation.Layers;

            for (var i = 0; i < layers.Count; i++) {
                var group = preferences.GetGroup(layers[i].Guid);
                
                // Set popup
                if(PixelAnimatorUtility.Button(groupRects[i].settingsRect, group.color)){
                    settingsPopup.ShowAsContext();
                    
                }
                if (groupRects[i].bodyRect.Contains(eventCurrent.mousePosition) &&
                    eventCurrent.type == EventType.MouseDown && eventCurrent.button == 1) {
                    settingsPopup.ShowAsContext();
                }
            }
            
            
        }

        

        private void SetFrames(){
            var eventCurrent = Event.current; 
            var layers = selectedAnimation.Layers;
            selectFrameRect = new Rect[layers.Count, selectedAnimation.GetSpriteList().Count];
            const int frameTextureSize = 16;

            while(frameRects.Count < selectedAnimation.GetSpriteList().Count){
                frameRects.Add(new Rect());
            }
        
            for(var f = 0; f < selectedAnimation.GetSpriteList().Count; f ++){
                for(var i = 0; i < layers.Count; i++) {

                    var yLineXMin = yLineRect[f].xMin;
                    var bottomLine = groupRects[i].bottomLine;
                    var bodyRect = groupRects[i].bodyRect;
                    
                    var width =  yLineXMin - (yLineXMin - greatestSpriteSize.x);
                    var height = bottomLine.yMin - bodyRect.yMin;


                    var yHalfPos = ( bodyRect.yMin + height/2 ) - frameTextureSize/2;           
                    var xHalfPos = (yLineXMin - greatestSpriteSize.x  + (yLineXMin - (yLineXMin - greatestSpriteSize.x))/2 ) - frameTextureSize/2;

                    frameRects[f] = new Rect(xHalfPos, yHalfPos, frameTextureSize, frameTextureSize);
                    
                    selectFrameRect[i, f] = new Rect(yLineXMin - greatestSpriteSize.x, bottomLine.yMin - height, width, height);
                    GUI.DrawTexture(frameRects[f], singleFrameT);


                    switch (eventCurrent.type) {
                        case EventType.MouseDown when eventCurrent.button == 0 && selectFrameRect[i, f].Contains(eventCurrent.mousePosition):
                            activeFrame = f;
                            propertyFocus = PropertyFocus.HitBox;
                            activeLayerIndex = i;
                            Repaint();
                            break;
                        case EventType.MouseDrag when eventCurrent.button == 0 && selectFrameRect[i, f].Contains(eventCurrent.mousePosition):
                            break;
                    }



                    var topLeft = new Rect( yLineRect[activeFrame].xMin - greatestSpriteSize.x, groupRects[i].bodyRect.yMin, 15, 15 );
                    var topRight = new Rect( yLineRect[activeFrame].xMin - 15, groupRects[i].bodyRect.yMin, 15, 15 );
                    var bottomLeft = new Rect( yLineRect[activeFrame].xMin - greatestSpriteSize.x, groupRects[i].bottomLine.yMin - 15, 15, 15 );
                    var bottomRight = new Rect( yLineRect[activeFrame].xMin - 15, groupRects[i].bottomLine.yMin - 15, 15, 15 );
                    
                    if (activeLayerIndex != i) continue;
                    GUI.DrawTexture(topLeft, (Texture2D)AssetDatabase.LoadAssetAtPath( ResourcesPath + "Top Left.png", typeof(Texture2D)) );
                    GUI.DrawTexture(topRight, (Texture2D)AssetDatabase.LoadAssetAtPath( ResourcesPath + "Top Right.png", typeof(Texture2D)));
                    GUI.DrawTexture(bottomLeft, (Texture2D)AssetDatabase.LoadAssetAtPath( ResourcesPath + "Bottom Left.png", typeof(Texture2D)));
                    GUI.DrawTexture(bottomRight, (Texture2D)AssetDatabase.LoadAssetAtPath( ResourcesPath + "Bottom Right.png", typeof(Texture2D)));


                }
            }
        }

        private void SetFrameCopyPaste(){
            if (windowFocus != WindowFocus.TimeLine || propertyFocus != PropertyFocus.HitBox) return;
            var eventCurrent = Event.current;
            if (eventCurrent.type == EventType.ValidateCommand && eventCurrent.commandName == "Copy")
                eventCurrent.Use();
            

            if (eventCurrent.type == EventType.ExecuteCommand && eventCurrent.commandName == "Copy") {
                EditorGUIUtility.systemCopyBuffer =
                    JsonUtility.ToJson(selectedAnimation.Layers[activeLayerIndex].frames[activeFrame]);
                Debug.Log(EditorGUIUtility.systemCopyBuffer);
            }
            
            if (eventCurrent.type == EventType.ValidateCommand && eventCurrent.commandName == "Paste")
                eventCurrent.Use();
            

            if (eventCurrent.type == EventType.ExecuteCommand && eventCurrent.commandName == "Paste") {
                var copiedFrame = JsonUtility.FromJson<Frame>(EditorGUIUtility.systemCopyBuffer);
                
                var frameProp = targetAnimation.FindProperty("layers").GetArrayElementAtIndex(activeLayerIndex)
                    .FindPropertyRelative("frames").GetArrayElementAtIndex(activeFrame);
                
                var hitBoxRectProp = frameProp.FindPropertyRelative("hitBoxRect");
                var colliderType = frameProp.FindPropertyRelative("colliderType");

                colliderType.enumValueIndex = (int)copiedFrame.colliderType;
                hitBoxRectProp.rectValue = copiedFrame.hitBoxRect;
                targetAnimation.ApplyModifiedProperties();

            }
        }

        #endregion


        #region Common
        private Vector2 GetGreatestSpriteSize(){
            var greatestX = selectedAnimation.GetSpriteList().Aggregate((current, next) => 
                current.rect.size.x > next.rect.size.x ? current : next).rect.size.x;
            
            var greatestY = selectedAnimation.GetSpriteList().Aggregate((current, next) => 
                current.rect.size.y > next.rect.size.y ? current : next).rect.size.y;
            var adjustedX = greatestX * thumbNailScale;
            var adjustedY = greatestY * thumbNailScale;

            return new Vector2(adjustedX, adjustedY);
        }
        private void SetPlayKeys(){
            var eventCurrent = Event.current;

            if (!eventCurrent.isKey || eventCurrent.type != EventType.KeyDown) return;
            var keyCode = eventCurrent.keyCode;
            switch (keyCode) {
                case KeyCode.Return:
                    isPlaying = !isPlaying;
                    break;
                case KeyCode.RightArrow:
                    activeFrame = (activeFrame + 1) % frameCount;
                    break;
                case KeyCode.LeftArrow when activeFrame != 0:
                    activeFrame--;
                    break;
                case KeyCode.LeftArrow when activeFrame == 0:
                    activeFrame = frameCount - 1;
                    break;
            }
        }

        private void SetLayerKeys(){
            var eventCurrent = Event.current;

            if (!eventCurrent.isKey || eventCurrent.type != EventType.KeyDown) return;
            var keyCode = eventCurrent.keyCode;

            switch (keyCode) {
                case KeyCode.UpArrow when activeLayerIndex == 0:
                    activeLayerIndex = selectedAnimation.Layers.Count - 1;
                    break;
                case KeyCode.UpArrow:
                    activeLayerIndex--;
                    break;
                case KeyCode.DownArrow when selectedAnimation.Layers.Count > 0:
                    activeLayerIndex = (activeLayerIndex + 1) % selectedAnimation.Layers.Count;
                    break;
            }

        }
        
        private void Play(){

            timer += (editorDeltaTime * selectedAnimation.frameRate);
            
            if(timer >= 1f){
                timer -= 1f;
                activeFrame = (activeFrame + 1) % selectedAnimation.GetSpriteList().Count;
                if(selectedAnimation.GetSpriteList().Count > 1)playT = (Texture2D)AssetDatabase.
                    LoadAssetAtPath(ResourcesPath + "Playing.png", typeof(Texture2D));
            }
            Repaint();
        }

        private void SetEditorDeltaTime(){

            if(lastTimeSinceStartup == 0f){
                lastTimeSinceStartup = (float)EditorApplication.timeSinceStartup;
            }
            
            editorDeltaTime = (float)(EditorApplication.timeSinceStartup - lastTimeSinceStartup);
            lastTimeSinceStartup = (float)EditorApplication.timeSinceStartup;
            
            
        }


        private void Update() {
            if(isPlaying) Play();
            else timer = 0;
            SetEditorDeltaTime();
        }
        
        private void SetZoom(){
            var eventCurrent = Event.current;
            // Change matrix thumbNailScale for mouse wheel move.
            layoutZoomFactor = timelineRect.position.y/200;
            if(eventCurrent.type == EventType.ScrollWheel){
                float scaleValue = Mathf.Sign(eventCurrent.delta.y) == 1 ? -1 : 1;
                
                scrollWhellZoom += scaleValue;
                scrollWhellZoom = Mathf.Clamp(scrollWhellZoom, 1f, 4);
                Repaint();

            }
        }


        private void SelectedObject(){
            foreach(var obj in Selection.objects) {
                if (obj is not PixelAnimation anim) continue;
                targetAnimation = new SerializedObject(anim);

                if (selectedAnimation == anim) continue;
                    
                if (anim.GetSpriteList() != null) {
                    frameCount = anim.GetSpriteList().Count;
                    yLineRect = new Rect[frameCount];
                        
                }

                timer = 0;
                selectedAnimation = anim;
                thumbNailScale = 1;
                activeFrame = 0;
                while (GetGreatestSpriteSize().x * thumbNailScale > 64) {
                    thumbNailScale -= 0.1f;
                }

            }

        }

            
        #endregion
    }



    public struct GroupRects{

        public Rect bodyRect;
        public Rect settingsRect;
        public Rect bottomLine;
        public Rect Parting => new(bodyRect.xMin, bodyRect.yMax, bodyRect.width, 5);


        public GroupRects(Rect bodyRect, Rect settingsRect, Rect bottomLine){
            this.bodyRect = bodyRect;
            this.settingsRect = settingsRect;
            this.bottomLine = bottomLine;



        }

    }
}

    


