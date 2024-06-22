using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace HungerNoFear_NativeModloader
{
    public class WorldBoxMod : MonoBehaviour
    {
        public void Awake()
        {
            Debug.Log($"{MethodBase.GetCurrentMethod().DeclaringType.Namespace} loading...");
            string path = Path.Combine(Application.streamingAssetsPath, "Mods");
            path = Path.Combine(path, "stuffthatjeansmodsuse");
            if (!Directory.Exists(path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                directoryInfo.Create();
                directoryInfo.Attributes |= FileAttributes.Hidden;
            }

            if (!File.Exists(Path.Combine(path, "0Harmony.dll"))) { File.WriteAllBytes(Path.Combine(path, "0Harmony.dll"), Assemblies.Resource._0Harmony); }
            if (!File.Exists(Path.Combine(path, "Mono.Cecil.dll"))) { File.WriteAllBytes(Path.Combine(path, "Mono.Cecil.dll"), Assemblies.Resource.Mono_Cecil); }
            if (!File.Exists(Path.Combine(path, "MonoMod.Utils.dll"))) { File.WriteAllBytes(Path.Combine(path, "MonoMod.Utils.dll"), Assemblies.Resource.MonoMod_Utils); }
            if (!File.Exists(Path.Combine(path, "MonoMod.RuntimeDetour.dll"))) { File.WriteAllBytes(Path.Combine(path, "MonoMod.RuntimeDetour.dll"), Assemblies.Resource.MonoMod_RuntimeDetour); }

            Assembly.LoadFrom(Path.Combine(path, "0Harmony.dll"));
            Assembly.LoadFrom(Path.Combine(path, "Mono.Cecil.dll"));
            Assembly.LoadFrom(Path.Combine(path, "MonoMod.Utils.dll"));
            Assembly.LoadFrom(Path.Combine(path, "MonoMod.RuntimeDetour.dll"));

            Debug.Log($"{MethodBase.GetCurrentMethod().DeclaringType.Namespace} loaded!");
            GameObject gameObject = new GameObject(MethodBase.GetCurrentMethod().DeclaringType.Namespace);
            DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<Main>();
        }
    }

    internal class Main : MonoBehaviour
    {
        public static Harmony harmony = new Harmony(MethodBase.GetCurrentMethod().DeclaringType.Namespace);
        private bool _initialized = false;

        public void Update()
        {
            if (global::Config.gameLoaded && !_initialized)
            {
                harmony.Patch(AccessTools.Method(typeof(Actor), "updateHunger"),
                transpiler: new HarmonyMethod(AccessTools.Method(typeof(Patches), "updateHunger_Transpiler")));

                _initialized = true;
            }
        }
    }

    public class Patches
    {
        public static IEnumerable<CodeInstruction> updateHunger_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            int index = codes.FindIndex(instr => instr.opcode == OpCodes.Ldc_I4_M1);

            if (index == -1)
            {
                Console.WriteLine("updateHunger_Transpiler: index not found");
                return codes.AsEnumerable();
            }

            codes.RemoveAt(index);

            codes.Insert(index, new CodeInstruction(OpCodes.Ldc_I4, -12));

            return codes.AsEnumerable();
        }
    }
}
