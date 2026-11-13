# WebGL setup

The installed Unity 6000.3.13f1 has Windows support but **does not have WebGL Build Support**. Install that module yourself:

1. Close Unity. Open Unity Hub > Installs > Unity 6000.3.13f1 > gear/three-dot menu > Add modules.
2. Select **Web Build Support / WebGL Build Support** (wording depends on Hub version), then install.
3. Reopen LemkoGames. Open **File > Build Profiles**, add/select the **Web** platform, then **Switch Platform**.
4. Keep Village first, followed by Shop, FamilyTree, Farm, House, Memory, Numbers, Alphabet and Weather. The existing shared scene list already includes these in this order. If your Web profile overrides the scene list, copy that order into the profile. The legacy LemkoVillage2D scene is disabled in the shared release list.
5. Choose **Build And Run** when you are ready. Choose an output directory outside Assets. No build was made for you.
6. For hosting, upload the entire output (index.html, Build, TemplateData and any StreamingAssets directory), not only index.html. Serve through HTTP(S); do not double-click the HTML with a file:// URL.

Configured in Project Settings:

- Gzip compression with decompression fallback, for hosts where you cannot configure compression response headers.
- Data caching enabled.
- Threads disabled, avoiding a threading-related cross-origin isolation requirement.
- Initial WebAssembly heap 128 MB; existing growth and maximum-memory settings retained.

These settings can be reapplied with **Lemko > Web > Configure WebGL (no build)**. It does not switch platforms, install modules, clear saves or build a player. It logs whether Web support is installed. Company/product identifiers were preserved so existing saves remain accessible.

Reference: [Unity WebGL settings](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityeditor/playersettings/webgl), [decompression fallback](https://docs.unity3d.com/cn/6000.0/ScriptReference/PlayerSettings.WebGL-decompressionFallback.html).

## Browser acceptance checklist

- Launch through Build And Run or the final HTTPS host. Enter and exit all eight activities.
- Click/tap pronunciation after a user gesture; browsers may restrict autoplay.
- Check hover/touch hints, drag/drop, the temperature slider, Menu, and the smaller window layout.
- Complete an activity and revisit the village. Refresh the page and confirm a fresh session starts with default settings.
