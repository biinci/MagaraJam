using System.Collections.Generic;
using UnityEngine;
using binc.PixelAnimator.PropertyData;



namespace binc.PixelAnimator.Elements{

    [System.Serializable]
    public class Frame{
        
        [SerializeField, ReadOnly]
        public string SpriteId;

        public enum ColliderTypes{NoTrigger, Trigger}
        public List<PropertyValue> hitBoxData;
        public List<PropertyValue> spriteData;
        public List<string> spriteMethodNames;
        public List<string> hitBoxMethodNames;
        public ColliderTypes colliderType;
        
        public Rect hitBoxRect = new(16, 16, 16, 16);

        public Frame(string guid){
            SpriteId = guid;
            hitBoxData = new List<PropertyValue>();

        }


    }


    [System.Serializable]
    public class PixelSprite{
        [field: ReadOnly] [field: SerializeField]
        public string SpriteId;

        public Sprite sprite;

        public PixelSprite(Sprite sprite, string gUid){
            this.sprite = sprite;

            SpriteId = gUid;
        }
    }






}
