﻿using Bloodstone.API;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Newtonsoft.Json;
using ProjectM;
using ProjectM.Network;
using ProjectM.UI;
using Stunlock.Network;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;
using Mono;
using UnityEngine.SceneManagement;
using System.Collections;
using BepInEx.Unity.IL2CPP.Utils;
using static RPGAddOns.Core.OnUserConnectedPatch;

namespace RPGAddOns.Core
{
    [HarmonyPatch(typeof(ServerBootstrapSystem), nameof(ServerBootstrapSystem.OnUserConnected))]
    public class OnUserConnectedPatch
    {
        [HarmonyPostfix]
        public static unsafe void Postfix(ServerBootstrapSystem __instance, NetConnectionId netConnectionId)
        {
            Plugin.Logger.LogInfo("Patching...");

            try
            {
                var entityManager = __instance.EntityManager;
                var gameBootstrap = __instance._GameBootstrap;

                var helper = new Helpers();
                var userIndex = __instance._NetEndPointToApprovedUserIndex[netConnectionId];
                var serverClient = __instance._ApprovedUsersLookup[userIndex];
                var userData = serverClient.UserEntity;
                //Plugin.Logger.LogInfo($"{userData}");

                Plugin.Logger.LogInfo($"{serverClient.NetConnectionId.GetHashCode().ToString()} connected.");

                BootstrapManager.Instance.Bootstraps(netConnectionId, __instance._GameBootstrap);
                CoroutineHelper.Instance.StartCoroutine(LoadSceneIfNotLoaded(serverClient.NetConnectionId.ToString()));
                // actually do this somewhere inside loadsceneifnotloaded
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError(ex);
            }
        }

        private static IEnumerator LoadSceneIfNotLoaded(string identifier)
        {
            string sceneName = "UIEntryPoint";
            if (!SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                // only want to modify the name for it here after LoadSceneIfNotLoaded is called I think
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

                // Wait until the asynchronous scene fully loads
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }

                Plugin.Logger.LogInfo($"{identifier} scene has been loaded.");
                Scene loadedScene = SceneManager.GetSceneByName(sceneName);
                SceneManager.SetActiveScene(loadedScene);
                GameObject inventoryBackground = FindGameObjectInScene(loadedScene, "InventoryBackground");
                if (inventoryBackground != null)
                {
                    inventoryBackground.SetActive(true); // Activate the GameObject
                }
                else
                {
                    Plugin.Logger.LogError("InventoryBackground GameObject not found in the loaded scene.");
                }

                // Method to find a GameObject by name in a given scene
            }
            else
            {
                Plugin.Logger.LogInfo($"{identifier} scene is already loaded.");
            }
        }

        private static GameObject FindGameObjectInScene(Scene scene, string gameObjectName)
        {
            foreach (GameObject rootGameObject in scene.GetRootGameObjects())
            {
                Transform foundTransform = rootGameObject.transform.Find(gameObjectName);
                if (foundTransform != null)
                    return foundTransform.gameObject;
            }
            return null;
        }

        public class CoroutineHelper : MonoBehaviour
        {
            private static CoroutineHelper _instance;

            public static CoroutineHelper Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        var gameObject = new GameObject("CoroutineHelper");
                        _instance = gameObject.AddComponent<CoroutineHelper>();
                        DontDestroyOnLoad(gameObject);
                    }
                    return _instance;
                }
            }

            // Other methods as needed
        }
    }

    public class Helpers
    {
        public Il2CppSystem.Type SystemTypeGet(Type type)
        {
            return Il2CppSystem.Type.GetType(type.AssemblyQualifiedName);
        }

        public ComponentType ComponentTypeGet(string component)
        {
            return ComponentType.ReadOnly(Il2CppSystem.Type.GetType(component));
        }

        public static class AotWorkaroundUtil
        {
            // alternative for Entitymanager.HasComponent
            public static bool HasComponent<T>(Entity entity) where T : struct
            {
                return VWorld.Server.EntityManager.HasComponent(entity, ComponentType<T>());
            }

            // more convenient than Entitymanager.AddComponent
            public static bool AddComponent<T>(Entity entity) where T : struct
            {
                return VWorld.Server.EntityManager.AddComponent(entity, ComponentType<T>());
            }

            // alternative for Entitymanager.AddComponentData
            public static void AddComponentData<T>(Entity entity, T componentData) where T : struct
            {
                AddComponent<T>(entity);
                SetComponentData(entity, componentData);
            }

            // alternative for Entitymanager.RemoveComponent
            public static bool RemoveComponent<T>(Entity entity) where T : struct
            {
                return VWorld.Server.EntityManager.RemoveComponent(entity, ComponentType<T>());
            }

            // alternative for EntityMManager.GetComponentData
            public static unsafe T GetComponentData<T>(Entity entity) where T : struct
            {
                void* rawPointer = VWorld.Server.EntityManager.GetComponentDataRawRO(entity, ComponentTypeIndex<T>());
                return Marshal.PtrToStructure<T>(new System.IntPtr(rawPointer));
            }

            // alternative for EntityManager.SetComponentData
            public static unsafe void SetComponentData<T>(Entity entity, T componentData) where T : struct
            {
                var size = Marshal.SizeOf(componentData);
                //byte[] byteArray = new byte[size];
                var byteArray = StructureToByteArray(componentData);
                fixed (byte* data = byteArray)
                {
                    //UnsafeUtility.CopyStructureToPtr(ref componentData, data);
                    VWorld.Server.EntityManager.SetComponentDataRaw(entity, ComponentTypeIndex<T>(), data, size);
                }
            }

            private static ComponentType ComponentType<T>()
            {
                return new ComponentType(Il2CppType.Of<T>());
            }

            private static int ComponentTypeIndex<T>()
            {
                return ComponentType<T>().TypeIndex;
            }

            private static byte[] StructureToByteArray<T>(T structure) where T : struct
            {
                int size = Marshal.SizeOf(structure);
                byte[] byteArray = new byte[size];
                IntPtr ptr = Marshal.AllocHGlobal(size);
                try
                {
                    Marshal.StructureToPtr(structure, ptr, true);
                    Marshal.Copy(ptr, byteArray, 0, size);
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
                return byteArray;
            }
        }
    }

    public class BootstrapManager
    {
        public static BootstrapManager Instance { get; } = new BootstrapManager();
        private Dictionary<NetConnectionId, GameBootstrap> playerGameBootstrapInstances = new Dictionary<NetConnectionId, GameBootstrap>();

        public void Bootstraps(NetConnectionId connectionId, GameBootstrap gameBootstrap)
        {
            playerGameBootstrapInstances[connectionId] = gameBootstrap;
        }

        public GameBootstrap GetGameBootstrap(NetConnectionId connectionId)
        {
            if (playerGameBootstrapInstances.TryGetValue(connectionId, out GameBootstrap gameBootstrap))
            {
                return gameBootstrap;
            }
            return null;
        }
    }

    public class ScenePoolManager
    {
        private Dictionary<string, Scene> scenePool = new Dictionary<string, Scene>();

        // Load and store a scene in the pool
        public void LoadPlayerScene(string identifier)
        {
            string sceneName = "UIEntryPoint";
            if (!scenePool.ContainsKey(identifier))
            {
                CoroutineHelper.Instance.StartCoroutine(LoadSceneCoroutine(sceneName, identifier));
            }
        }

        private IEnumerator LoadSceneCoroutine(string sceneName, string identifier)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            yield return asyncLoad;

            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            SceneManager.SetActiveScene(loadedScene);
            scenePool.Add(identifier, loadedScene);
        }

        // Activate a scene from the pool
        public void ActivateScene(string sceneName)
        {
            if (scenePool.TryGetValue(sceneName, out Scene scene))
            {
                SceneManager.SetActiveScene(scene);
            }
        }

        // Deactivate a scene (but keep it in memory)
        public void DeactivateScene(string sceneName)
        {
            if (scenePool.TryGetValue(sceneName, out Scene scene))
            {
                // Add logic to deactivate the scene (e.g., hide UI elements, pause updates)
            }
        }

        // Unload a scene to free up memory
        public void UnloadScene(string sceneName)
        {
            if (scenePool.TryGetValue(sceneName, out Scene scene))
            {
                SceneManager.UnloadSceneAsync(scene);
                scenePool.Remove(sceneName);
            }
        }

        // Optional: Function to monitor memory usage and unload scenes if needed
        public void ManageMemoryUsage()
        {
            // Implement memory checks and unload scenes if memory threshold is exceeded
        }
    }
}