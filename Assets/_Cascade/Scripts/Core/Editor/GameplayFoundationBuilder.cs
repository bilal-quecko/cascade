#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Cascade.CameraSystem;
using Cascade.Levels;
using Cascade.Simulation;
using Cascade.State;
using Cascade.UI;
using TMPro;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Cascade.EditorTools
{
    /// <summary>
    /// Builds the shared Gameplay scene, reusable greybox prefabs, Addressable opening-level prefabs,
    /// and LevelDefinitionSO assets for Levels 1-10. This is a foundation/greybox generator, not final art.
    /// </summary>
    public static class GameplayFoundationBuilder
    {
        private const string GameplayScenePath = "Assets/_Cascade/Scenes/SCN_Gameplay.unity";
        private const string LevelPrefabDir = "Assets/_Cascade/Prefabs/Levels/Opening";
        private const string LevelDataDir = "Assets/_Cascade/Data/Levels/Opening";
        private const string ReusableDir = "Assets/_Cascade/Prefabs/Core/Gameplay";
        private const string CatalogPath = LevelDataDir + "/SO_LevelCatalog_Opening.asset";

        private sealed class LevelSpec
        {
            public string Id;
            public string Name;
            public PuzzleCategory Category;
            public string Objective;
            public string Teaching;
            public string Tools;
            public string Cascade;
            public int MaxTools;
            public string[] Pieces;
        }

        private static readonly LevelSpec[] Levels =
        {
            new() { Id="L01", Name="The First Push", Category=PuzzleCategory.Place|PuzzleCategory.Aim, Objective="Destroy the wooden watchtower.", Teaching="Restricted ramp placement + rotation; indirect routes can outperform direct impact.", Tools="1 Ramp", Cascade="Boulder -> Ramp -> direct tower OR barrels OR crates -> barrels -> watchtower.", MaxTools=1, Pieces=new[]{"Boulder","Crate","Crate","Barrel","Barrel","WoodTower","Ramp"}},
            new() { Id="L02", Name="Heavy Delivery", Category=PuzzleCategory.Place|PuzzleCategory.Aim, Objective="Collapse the warehouse.", Teaching="Momentum and secondary destruction.", Tools="Ramp + Movable Weight", Cascade="Cart -> crates -> bell -> release -> weight -> warehouse.", MaxTools=2, Pieces=new[]{"Cart","Crate","Crate","Bell","ReleaseLatch","MovableWeight","Warehouse","Shed","Ramp"}},
            new() { Id="L03", Name="Swing Into Action", Category=PuzzleCategory.Trigger, Objective="Destroy the stone gate.", Teaching="Choose the pendulum cut/release location.", Tools="1 Rope Cutter", Cascade="Pendulum heavy ball -> crates -> cart -> stone gate.", MaxTools=1, Pieces=new[]{"Ball","Pendulum","RopeCutterTarget","RopeCutterTarget","RopeCutterTarget","Crate","Cart","StoneGate"}},
            new() { Id="L04", Name="Knock Knock", Category=PuzzleCategory.Choose, Objective="Activate the chain and destroy the house.", Teaching="Different tool classes can solve the same linkage.", Tools="Hammer / Spring / Fan; choose 1", Cascade="Ball -> chosen tool -> bell -> release -> boulder -> house.", MaxTools=1, Pieces=new[]{"Ball","Bell","ReleaseLatch","Boulder","WoodTower","Hammer","Spring","Fan"}},
            new() { Id="L05", Name="Weakest Link", Category=PuzzleCategory.Aim, Objective="Collapse the bridge.", Teaching="Attack the structural dependency, not necessarily the obvious target.", Tools="Impact Weight / Wrecking Ball", Cascade="Impact support -> bridge folds -> cart falls -> dam lever/water release.", MaxTools=1, Pieces=new[]{"MovableWeight","Bridge","Cart","ReleaseLatch","WaterGate"}},
            new() { Id="L06", Name="Flood Route", Category=PuzzleCategory.Connect, Objective="Destroy both abandoned mills.", Teaching="Split one controlled water source across two connected paths.", Tools="2 Barriers", Cascade="Water split -> Mill A AND WaterWheel -> Hammer -> Mill B + Shed.", MaxTools=2, Pieces=new[]{"WaterEmitter","Barrier","Barrier","WaterWheel","Hammer","WoodTower","WoodTower","Shed"}},
            new() { Id="L07", Name="Up, Not Down", Category=PuzzleCategory.Place|PuzzleCategory.Aim, Objective="Destroy the tower.", Teaching="Cascades can travel upward before dropping force back down.", Tools="Spring + Movable Crate", Cascade="Ball -> spring -> tether release -> weight drop -> boulder -> tower.", MaxTools=2, Pieces=new[]{"Ball","Spring","Crate","ReleaseLatch","MovableWeight","Boulder","WoodTower"}},
            new() { Id="L08", Name="Falling Bridge", Category=PuzzleCategory.Connect, Objective="Destroy the tower below the bridge.", Teaching="Multiple entry routes with a hidden bridge/water secondary chain.", Tools="Ramp + Weight", Cascade="Boulder -> cart -> boxes -> hammer -> bridge -> tower -> waterwheel secondary structure.", MaxTools=2, Pieces=new[]{"Boulder","Gate","Cart","Crate","Crate","Hammer","Bridge","WoodTower","WaterWheel","Shed","Ramp","MovableWeight"}},
            new() { Id="L09", Name="Two Birds", Category=PuzzleCategory.MultiConnect, Objective="Destroy Tower A and Tower B.", Teaching="One impact should branch into asynchronous cascades.", Tools="Seesaw + Weight", Cascade="Ball -> seesaw -> left boulder -> Tower A AND right launch/cart -> Tower B.", MaxTools=2, Pieces=new[]{"Ball","Seesaw","MovableWeight","Boulder","Cart","WoodTower","WoodTower"}},
            new() { Id="L10", Name="The Clock Tower", Category=PuzzleCategory.Choose|PuzzleCategory.Connect, Objective="Destroy the clock tower and maximize the machine chain.", Teaching="First showcase: choose two of three tools and discover the machine connections.", Tools="Ramp + Rope Cutter + Movable Weight; use only 2", Cascade="Boulder -> bell/gear -> pendulum/cart/water gate/waterwheel -> hammer -> bridge -> clock tower.", MaxTools=2, Pieces=new[]{"Boulder","Gear","Bell","WaterGate","Pendulum","WaterWheel","Crate","Crate","Hammer","Cart","Bridge","ClockTower","Ramp","RopeCutterTarget","MovableWeight"}}
        };

        [MenuItem("Cascade/Gameplay/Build Shared Gameplay + Opening 10", priority = 300)]
        public static void BuildAll()
        {
            if (!EditorUtility.DisplayDialog("Build Cascade Gameplay Foundation",
                    "This rebuilds the shared Gameplay scene and the generated greybox opening-level prefabs/data. Continue?",
                    "Build", "Cancel"))
                return;

            EnsureDir(ReusableDir);
            EnsureDir(LevelPrefabDir);
            EnsureDir(LevelDataDir);

            CreateReusablePrefabs();
            var definitions = CreateOpeningLevels();
            CreateCatalog(definitions);
            CreateGameplayScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Cascade] Shared Gameplay scene + reusable greybox library + opening Levels 1-10 created.");
        }

        private static void CreateReusablePrefabs()
        {
            CreatePrimitivePrefab("Boulder", PrimitiveType.Sphere, new Vector3(1.4f,1.4f,1.4f), true);
            CreatePrimitivePrefab("Ball", PrimitiveType.Sphere, Vector3.one, true);
            CreatePrimitivePrefab("Cart", PrimitiveType.Cube, new Vector3(2.2f,0.7f,1.3f), true);
            CreatePrimitivePrefab("MovableWeight", PrimitiveType.Cube, new Vector3(1.3f,1.3f,1.3f), true);
            CreatePrimitivePrefab("Crate", PrimitiveType.Cube, Vector3.one, true);
            CreatePrimitivePrefab("Barrel", PrimitiveType.Cylinder, new Vector3(0.8f,1.2f,0.8f), true);
            CreatePrimitivePrefab("Ramp", PrimitiveType.Cube, new Vector3(3f,0.25f,2f), false);
            CreatePrimitivePrefab("Spring", PrimitiveType.Cylinder, new Vector3(1.2f,0.35f,1.2f), false);
            CreatePrimitivePrefab("Seesaw", PrimitiveType.Cube, new Vector3(4f,0.25f,1f), false);
            CreatePrimitivePrefab("Fan", PrimitiveType.Cylinder, new Vector3(1.2f,0.35f,1.2f), false);
            CreatePrimitivePrefab("Bell", PrimitiveType.Sphere, new Vector3(0.8f,0.8f,0.8f), false);
            CreatePrimitivePrefab("RopeCutterTarget", PrimitiveType.Cube, new Vector3(0.25f,0.25f,0.25f), false);
            CreatePrimitivePrefab("ReleaseLatch", PrimitiveType.Cube, new Vector3(0.5f,0.5f,0.5f), false);
            CreatePrimitivePrefab("Gate", PrimitiveType.Cube, new Vector3(2.5f,2f,0.3f), false);
            CreatePrimitivePrefab("Pendulum", PrimitiveType.Sphere, new Vector3(1.4f,1.4f,1.4f), true);
            CreatePrimitivePrefab("Hammer", PrimitiveType.Cube, new Vector3(2f,0.6f,0.7f), true);
            CreatePrimitivePrefab("Gear", PrimitiveType.Cylinder, new Vector3(1.5f,0.4f,1.5f), false);
            CreatePrimitivePrefab("WaterWheel", PrimitiveType.Cylinder, new Vector3(2f,0.5f,2f), false);
            CreatePrimitivePrefab("WoodTower", PrimitiveType.Cube, new Vector3(3f,5f,3f), false);
            CreatePrimitivePrefab("Warehouse", PrimitiveType.Cube, new Vector3(6f,3.5f,5f), false);
            CreatePrimitivePrefab("StoneGate", PrimitiveType.Cube, new Vector3(5f,4f,1f), false);
            CreatePrimitivePrefab("Bridge", PrimitiveType.Cube, new Vector3(8f,0.7f,3f), false);
            CreatePrimitivePrefab("ClockTower", PrimitiveType.Cube, new Vector3(4f,8f,4f), false);
            CreatePrimitivePrefab("Shed", PrimitiveType.Cube, new Vector3(3f,2.5f,3f), false);
            CreatePrimitivePrefab("WaterEmitter", PrimitiveType.Cube, new Vector3(3f,0.5f,2f), false);
            CreatePrimitivePrefab("Barrier", PrimitiveType.Cube, new Vector3(2.5f,1f,0.25f), false);
            CreatePrimitivePrefab("WaterGate", PrimitiveType.Cube, new Vector3(3f,2f,0.3f), false);
        }

        private static void CreatePrimitivePrefab(string name, PrimitiveType type, Vector3 scale, bool rigidbody)
        {
            string path = $"{ReusableDir}/PF_{name}.prefab";
            var go = GameObject.CreatePrimitive(type);
            go.name = $"PF_{name}";
            go.transform.localScale = scale;
            if (rigidbody)
            {
                var rb = go.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = name is "Boulder" or "Ball" or "MovableWeight" ? CollisionDetectionMode.ContinuousDynamic : CollisionDetectionMode.Discrete;
            }
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static List<LevelDefinitionSO> CreateOpeningLevels()
        {
            var definitions = new List<LevelDefinitionSO>();
            for (int i = 0; i < Levels.Length; i++)
            {
                var spec = Levels[i];
                string safe = spec.Name.Replace(" ", string.Empty).Replace(",", string.Empty);
                string prefabPath = $"{LevelPrefabDir}/LVL_{i+1:00}_{safe}.prefab";
                CreateLevelPrefab(spec, i + 1, prefabPath);
                MarkAddressable(prefabPath, $"levels/opening/L{i+1:00}_{safe}");

                string assetPath = $"{LevelDataDir}/SO_Level_{i+1:00}_{safe}.asset";
                var def = AssetDatabase.LoadAssetAtPath<LevelDefinitionSO>(assetPath);
                if (def == null)
                {
                    def = ScriptableObject.CreateInstance<LevelDefinitionSO>();
                    AssetDatabase.CreateAsset(def, assetPath);
                }

                def.levelId = spec.Id;
                def.displayName = spec.Name;
                def.sequenceIndex = i + 1;
                def.worldId = "opening";
                def.puzzleCategories = spec.Category;
                def.primaryObjective = spec.Objective;
                def.teaching = spec.Teaching;
                def.toolSummary = spec.Tools;
                def.targetCascade = spec.Cascade;
                def.maxToolsUsable = spec.MaxTools;
                def.nextLevelId = i < Levels.Length - 1 ? Levels[i + 1].Id : string.Empty;
                def.productionReady = false;
                def.levelPrefab = new UnityEngine.AddressableAssets.AssetReferenceGameObject(AssetDatabase.AssetPathToGUID(prefabPath));
                EditorUtility.SetDirty(def);
                definitions.Add(def);
            }
            return definitions;
        }

        private static void CreateLevelPrefab(LevelSpec spec, int index, string path)
        {
            var root = new GameObject($"LVL_{index:00}_{spec.Name.Replace(" ", string.Empty)}", typeof(LevelRuntimeBinder));
            var env = Child(root.transform, "EnvironmentRoot");
            var machine = Child(root.transform, "MachineRoot");
            var movers = Child(machine, "Movers");
            var triggers = Child(machine, "Triggers");
            var transfers = Child(machine, "Transfers");
            var destruction = Child(machine, "Destruction");
            var placement = Child(root.transform, "PlacementRoot");
            var zones = Child(placement, "PlacementZones");
            Child(placement, "ToolSpawnPoints");
            var objectives = Child(root.transform, "ObjectivesRoot");
            var cameraRoot = Child(root.transform, "CameraRoot");
            var observation = Child(cameraRoot, "ObservationAnchor");
            var interests = Child(cameraRoot, "SimulationInterests");
            var result = Child(cameraRoot, "ResultAnchor");
            var vfx = Child(root.transform, "VFXRoot");
            var lighting = Child(root.transform, "LightingRoot");

            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.SetParent(env, false);
            ground.transform.localPosition = new Vector3(0,-0.5f,0);
            ground.transform.localScale = new Vector3(24,1,18);

            var zone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            zone.name = "PlacementZone_Greybox";
            zone.transform.SetParent(zones, false);
            zone.transform.localPosition = new Vector3(-4,0.05f,-2);
            zone.transform.localScale = new Vector3(6,0.1f,5);
            var zoneCollider = zone.GetComponent<Collider>();
            if (zoneCollider != null) zoneCollider.isTrigger = true;

            for (int i = 0; i < spec.Pieces.Length; i++)
            {
                string piece = spec.Pieces[i];
                Transform parent = ClassifyParent(piece, movers, triggers, transfers, destruction, placement);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ReusableDir}/PF_{piece}.prefab");
                if (prefab == null) continue;
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.name = $"{piece}_{i+1:00}";
                instance.transform.SetParent(parent, false);
                float x = -8f + (i % 5) * 4f;
                float z = -3f + (i / 5) * 4f;
                float y = piece.Contains("Tower") ? 2.5f : piece == "Warehouse" ? 1.75f : piece == "ClockTower" ? 4f : piece == "Bridge" ? 2f : 1f;
                instance.transform.localPosition = new Vector3(x, y, z);
            }

            var objectiveMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            objectiveMarker.name = "PrimaryObjectiveMarker";
            objectiveMarker.transform.SetParent(objectives, false);
            objectiveMarker.transform.localPosition = new Vector3(7,1,4);
            objectiveMarker.transform.localScale = Vector3.one * 0.5f;
            Object.DestroyImmediate(objectiveMarker.GetComponent<Collider>());

            observation.localPosition = new Vector3(14,14,-18);
            observation.localRotation = Quaternion.Euler(28,-35,0);
            result.localPosition = new Vector3(12,10,-14);
            result.localRotation = Quaternion.Euler(24,-35,0);
            interests.localPosition = new Vector3(0,2,0);

            var binder = root.GetComponent<LevelRuntimeBinder>();
            binder.environmentRoot = env;
            binder.machineRoot = machine;
            binder.placementRoot = placement;
            binder.objectivesRoot = objectives;
            binder.cameraRoot = cameraRoot;
            binder.vfxRoot = vfx;
            binder.lightingRoot = lighting;
            binder.observationAnchor = observation;
            binder.simulationInterestRoot = interests;
            binder.resultAnchor = result;

            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static Transform ClassifyParent(string piece, Transform movers, Transform triggers, Transform transfers, Transform destruction, Transform placement)
        {
            if (piece is "Ramp" or "Spring" or "Seesaw" or "Fan" or "Barrier" or "RopeCutterTarget" or "MovableWeight") return placement;
            if (piece is "Bell" or "ReleaseLatch" or "Gate" or "WaterGate") return triggers;
            if (piece is "Pendulum" or "Hammer" or "Gear" or "WaterWheel") return transfers;
            if (piece is "WoodTower" or "Warehouse" or "StoneGate" or "Bridge" or "ClockTower" or "Shed") return destruction;
            return movers;
        }

        private static void CreateCatalog(List<LevelDefinitionSO> definitions)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalogSO>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<LevelCatalogSO>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            catalog.levels = definitions;
            EditorUtility.SetDirty(catalog);
        }

        private static void CreateGameplayScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var context = new GameObject("GameplayContext", typeof(Cascade.Core.GameplayContext));
            var stateGo = SystemObject<GameStateManager>(context.transform, "GameStateManager");
            var levelGo = SystemObject<LevelManager>(context.transform, "LevelManager");
            var placementGo = SystemObject<Cascade.Core.PlacementController>(context.transform, "PlacementController");
            var simulationGo = SystemObject<SimulationController>(context.transform, "SimulationController");
            SystemObject<Cascade.Core.ObjectiveManager>(context.transform, "ObjectiveManager");
            SystemObject<Cascade.Core.CascadeScoreManager>(context.transform, "CascadeScoreManager");
            SystemObject<Cascade.Core.ReactionEventBus>(context.transform, "ReactionEventBus");
            var cameraDirectorGo = SystemObject<CameraDirector>(context.transform, "CameraDirector");
            SystemObject<Cascade.Core.FeedbackManager>(context.transform, "FeedbackManager");
            SystemObject<Cascade.Core.AudioManager>(context.transform, "AudioManager");
            SystemObject<Cascade.Core.HapticManager>(context.transform, "HapticManager");
            SystemObject<Cascade.Core.PoolManager>(context.transform, "PoolManager");
            SystemObject<Cascade.Core.GameplayUIManager>(context.transform, "UIManager");

            var levelContainer = new GameObject("LevelContainer").transform;
            levelContainer.SetParent(context.transform, false);

            var cameraGo = new GameObject("GameplayCamera", typeof(Camera), typeof(AudioListener));
            cameraGo.tag = "MainCamera";
            cameraGo.transform.position = new Vector3(14,14,-18);
            cameraGo.transform.rotation = Quaternion.Euler(28,-35,0);

            CreateLighting();
            var hud = CreateHUD(context.transform);

            var levelManager = levelGo.GetComponent<LevelManager>();
            var gameState = stateGo.GetComponent<GameStateManager>();
            var simulation = simulationGo.GetComponent<SimulationController>();
            var cameraDirector = cameraDirectorGo.GetComponent<CameraDirector>();
            var hudController = hud.GetComponent<GameplayHUDController>();
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalogSO>(CatalogPath);

            SetObject(levelManager, "catalog", catalog);
            SetObject(levelManager, "levelContainer", levelContainer);
            SetObject(levelManager, "gameStateManager", gameState);
            SetObject(simulation, "gameStateManager", gameState);
            SetObject(simulation, "levelManager", levelManager);
            SetObject(cameraDirector, "gameplayCamera", cameraGo.GetComponent<Camera>());
            SetObject(cameraDirector, "levelManager", levelManager);
            SetObject(hudController, "simulationController", simulation);
            SetObject(hudController, "levelManager", levelManager);
            SetObject(hudController, "gameStateManager", gameState);

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            EditorSceneManager.SaveScene(scene, GameplayScenePath);
        }

        private static GameObject CreateHUD(Transform parent)
        {
            var root = new GameObject("GameplayHUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(GameplayHUDController));
            root.transform.SetParent(parent, false);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080,1920);
            scaler.matchWidthOrHeight = 0.5f;

            var levelText = CreateTMP(root.transform, "LevelText", "01  THE FIRST PUSH", 38, new Vector2(0,820), new Vector2(900,80));
            var objectiveText = CreateTMP(root.transform, "ObjectiveText", "Destroy the wooden watchtower.", 30, new Vector2(0,735), new Vector2(900,100));
            var prep = Child(root.transform, "PreparationControls").gameObject;
            Stretch(prep.GetComponent<RectTransform>() ?? prep.AddComponent<RectTransform>());
            var start = CreateButton(prep.transform, "StartCascadeButton", "START CASCADE", new Vector2(0,-720), new Vector2(620,130));
            CreateButton(prep.transform, "RotateButton", "ROTATE", new Vector2(-240,-560), new Vector2(260,90));
            CreateButton(prep.transform, "ResetPlacementButton", "RESET", new Vector2(240,-560), new Vector2(260,90));

            var result = Child(root.transform, "ResultPanel").gameObject;
            Stretch(result.GetComponent<RectTransform>() ?? result.AddComponent<RectTransform>());
            CreateTMP(result.transform, "ResultTitle", "CASCADE COMPLETE", 54, new Vector2(0,120), new Vector2(900,100));
            var replay = CreateButton(result.transform, "ReplayButton", "REPLAY", new Vector2(-220,-80), new Vector2(320,100));
            var menu = CreateButton(result.transform, "MenuButton", "MENU", new Vector2(220,-80), new Vector2(320,100));
            result.SetActive(false);

            var controller = root.GetComponent<GameplayHUDController>();
            SetObject(controller, "objectiveText", objectiveText);
            SetObject(controller, "levelText", levelText);
            SetObject(controller, "preparationControls", prep);
            SetObject(controller, "resultPanel", result);
            SetObject(controller, "startCascadeButton", start);
            SetObject(controller, "replayButton", replay);
            SetObject(controller, "menuButton", menu);
            return root;
        }

        private static TMP_Text CreateTMP(Transform parent, string name, string value, float fontSize, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f,0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = value;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            return tmp;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f,0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            var image = go.GetComponent<Image>();
            image.color = new Color(0.12f,0.16f,0.19f,0.95f);
            var labelText = CreateTMP(go.transform, "Label", label, 30, Vector2.zero, size);
            labelText.raycastTarget = false;
            return go.GetComponent<Button>();
        }

        private static void CreateLighting()
        {
            var light = new GameObject("Directional Light", typeof(Light));
            light.transform.rotation = Quaternion.Euler(45,-30,0);
            light.GetComponent<Light>().type = LightType.Directional;
            light.GetComponent<Light>().intensity = 1.2f;
        }

        private static GameObject SystemObject<T>(Transform parent, string name) where T : Component
        {
            var go = new GameObject(name, typeof(T));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static Transform Child(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static void MarkAddressable(string assetPath, string address)
        {
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            if (settings == null) return;
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            var entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
            entry.address = address;
            entry.SetLabel("level_opening_01_10", true, true);
            EditorUtility.SetDirty(settings);
        }

        private static void SetObject(Object target, string property, Object value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(property);
            if (p != null) p.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureDir(string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
#endif
