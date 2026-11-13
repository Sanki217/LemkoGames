using UnityEditor;
using UnityEngine;
namespace Lemko.Editor {
    public static class LocalDataAndWebSetup {
        [MenuItem("Lemko/Web/Configure WebGL (no build)")]
        public static void ConfigureWeb() {
            PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback=true;
            PlayerSettings.WebGL.dataCaching=true;
            PlayerSettings.WebGL.threadsSupport=false;
            PlayerSettings.WebGL.initialMemorySize=128;
            AssetDatabase.SaveAssets();
            bool installed=BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL,BuildTarget.WebGL);
            Debug.Log("LEMKO_WEB_SETTINGS_CONFIGURED: Gzip, decompression fallback, caching, single thread, 128MB initial growing heap. WebGL module installed: "+installed+". No platform switch or player build.");
        }
    }
}
