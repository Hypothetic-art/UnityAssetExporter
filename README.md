# Unity Asset Exporter

This plugin enables a user to batch export GameAssets or Prefabs or Textures
from a Unity project to a local folder destination.
3D models are exported in web friendly GLB format.

## Installation
In Unity Editor:
1. Open Window > Package Manager
2. Click +
3. Select Add/Install Package from git URL
4. Paste https://github.com/KhronosGroup/UnityGLTF.git
5. Click +
6. Select Add/Install Package from git URL
7. Paste https://github.com/Hypothetic-art/UnityAssetExporter.git

## Usage

One can export:
- Models / GameAssets
- Prefabs
- Textures

3D models and prefabs are exported in a binary glb format by default.

> FBX files often don’t include links to external textures and materials.
> To avoid missing texture issues, we recommend exporting Prefabs instead. 
> Prefabs store material and texture references, ensuring your model imports correctly.

### Instructions
1. Open Hypothetic (new item in menu bar)![MenuBar.png](img%2FMenuBar.png)
2. Click Export Prefabs
3. Select all models to be exported![ExportPrefabsWindow.png](img%2FExportPrefabsWindow.png)
4. Press Export (at the very bottom)
5. Select a folder to be exported to.

Done!

All the prefabs are exported to the selected folder as binary glbs.
Texture files are included withing the glbs.