using UnityEditor;

namespace Naxmaardur.SceneList
{
	public class SceneWatcher : AssetPostprocessor
	{
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			bool sendUpdate = false;
			// Detect added assets
			foreach (string path in importedAssets)
			{
				if (path.EndsWith(".unity"))
				{
					sendUpdate = true;
				}
			}

			// Detect deleted assets
			foreach (string path in deletedAssets)
			{
				if (path.EndsWith(".unity"))
				{
					sendUpdate = true;
				}
			}

			// Detect moved assets
			foreach (string path in movedAssets)
			{
				if (path.EndsWith(".unity"))
				{
					sendUpdate = true;
				}
			}
			foreach (string path in movedFromAssetPaths)
			{
				if (path.EndsWith(".unity"))
				{
					sendUpdate = true;
				}
			}

			if (sendUpdate)
			{
				SceneListData.instance.ListScenes();
			}
		}
	}
}
