using UnityEditor;

namespace Naxmaardur.SceneList
{
	public class SceneWatcher : AssetPostprocessor
	{
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			// Detect added assets
			foreach (string path in importedAssets)
			{
				if (path.EndsWith(".unity"))
				{
					SceneListData.instance.ListScenes();
					return;
				}
			}

			// Detect deleted assets
			foreach (string path in deletedAssets)
			{
				if (path.EndsWith(".unity"))
				{
					SceneListData.instance.ListScenes();
					return;
				}
			}

			// Detect moved assets
			foreach (string path in movedAssets)
			{
				if (path.EndsWith(".unity"))
				{
					SceneListData.instance.ListScenes();
					return;
				}
			}
			foreach (string path in movedFromAssetPaths)
			{
				if (path.EndsWith(".unity"))
				{
					SceneListData.instance.ListScenes();
					return;
				}
			}
		}
	}
}
