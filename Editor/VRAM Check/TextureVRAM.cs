﻿#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
#if VRC_SDK_VRCSDK3 && !UDON
#endif

namespace Thry.AvatarHelpers
{
    public class TextureVRAM : EditorWindow
    {
        [MenuItem("Thry/Avatar/VRAM")]
        public static void Init()
        {
            TextureVRAM window = (TextureVRAM)EditorWindow.GetWindow(typeof(TextureVRAM));
            window.titleContent = new GUIContent("VRAM Calculator");
            window.Show();
        }

        [MenuItem("GameObject/Thry/Avatar/VRAM", true, 0)]
        static bool CanShowFromSelection() => Selection.activeGameObject != null;

        [MenuItem("GameObject/Thry/Avatar/VRAM", false, 0)]
        public static void ShowFromSelection()
        {
            TextureVRAM window = (TextureVRAM)EditorWindow.GetWindow(typeof(TextureVRAM));
            window.titleContent = new GUIContent("VRAM Calculator");
            window._avatar = Selection.activeGameObject;
            window.Show();
        }

        public static void Init(GameObject avatar)
        {
            TextureVRAM window = (TextureVRAM)EditorWindow.GetWindow(typeof(TextureVRAM));
            window.titleContent = new GUIContent("VRAM Calculator");
            window._avatar = avatar;
            window.Calc(avatar);
            window.Show();
        }

        //https://docs.unity3d.com/Manual/class-TextureImporterOverride.html
        static Dictionary<TextureImporterFormat, int> BPP = new Dictionary<TextureImporterFormat, int>()
    {
        { TextureImporterFormat.BC7 , 8 },
        { TextureImporterFormat.DXT5 , 8 },
        { TextureImporterFormat.DXT5Crunched , 8 },
        { TextureImporterFormat.RGBA64 , 64 },
        { TextureImporterFormat.RGBA32 , 32 },
        { TextureImporterFormat.RGBA16 , 16 },
        { TextureImporterFormat.DXT1 , 4 },
        { TextureImporterFormat.DXT1Crunched , 4 },
        { TextureImporterFormat.RGB48 , 64 },
        { TextureImporterFormat.RGB24 , 32 },
        { TextureImporterFormat.RGB16 , 16 },
        { TextureImporterFormat.BC5 , 8 },
        { TextureImporterFormat.RG32 , 32 },
        { TextureImporterFormat.BC4 , 4 },
        { TextureImporterFormat.R8 , 8 },
        { TextureImporterFormat.R16 , 16 },
        { TextureImporterFormat.Alpha8 , 8 },
        { TextureImporterFormat.RGBAHalf , 64 },
        { TextureImporterFormat.BC6H , 8 },
        { TextureImporterFormat.RGB9E5 , 32 },
        { TextureImporterFormat.ETC2_RGBA8Crunched , 8 },
        { TextureImporterFormat.ETC2_RGB4 , 4 },
        { TextureImporterFormat.ETC2_RGBA8 , 8 },
        { TextureImporterFormat.ETC2_RGB4_PUNCHTHROUGH_ALPHA , 4 },
        { TextureImporterFormat.PVRTC_RGB2 , 2 },
        { TextureImporterFormat.PVRTC_RGB4 , 4 },
        { TextureImporterFormat.ARGB32 , 32 },
        { TextureImporterFormat.ARGB16 , 16 }
    };

        static Dictionary<RenderTextureFormat, int> RT_BPP = new Dictionary<RenderTextureFormat, int>()
        {
            //
            // Summary:
            //     Color render texture format, 8 bits per channel.
            { RenderTextureFormat.ARGB32 , 32 },
            //
            // Summary:
            //     A depth render texture format.
            { RenderTextureFormat.Depth , 0 },
            //
            // Summary:
            //     Color render texture format, 16 bit floating point per channel.
            { RenderTextureFormat.ARGBHalf , 64 },
            //
            // Summary:
            //     A native shadowmap render texture format.
            { RenderTextureFormat.Shadowmap , 8 }, //guessed bpp
            //
            // Summary:
            //     Color render texture format.
            { RenderTextureFormat.RGB565 , 32 }, //guessed bpp
            //
            // Summary:
            //     Color render texture format, 4 bit per channel.
            { RenderTextureFormat.ARGB4444 , 16 }, 
            //
            // Summary:
            //     Color render texture format, 1 bit for Alpha channel, 5 bits for Red, Green and
            //     Blue channels.
            { RenderTextureFormat.ARGB1555 , 16 },
            //
            // Summary:
            //     Default color render texture format: will be chosen accordingly to Frame Buffer
            //     format and Platform.
            { RenderTextureFormat.Default , 32 }, //fuck
            //
            // Summary:
            //     Color render texture format. 10 bits for colors, 2 bits for alpha.
            { RenderTextureFormat.ARGB2101010 , 32 },
            //
            // Summary:
            //     Default HDR color render texture format: will be chosen accordingly to Frame
            //     Buffer format and Platform.
            { RenderTextureFormat.DefaultHDR , 128 }, //fuck
            //
            // Summary:
            //     Four color render texture format, 16 bits per channel, fixed point, unsigned
            //     normalized.
            { RenderTextureFormat.ARGB64 , 64 },
            //
            // Summary:
            //     Color render texture format, 32 bit floating point per channel.
            { RenderTextureFormat.ARGBFloat , 128 },
            //
            // Summary:
            //     Two color (RG) render texture format, 32 bit floating point per channel.
            { RenderTextureFormat.RGFloat , 64 },
            //
            // Summary:
            //     Two color (RG) render texture format, 16 bit floating point per channel.
            { RenderTextureFormat.RGHalf , 32 },
            //
            // Summary:
            //     Scalar (R) render texture format, 32 bit floating point.
            { RenderTextureFormat.RFloat , 32 },
            //
            // Summary:
            //     Scalar (R) render texture format, 16 bit floating point.
            { RenderTextureFormat.RHalf , 16 },
            //
            // Summary:
            //     Single channel (R) render texture format, 8 bit integer.
            { RenderTextureFormat.R8 , 8 },
            //
            // Summary:
            //     Four channel (ARGB) render texture format, 32 bit signed integer per channel.
            { RenderTextureFormat.ARGBInt , 128 },
            //
            // Summary:
            //     Two channel (RG) render texture format, 32 bit signed integer per channel.
            { RenderTextureFormat.RGInt , 64 },
            //
            // Summary:
            //     Scalar (R) render texture format, 32 bit signed integer.
            { RenderTextureFormat.RInt , 32 },
            //
            // Summary:
            //     Color render texture format, 8 bits per channel.
            { RenderTextureFormat.BGRA32 , 32 },
            //
            // Summary:
            //     Color render texture format. R and G channels are 11 bit floating point, B channel is 10 bit floating point.
            { RenderTextureFormat.RGB111110Float , 32 },
            //
            // Summary:
            //     Two color (RG) render texture format, 16 bits per channel, fixed point, unsigned normalized
            { RenderTextureFormat.RG32 , 32 },
            //
            // Summary:
            //     Four channel (RGBA) render texture format, 16 bit unsigned integer per channel.
            { RenderTextureFormat.RGBAUShort , 64 },
            //
            // Summary:
            //     Two channel (RG) render texture format, 8 bits per channel.
            { RenderTextureFormat.RG16 , 16 },
            //
            // Summary:
            //     Color render texture format, 10 bit per channel, extended range.
            { RenderTextureFormat.BGRA10101010_XR , 40 },
            //
            // Summary:
            //     Color render texture format, 10 bit per channel, extended range.
            { RenderTextureFormat.BGR101010_XR , 30 },
            //
            // Summary:
            //     Single channel (R) render texture format, 16 bit integer.
            { RenderTextureFormat.R16 , 16 }
        };

        static Dictionary<VertexAttributeFormat, int> VertexAttributeByteSize = new Dictionary<VertexAttributeFormat, int>()
        {
            { VertexAttributeFormat.UNorm8, 1},
            { VertexAttributeFormat.SNorm8, 1},
            { VertexAttributeFormat.UInt8, 1},
            { VertexAttributeFormat.SInt8, 1},

            { VertexAttributeFormat.UNorm16, 2},
            { VertexAttributeFormat.SNorm16, 2},
            { VertexAttributeFormat.UInt16, 2},
            { VertexAttributeFormat.SInt16, 2},
            { VertexAttributeFormat.Float16, 2},

            { VertexAttributeFormat.Float32, 4},
            { VertexAttributeFormat.UInt32, 4},
            { VertexAttributeFormat.SInt32, 4},
        };

        const long PC_TEXTURE_MEMORY_EXCELLENT_MB = 40;
        const long PC_TEXTURE_MEMORY_GOOD_MB = 75;
        const long PC_TEXTURE_MEMORY_MEDIUM_MB = 110;
        const long PC_TEXTURE_MEMORY_POOR_MB = 150;

        const long PC_MESH_MEMORY_EXCELLENT_MB = 20;
        const long PC_MESH_MEMORY_GOOD_MB = 35;
        const long PC_MESH_MEMORY_MEDIUM_MB = 55;
        const long PC_MESH_MEMORY_POOR_MB = 80;

        const long QUEST_TEXTURE_MEMORY_EXCELLENT_MB = 10;
        const long QUEST_TEXTURE_MEMORY_GOOD_MB = 18;
        const long QUEST_TEXTURE_MEMORY_MEDIUM_MB = 25;
        const long QUEST_TEXTURE_MEMORY_POOR_MB = 40;

        const long QUEST_MESH_MEMORY_EXCELLENT_MB = 5;
        const long QUEST_MESH_MEMORY_GOOD_MB = 10;
        const long QUEST_MESH_MEMORY_MEDIUM_MB = 15;
        const long QUEST_MESH_MEMORY_POOR_MB = 25;

        struct TextureInfo
        {
            public Texture texture;
            public string print;
            public long size;
            public bool isActive;
            public int BPP;
            public int minBPP;
            public string format;
            public bool hasAlpha;

            public List<Material> materials;
            public bool materialDropDown;
        }

        struct MeshInfo
        {
            public Mesh mesh;
            public string print;
            public long size;
            public bool isActive;
        }

        GameObject _avatar;
        long _sizeActive;
        long _sizeAll;
        long _sizeAllTextures;
        long _sizeAllMeshes;
        AvatarEvaluator.Quality _pcTextureQuality;
        AvatarEvaluator.Quality _questTextureQuality;
        AvatarEvaluator.Quality _pcMeshQuality;
        AvatarEvaluator.Quality _questMeshQuality;
        bool _includeInactive = true;
        List<TextureInfo> _texturesList;
        List<MeshInfo> _meshesList;
        Vector2 _scrollPosMajor;
        Vector2 _scrollPosMesh;
        Vector2 _scrollposTex;

        GUIContent matActiveIcon;
        GUIContent matInactiveIcon;
        GUIContent meshInactiveIcon;
        GUIContent meshActiveIcon;
        GUIContent refreshIcon;
        Texture infoIcon;

        bool _texturesFoldout;
        bool _meshesFoldout;

        private void OnEnable() {
            matActiveIcon = EditorGUIUtility.IconContent("d_Material Icon");
            matInactiveIcon = EditorGUIUtility.IconContent("d_Material On Icon");
            infoIcon = EditorGUIUtility.Load("console.infoicon") as Texture;
            meshInactiveIcon = EditorGUIUtility.IconContent("d_Mesh Icon");
            meshActiveIcon = EditorGUIUtility.IconContent("d_MeshCollider Icon");
            refreshIcon = EditorGUIUtility.IconContent("RotateTool On", "Recalculate");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"<size=20><color=magenta>Thry's Avatar VRAM Calculator</color></size> v{AvatarEvaluator.VERSION}", new GUIStyle(EditorStyles.label) { richText = true, alignment = TextAnchor.MiddleCenter });
            if (GUILayout.Button("Click here & follow me on twitter", EditorStyles.centeredGreyMiniLabel))
                Application.OpenURL("https://twitter.com/thryrallo");
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Information", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("VRAM is not Download size", MessageType.Warning);
            EditorGUILayout.HelpBox("The video memory size can affect your fps greatly. Your graphics card only has a " +
                "certain amount of video memory and if that is used up it has to start moving assets between your system memory and the video memory, which is really slow.", MessageType.Warning);
            //EditorGUILayout.HelpBox("Video memory usage adds up quickly\nExample: 150MB / per avatar * 40 Avatars + 2GB World = 8GB\n=> Uses up all VRAM on an RTX 3070", MessageType.Info);
            EditorGUILayout.HelpBox("Video memory usage adds up quickly\n" +
                "Taking into account a world VRAM usage of 2GB - If your model uses 150MB of VRAM and there were 40 of you, all 8 GB of VRAM would be used up on an RTX 3070.",
                MessageType.Info);

            GUILine();

            EditorGUILayout.LabelField("Input", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            using (new EditorGUILayout.HorizontalScope())
            {
                //GUILayout.Label("Avatar", GUILayout.Width(40));
                GUI.enabled = _avatar != null;
                if(GUILayout.Button(refreshIcon, GUILayout.Width(30), GUILayout.Height(30))) {
                    meshSizeCache.Clear();
                    Calc(_avatar);
                }
                GUI.enabled = true;

                _avatar = (GameObject)EditorGUILayout.ObjectField(GUIContent.none, _avatar, typeof(GameObject), true, GUILayout.Height(30));
                if (EditorGUI.EndChangeCheck() && _avatar != null) {
                    Calc(_avatar);
                }

            }

            if (_avatar != null)
            {
                _scrollPosMajor = EditorGUILayout.BeginScrollView(_scrollPosMajor);

                GUILine();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
                // TODO
                // if (_sizeAll > VRAM_EXCESSIVE_THRESHOLD) EditorGUILayout.HelpBox("Your avatar uses a lot of video memory. Please reduce the texture sizes or change the compression to prevent bottlenecking yourself and others.", MessageType.Error);
                // else if (_sizeAll > VRAM_WARNING_THRESHOLD) EditorGUILayout.HelpBox("Your avatar is still ok. Try not to add too many more big textures.", MessageType.Warning);
                // else EditorGUILayout.HelpBox("Your avatar is in a good place regarding video memeory size.", MessageType.None);

                EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal(GUI.skin.box);
                        EditorGUILayout.LabelField("Texture Memory: ", AvatarEvaluator.ToMebiByteString(_sizeAllTextures), GUILayout.Width(250));
                        EditorGUILayout.LabelField("PC", GUILayout.Width(20));
                        AvatarEvaluator.DrawQualityIcon(_pcTextureQuality);
                        EditorGUILayout.LabelField("Quest", GUILayout.Width(40));
                        AvatarEvaluator.DrawQualityIcon(_questTextureQuality);         
                        GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal(GUI.skin.box);
                        EditorGUILayout.LabelField("Mesh Memory: ", AvatarEvaluator.ToMebiByteString(_sizeAllMeshes), GUILayout.Width(250));
                        EditorGUILayout.LabelField("PC", GUILayout.Width(20));
                        AvatarEvaluator.DrawQualityIcon(_pcMeshQuality);
                        EditorGUILayout.LabelField("Quest", GUILayout.Width(40));
                        AvatarEvaluator.DrawQualityIcon(_questMeshQuality);
                        GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                    EditorGUILayout.LabelField("Combined (all): ", AvatarEvaluator.ToMebiByteString(_sizeAll));
                    EditorGUILayout.LabelField("Combined (only active): ", AvatarEvaluator.ToMebiByteString(_sizeActive));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.HelpBox("Inactive Objects are not unloaded. They are moved to system memory first if you run out of VRAM, " +
                "so they are not as bad as active textures, but you should still try to keep their VRAM low.", MessageType.None);

                EditorGUILayout.LabelField("If there were 40 of your avatar you would take up <b>" + AvatarEvaluator.ToMebiByteString(_sizeAll * 40) + "</b> of Video Memory.", new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true });
                EditorGUILayout.LabelField("If there were 80 of your avatar you would take up <b>" + AvatarEvaluator.ToMebiByteString(_sizeAll * 80) + "</b> of Video Memory.", new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true });
                //EditorGUILayout.LabelField("Size of 40 avatars:", );

                if (_texturesList == null) Calc(_avatar);
                if (_texturesList != null)
                {
                    GUILine();
                    EditorGUILayout.LabelField("Assets", EditorStyles.boldLabel);
                    _includeInactive = EditorGUILayout.ToggleLeft("Show assets of disabled Objects", _includeInactive);

                    //EditorGUI.indentLevel += 2;

                    _texturesFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_texturesFoldout, $"Textures  ({AvatarEvaluator.ToMebiByteString(_sizeAllTextures)})");
                    if (_texturesFoldout)
                    {
                        EditorGUILayout.HelpBox("The determining factor of texture VRAM size is the resolution of the texutre and the used compression format.\n" +
                            "Both can be changed in the importer settings of each texture.\n\n" +
                            "Resolution: View your avatar from ~1 meter away. Reduce the texture size until you see a noticable drop in quality.\n" +
                            "Compression: BC7 or DXT5 for textures with alpha. DXT1 for images without alpha.\n\n" +
                            "BC7 and DXT5 have the same VRAM size. BC7 is higher quality, DXT5 is smaller in download size.", MessageType.Info);
                        EditorGUILayout.Space(5);

                        _scrollposTex = EditorGUILayout.BeginScrollView(_scrollposTex, GUILayout.Height(500));
                        for (int texIdx = 0; texIdx < _texturesList.Count; texIdx++)
                        {
                            TextureInfo texInfo = _texturesList[texIdx];
                            if (_includeInactive || texInfo.isActive)
                            {
                                // get info messages
                                List<string> infoMessages = new List<string>();
                                if(texInfo.format.Length < 1) {
                                    infoMessages.Add("This texture is not compressable");
                                }
                                if(texInfo.materials.Count < 1) {
                                    infoMessages.Add("This texture is used in a material swap");
                                }


                                // idk how to make a fake indent (because im using guilayout.button which ignores indents) so i just used a horizontal scope with a space :)
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(25);
                                EditorGUILayout.BeginVertical("box");
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    using (new EditorGUI.DisabledGroupScope(texInfo.materials.Count < 1))
                                    {
                                        GUIContent content = texInfo.materialDropDown ? matActiveIcon : matInactiveIcon;
                                        if (GUILayout.Button(content, GUILayout.Width(25), GUILayout.Height(20)))
                                        {
                                            texInfo.materialDropDown = !texInfo.materialDropDown;
                                            _texturesList[texIdx] = texInfo;
                                        }
                                    }

                                    // set labelwidth for guilayout.label
                                    EditorGUIUtility.labelWidth = 50;
                                    GUILayout.Label(texInfo.print, GUILayout.Width(70), GUILayout.Height(20));

                                    if (infoMessages.Count > 0) {
                                        string messages = string.Join("\n", infoMessages);
                                        GUILayout.Label(new GUIContent(infoIcon, messages), GUILayout.Width(20), GUILayout.Height(20));
                                    } else {
                                        GUILayout.Space(25);
                                    }

                                    EditorGUILayout.ObjectField(texInfo.texture, typeof(object), false, GUILayout.Width(280), GUILayout.Height(20));

                                    if(texInfo.format.Length > 0 
                                        && GUILayout.Button(new GUIContent($"({texInfo.format})", texInfo.format), EditorStyles.label, GUILayout.Width(100), GUILayout.Height(20))) {
                                        Application.OpenURL("https://docs.unity.cn/2019.4/Documentation/Manual/class-TextureImporterOverride.html");
                                    }

                                    if (texInfo.format.Length > 0 && texInfo.texture is Texture2D && texInfo.BPP > texInfo.minBPP)
                                    {
                                        TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texInfo.texture)) as TextureImporter;
                                        TextureImporterFormat newFormat = texInfo.hasAlpha || importer.textureType == TextureImporterType.NormalMap ?
                                            TextureImporterFormat.BC7 : TextureImporterFormat.DXT1;
                                        string savedSize = AvatarEvaluator.ToMebiByteString(texInfo.size - TextureToBytesUsingBPP(texInfo.texture, BPP[newFormat]));
                                        if (GUILayout.Button($"{newFormat} => Save {savedSize}", GUILayout.Width(200), GUILayout.Height(20))
                                            && EditorUtility.DisplayDialog("Confirm Compression Format Change!", $"You are about to change the compression format of texture '{texInfo.texture.name}' from {texInfo.format} => {newFormat}\n\n" +
                                            $"If you wish to return this textures' compression to {texInfo.format}, you will have to do so manually as this action is not undo-able.\n\nAre you sure?", "Yes", "No"))
                                        {
                                            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
                                            {
                                                name = "PC",
                                                overridden = true,
                                                format = newFormat,
                                                maxTextureSize = texInfo.texture.width,
                                                compressionQuality = 100
                                            });
                                            importer.SaveAndReimport();
                                            Calc(_avatar);
                                        }
                                        else
                                        {
                                            GUILayout.Space(200);
                                        }
                                    }
                                    else
                                    {
                                        EditorGUILayout.GetControlRect(GUILayout.Width(200), GUILayout.Height(20));
                                    }
                                }

                                if (texInfo.materialDropDown)
                                {
                                    GUILayout.Label($"Used in {texInfo.materials.Count()} material(s) on '{_avatar.name}'", EditorStyles.boldLabel);
                                    EditorGUI.indentLevel++;
                                    foreach (Material mat in texInfo.materials){
                                        EditorGUILayout.ObjectField(mat, typeof(Material), false, GUILayout.Width(395), GUILayout.Height(20));
                                    }
                                    EditorGUI.indentLevel--;
                                    GUILayout.Space(5);
                                }

                                EditorGUILayout.EndVertical();
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        EditorGUILayout.EndScrollView();
                    }

                    EditorGUILayout.EndFoldoutHeaderGroup();

                    _meshesFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_meshesFoldout, $"Meshes  ({AvatarEvaluator.ToMebiByteString(_sizeAllMeshes)})");
                    if (_meshesFoldout)
                    {
                        _scrollPosMesh = EditorGUILayout.BeginScrollView(_scrollPosMesh, GUILayout.Height(300));
                        for (int mIdx = 0; mIdx < _meshesList.Count; mIdx++) {
                            MeshInfo meshInfo = _meshesList[mIdx];
                            if (_includeInactive || meshInfo.isActive)
                            {
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(25);
                                using (new EditorGUILayout.HorizontalScope("box")) {
                                    EditorGUILayout.LabelField(new GUIContent(meshInfo.print, meshInfo.print), GUILayout.Width(100), GUILayout.Height(20));
                                    EditorGUILayout.ObjectField(meshInfo.mesh, typeof(Mesh), false, GUILayout.Width(400), GUILayout.Height(20));
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        EditorGUILayout.EndScrollView();
                    }
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    //EditorGUI.indentLevel -= 2;
                }
                EditorGUILayout.EndScrollView();
            }
        }

        static void GUILine(int i_height = 1)
        {
            GUILayout.Space(10);
            Rect rect = EditorGUILayout.GetControlRect(false, i_height);
            if (rect != null){
                rect.width = EditorGUIUtility.currentViewWidth;
                GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
            }
            GUILayout.Space(10);
        }

        static Dictionary<Texture, bool> GetTextures(GameObject avatar)
        {
            IEnumerable<Material>[] materials = AvatarEvaluator.GetMaterials(avatar);
            Dictionary<Texture, bool> textures = new Dictionary<Texture, bool>();
            foreach (Material m in materials[1])
            {
                if (m == null) continue;
                int[] textureIds = m.GetTexturePropertyNameIDs();
                bool isActive = materials[0].Contains(m);
                foreach (int id in textureIds)
                {
                    if (!m.HasProperty(id)) continue;
                    Texture t = m.GetTexture(id);
                    if (t == null) continue;
                    if (textures.ContainsKey(t))
                    {
                        if (textures[t] == false && isActive) textures[t] = true;
                    }
                    else
                    {
                        textures.Add(t, isActive);
                    }
                }
            }
            return textures;
        }

        public static long QuickCalc(GameObject avatar)
        {
            Dictionary<Texture, bool> textures = GetTextures(avatar);
            long size = 0;
            foreach (KeyValuePair<Texture, bool> t in textures)
                size += CalculateTextureSize(t.Key, t.Value).Item1;
            IEnumerable<Mesh> allMeshes = avatar.GetComponentsInChildren<Renderer>(true).Select(r => r is SkinnedMeshRenderer ? (r as SkinnedMeshRenderer).sharedMesh : r is MeshRenderer ? r.GetComponent<MeshFilter>().sharedMesh : null);
            foreach (Mesh m in allMeshes)
            {
                if (m == null) continue;
                size += CalculateMeshSize(m);
            }
            return size;
        }

        List<Material> GetMaterialsUsingTexture(Texture texture, List<Material> materialsToSearch) {
            List<Material> materials = new List<Material>();

            foreach(Material mat in materialsToSearch) {
                foreach (string propName in mat.GetTexturePropertyNames()) {
                    Texture matTex = mat.GetTexture(propName);
                    if (matTex != null && matTex == texture) {
                        materials.Add(mat);
                        break;
                    }
                }
            }

            return materials;
        }

        public long Calc(GameObject avatar)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Getting VRAM Data", "Getting Materials", 0.5f);
                // get all materials in avatar
                List<Material> tempMaterials = avatar.GetComponentsInChildren<Renderer>(true)
                    .SelectMany(r => r.sharedMaterials)
                    .Where(mat => mat != null)
                    .Distinct()
                    .ToList();

                EditorUtility.DisplayProgressBar("Getting VRAM Data", "Getting Textures", 0.5f);
                Dictionary<Texture, bool> textures = GetTextures(avatar);
                _texturesList = new List<TextureInfo>();
                _sizeAll = 0;
                _sizeActive = 0;
                _sizeAllTextures = 0;
                _sizeAllMeshes = 0;

                int numTextures = textures.Keys.Count;
                int texIdx = 1;
                foreach (KeyValuePair<Texture, bool> t in textures)
                {
                    EditorUtility.DisplayProgressBar("Getting VRAM Data", $"Calculating Texture size for {t.Key.name}", texIdx / (float)numTextures);
                    (long size, string format, int BPP, int minBPP, bool hasAlpha) textureInfo = CalculateTextureSize(t.Key, t.Value);
                    TextureInfo texInfo = new TextureInfo();
                    texInfo.texture = t.Key;
                    texInfo.size = textureInfo.size;
                    texInfo.print = AvatarEvaluator.ToMebiByteString(textureInfo.size);
                    texInfo.format = textureInfo.format;
                    texInfo.BPP = textureInfo.BPP;
                    texInfo.minBPP = textureInfo.minBPP;
                    texInfo.hasAlpha = textureInfo.hasAlpha;
                    texInfo.isActive = t.Value;

                    // get materials
                    texInfo.materials = GetMaterialsUsingTexture(texInfo.texture, tempMaterials);
                    texInfo.materialDropDown = false;

                    _texturesList.Add(texInfo);

                    if (t.Value) _sizeActive += textureInfo.Item1;
                    _sizeAllTextures += textureInfo.Item1;

                    texIdx++;
                }
                _texturesList.Sort((t1, t2) => t2.size.CompareTo(t1.size));

                EditorUtility.DisplayProgressBar("Getting VRAM Data", "Getting Meshes", 0.5f);
                //Meshes
                Dictionary<Mesh, bool> meshes = new Dictionary<Mesh, bool>();
                IEnumerable<Mesh> allMeshes = avatar.GetComponentsInChildren<Renderer>(true).Select(r => r is SkinnedMeshRenderer ? (r as SkinnedMeshRenderer).sharedMesh : r is MeshRenderer ? r.GetComponent<MeshFilter>().sharedMesh : null);
                IEnumerable<Mesh> activeMeshes = avatar.GetComponentsInChildren<Renderer>().Select(r => r is SkinnedMeshRenderer ? (r as SkinnedMeshRenderer).sharedMesh : r is MeshRenderer ? r.GetComponent<MeshFilter>().sharedMesh : null);
                foreach (Mesh m in allMeshes)
                {
                    if (m == null) continue;
                    bool isActive = activeMeshes.Contains(m);
                    if (meshes.ContainsKey(m))
                    {
                        if (meshes[m] == false && isActive) meshes[m] = true;
                    }
                    else
                    {
                        meshes.Add(m, isActive);
                    }
                }

                int numMeshes = meshes.Keys.Count;
                int meshIdx = 1;
                _meshesList = new List<MeshInfo>();
                foreach (KeyValuePair<Mesh, bool> m in meshes)
                {
                    EditorUtility.DisplayProgressBar("Getting VRAM Data", $"Calculating mesh size for '{m.Key.name}'", meshIdx / (float)numMeshes);
                    long bytes = CalculateMeshSize(m.Key);
                    if (m.Value) _sizeActive += bytes;
                    _sizeAllMeshes += bytes;

                    MeshInfo meshInfo = new MeshInfo();
                    meshInfo.mesh = m.Key;
                    meshInfo.size = bytes;
                    meshInfo.print = AvatarEvaluator.ToMebiByteString(bytes);
                    meshInfo.isActive = m.Value;
                    _meshesList.Add(meshInfo);
                    meshIdx++;
                }
                EditorUtility.DisplayProgressBar("Getting VRAM Data", "Finishing Up", 0.5f);
                _meshesList.Sort((m1, m2) => m2.size.CompareTo(m1.size));
                _sizeAll = _sizeAllTextures + _sizeAllMeshes;

                // Assign quality
                _pcTextureQuality = GetTextureQuality(_sizeAllTextures, false);
                _pcMeshQuality = GetMeshQuality(_sizeAllMeshes, false);
                _questTextureQuality = GetTextureQuality(_sizeAllTextures, true);
                _questMeshQuality = GetMeshQuality(_sizeAllMeshes, true);
            } finally {
                EditorUtility.ClearProgressBar();
            }

            return _sizeAll;
        }

        public static AvatarEvaluator.Quality GetTextureQuality(long size, bool quest)
        {
            if (quest)
                return GetQuality(size, QUEST_TEXTURE_MEMORY_EXCELLENT_MB, QUEST_TEXTURE_MEMORY_GOOD_MB, QUEST_TEXTURE_MEMORY_MEDIUM_MB, QUEST_TEXTURE_MEMORY_POOR_MB);
            else
                return GetQuality(size, PC_TEXTURE_MEMORY_EXCELLENT_MB, PC_TEXTURE_MEMORY_GOOD_MB, PC_TEXTURE_MEMORY_MEDIUM_MB, PC_TEXTURE_MEMORY_POOR_MB);
        }

        public static AvatarEvaluator.Quality GetMeshQuality(long size, bool quest)
        {
            if (quest)
                return GetQuality(size, QUEST_MESH_MEMORY_EXCELLENT_MB, QUEST_MESH_MEMORY_GOOD_MB, QUEST_MESH_MEMORY_MEDIUM_MB, QUEST_MESH_MEMORY_POOR_MB);
            else
                return GetQuality(size, PC_MESH_MEMORY_EXCELLENT_MB, PC_MESH_MEMORY_GOOD_MB, PC_MESH_MEMORY_MEDIUM_MB, PC_MESH_MEMORY_POOR_MB);
        }

        static AvatarEvaluator.Quality GetQuality(long size, long excellent, long good, long medium, long poor)
        {
            if (size < excellent * 1000000)
                return AvatarEvaluator.Quality.Excellent;
            else if (size < good * 1000000)
                return AvatarEvaluator.Quality.Good;
            else if (size < medium * 1000000)
                return AvatarEvaluator.Quality.Medium;
            else if (size < poor * 1000000)
                return AvatarEvaluator.Quality.Poor;
            else
                return AvatarEvaluator.Quality.VeryPoor;
        }

        static Dictionary<Mesh, long> meshSizeCache = new Dictionary<Mesh, long>();

        static long CalculateMeshSize(Mesh mesh)
        {
            if (meshSizeCache.ContainsKey(mesh))
                return meshSizeCache[mesh];
            
            long bytes = 0;

            var vertexAttributes = mesh.GetVertexAttributes();
            long vertexAttributeVRAMSize = 0;
            foreach (var vertexAttribute in vertexAttributes)
            {
                int skinnedMeshPositionNormalTangentMultiplier = 1;
                // skinned meshes have 2x the amount of position, normal and tangent data since they store both the un-skinned and skinned data in VRAM
                if (mesh.HasVertexAttribute(VertexAttribute.BlendIndices) && mesh.HasVertexAttribute(VertexAttribute.BlendWeight) &&
                    (vertexAttribute.attribute == VertexAttribute.Position || vertexAttribute.attribute == VertexAttribute.Normal || vertexAttribute.attribute == VertexAttribute.Tangent))
                    skinnedMeshPositionNormalTangentMultiplier = 2;
                vertexAttributeVRAMSize += VertexAttributeByteSize[vertexAttribute.format] * vertexAttribute.dimension * skinnedMeshPositionNormalTangentMultiplier;
            }
            var deltaPositions = new Vector3[mesh.vertexCount];
            var deltaNormals = new Vector3[mesh.vertexCount];
            var deltaTangents = new Vector3[mesh.vertexCount];
            long blendShapeVRAMSize = 0;
            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                var blendShapeName = mesh.GetBlendShapeName(i);
                var blendShapeFrameCount = mesh.GetBlendShapeFrameCount(i);
                for (int j = 0; j < blendShapeFrameCount; j++)
                {
                    mesh.GetBlendShapeFrameVertices(i, j, deltaPositions, deltaNormals, deltaTangents);
                    for (int k = 0; k < deltaPositions.Length; k++)
                    {
                        if (deltaPositions[k] != Vector3.zero || deltaNormals[k] != Vector3.zero || deltaTangents[k] != Vector3.zero)
                        {
                            // every affected vertex has 1 uint for the index, 3 floats for the position, 3 floats for the normal and 3 floats for the tangent
                            // this is true even if all normals or tangents in all blend shapes are zero
                            blendShapeVRAMSize += 40;
                        }
                    }
                }
            }
            bytes = vertexAttributeVRAMSize * mesh.vertexCount + blendShapeVRAMSize;
            meshSizeCache[mesh] = bytes;
            return bytes;
        }

        static (long size, string format, int BPP, int minBPP, bool hasAlpha) CalculateTextureSize(Texture t, bool addToList)
        {
            string format = "";
            int bpp = 0;
            int minBPP = 8;
            bool hasAlpha = false;
            long size = 0;

            string path = AssetDatabase.GetAssetPath(t);
            if (t != null && path != null && t is RenderTexture == false && t.dimension == UnityEngine.Rendering.TextureDimension.Tex2D)
            {
                AssetImporter assetImporter = AssetImporter.GetAtPath(path);
                if (assetImporter is TextureImporter)
                {
                    TextureImporter textureImporter = (TextureImporter)assetImporter;
                    TextureImporterFormat textureFormat = textureImporter.GetPlatformTextureSettings("PC").format;
#pragma warning disable CS0618
                    if (textureFormat == TextureImporterFormat.AutomaticCompressed) textureFormat = textureImporter.GetAutomaticFormat("PC");
#pragma warning restore CS0618

                    hasAlpha = textureImporter.DoesSourceTextureHaveAlpha();
                    minBPP = (hasAlpha || textureImporter.textureType == TextureImporterType.NormalMap) ? 8 : 4;

                    if (BPP.ContainsKey(textureFormat))
                    {
                        format = textureFormat.ToString();
                        bpp = BPP[textureFormat];
                        size = TextureToBytesUsingBPP(t, BPP[textureFormat]);
                    }
                    else
                    {
                        Debug.LogWarning("[Thry][VRAM] Does not have BPP for " + textureFormat);
                    }
                }
                else
                {
                    size = Profiler.GetRuntimeMemorySizeLong(t);
                    bpp = (int)(size * 8 / (t.width * t.height));
                }
            }
            else if (t is RenderTexture)
            {
                RenderTexture rt = t as RenderTexture;
                bpp = RT_BPP[rt.format] + rt.depth;
                format = rt.format.ToString();
                hasAlpha = rt.format == RenderTextureFormat.ARGB32 || rt.format == RenderTextureFormat.ARGBHalf || rt.format == RenderTextureFormat.ARGBFloat;
                size = TextureToBytesUsingBPP(t, bpp);
            }
            else
            {
                size = Profiler.GetRuntimeMemorySizeLong(t);
            }

            return (size,format, bpp, minBPP, hasAlpha);
        }

        static long TextureToBytesUsingBPP(Texture t, int bpp)
        {
            long bytes = 0;
            if (t != null && t is RenderTexture == false && t.dimension == UnityEngine.Rendering.TextureDimension.Tex2D)
            {
                double mipmaps = 1;
                for (int i = 0; i < t.mipmapCount; i++) mipmaps += Math.Pow(0.25, i + 1);
                bytes = (long)(bpp * t.width * t.height * (t.mipmapCount > 1 ? mipmaps : 1) / 8);
            }
            else if (t is RenderTexture)
            {
                RenderTexture rt = t as RenderTexture;
                double mipmaps = 1;
                for (int i = 0; i < rt.mipmapCount; i++) mipmaps += Math.Pow(0.25, i + 1);
                bytes = (long)((RT_BPP[rt.format] + rt.depth) * rt.width * rt.height * (rt.useMipMap ? mipmaps : 1) / 8);
            }
            else
            {
                bytes = Profiler.GetRuntimeMemorySizeLong(t);
            }
            return bytes;
        }
    }
}
#endif
