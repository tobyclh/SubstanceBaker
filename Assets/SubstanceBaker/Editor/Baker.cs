using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEngine.Assertions;
using System;
using System.IO;
using System.Linq;

//Baker script that does the heavy lifting for you
namespace SubstanceBaker
{
    public static class Baker
    {
        public static bool isBaking = false;
        public static UnityEvent<Material> OnFinishedBaking;
        public static UnityEvent OnFinishedBatchBaking;
        public static UnityEvent<Texture2D> OnTextureImported;
        private static List<string> pendingTextures = new List<string>();
        private static List<Texture2D> importedTexture = new List<Texture2D>();
        private static bool areTexturesImported = false;
        public static void ApplySettings(BakerProfile profile)
        {
            var materials = GatherProceduralMaterials();
            for (int i = 0; i < materials.Count; i++)
            {
                SubstanceImporter substanceImporter = AssetImporter.GetAtPath(materials[0]) as SubstanceImporter; // Get the substance importer to change the settings
                var proceduralMaterials = substanceImporter.GetMaterials();  //Get all the materials within that particular sbsar
                foreach (ProceduralMaterial proceduralMaterial in proceduralMaterials)  //For each procedural material in the sbsar...
                {
                    ApplySettings(profile, proceduralMaterial, substanceImporter);
                }
                Resources.UnloadUnusedAssets();
            }
        }

        public static void ApplySettings(BakerProfile profile, ProceduralMaterial proceduralMaterial)
        {
            ApplySettings(profile, proceduralMaterial, AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(proceduralMaterial)) as SubstanceImporter);
        }

        public static void ApplySettings(BakerProfile profile, ProceduralMaterial proceduralMaterial, SubstanceImporter substanceImporter)
        {
            int width, height, format, loadbehaviour;
            substanceImporter.GetPlatformTextureSettings(proceduralMaterial.name, profile.platform, out width, out height, out format, out loadbehaviour);
            width = profile.TargetWidth == 0 ? width : profile.TargetWidth;
            height = profile.TargetHeight == 0 ? height : profile.TargetHeight;
            format = profile.format == BakerProfile.Format.Unchanged ? format : (int)profile.format;
            loadbehaviour = profile.loadingBehaviour == BakerProfile.LoadingBehavior.Unchanged ? loadbehaviour : (int)profile.loadingBehaviour;

            Debug.Log("Applying Settings to " + proceduralMaterial.name); //Print the name of the material
            substanceImporter.SetPlatformTextureSettings(proceduralMaterial, profile.platform, width, height, format, loadbehaviour);
            substanceImporter.SetGenerateAllOutputs(proceduralMaterial, true);
            substanceImporter.SaveAndReimport(); //Reimport under the new settings
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        public static void Bake(BakerProfile profile)
        {
            var materials = GatherProceduralMaterials();
            for (int i = 0; i < materials.Count; i++)
            {
                SubstanceImporter substanceImporter = AssetImporter.GetAtPath(materials[0]) as SubstanceImporter; // Get the substance importer to change the settings
                var importerMats = substanceImporter.GetMaterials();
                for (int j = 0; j < importerMats.Length; j++)
                {
                    //Bake 
                    Bake(profile, importerMats[j], substanceImporter);
                }
            }
        }

        public static void Bake(BakerProfile profile, ProceduralMaterial proceduralMaterial)
        {
            Bake(profile, proceduralMaterial, AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(proceduralMaterial)) as SubstanceImporter);
        }

        public static IEnumerator Bake(BakerProfile profile, ProceduralMaterial proceduralMaterial, SubstanceImporter substanceImporter)
        {
            var exportTo = UnityPath(Path.Combine(profile.materialFolder, proceduralMaterial.name)) + "/";
            substanceImporter.ExportBitmaps(proceduralMaterial, exportTo, profile.remapAlpha);
            pendingTextures.AddRange(proceduralMaterial.GetGeneratedTextures().Select(x => x.name));
            OnTextureImported.AddListener(RemoveTextureFromPending);
            yield return new WaitUntil(()=> pendingTextures.Count == 0);
            Assert.AreNotEqual(textures.Count(), 0, "Cannot find any texture in folder");
            Assert.IsNotNull(profile.shader);
            Material mat = new Material(profile.shader);
            foreach (Texture tex in textures)
            {
                
                if (tex.name.Contains("ambient_occlusion"))
                {
                    mat.SetTexture(profile.AOName, tex);
                }
                else if (tex.name.Contains("basecolor"))
                {
                    mat.SetTexture(profile.albedoName, tex);
                }
                else if (tex.name.Contains("roughness"))
                {
                    mat.SetTexture(profile.roughnessName, tex);
                }
                else if (tex.name.Contains("height"))
                {
                    mat.SetTexture(profile.heightName, tex);
                }
                else if (tex.name.Contains("normal"))
                {
                    mat.SetTexture(profile.normalName, tex);
                }
                else if (tex.name.Contains("specular"))
                {
                    mat.SetTexture(profile.specularName, tex);
                }
                else if (tex.name.Contains("glossiness"))
                {
                    mat.SetTexture(profile.glossinessName, tex);
                }
            }
            AssetDatabase.CreateAsset(mat, exportTo + proceduralMaterial.name + ".mat");

        }

        private static void RemoveTextureFromPending(Texture2D tex)
        {
            if(pendingTextures.Contains(tex.name))
            {
                pendingTextures.Remove(tex.name);
                importedTexture.Add(tex);
            }
            if(pendingTextures.Count == 0)
            {
                OnTextureImported.RemoveListener(RemoveTextureFromPending);
            }

        }
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
                    substanceImporter.ExportBitmaps(proceduralMaterial, profile.materialFolder.EndsWith("/") ? profile.materialFolder : profile.materialFolder + "/" + proceduralMaterial.name + "/", false);
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
                    string[] filepaths = Directory.GetFiles(Path.GetDirectoryName(Application.dataPath) + "/" + path, "*.sbsar", SearchOption.AllDirectories);
                    foreach (string file in filepaths)
                    {
                        //Convert path string to relative
                        string filePath = GetRightPartOfPath(UnityPath(file), "Assets");
                        if (!materials.Contains(filePath))
                        {
                            materials.Add(filePath);
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

                AssetDatabase.CreateAsset(mat, path + "/" + path + ".mat");
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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

        //Path cleaner to ensure the path is correct
        public static string UnityPath(string OSPath)
        {
            return OSPath.Replace('\\', '/');
        }

    }
}