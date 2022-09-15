using System.Collections.Generic;
using System.Linq;
using binc.PixelAnimator.Elements;
using binc.PixelAnimator.PropertyData;
using UnityEditor;
using UnityEngine;


namespace binc.PixelAnimator.Preferences{


    [CreateAssetMenu(menuName = "Pixel Animator/ Preferences")]
    
    public class PixelAnimatorPreferences : ScriptableObject{
        [SerializeField] private List<PropertyWay> spriteProperties;
        [SerializeField] private List<PropertyWay> hitBoxProperties;
        [SerializeField] private List<Group> groups;
        
        public List<PropertyWay> SpriteProperties => spriteProperties;
        public List<PropertyWay> HitBoxProperties => hitBoxProperties;
        public List<Group> Groups => groups;
        

        public enum FrameType {HitBox, Sprite }

        public Group GetGroup(string guid){
            return groups.First(x => x.Guid == guid);
        }
        
        
        public List<MainProperty> GetPropertyList(FrameType frameType){
            return frameType == FrameType.Sprite ? 
                spriteProperties.Select(x => x.mainProperty).ToList() 
                : hitBoxProperties.Select(x => x.mainProperty).ToList();
        }
        

    }

}