using UnityEngine;
using System;
using Object = UnityEngine.Object;



namespace binc.PixelAnimator.PropertyData{

    #region Blind

    


    public enum PropertyType{
        Manuel,
        Component

    }

    public enum ComponentWay{
        Parent, 
        Manuel
    }
    public enum FourTransactions{
        Equal,
        Subtraction, 
        Addition,
        Multiplication,
        Division
    }

    
    public enum DataType{
        IntData,
        StringData,
        BoolData,
        FloatData,
        DoubleData,
        LongData,
        RectData,
        RectIntData,
        ColorData,
        AnimationCurveData,
        BoundsData,
        BoundsIntData,
        Vector2Data,
        Vector3Data,
        Vector4Data,
        Vector2INTData,
        Vector3INTData,
        UnityObjectData,
        GradientData,

    }

    #endregion

    [Serializable]
    public class BaseData{
        [ReadOnly, SerializeField]
        private string name;
        [ReadOnly, SerializeField]
        private string guid;

        public string Name => name;
        public string Guid => guid;
        public BaseData(string guid, string name){
            this.guid = guid;
            this.name = name;
        }
        public BaseData(){
        }

        public void SetGuid(string guid){
            this.guid = guid;
        }

        public void SetName(string name){
            this.name = name;
        }

    }
    
    [Serializable]
    public struct PropertyValue{
        [SerializeReference] public BaseData baseData;
        public DataType dataType;

        
    }

    [Serializable]
    public class PropertyWay{
        [SerializeReference] public MainProperty mainProperty;
        public PropertyType propertyType;
    }
    

    #region DataClasses 
    
    [Serializable]
    public class IntData : BaseData{
        [SerializeField] private int data;
        public int Data => data;
        public IntData(string guid, string name) : base(guid, name){
        }

        public IntData(){
            
        }
        
    }
    [Serializable]
    public class StringData : BaseData{
        [SerializeField] private string data;
        public string Data => data;
        public StringData(string guid, string name) : base(guid, name){
        }

        public StringData(){
            
        }
        
    }

    [Serializable]
    public class BoolData : BaseData{
        [SerializeField] private bool data;
        public bool Data => data;
        public BoolData(string guid, string name) : base(guid, name){
        }

        public BoolData(){
            
        }
    }
    
    [Serializable]
    public class FloatData : BaseData{
        [SerializeField] private float data;
        public float Data => data;
        public FloatData(string guid, string name) : base(guid, name){
        }

        public FloatData(){
            
        }
    }
    
    
    [Serializable]
    public class DoubleData : BaseData{
        [SerializeField] private double data;
        public double Data => data;
        public DoubleData(string guid, string name) : base(guid, name){
        }

        public DoubleData(){
            
        }
    }

    
    [Serializable]
    public class LongData : BaseData{
        [SerializeField] private long data;
        public long Data => data;
        public LongData(string guid, string name) : base(guid, name){
        }

        public LongData(){
            
        }
    }
    
    public class RectData : BaseData{
        [SerializeField] private Rect data;
        public Rect Data => data;
        public RectData(string guid, string name) : base(guid, name){
        }

        public RectData(){
            
        }
    }
    
    public class RectIntData : BaseData{
        [SerializeField] private RectInt data;
        public RectInt Data => data;
        public RectIntData(string guid, string name) : base(guid, name){
        }

        public RectIntData(){
            
        }
    }
    
    [Serializable]
    public class ColorData : BaseData{
        [SerializeField] private Color data;
        public Color Data => data;
        public ColorData(string guid, string name) : base(guid, name){
        }

        public ColorData(){
            
        }
    }
    
    [Serializable]
    public class AnimationCurveData : BaseData{
        [SerializeField] private AnimationCurve data;
        public AnimationCurve Data => data;
        public AnimationCurveData(string guid, string name) : base(guid, name){
        }

        public AnimationCurveData(){
            
        }
    }
    
    [Serializable]
    public class BoundsData : BaseData{
        [SerializeField] private Bounds data;
        public Bounds Data => data;
        public BoundsData(string guid, string name) : base(guid, name){
        }

        public BoundsData(){
            
        }
    }
    [Serializable]
    public class BoundsIntData : BaseData{
        [SerializeField] private BoundsInt data;
        public BoundsInt Data => data;
        public BoundsIntData(string guid, string name) : base(guid, name){
        }

        public BoundsIntData(){
            
        }
    }
    
    
    [Serializable]
    public class Vector2Data : BaseData{
        [SerializeField] private Vector2 data;
        public Vector2 Data => data;
        public Vector2Data(string guid, string name) : base(guid, name){
        }

        public Vector2Data(){
            
        }
    }
    
    [Serializable]
    public class Vector3Data : BaseData{
        [SerializeField] private Vector3 data;
        public Vector3 Data => data;
        public Vector3Data(string guid, string name) : base(guid, name){
        }

        public Vector3Data(){
            
        }
    }
    
    [Serializable]
    public class Vector4Data : BaseData{
        [SerializeField] private Vector4 data;
        public Vector4 Data => data;
        public Vector4Data(string guid, string name) : base(guid, name){
        }

        public Vector4Data(){
            
        }
    }
    
    
    [Serializable]
    public class Vector2IntData : BaseData{
        [SerializeField] private Vector2Int data;
        public Vector2Int Data => data;
        public Vector2IntData(string guid, string name) : base(guid, name){
        }

        public Vector2IntData(){
            
        }
    }
    
    [Serializable]
    public class Vector3IntData : BaseData{
        [SerializeField] private Vector3Int data;
        public Vector3Int Data => data;
        public Vector3IntData(string guid, string name) : base(guid, name){
        }

        public Vector3IntData(){
            
        }
    }
    
    [Serializable]
    public class UnityObjectData : BaseData{
        [SerializeField] private Object data;
        public Object Data => data;
        public UnityObjectData(string guid, string name) : base(guid, name){
        }

        public UnityObjectData(){
            
        }
    }
    
    [Serializable]
    public class GradientData : BaseData{
        [SerializeField] private Gradient data;
        public Gradient Data => data;
        public GradientData(string guid, string name) : base(guid, name){
        }

        public GradientData(){
            
        }
    }
    #endregion
    
    [Serializable]
    public class MainProperty{
        [SerializeField] private string name;
        public string Name => name;
        
        [SerializeField] private string sourceName;
        public string SourceName => sourceName;
        
        public FourTransactions fourTransactions;
        

        [ReadOnly, SerializeField] protected string guid;

        public string Guid => guid;

        public MainProperty(string name, string sourceName, PropertyType propertyType, string guid){
            this.name = name;
            this.sourceName = sourceName;
            this.guid = guid;
        }
        
        public MainProperty(string guid){
            this.guid = guid;
        }

        public void SetName(string name){
            this.name = name;
        }

        public void SetSourceName(string sourceName){
            this.sourceName = sourceName;
        }
        
    }
    

    
    [Serializable]
    public class ComponentProperty : MainProperty{

        public SerializableSystemType selectedComponent;
        public ComponentWay componentWay = ComponentWay.Parent;
        public SerializablePropertyInfo serializablePropertyInfo;
        [SerializeField] private SerializableSystemType selectedData;
        public SerializableSystemType SelectedData => selectedData;
        public string gameObjectName;

        public ComponentProperty(string guid) : base(guid){
            
        }

        public void SetSelectedData(SerializableSystemType selectedData){
            this.selectedData = selectedData;
        }


    }
    
    [Serializable]
    public class ManuelProperty : MainProperty{
        public DataType dataType;

        public ManuelProperty(string guid) : base(guid){

        }


    }






}