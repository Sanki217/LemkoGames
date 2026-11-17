using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
namespace Lemko.Editor {
    // Painted sprites use the browser target's automatic compressed format at their authored resolution.
    sealed class WebSpriteImporter : AssetPostprocessor {
        void OnPreprocessTexture() {
            if(!assetPath.StartsWith("Assets/Lemko/Sprites/Placeholders/",System.StringComparison.Ordinal) &&
               !assetPath.StartsWith("Assets/Lemko/Sprites/Products/",System.StringComparison.Ordinal) &&
               !assetPath.StartsWith("Assets/Lemko/Resources/",System.StringComparison.Ordinal)) return;
            var importer=(TextureImporter)assetImporter;
            if(importer.textureType!=TextureImporterType.Sprite) return;
            var web=importer.GetPlatformTextureSettings("WebGL");
            if(web.overridden) return;
            web.overridden=true;
            web.format=TextureImporterFormat.Automatic;
            web.textureCompression=TextureImporterCompression.Compressed;
            importer.SetPlatformTextureSettings(web);
        }
    }

    public static class LocalDataAndWebSetup {
        [MenuItem("Lemko/Web/Configure WebGL (no build)")]
        public static void ConfigureWeb() {
            PlayerSettings.WebGL.template="PROJECT:Lemko";
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.WebGL,Il2CppCodeGeneration.OptimizeSize);
            PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback=true;
            PlayerSettings.WebGL.dataCaching=true;
            PlayerSettings.WebGL.threadsSupport=false;
            PlayerSettings.WebGL.initialMemorySize=128;
            AssetDatabase.SaveAssets();
            bool installed=BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL,BuildTarget.WebGL);
            Debug.Log("LEMKO_WEB_SETTINGS_CONFIGURED: Brotli, decompression fallback, caching, single thread, 128MB initial growing heap. WebGL module installed: "+installed+".");
        }
    }
}
