#if UNITY_EDITOR
using Cascade.Core;
using Cascade.Levels;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Cascade.EditorTools
{
    public static class LevelOnePrefabBuilder
    {
        private const string LevelPath = "Assets/_Cascade/Prefabs/Levels/Opening/LVL_01_TheFirstPush.prefab";
        private const string ReusableDir = "Assets/_Cascade/Prefabs/Core/Gameplay";

        [MenuItem("Cascade/Gameplay/Level 1/Build Functional Level 1", priority = 320)]
        public static void BuildLevelOne()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(LevelPath);
            if (prefab == null)
            {
                EditorUtility.DisplayDialog("Cascade Level 1", "Level 1 prefab was not found. Run 'Build Shared Gameplay + Opening 10' first.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog("Build Functional Level 1",
                    "This rebuilds the greybox contents of Level 1 while preserving the prefab asset path and Addressable reference. Continue?",
                    "Build Level 1", "Cancel"))
                return;

            BuildInternal();
            Debug.Log("[Cascade] Level 1 is now configured with placement, physics, destructible tower, scoring routes and camera anchors.");
        }

        public static void BuildInternal()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(LevelPath);
            try
            {
                var binder = root.GetComponent<LevelRuntimeBinder>() ?? root.AddComponent<LevelRuntimeBinder>();

                Transform env = EnsureRoot(root.transform, "EnvironmentRoot");
                Transform machine = EnsureRoot(root.transform, "MachineRoot");
                Transform movers = EnsureRoot(machine, "Movers");
                Transform triggers = EnsureRoot(machine, "Triggers");
                Transform transfers = EnsureRoot(machine, "Transfers");
                Transform destruction = EnsureRoot(machine, "Destruction");
                Transform placement = EnsureRoot(root.transform, "PlacementRoot");
                Transform zones = EnsureRoot(placement, "PlacementZones");
                Transform spawnPoints = EnsureRoot(placement, "ToolSpawnPoints");
                Transform objectives = EnsureRoot(root.transform, "ObjectivesRoot");
                Transform cameraRoot = EnsureRoot(root.transform, "CameraRoot");
                Transform observation = EnsureRoot(cameraRoot, "ObservationAnchor");
                Transform interests = EnsureRoot(cameraRoot, "SimulationInterests");
                Transform result = EnsureRoot(cameraRoot, "ResultAnchor");
                Transform vfx = EnsureRoot(root.transform, "VFXRoot");
                Transform lighting = EnsureRoot(root.transform, "LightingRoot");

                Clear(env); Clear(movers); Clear(triggers); Clear(transfers); Clear(destruction);
                Clear(zones); Clear(spawnPoints); Clear(objectives); Clear(interests); Clear(vfx); Clear(lighting);

                CreateGround(env);
                RestrictedFreePlacementZone zone = CreatePlacementZone(zones);
                CreateRamp(placement, zone);
                CreateBoulder(movers);
                CreateCrates(movers);
                CreateBarrels(movers);
                CreateWatchtower(destruction);
                CreateObjectiveMarker(objectives);
                ConfigureCameraAnchors(observation, interests, result);

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

                PrefabUtility.SaveAsPrefabAsset(root, LevelPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateGround(Transform parent)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.SetParent(parent, false);
            ground.transform.localPosition = new Vector3(0f, -0.5f, 2f);
            ground.transform.localScale = new Vector3(22f, 1f, 24f);

            CreateBoundary(parent, "LeftBoundary", new Vector3(-10.75f, 0.75f, 2f), new Vector3(0.5f, 1.5f, 24f));
            CreateBoundary(parent, "RightBoundary", new Vector3(10.75f, 0.75f, 2f), new Vector3(0.5f, 1.5f, 24f));
            CreateBoundary(parent, "BackBoundary", new Vector3(0f, 0.75f, -9.75f), new Vector3(22f, 1.5f, 0.5f));
        }

        private static void CreateBoundary(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
        }

        private static RestrictedFreePlacementZone CreatePlacementZone(Transform parent)
        {
            var zoneGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            zoneGo.name = "RestrictedFreePlacementZone";
            zoneGo.transform.SetParent(parent, false);
            zoneGo.transform.localPosition = new Vector3(0f, 0.05f, -3.25f);
            zoneGo.transform.localScale = new Vector3(8f, 0.1f, 5.5f);
            var collider = zoneGo.GetComponent<BoxCollider>();
            collider.isTrigger = true;
            var zone = zoneGo.AddComponent<RestrictedFreePlacementZone>();

            var renderer = zoneGo.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.color = new Color(0.15f, 0.55f, 0.8f, 0.22f);
                renderer.sharedMaterial = material;
            }
            return zone;
        }

        private static void CreateRamp(Transform parent, RestrictedFreePlacementZone zone)
        {
            GameObject ramp = InstantiateReusable("Ramp", parent);
            ramp.name = "Ramp_PlayerTool";
            ramp.transform.localPosition = new Vector3(0f, 0.55f, -3.4f);
            ramp.transform.localScale = new Vector3(3.2f, 0.28f, 4.2f);
            ramp.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);
            var tool = ramp.GetComponent<PlaceableTool>() ?? ramp.AddComponent<PlaceableTool>();
            tool.Configure(zone);
        }

        private static void CreateBoulder(Transform parent)
        {
            GameObject boulder = InstantiateReusable("Boulder", parent);
            boulder.name = "Boulder_Start";
            boulder.transform.localPosition = new Vector3(0f, 6.5f, -4.6f);
            boulder.transform.localScale = Vector3.one * 1.25f;
            Rigidbody rb = boulder.GetComponent<Rigidbody>() ?? boulder.AddComponent<Rigidbody>();
            rb.mass = 8f;
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            var emitter = boulder.GetComponent<ImpactEventEmitter>() ?? boulder.AddComponent<ImpactEventEmitter>();
            emitter.Configure(ImpactEventKind.Boulder, 1f);
        }

        private static void CreateCrates(Transform parent)
        {
            CreateCrate(parent, "Crate_A", new Vector3(2.7f, 0.55f, 0.2f));
            CreateCrate(parent, "Crate_B", new Vector3(2.55f, 0.55f, 1.35f));
            CreateCrate(parent, "Crate_C", new Vector3(1.9f, 0.55f, 2.25f));
        }

        private static void CreateCrate(Transform parent, string name, Vector3 position)
        {
            GameObject crate = InstantiateReusable("Crate", parent);
            crate.name = name;
            crate.transform.localPosition = position;
            Rigidbody rb = crate.GetComponent<Rigidbody>() ?? crate.AddComponent<Rigidbody>();
            rb.mass = 2.2f;
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            var emitter = crate.GetComponent<ImpactEventEmitter>() ?? crate.AddComponent<ImpactEventEmitter>();
            emitter.Configure(ImpactEventKind.Crate, 0.7f);
        }

        private static void CreateBarrels(Transform parent)
        {
            CreateBarrel(parent, "Barrel_A", new Vector3(-2.15f, 0.65f, 3.4f));
            CreateBarrel(parent, "Barrel_B", new Vector3(-1.15f, 0.65f, 4.2f));
            CreateBarrel(parent, "Barrel_C", new Vector3(1.2f, 0.65f, 4.1f));
        }

        private static void CreateBarrel(Transform parent, string name, Vector3 position)
        {
            GameObject barrel = InstantiateReusable("Barrel", parent);
            barrel.name = name;
            barrel.transform.localPosition = position;
            barrel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            Rigidbody rb = barrel.GetComponent<Rigidbody>() ?? barrel.AddComponent<Rigidbody>();
            rb.mass = 2.8f;
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            var emitter = barrel.GetComponent<ImpactEventEmitter>() ?? barrel.AddComponent<ImpactEventEmitter>();
            emitter.Configure(ImpactEventKind.Barrel, 0.7f);
        }

        private static void CreateWatchtower(Transform parent)
        {
            var tower = new GameObject("WoodWatchtower", typeof(DamageableStructure));
            tower.transform.SetParent(parent, false);
            tower.transform.localPosition = new Vector3(0f, 0f, 7.6f);
            tower.GetComponent<DamageableStructure>().Configure(100f, 5.5f, 0.48f);

            for (int y = 0; y < 4; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    block.name = $"TowerBlock_{y}_{x + 1}";
                    block.transform.SetParent(tower.transform, false);
                    block.transform.localPosition = new Vector3(x * 1.05f, 0.55f + y * 1.05f, 0f);
                    block.transform.localScale = new Vector3(1f, 1f, 1.35f);
                    Rigidbody rb = block.AddComponent<Rigidbody>();
                    rb.mass = 1.2f;
                    rb.isKinematic = true;
                    rb.interpolation = RigidbodyInterpolation.Interpolate;
                    block.AddComponent<DamageablePiece>();
                }
            }

            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = "TowerRoof";
            roof.transform.SetParent(tower.transform, false);
            roof.transform.localPosition = new Vector3(0f, 4.55f, 0f);
            roof.transform.localScale = new Vector3(3.6f, 0.35f, 2.2f);
            Rigidbody roofRb = roof.AddComponent<Rigidbody>();
            roofRb.mass = 2f;
            roofRb.isKinematic = true;
            roof.AddComponent<DamageablePiece>();
        }

        private static void CreateObjectiveMarker(Transform parent)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "PrimaryObjective_Watchtower";
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = new Vector3(0f, 0.04f, 7.6f);
            marker.transform.localScale = new Vector3(2.3f, 0.03f, 2.3f);
            Object.DestroyImmediate(marker.GetComponent<Collider>());
        }

        private static void ConfigureCameraAnchors(Transform observation, Transform interests, Transform result)
        {
            observation.localPosition = new Vector3(11.5f, 10f, -14.5f);
            LookAtLocal(observation, new Vector3(0f, 1.8f, 2.5f));

            interests.localPosition = new Vector3(0f, 2f, 3f);

            result.localPosition = new Vector3(10f, 7.5f, -3f);
            LookAtLocal(result, new Vector3(0f, 2.2f, 7f));
        }

        private static void LookAtLocal(Transform transform, Vector3 localTarget)
        {
            Vector3 direction = localTarget - transform.localPosition;
            if (direction.sqrMagnitude > 0.001f) transform.localRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private static GameObject InstantiateReusable(string name, Transform parent)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ReusableDir}/PF_{name}.prefab");
            GameObject instance;
            if (prefab != null)
                instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            else
                instance = GameObject.CreatePrimitive(name is "Boulder" ? PrimitiveType.Sphere : name == "Barrel" ? PrimitiveType.Cylinder : PrimitiveType.Cube);

            instance.transform.SetParent(parent, false);
            return instance;
        }

        private static Transform EnsureRoot(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            if (child != null) return child;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static void Clear(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }
}
#endif
