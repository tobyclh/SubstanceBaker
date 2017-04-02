using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using System.Linq;
using UnityEngine;
using UnityEditor;
namespace SubstanceBaker
{
    public class BakerWindow : EditorWindow
    {
        public BakerProfile _profile;
        [HideInInspector]

        [MenuItem("Window/SubstanceBaker/Main Panel")]
        public static void Init()
        {
            EditorWindow.GetWindow(typeof(BakerWindow)).Show();
        }

        void OnGUI()
        {
            GUILayout.Label("Profile", EditorStyles.boldLabel);
            EditorGUILayout.PrefixLabel("Profile to use:");
            _profile = EditorGUILayout.ObjectField(_profile, typeof(BakerProfile), false) as BakerProfile;
            if (_profile != null)
            {
                Editor editor = Editor.CreateEditor(_profile);
                editor.DrawDefaultInspector();
                if (GUILayout.Button("Apply settings to selected materials"))
                {
                    Baker.ApplySettings(_profile);
                }
                if (GUILayout.Button("Start Baking selected materials"))
                {
                    Baker.Bake(_profile);
                }
                EditorGUILayout.LabelField("Selected " + ProceduralMaterialCount() + " Procedural Materials");
            }
            if (GUILayout.Button("Create new profile"))
            {
                var profile = ScriptableObject.CreateInstance("BakerProfile") as BakerProfile;
                string pathToProfile;
                try
                {
                    pathToProfile = "Assets/SubstanceBaker/Profiles/New BakerProfile.asset";
                    AssetDatabase.CreateAsset(profile, pathToProfile);
                }
                catch
                {
                    pathToProfile = "Assets/New BakerProfile.asset";
                    AssetDatabase.CreateAsset(profile, pathToProfile);
                }

                if (_profile == null)
                {
                    _profile = profile;
                }

            }


        }

        [MenuItem("CONTEXT/ProceduralMaterial/Bake And Replace")]
        private static void BakeAndReplace(MenuCommand menuCommand)
        {
            Assert.AreEqual(Selection.objects.Count(), 1, "Select 1 Gameobject");
            var bakerWin = EditorWindow.GetWindow(typeof(BakerWindow)) as BakerWindow;
            var proceduralMat = menuCommand.context as ProceduralMaterial;
            Baker.Bake(bakerWin._profile, proceduralMat);
        }

        [MenuItem("CONTEXT/ProceduralMaterial/Bake Without Replace")]
        private static void BakeNoReplace(MenuCommand menuCommand)
        {
            Assert.AreEqual(Selection.objects.Count(), 1, "Inplace baking supports only 1 material in a time");
            var bakerWin = EditorWindow.GetWindow(typeof(BakerWindow)) as BakerWindow;
            Baker.Bake(bakerWin._profile, menuCommand.context as ProceduralMaterial);
        }


        private static int ProceduralMaterialCount()
        {
            return Selection.objects.Where(x => x is ProceduralMaterial || x is SubstanceArchive).Count();
        }




    }
}