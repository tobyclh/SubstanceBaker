﻿using System.Collections;
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

        private static Dictionary<string, List<string>> pendingTextures = new Dictionary<string, List<string>>();
        private static Dictionary<string, ProceduralMaterial> bakingMaterials = new Dictionary<string, ProceduralMaterial>();
        public static void ApplySettings(BakerProfile profile)
        {
            var materials = GatherProceduralMaterials();
            for (int i = 0; i < materials.Count; i++)
            {
                SubstanceImporter substanceImporter = AssetImporter.GetAtPath(materials.ElementAt(i).Key) as SubstanceImporter; // Get the substance importer to change the settings
                var proceduralMaterials = substanceImporter.GetMaterials();  //Get all the materials within that particular sbsar
                foreach (ProceduralMaterial proceduralMaterial in proceduralMaterials)  //For each procedural material in the sbsar...
                {
                    if (materials.ElementAt(i).Value.Contains(proceduralMaterial.name))
                    {
                        ApplySettings(profile, proceduralMaterial, substanceImporter);
                    }
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
            foreach (var property in profile.customFloat)
            {
                try
                {
                    if (proceduralMaterial.HasProceduralProperty(property.Name))
                    {
                        proceduralMaterial.SetProceduralFloat(property.Name, property.value);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            substanceImporter.SetPlatformTextureSettings(proceduralMaterial, profile.platform, width, height, format, loadbehaviour);
            substanceImporter.SetGenerateAllOutputs(proceduralMaterial, profile.generateAllMaps);
            substanceImporter.SaveAndReimport(); //Reimport under the new settings
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        public static void Bake(BakerProfile profile)
        {
            isBaking = true;
            var materials = GatherProceduralMaterials();
            for (int i = 0; i < materials.Count; i++)
            {
                SubstanceImporter substanceImporter = AssetImporter.GetAtPath(materials.ElementAt(i).Key) as SubstanceImporter; // Get the substance importer to change the settings
                var proceduralMaterials = substanceImporter.GetMaterials();  //Get all the materials within that particular sbsar
                foreach (ProceduralMaterial proceduralMaterial in proceduralMaterials)  //For each procedural material in the sbsar...
                {
                    if (materials.ElementAt(i).Value.Contains(proceduralMaterial.name))
                    {
                        Bake(profile, proceduralMaterial, substanceImporter);
                    }
                }
                Resources.UnloadUnusedAssets();
            }
        }

        public static void Bake(BakerProfile profile, ProceduralMaterial proceduralMaterial)
        {
            isBaking = true;
            Bake(profile, proceduralMaterial, AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(proceduralMaterial)) as SubstanceImporter);
        }

        public static void Bake(BakerProfile profile, ProceduralMaterial proceduralMaterial, SubstanceImporter substanceImporter)
        {
            isBaking = true;
            var exportTo = UnityPath(Path.Combine(profile.materialFolder, proceduralMaterial.name)) + "/";
            Debug.Log("Baking : " + proceduralMaterial.name);
            if (!pendingTextures.ContainsKey((proceduralMaterial.name)))
            {
                pendingTextures.Add(proceduralMaterial.name, proceduralMaterial.GetGeneratedTextures().Select(x => x.name).ToList<string>());
                bakingMaterials.Add(proceduralMaterial.name, proceduralMaterial);
                // foreach (var name in proceduralMaterial.GetGeneratedTextures().Select(x => x.name))
                // {
                //     Debug.Log("Name added to pendingTextures    " + name);
                // }
            }

            Debug.Log("Exporting");
            substanceImporter.ExportBitmaps(proceduralMaterial, exportTo, profile.remapAlpha);

            Debug.Log("Exported");
            AssetDatabase.Refresh();
        }

        private static Material CreateMaterialFromMaps(BakerProfile profile, string name)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Assert.IsNotNull(profile.shader);
            ProceduralMaterial pm = bakingMaterials[name];
            bakingMaterials.Remove(name);
            Material mat;
            if (profile.shader != pm.shader)
            {
                mat = new Material(profile.shader);
                foreach (var field in profile.AdditionFields)
                {
                    try
                    {
                        switch (field.FieldType)
                        {
                            case (MappableFields.Color):
                                {
                                    mat.SetColor(field.To, pm.GetColor(field.From));
                                    break;
                                }
                            case (MappableFields.Float):
                                {
                                    mat.SetFloat(field.To, pm.GetFloat(field.From));
                                    break;
                                }
                            case (MappableFields.Texture):
                                {
                                    mat.SetTexture(field.To, pm.GetTexture(field.From));
                                    break;
                                }
                            case (MappableFields.Int):
                                {
                                    mat.SetInt(field.To, pm.GetInt(field.From));
                                    break;
                                }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Additional fields " + field.From + " fialed to set, Skipping field.");
                        Debug.LogException(e);
                        continue;
                    }
                }

            }
            else
            {
                mat = new Material(pm);
            }
            var path = UnityPath(Path.Combine(profile.materialFolder, name));
            var mats = Baker.LoadAllAssetsAtPath<Texture>(path);
            foreach (Texture2D tex in mats)
            {
                if (tex.name.ToLower().Contains("ambient_occlusion"))
                {
                    mat.SetTexture(profile.AOName, tex);
                }
                else if (tex.name.ToLower().Contains("basecolor") || tex.name.ToLower().Contains("diffuse"))
                {
                    mat.SetTexture(profile.albedoName, tex);
                }
                else if (tex.name.ToLower().Contains("roughness"))
                {
                    mat.SetTexture(profile.roughnessName, tex);
                }
                else if (tex.name.ToLower().Contains("height"))
                {
                    mat.SetTexture(profile.heightName, tex);
                }
                else if (tex.name.ToLower().Contains("normal"))
                {
                    mat.SetTexture(profile.normalName, tex);
                }
                else if (tex.name.ToLower().Contains("specular"))
                {
                    mat.SetTexture(profile.specularName, tex);
                }
                else if (tex.name.ToLower().Contains("glossiness"))
                {
                    mat.SetTexture(profile.glossinessName, tex);
                }
            }
            if (profile.removeSubstance)
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(pm));

            }
            AssetDatabase.CreateAsset(mat, path + "/" + name + ".mat");
            Material mat2 = new Material(pm);
            AssetDatabase.CreateAsset(mat2, path + "/" + name + "2.mat");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Resources.UnloadUnusedAssets();
            return mat;
        }


        public static void RemoveTextureFromPending(string texName)
        {
            string materialName;
            for (int i = 0; i < pendingTextures.Count; i++)
            {
                if (pendingTextures.ElementAt(i).Value.Contains(texName))
                {
                    materialName = pendingTextures.ElementAt(i).Key;
                    Debug.Log("RemoveTextureFromPending : " + texName);
                    pendingTextures.ElementAt(i).Value.Remove(texName);
                    if (pendingTextures.ElementAt(i).Value.Count == 0)
                    {
                        Debug.Log("CreateMaterialFromMaps " + materialName);
                        var window = EditorWindow.GetWindow(typeof(BakerWindow)) as BakerWindow;
                        EditorApplication.delayCall += (() => CreateMaterialFromMaps(window._profile, materialName));
                    }
                }
            }
        }




        public static Dictionary<string, List<string>> GatherProceduralMaterials()
        {
            Dictionary<string, List<string>> materials = new Dictionary<string, List<string>>();

            //search for materials
            foreach (var obj in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (obj is ProceduralMaterial)
                {
                    if (!materials.Keys.Contains(path))
                    {
                        materials.Add(path, new List<string>() { obj.name });
                    }
                    else
                    {
                        if (!materials[path].Contains(obj.name))
                        {
                            materials[path].Add(obj.name);
                        }
                    }
                }
                else if (obj is SubstanceArchive)
                {
                    SubstanceImporter substanceImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(obj)) as SubstanceImporter; // Get the substance importer to change the settings
                    var importerMats = substanceImporter.GetMaterials();
                    foreach (var mat in importerMats)
                    {
                        if (!materials.Keys.Contains(path))
                        {
                            materials.Add(path, new List<string>() { mat.name });
                        }
                        else
                        {
                            if (!materials[path].Contains(mat.name))
                            {
                                materials[path].Add(mat.name);
                            }
                        }

                    }

                }

            }
            Debug.Log("Substance Baker : Found " + materials.Count.ToString() + " materials");
            return materials;
        }

        public static bool IsWaitingFor(string name)
        {
            var textures = pendingTextures.Where(x => x.Value.Contains(name));
            if (textures.Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

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

        private static T[] LoadAllAssetsAtPath<T>(string path) where T : UnityEngine.Object
        {
            if (path == "")
            {
                Debug.LogError("Selecting the asset directory is not allowed");
                return null;
            }
            if (!AssetDatabase.IsValidFolder(path))
            {
                Debug.LogError("Invalid folder");
                return null;
            }
            string[] filepaths = Directory.GetFiles(Path.GetDirectoryName(Application.dataPath) + "/" + path, "*.*", SearchOption.AllDirectories);
            Debug.Log("Files Count : " + Path.GetDirectoryName(Application.dataPath) + "/" + path + " " + filepaths.Count());
            List<T> objs = new List<T>();
            foreach (string file in filepaths)
            {
                Debug.Log("File : " + file);
                string fileName = file.Replace('\\', '/');
                T obj = AssetDatabase.LoadAssetAtPath<T>(GetRightPartOfPath(fileName, "Assets"));
                if (obj != null)
                    objs.Add(obj);
            }
            return objs.ToArray<T>();

        }
    }
}