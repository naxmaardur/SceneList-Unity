using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using Eflatun.SceneReference;

namespace Naxmaardur.SceneList
{
	public class SceneListWindow : EditorWindow
	{
		public VisualTreeAsset WindowTemplate;
		public VisualTreeAsset ElementTemplate;
		private ScrollView scrollView;
		private ScrollView scrollViewPins;
		private List<SceneVisualElement> pinnedElements = new();
		private List<SceneVisualElement> elements = new();
		private SceneListData sceneListData;
		private Texture sceneIcon;

		public static System.Action<SceneReference> OpenSceneEvent;

		[MenuItem("Tools/Scene List")]
		public static void ShowWindow()
		{
			GetWindow<SceneListWindow>("Scene List");
		}

		public void CreateGUI()
		{
			sceneIcon = EditorGUIUtility.IconContent("d_Scene").image;
			//WindowTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_ProjectSurvival/editor/ProjectSurvival_Editor/EditorWindows/SceneListWindow/SceneListWindow.uxml");
			//ElementTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_ProjectSurvival/editor/ProjectSurvival_Editor/EditorWindows/SceneListWindow/SceneListWindowElement.uxml");

			TemplateContainer window = WindowTemplate.Instantiate();
			window.style.height = new StyleLength(Length.Percent(100));
			rootVisualElement.Add(window);
			scrollView = rootVisualElement.Q<ScrollView>("ScrollView");
			scrollViewPins = rootVisualElement.Q<ScrollView>("ScrollViewPins");
			sceneListData = SceneListData.instance;
			UpdateElementListLenght(ref elements, 50, false);
			UpdateElementListLenght(ref pinnedElements, 50, true);
			sceneListData.onChanged += RefreshElements;
			sceneListData.ListScenes();
		}

		private void OnDestroy()
		{
			sceneListData.onChanged -= RefreshElements;
		}

		private void RefreshElements()
		{
			RefreshPinnedElements();
			UpdateElementListLenght(ref elements, sceneListData.SceneCount, false);
			RefreshElementListValues(elements, sceneListData.SceneCount, sceneListData.PinnedCount);
		}

		private void RefreshPinnedElements()
		{
			UpdateElementListLenght(ref pinnedElements, sceneListData.PinnedCount, true);
			RefreshElementListValues(pinnedElements, sceneListData.PinnedCount, 0);
		}

		private void UpdateElementListLenght(ref List<SceneVisualElement> elements, int targetCount, bool favorite)
		{
			int currentCount = elements.Count;

			// Create additional visual elements if there aren't enough to show all.
			while (sceneListData.SceneCount > elements.Count)
			{
				TemplateContainer tree = ElementTemplate.Instantiate();
				elements.Add(new(tree));

				tree.RegisterCallback<ClickEvent, VisualElement>(OnElementClicked, tree);

				VisualElement pinIcon = tree.Q<VisualElement>("PinIcon");
				Button button = tree.Q<Button>("Button");
				Button button2 = tree.Q<Button>("Button2");
				Image ObjectIcon = tree.Q<Image>("ObjectIcon");

				pinIcon.RegisterCallback<ClickEvent, VisualElement>(OnPinClicked, tree);
				button.RegisterCallback<ClickEvent, VisualElement>(OnOpenClicked, tree);
				button2.RegisterCallback<ClickEvent, VisualElement>(OnPlayClicked, tree);
				ObjectIcon.image = sceneIcon;
				if (favorite)
				{
					SetClass(tree, "Pinned", true);
					SetClass(tree, "PinnedTab", true);
					scrollViewPins.Add(tree);
				}
				else
				{
					scrollView.Add(tree);
				}
				currentCount++;
			}
		}

		private void RefreshElementListValues(List<SceneVisualElement> elements, int itemCount, int offset)
		{
			int listLenght = elements.Count;
			for (int i = 0; i < listLenght; i++)
			{
				SceneVisualElement element = elements[i];
				if (i < itemCount)
				{
					int x = i + offset;
					SceneReference scene = sceneListData[x];
					element.VisualElement.style.display = DisplayStyle.Flex;
					string nameToUse = scene.Name;
					element.ObjectLabel.text = nameToUse;
					element.SceneIndex.text = "" + scene.BuildIndex;

					SetClass(element.VisualElement, "Selected", sceneListData.IsOpen(scene));
					SetClass(element.VisualElement, "Pinned", sceneListData.IsPinned(scene));
				}
				else
				{
					element.VisualElement.style.display = DisplayStyle.None;
				}
			}
		}

		private void SetClass(VisualElement element, string ussClass, bool value)
		{
			if (value)
			{
				element.AddToClassList(ussClass);
			}
			else
			{
				element.RemoveFromClassList(ussClass);
			}
		}

		private void OnElementClicked(ClickEvent evt, VisualElement root)
		{
			SceneReference sceneRef = ElementToSceneRef(root);
			SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(sceneRef.Path);
			if (Selection.activeObject != sceneAsset)
			{
				Selection.activeObject = sceneAsset;
			}
		}

		private void OnPinClicked(ClickEvent evt, VisualElement element)
		{
			SceneReference scene = ElementToSceneRef(element);
			if (!sceneListData.IsPinned(scene))
			{
				sceneListData.Pin(scene);
			}
			else
			{
				sceneListData.Unpin(scene);
			}
			evt.StopPropagation();
		}

		private void OnOpenClicked(ClickEvent evt, VisualElement element)
		{
			SceneReference scene = ElementToSceneRef(element);
			OpenScene(scene);
			evt.StopPropagation();
		}

		private void OpenScene(SceneReference sceneReference)
		{
			if (!Application.isPlaying)
			{
				if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				{
					EditorSceneManager.OpenScene(sceneReference.Path, Event.current.control ? OpenSceneMode.Additive : OpenSceneMode.Single);
					RefreshElements();
				}
			}
			else
			{
				OpenSceneEvent?.Invoke(sceneReference);
			}
		}

		private void OnPlayClicked(ClickEvent evt, VisualElement element)
		{
			SceneReference scene = ElementToSceneRef(element);
			PlayScene(scene);
			evt.StopPropagation();
		}

		private void PlayScene(SceneReference sceneReference)
		{

			if (!Application.isPlaying)
			{
				if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				{
					EditorSceneManager.OpenScene(sceneReference.Path, Event.current.control ? OpenSceneMode.Additive : OpenSceneMode.Single);
					EditorApplication.isPlaying = true;
					RefreshElements();
				}
			}
			else
			{
				OpenSceneEvent?.Invoke(sceneReference);
			}
		}

		public SceneReference ElementToSceneRef(VisualElement root)
		{
			int index = root.parent.IndexOf(root);
			if (root.parent != scrollViewPins)
			{
				index += sceneListData.PinnedCount;
			}
			SceneReference sceneRef = sceneListData[index];
			return sceneRef;
		}

		private struct SceneVisualElement
		{
			public VisualElement VisualElement;
			public Label ObjectLabel;
			public Label SceneIndex;

			public SceneVisualElement(VisualElement element)
			{
				VisualElement = element;
				ObjectLabel = element.Q<Label>("ObjectLabel");
				SceneIndex = element.Q<Label>("SceneIndex");
				
			}
		}
	}
}
