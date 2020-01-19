using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

//C:\platform-tools\adb.exe install F:\Users\IZQUIERDO\Documents\ChickenWhackRepo\ChickenWhack\Builds\ChickenWhack.arm64-v8a.apk

static class BuildUtil
{
    enum BuildType { Development, DevelopmentOptimized, Release }

    [MenuItem("AleMC/Build/Development")]
    static void b0()
    {
        Build(false, BuildType.Development);
    }
    [MenuItem("AleMC/Build/Development Optimized")]
    static void b1()
    {
        Build(false, BuildType.DevelopmentOptimized);
    }
    [MenuItem("AleMC/Build/Release")]
    static void b2()
    {
        Build(false, BuildType.Release);
    }
    [MenuItem("AleMC/Build And Run/Development")]
    static void b3()
    {
        Build(true, BuildType.Development);
    }
    [MenuItem("AleMC/Build And Run/Development Optimized")]
    static void b4()
    {
        Build(true, BuildType.DevelopmentOptimized);
    }
    [MenuItem("AleMC/Build And Run/Release")]
    static void b5()
    {
        Build(true, BuildType.Release);
    }

    static void Build(bool run, BuildType buildType)
    {
        BuildOptions buildOptions = new BuildOptions();

        buildOptions &= ~BuildOptions.ShowBuiltPlayer;

        if (run)
            buildOptions |= BuildOptions.AutoRunPlayer;

        switch (buildType)
        {
            case BuildType.Development:
                buildOptions |= BuildOptions.Development;
                buildOptions |= BuildOptions.CompressWithLz4;
                PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Debug);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                break;
            case BuildType.DevelopmentOptimized:
                buildOptions |= BuildOptions.Development;
                buildOptions |= BuildOptions.ConnectWithProfiler;
                buildOptions |= BuildOptions.CompressWithLz4;
                PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Master);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                break;
            case BuildType.Release:
                buildOptions &= ~BuildOptions.Development;
                buildOptions |= BuildOptions.CompressWithLz4HC;
                PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Master);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.All;
                break;
        }

        EditorUserBuildSettings.development = buildType != BuildType.Release; //needed for some reason

        //build the build
        BuildInternalCall(false, buildOptions);
    }

    static void BuildInternalCall(bool askForLocation, BuildOptions defaultOptions)
    {
        // Get static internal "CallBuildMethods" method
        MethodInfo method = typeof(BuildPlayerWindow).GetMethod("CallBuildMethods", BindingFlags.NonPublic | BindingFlags.Static);

        // invoke internal method
        method.Invoke(null, new object[] { askForLocation, defaultOptions });
    }
}
