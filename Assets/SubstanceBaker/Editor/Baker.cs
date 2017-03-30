using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;
using System;
using System.IO;
using System.Linq;

//Baker script that does the heavy lifting for you
namespace SubstanceBaker
{
    public class Baker
    {
        //Batch convert substances toc ordinary materials
        [MenuItem("Window/SubstanceBaker/BatchCovert")]
        public static void BatchCovert(BakerProfile profile)
        {
            var materials = GatherProceduralMaterials();
            for (int i = 0; i < materials.Count; i++)
            {
                SubstanceImporter substanceImporter = AssetImporter.GetAtPath(materials[0]) as SubstanceImporter; // Get the substance importer to change the settings
                var proceduralMaterials = substanceImporter.GetMaterials();  //Get all the materials within that particular sbsar
                foreach (ProceduralMaterial proceduralMaterial in proceduralMaterials)  //For each procedural material in the sbsar...
                {
                    int width, height, format, loadbehaviour;
                    substanceImporter.GetPlatformTextureSettings(substanceImporter.name, profile.platform, out width, out height, out format, out loadbehaviour);
                    width = profile.TargetWidth == 0 ? width : profile.TargetWidth;
                    height = profile.TargetHeight == 0 ? height : profile.TargetHeight;
                    format = profile.format == BakerProfile.Format.Unchanged ? format : (int)profile.format;
                    loadbehaviour = profile.loadingBehaviour == BakerProfile.LoadingBehavior.Unchanged ? loadbehaviour : (int)profile.loadingBehaviour;

                    Debug.Log("Processing : " + proceduralMaterial.name); //Print the name of the material
                    substanceImporter.SetPlatformTextureSettings(proceduralMaterial, profile.platform, width, height, format, loadbehaviour);
                    substanceImporter.SetGenerateAllOutputs(proceduralMaterial, true);
                    substanceImporter.SaveAndReimport(); //Reimport under the new settings
                    substanceImporter.ExportBitmaps(proceduralMaterial, profile.materialFolder.EndsWith("/") ? profile.materialFolder : profile.materialFolder + "/" + substanceImporter.name + "/", false);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                Resources.UnloadUnusedAssets();
            }

        }

        public static List<string> GatherProceduralMaterials()
        {
            List<string> materials = new List<string>();

            //search for materials
            foreach (var obj in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(obj);

                if (AssetDatabase.IsValidFolder(path))
                {
                    string[] filepaths = Directory.GetFiles(Path.GetDirectoryName(Application.dataPath) + "/" + path, "*.*", SearchOption.AllDirectories);
                    foreach (string file in filepaths)
                    {
                        if (file.EndsWith(".sbsar"))
                        {
                            //Convert path string to relative
                            string filePath = GetRightPartOfPath(file.Replace('\\', '/'), "Assets");
                            if (!materials.Contains(filePath))
                            {
                                materials.Add(filePath);
                            }
                        }
                    }
                }
                else
                {
                    if (path.EndsWith(".sbsar"))
                    {
                        if (!materials.Contains(path))
                        {
                            materials.Add(path);
                        }

                    }
                }
            }
            int count = materials.Count;
            Debug.Log("Substance Baker : Found " + count.ToString() + " materials");
            return materials;
        }

        public static void ApplySettings(BakerProfile profile)
        {
            var materials = GatherProceduralMaterials();
        }

        [MenuItem("Window/SubstanceBaker/CreateMaterialFromMaps")]
        private static void CreateMaterialFromMaps(BakerProfile profile)
        {
            foreach (var obj in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (!AssetDatabase.IsValidFolder(path))
                {
                    Debug.LogError("All maps must be contained within a folder, Skipping");
                    continue;
                }
                string[] filepaths = Directory.GetFiles(Path.GetDirectoryName(Application.dataPath) + "/" + path, "*.*", SearchOption.AllDirectories);
                //Debug.Log("Files Count : " + Path.GetDirectoryName(Application.dataPath) + "/" + path + filepaths.Count());
                List<Texture> textures = new List<Texture>();
                foreach (string file in filepaths)
                {
                    string fileName = file.Replace('\\', '/');
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(GetRightPartOfPath(fileName, "Assets"));
                    if (tex != null)
                        textures.Add(tex);
                }
                Assert.AreNotEqual(textures.Count(), 0, "Cannot find any texture in folder");
                Assert.IsNotNull(profile.shader);
                Material mat = new Material(profile.shader);

                foreach (Texture tex in textures)
                {
                    if (tex.name.Contains("ambient_occlusion"))
                    {
                        mat.SetTexture("_AmbientOcclusion", tex);
                    }
                    else if (tex.name.Contains("basecolor"))
                    {
                        mat.SetTexture("_MainTex", tex);
                    }
                    else if (tex.name.Contains("roughness"))
                    {
                        mat.SetTexture("_Roughness", tex);
                    }
                    else if (tex.name.Contains("height"))
                    {
                        mat.SetTexture("_DisplacementMap", tex);
                    }
                    else if (tex.name.Contains("normal"))
                    {
                        mat.SetTexture("_BumpMap", tex);
                    }
                    else if (tex.name.Contains("specular"))
                    {
                        mat.SetTexture("_Specular", tex);
                    }
                    else if (tex.name.Contains("glossiness"))
                    {
                        mat.SetTexture("_Cavity", tex);
                    }
                }

                AssetDatabase.CreateAsset(mat, path + "/" + GetTextureName(path) + ".mat");
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static string GetTextureName(string path)
        {
            return path;
        }

        private static string GetRightPartOfPath(string path, string after)
        {
            var parts = path.Split('/');
            int afterIndex = Array.IndexOf(parts, after);

            if (afterIndex == -1)
            {
                return null;
            }

            return string.Join(Path.DirectorySeparatorChar.ToString(),
            parts, afterIndex, parts.Length - afterIndex).Replace('\\', '/');
        }
    }
}