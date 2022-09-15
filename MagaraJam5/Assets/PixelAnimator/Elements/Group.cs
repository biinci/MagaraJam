using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace binc.PixelAnimator.Elements{

    [System.Serializable]
    public struct Group{
        public Color color;
        
        public string boxType;
        [Tooltip("LayerMask Index")]
        public int activeLayer;
        public PhysicsMaterial2D physicMaterial;
        public bool rounded;
        public int collisionLayer;
        public enum ColliderDetection{Enter, Stay, Exit}
        public ColliderDetection colliderDetection;
        
        [ReadOnly, SerializeField]
        private string guid;

        public string Guid => guid;

    }



}