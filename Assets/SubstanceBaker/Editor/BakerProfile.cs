using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SubstanceBaker
{
    public class BakerProfile : ScriptableObject
    {
        [HeaderAttribute("Substance Settings")]
        [TooltipAttribute("Platform Settings to tweak")]
        public string platform = "Default";
        [TooltipAttribute("0 to keep unchanged")]
        public int TargetWidth = 0;
        [TooltipAttribute("0 to keep unchanged")]
        public int TargetHeight = 0;
        public Format format = Format.Unchanged;
        public LoadingBehavior loadingBehaviour = LoadingBehavior.Unchanged;
        public CustomFloat[] customFloat;

        [HeaderAttribute("Baking Settings")]

        [TooltipAttribute("Shader for new material")]
        public Shader shader;
        [TooltipAttribute("Location to store extracted maps")]
        public string materialFolder = "Assets/SubstanceBaker/Materials/";
        [TooltipAttribute("Remove procedural material after baking")]
        public bool removeSubstance = false;
        [TooltipAttribute("Generate all the maps included in the substances even if it is not used by the shader")]
        public bool generateAllMaps = false;
        [SpaceAttribute(5)]

        [TextAreaAttribute]
        public string shaderTextureName = "Fill in your shader's internal texture names, leave blank if inapplicable";
        [TooltipAttribute("Remap alpha channel, https://docs.unity3d.com/ScriptReference/SubstanceImporter.ExportBitmaps.html")]
        public bool remapAlpha = true;
        public string albedoName = "_MainTex";
        public string normalName = "";
        public string specularName = "";
        public string glossinessName = "";
        public string roughnessName = "";
        public string metallicName = "";
        public string AOName = "";
        public string heightName = "";
        public FieldMapper[] AdditionFields;

        public enum Format
        {
            Compressed, Raw, Unchanged
        }
        public enum LoadingBehavior
        {
            DoNothing, Generate, BakeAndKeep, BakeAndDiscard, Cache, DoNothingAndCache, Unchanged
        }

    }

    [System.Serializable]
    public class CustomFloat
    {
        public string Name;
        public float value;
    }
    [System.Serializable]
    public class FieldMapper
    {
        public MappableFields FieldType;
        public string From;
        public string To;

    }

    public enum MappableFields
    {
        Float, Color, Texture2D
    }
}