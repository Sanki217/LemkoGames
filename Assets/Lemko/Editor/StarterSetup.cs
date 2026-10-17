using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
namespace Lemko.Editor
{
    public static class StarterSetup
    {
        public const string ScenePath = "Assets/Lemko/Scenes/LemkoVillageUI.unity";
        static TMP_FontAsset font;
        public static bool HasTmpResources => Resources.Load<TMP_Settings>("TMP Settings") != null;
        public static void ImportTmpResources(Action continuation)
        {
            var package = Directory.GetFiles("Library/PackageCache", "TMP Essential Resources.unitypackage", SearchOption.AllDirectories).First();
            double deadline = EditorApplication.timeSinceStartup + 120;
            EditorApplication.CallbackFunction waitForImport = null;
            waitForImport = () =>
            {
                if (EditorApplication.timeSinceStartup > deadline)
                {
                    EditorApplication.update -= waitForImport;
                    Debug.LogError("TMP essentials import timed out. Check the Console and rerun setup.");
                    if (Application.isBatchMode) EditorApplication.Exit(1);
                    return;
                }
                if (EditorApplication.isUpdating || EditorApplication.isCompiling || !HasTmpResources) return;
                EditorApplication.update -= waitForImport;
                EditorApplication.delayCall += () => continuation();
            };
            EditorApplication.update += waitForImport;
            AssetDatabase.ImportPackage(package, false);
        }
        [MenuItem("Lemko/Create starter scene (preserve existing)")]
        public static void Create()
        {
            if (File.Exists(ScenePath)) { Debug.Log("Starter scene already exists; preserved. Open " + ScenePath); return; }
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!AssetDatabase.LoadAssetAtPath<ShopDefinition>(VocabularyImporter.Root + "/Shop.asset"))
                VocabularyImporter.Import(Directory.Exists("Content/CSV") ? "Content/CSV" : "Content/Starter");
            if (!HasTmpResources) { ImportTmpResources(Create); return; }
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Lemko/Fonts/LemkoFont.asset");
            if (!font)
            {
                var source = AssetDatabase.LoadAssetAtPath<Font>("Assets/Lemko/Fonts/Roboto-Regular.ttf");
                font = TMP_FontAsset.CreateFontAsset(source, 48, 6, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 2048, 2048);
                font.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                AssetDatabase.CreateAsset(font, "Assets/Lemko/Fonts/LemkoFont.asset");
                AssetDatabase.AddObjectToAsset(font.material, font);
                foreach (var texture in font.atlasTextures) AssetDatabase.AddObjectToAsset(texture, font);
                var characters = string.Concat(Directory.GetFiles("Content", "*.csv", SearchOption.AllDirectories).Select(File.ReadAllText)) + "ąćęłńóśźżĄĆĘŁŃÓŚŹŻABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789[]:!?/-.,";
                if (!font.TryAddCharacters(new string(characters.Where(c => !char.IsControl(c)).Distinct().ToArray()), out string missing))
                    throw new InvalidOperationException("Font lacks characters: " + missing);
                EditorUtility.SetDirty(font);
            }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Camera", typeof(Camera));
            var camera = cameraObject.GetComponent<Camera>(); camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.12f,.22f,.19f);
            var canvasObject = new GameObject("Lemko UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var root = canvasObject.transform;
            var services = new GameObject("Session", typeof(GameSession), typeof(PronunciationService), typeof(TooltipService));
            var session = services.GetComponent<GameSession>(); session.audioService = services.GetComponent<PronunciationService>();
            services.AddComponent<MilestoneSmoke>().session = session;
            services.GetComponent<AudioSource>().playOnAwake = false;
            session.tooltip = services.GetComponent<TooltipService>();
            session.shop = AssetDatabase.LoadAssetAtPath<ShopDefinition>(VocabularyImporter.Root + "/Shop.asset");
            session.hud = Text(root, "HUD", "Lives", new Vector2(380, 45), new Vector2(-410, 325), 22);
            session.muteButton = Button(root, "Audio", new Vector2(150, 44), new Vector2(190, 325), session.ToggleMute);
            session.languageButton = Button(root, "Hints", new Vector2(180, 44), new Vector2(370, 325), session.ToggleLanguage);
            Button(root, "Village", new Vector2(140, 44), new Vector2(545, 325), session.Village);
            session.villagePanel = Panel(root, "Village", new Color(.24f,.38f,.29f));
            var village = session.villagePanel.transform;
            Text(village, "Title", "LEMKO VILLAGE", new Vector2(900,70), new Vector2(0,245),42);
            Text(village, "Subtitle", "A small place to learn - development milestone 1", new Vector2(1000,45), new Vector2(0,190),24);
            string[] locations = {"Shop", "Family Tree", "Farm", "House", "Memory", "Numbers", "Alphabet", "Weather"};
            for (int i = 0; i < locations.Length; i++)
            {
                var b = Button(village, locations[i] + (i == 0 ? "\nEnter" : "\nPlanned"), new Vector2(250,130), new Vector2(-420 + (i % 4) * 280, 65 - (i / 4) * 175), i == 0 ? (UnityAction)session.OpenShop : null);
                b.interactable = i == 0;
            }
            session.villageStatus = Text(village,"Progress", "",new Vector2(1150,70),new Vector2(0,-250),22);
            session.shopPanel = Panel(root, "Shop", new Color(.17f,.27f,.24f));
            var shopRoot = session.shopPanel.transform;
            var controller = session.shopPanel.AddComponent<ShopController>(); session.shopController = controller; controller.definition = session.shop;
            controller.feedback = Text(shopRoot,"Feedback", "",new Vector2(1160,85),new Vector2(0,235),22);
            controller.shoppingList = Text(shopRoot,"Shopping list", "",new Vector2(360,390),new Vector2(430,-25),27);
            controller.shoppingList.alignment = TextAlignmentOptions.TopLeft;
            controller.productRoot = Rect(shopRoot,"Products",new Vector2(810,430),new Vector2(-185,-35));
            var grid = controller.productRoot.gameObject.AddComponent<GridLayoutGroup>(); grid.cellSize = new Vector2(190,130); grid.spacing = new Vector2(12,12);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 4;
            controller.wordPrefab = CreateWordPrefab(root);
            controller.finishButton = Button(shopRoot,"View results",new Vector2(280,50),new Vector2(420,-265),controller.Complete);
            session.resultsPanel = Panel(root,"Results", new Color(.2f,.33f,.28f));
            session.results = Text(session.resultsPanel.transform,"Results", "",new Vector2(1000,240),new Vector2(0,70),36);
            Button(session.resultsPanel.transform,"Shop again",new Vector2(250,65),new Vector2(-160,-170),session.OpenShop);
            Button(session.resultsPanel.transform,"Village",new Vector2(250,65),new Vector2(160,-170),session.Village);
            session.exhaustedPanel = Panel(root,"Zero lives",new Color(.27f,.18f,.18f));
            Text(session.exhaustedPanel.transform,"Message","No lives left\nKeep learning in practice, or start a fresh attempt.",new Vector2(1100,200),new Vector2(0,90),32);
            Button(session.exhaustedPanel.transform,"Fresh attempt",new Vector2(300,70),new Vector2(-180,-100),session.FreshAttempt);
            Button(session.exhaustedPanel.transform,"Unlimited practice",new Vector2(300,70),new Vector2(180,-100),session.StartPractice);
            var tip = Rect(root,"Translation tooltip",new Vector2(340,90),Vector2.zero);
            tip.gameObject.AddComponent<Image>().color = new Color(.08f,.12f,.1f,.98f);
            session.tooltip.box = tip; session.tooltip.group = tip.gameObject.AddComponent<CanvasGroup>();
            session.tooltip.group.blocksRaycasts = false;
            session.tooltip.label = Text(tip,"Translation","",new Vector2(320,80),Vector2.zero,24);
            tip.gameObject.SetActive(false);
            session.shopPanel.SetActive(false); session.resultsPanel.SetActive(false); session.exhaustedPanel.SetActive(false);
            AssetDatabase.SaveAssets(); EditorSceneManager.SaveScene(scene,ScenePath);
            Debug.Log("LEMKO_SETUP_OK: " + ScenePath + "; font source covers supplied CSV characters.");
        }
        static WordView CreateWordPrefab(Transform root)
        {
            string path = "Assets/Lemko/Prefabs/Words/UIWord.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path); if (existing) return existing.GetComponent<WordView>();
            var rect = Rect(root,"WordView",new Vector2(190,130),Vector2.zero);
            var view = rect.gameObject.AddComponent<WordView>();
            view.selectButton = Button(rect,"Select",new Vector2(190,95),new Vector2(0,17),null);
            view.label = view.selectButton.GetComponentInChildren<TMP_Text>(); view.label.fontSize = 20;
            var picture = Rect(view.selectButton.transform,"Picture",new Vector2(32,32),new Vector2(-72,27));
            view.picture = picture.gameObject.AddComponent<Image>(); view.picture.raycastTarget = false;
            view.audioButton = Button(rect,"Speak",new Vector2(190,30),new Vector2(0,-50),null);
            var saved = PrefabUtility.SaveAsPrefabAsset(rect.gameObject,path); UnityEngine.Object.DestroyImmediate(rect.gameObject);
            return saved.GetComponent<WordView>();
        }
        static GameObject Panel(Transform parent,string name,Color color)
        {
            var rect = Rect(parent,name,new Vector2(1280,630),new Vector2(0,-40)); rect.gameObject.AddComponent<Image>().color = color; return rect.gameObject;
        }
        static RectTransform Rect(Transform parent,string name,Vector2 size,Vector2 position)
        {
            var go = new GameObject(name,typeof(RectTransform)); var rect = (RectTransform)go.transform; rect.SetParent(parent,false); rect.sizeDelta=size; rect.anchoredPosition=position; return rect;
        }
        static TMP_Text Text(Transform parent,string name,string value,Vector2 size,Vector2 position,int fontSize)
        {
            var rect = Rect(parent,name,size,position); var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font=font; text.text=value; text.fontSize=fontSize; text.color=Color.white; text.alignment=TextAlignmentOptions.Center; text.raycastTarget=false;
            return text;
        }
        static Button Button(Transform parent,string text,Vector2 size,Vector2 position,UnityAction action)
        {
            var rect = Rect(parent,text,size,position); rect.gameObject.AddComponent<Image>().color = new Color(.39f,.53f,.39f);
            var button = rect.gameObject.AddComponent<Button>(); var label=Text(rect,"Label",text,size- new Vector2(12,4),Vector2.zero,22);
            if(action != null) UnityEventTools.AddPersistentListener(button.onClick,action); return button;
        }
        [MenuItem("Lemko/Build Windows milestone")]
        public static void Build()
        {
            if (!File.Exists(ScenePath)) Create();
            var report = BuildPipeline.BuildPlayer(new[]{ScenePath},"Builds/Windows/LemkoVillage.exe",BuildTarget.StandaloneWindows64,BuildOptions.Development);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new Exception("Windows build failed: " + report.summary.result);
            Debug.Log("LEMKO_BUILD_OK");
        }
    }
}

