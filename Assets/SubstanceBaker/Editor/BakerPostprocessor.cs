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
            
            isImporting = true;
            importingTexture++;
            if (Baker.isBaking)
            {
                if (assetPath.Contains("_bump"))
                {
                    TextureImporter ti = assetImporter as TextureImporter;
                    ti.convertToNormalmap = true;
                }
            }
        }

        void OnPostprocessTexture(Texture2D texture)
        {
            importingTexture--;
            if(importingTexture == 0)
            {
                isImporting = false;
            }

            if (Baker.isBaking)
            {
                Baker.OnFinishedImporting.Invoke(texture);
            }
        }
    }

}
