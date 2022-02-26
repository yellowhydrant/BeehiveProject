#if UNITY_EDITOR
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor;

public class BuildPreprocessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Asm;
        PlayerSettings.WebGL.memorySize = 256;
    }
}
#endif