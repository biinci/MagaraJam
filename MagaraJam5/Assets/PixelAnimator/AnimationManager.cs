using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using binc.PixelAnimator.Elements;
using binc.PixelAnimator.Common;
using binc.PixelAnimator.Preferences;
using binc.PixelAnimator.Utility;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace binc.PixelAnimator{

    public class AnimationManager : MonoBehaviour{
        public PixelAnimation CurrentAnimation{ get; private set; }

        public int ActiveFrame => activeFrame;

        [SerializeField] private int activeFrame;

        [SerializeField] private SpriteRenderer spriteRenderer;

        private float timer;

        private GameObject baseObject;
        [SerializeField] private List<GameObject> gameObjects;

        private PixelAnimatorPreferences preferences;

        private readonly Dictionary<string, PixelAnimatorListener> listeners = new();

        private readonly Dictionary<string, Action<object>> applyPropertyMethods = new();
        private List<Group> groups;

        private void Awake(){
            #if UNITY_EDITOR
            preferences = (PixelAnimatorPreferences)AssetDatabase.LoadAssetAtPath("Assets/PixelAnimator/Preferences/PixelAnimatorPreferences.asset", 
                                                    typeof(PixelAnimatorPreferences));
            #endif
            baseObject = new GameObject("Pixel Animator Colliders"){
                transform ={
                    parent = transform,
                    localPosition = Vector3.zero
                }
            };
            groups = preferences.Groups;
            gameObjects ??= new List<GameObject>();
        }


        private void Update() {
            if(CurrentAnimation != null){
                Play();
            }
            

        }

        private void Play(){
            var sprites = CurrentAnimation.GetSpriteList();
            var frameRate = CurrentAnimation.frameRate;
            var loop = CurrentAnimation.loop;

            timer += Time.deltaTime;
            var secondsPerFrame = 1/ frameRate;
            if (!(timer >= secondsPerFrame)) return;
            timer -= secondsPerFrame;


            if (loop) {
                activeFrame = (activeFrame + 1) % sprites.Count;
                ApplySpriteProperty();
                
            }
            else{
                if (spriteRenderer.sprite != sprites[^1]) {
                    activeFrame = (activeFrame + 1) % sprites.Count;
                    ApplySpriteProperty();
                }
            }
            
            spriteRenderer.sprite = sprites[activeFrame];
            ApplyHitBox();
        }

        public void ChangeAnimation(PixelAnimation newAnimation){
            
            if (CurrentAnimation == newAnimation) return;
            CurrentAnimation = newAnimation;
            activeFrame = 0;
            timer = 0;
            
                        
            if (baseObject.transform.childCount > 0) {
                foreach (Transform child in baseObject.transform) {
                    foreach (var layer in newAnimation.Layers) {
                        if (gameObjects.Any(x => x.name != preferences.GetGroup(layer.Guid).boxType)) {
                            Destroy(child.gameObject);
                            AddGameObject(groups.First(x => x.Guid == layer.Guid));
                        }
                    }

                }
            }
            else {
                foreach (var layer in CurrentAnimation.Layers) {
                    AddGameObject(groups.First(x => x.Guid == layer.Guid));
                }
            }
            ApplyHitBox();

        }



        private void ApplySpriteProperty(){
            foreach (var layer in CurrentAnimation.Layers) {
                foreach (var spriteMethodName in layer.frames[activeFrame].spriteMethodNames) {
                    if (listeners.ContainsKey(spriteMethodName)) {
                        listeners[spriteMethodName].Invoke();
                    }
                    else {
                        Debug.LogWarning($"Method name '{spriteMethodName}' does not exist");
                    }
                }
                foreach (var value in layer.frames[activeFrame].spriteData) {
                    foreach (var pair in applyPropertyMethods.Where(pair => value.baseData.Name == pair.Key)) {
                        applyPropertyMethods[pair.Key].Invoke(value.baseData.GetData());
                    }
                }
            }
        }
        
        public void SetProperty(string name, Action<object> action){
            applyPropertyMethods.Add(name, action);
        }



        private void ApplyHitBox(){
            var layers = CurrentAnimation.Layers;
            var sprites = CurrentAnimation.GetSpriteList();

            if (layers.Count <= 0) return;
            
            for(var i = 0; i < layers.Count; i++){

                    
                var layer = layers.FirstOrDefault(x => x.Guid.ToString() == preferences.GetGroup(x.Guid).Guid);
                    
                // Trigger
                if (layer == null || i >= gameObjects.Count) continue;
                    
                var boxCol = gameObjects[i].GetComponent<BoxCollider2D>();

                boxCol.isTrigger = layer.frames[activeFrame].colliderType switch{
                    Frame.ColliderTypes.NoTrigger => false,
                    _ => true
                };

                // Set hitBox size
                var spriteRect = sprites[activeFrame].rect;
                var size = layer.frames[activeFrame].hitBoxRect.size;
                var offset = layer.frames[activeFrame].hitBoxRect.position;
                var adjustedHitBoxPivot = new Vector2(offset.x + size.x / 2, size.y / 2 + offset.y);


                var adjustedXSize = size.x * sprites[activeFrame].bounds.size.x/spriteRect.width;
                var adjustedXOffset = (adjustedHitBoxPivot.x - sprites[activeFrame].pivot.x) *
                    sprites[activeFrame].bounds.size.x / spriteRect.width;
                
                var adjustedYOffset = (adjustedHitBoxPivot.y -  (spriteRect.height - sprites[activeFrame].pivot.y)) * -1 *
                    sprites[activeFrame].bounds.size.y / spriteRect.height;


                if (!boxCol.enabled) boxCol.enabled = true;
                boxCol.size = new Vector2(adjustedXSize, (size.y * sprites[activeFrame].bounds.size.y)/spriteRect.height );
                boxCol.offset = new Vector2( adjustedXOffset, adjustedYOffset);
            }
        }
        

        private void AddGameObject(Group activeGroup){
            var alreadyExist = gameObjects.Any(x => x.name == activeGroup.boxType);
            if (alreadyExist) return;
            gameObjects.Add(new GameObject(activeGroup.boxType));
            var obj = gameObjects[^1];
            obj.AddComponent<BoxCollider2D>();
            obj.GetComponent<BoxCollider2D>().enabled = false;
            obj.transform.parent = baseObject.transform;
            obj.transform.localPosition = Vector3.zero;
            obj.layer = activeGroup.activeLayer;
            ApplyHitBox();
        }


        #region Set Collider Detect
        
        // private void OnTriggerEnter2D(Collider2D other){
        //     OnTouch(other, Frame.ColliderTypes.Trigger, Group.ColliderDetection.Enter);
        // }
        //
        //
        // private void OnTriggerStay2D(Collider2D other) {
        //     OnTouch(other, Frame.ColliderTypes.Trigger, Group.ColliderDetection.Stay);
        //
        // }
        //
        // private void OnTriggerExit2D(Collider2D other) {
        //     OnTouch(other, Frame.ColliderTypes.Trigger, Group.ColliderDetection.Exit);
        //     
        // }
        //
        // private void OnCollisionEnter2D(Collision2D other) {
        //     OnTouch(other, Frame.ColliderTypes.NoTrigger, Group.ColliderDetection.Enter);
        // }
        //
        // private void OnCollisionStay2D(Collision2D other) {
        //     OnTouch(other, Frame.ColliderTypes.NoTrigger, Group.ColliderDetection.Stay);
        //     
        // }
        //
        // private void OnCollisionExit2D(Collision2D other) {
        //     OnTouch(other, Frame.ColliderTypes.NoTrigger, Group.ColliderDetection.Exit);
        //     
        // }
        

        private void OnTouch(Collider2D other, Frame.ColliderTypes colliderType, Group.ColliderDetection colliderDetection){
            var layers = CurrentAnimation.Layers;
            for(var i = 0; i < gameObjects.Count; i ++) {
                var group = GetGroupForIndex(i);
                var frame = layers[i].frames[activeFrame];
                var boxCollider2D = gameObjects[i].GetComponent<BoxCollider2D>();

                if (group.colliderDetection != colliderDetection || frame.colliderType != colliderType) continue;
                if (!boxCollider2D.IsTouching(other)) continue;
                
                if(other.gameObject.layer == group.collisionLayer){
                    
                }

            }

        }
        private void OnTouch(Collision2D other, Frame.ColliderTypes colliderType, Group.ColliderDetection colliderDetection){
            var layers = CurrentAnimation.Layers;
            for(var i = 0; i < gameObjects.Count; i ++) {
                var group = GetGroupForIndex(i);
                if (group.colliderDetection != colliderDetection ||
                    layers[i].frames[activeFrame].colliderType != colliderType) continue;
                if(other.gameObject.layer == group.collisionLayer){
                    
                }

            }

        }

        #endregion
        
        

        public void AddListener(string eventName, PixelAnimatorListener listener){
            if(listeners.ContainsKey(eventName))
                listeners[eventName] += listener;
            else
                listeners[eventName] = listener;
        }

        public void RemoveListener(string eventName, PixelAnimatorListener listener){
            if(!listeners.ContainsKey(eventName)) return;

            listeners[eventName] -= listener;
            if(listeners[eventName] == null)
                listeners.Remove(eventName);
        }



        private Group GetGroupForIndex(int index){
            return preferences.GetGroup(CurrentAnimation.Layers[index].Guid);
        }
        


    }
}
