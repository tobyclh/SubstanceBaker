using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SubstanceBaker
{
    public class BakerPostprocessor : AssetPostprocessor
    {
        public static bool isImporting = false;
        public static int importingTexture = 0;


        void OnPreprocessTexture()
        {
            Debug.Log("OnPreprocessTexture");
            isImporting = true;
            importingTexture++;
            if (Baker.isBaking)
            {
                TextureImporter ti = assetImporter as TextureImporter;
                ti.isReadable = true;
                if (assetPath.Contains("_Normal"))
                {
                    ti.normalmap = true;
                }
            }
        }

        void OnPostprocessTexture(Texture2D texture)
        {
            Debug.Log("OnPostprocessTexture");
            importingTexture--;
            if (importingTexture == 0)
            {
                isImporting = false;
            }
            //for some reason texture doesn't have a name here, strange
            var pieces = assetPath.Split('/');
            var name = pieces[pieces.Length - 1].Split('.')[0];
            if (Baker.IsWaitingFor(name))
            {
                Baker.RemoveTextureFromPending(name);
            }
        }
    }

}
