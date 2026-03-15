using UnityEngine;
using UnityEditor;
using Eflatun.SceneReference;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Naxmaardur.SceneList
{
	[InitializeOnLoad]
	[FilePath("Library/SceneListData.asset", FilePathAttribute.Location.ProjectFolder)]
	public class SceneListData : ScriptableSingleton<SceneListData>
    {
		public event Action onChanged;
		[SerializeField]
		private List<SceneReference> scenes = new();
		[SerializeField]
		private List<SceneReference> pinned = new();
		private HashSet<string> pinnedHaset;


		static SceneListData()
		{
			// Register these callbacks on editor load.
			EditorSceneManager.sceneOpened += OnSceneOpenEditor;
			EditorSceneManager.sceneClosed += OnSceneClosed;
			EditorApplication.quitting += OnEditorQuitting;
			EditorApplication.playModeStateChanged += OnplayModeStateChanged;
		}

		private void OnEnable()
		{
			pinnedHaset = new();

			foreach(SceneReference scene in pinned)
			{
				pinnedHaset.Add(scene.Guid);
			}
		}

		public SceneReference this[int i]
		{
			get { return i < pinned.Count ? pinned[i] : scenes[i - pinned.Count]; }
		}
		public int Count => scenes.Count + pinned.Count;
		public int SceneCount => scenes.Count;
		public int PinnedCount => pinned.Count;

		public void ListScenes()
		{
			scenes.Clear();
			string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });

			foreach (string guid in guids)
			{
				SceneReference scene = new(guid);
				scenes.Add(scene);
			}
			//Order ascending but -1 at end
			scenes = scenes.OrderBy(scene => scene.BuildIndex == -1 ? int.MaxValue : scene.BuildIndex).ToList();
			Clean();
			onChanged?.Invoke();
		}

		private void SortPinned()
		{
			pinned = pinned.OrderBy(scene => scene.BuildIndex == -1 ? int.MaxValue : scene.BuildIndex).ToList();
		}

		public void Pin(SceneReference sceneToPin)
		{
			pinned.Add(sceneToPin);
			pinnedHaset.Add(sceneToPin.Guid);
			SortPinned();
			onChanged?.Invoke();
		}

		public void Unpin(SceneReference scene)
		{
			for (int i = pinned.Count - 1; i >= 0; i--)
			{
				if (pinned[i].Guid == scene.Guid)
				{
					pinned.RemoveAt(i);
				}
			}
			pinnedHaset.Remove(scene.Guid);
			SortPinned();
			onChanged?.Invoke();

		}

		public bool IsPinned(SceneReference scene)
		{
			return pinnedHaset.Contains(scene.Guid);
		}

		public bool IsBuild(SceneReference scene)
		{
			return scene.UnsafeReason != SceneReferenceUnsafeReason.NotInBuild;
		}

		public bool IsOpen(SceneReference scene)
		{
			return scene.LoadedScene != null && scene.LoadedScene.isLoaded;
		}

		public void Clean()
		{
			RemoveNullEntries(pinned);
			pinnedHaset = new();
			foreach (SceneReference scene in pinned)
			{
				pinnedHaset.Add(scene.Guid);
			}
		}

		private void RemoveNullEntries(List<SceneReference> list)
		{
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (list[i] == null)
				{
					list.RemoveAt(i);
					continue;
				}
				if (list[i].State == SceneReferenceState.Unsafe)
				{
					list.RemoveAt(i);
					continue;
				}
			}
		}

		private static void OnEditorQuitting()
		{
			SceneListData.instance.Save(true);
		}

		private static void OnSceneClosed(Scene scene)
		{
			SceneListData.instance.onChanged?.Invoke();
		}

		private static void OnSceneOpenEditor(Scene scene, OpenSceneMode loadSceneMode)
		{
			SceneListData.instance.onChanged?.Invoke();
		}

		private static void OnSceneOpen(Scene scene, LoadSceneMode loadSceneMode)
		{
			SceneListData.instance.onChanged?.Invoke();
		}

		private static void EnterPlayMode()
		{
			SceneManager.sceneLoaded += OnSceneOpen;
			SceneManager.sceneUnloaded += OnSceneClosed;
		}

		private static void ExitPlayMode()
		{
			SceneManager.sceneLoaded -= OnSceneOpen;
			SceneManager.sceneUnloaded -= OnSceneClosed;
		}

		private static void OnplayModeStateChanged(PlayModeStateChange playModeStateChange)
		{
			switch (playModeStateChange)
			{
				case PlayModeStateChange.EnteredPlayMode:
					EnterPlayMode();
					break;
				case PlayModeStateChange.ExitingEditMode:
					ExitPlayMode();
					break;
			}
		}
	}
}
