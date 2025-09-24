using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Entities.Core.Scripts.GUI
{
	[InlineEditor]
	public class ScreenStack : MonoBehaviour, IInitializable
	{
		[SerializeField] private InputAction m_UIBackAction;
		
		private ServiceLocator m_Locator;
		
		[ShowInInspector]
		private List<ScreenView> m_ScreenStack = new List<ScreenView>();
		private Dictionary<ScreenView, ScreenView> m_LoadedScreens = new Dictionary<ScreenView, ScreenView>();
		
		void IInitializable.Initialize(ServiceLocator locator)
		{
			m_Locator = locator;
			
			m_UIBackAction.performed += OnBackAction;
			m_UIBackAction.Enable();
		}

		void IInitializable.Deinitialize()
		{
			m_UIBackAction.performed -= OnBackAction;
			m_UIBackAction.Disable();

			// Finalize - Call hide on all opened screens
			while (m_ScreenStack.Count > 0)
			{
				var topOnStack = m_ScreenStack.Last();
				topOnStack.OnHide();
				m_ScreenStack.Remove(topOnStack);	
			}
			
			// And deinitialize all previously loaded screens
			foreach (var screen in m_LoadedScreens.Values)
			{
				screen.Deinitialize();
			}
		}

		public T OpenScreen<T,TData>(T screenPrefab, TData data) where T : ScreenView<TData>
		{
			var screen = GetInstantiatedScreen<T>(screenPrefab, true);
			if (screen.IsTopOnStack)
			{
				Debug.Log("Screen already on top of stack. Skipping screen push logic. " + screen.GetType().Name);
				return screen;
			}

			PushToStackTop(screen);

			// Data binding
			screen.BindScreenData(data);
			
			ShowScreen(screen);
			
			return screen;
		}		
		
		public T OpenScreen<T>(T screenPrefab) where T : ScreenView
		{
			var screen = GetInstantiatedScreen<T>(screenPrefab, true);
			if (screen.IsTopOnStack)
			{
				Debug.Log("Screen already on top of stack. Skipping screen push logic. " + screen.GetType().Name);
				return screen;
			}
			
			PushToStackTop(screen);
			
			// No data binding

			ShowScreen(screen);

			return screen;
		}

		private void PushToStackTop<T>(T screen) where T : ScreenView
		{
			// Screen Was opened already, but is deep in stack - we'll remove it from the stack middle and push it on top again.
			if (m_ScreenStack.Contains(screen))
			{
				m_ScreenStack.Remove(screen);
			}

			var currentTopScreen = GetTopOnStack(); 
			if (currentTopScreen != null)
			{
				currentTopScreen.gameObject.SetActive(false);
				currentTopScreen.OnStackVisibilityChanged(false);
			}

			m_ScreenStack.Add(screen);
		}

		private static void ShowScreen<T>(T screen) where T : ScreenView
		{
			screen.transform.SetAsLastSibling();
			screen.gameObject.SetActive(true);
			screen.OnShow();
		}

		public void CloseLastScreen()
		{
			CloseScreen(GetTopOnStack());
		}

		public ScreenView GetTopOnStack()
		{
			return m_ScreenStack.LastOrDefault();
		}
		
		private T GetInstantiatedScreen<T>(T screenPrefab, bool instantiate) where T : ScreenView
		{
			if (m_LoadedScreens.TryGetValue(screenPrefab, out var screen) == false && instantiate)
			{
				screen = Instantiate(screenPrefab, this.transform);
				try
				{
					screen.Initialize(this, m_Locator);
				}
				catch (Exception e)
				{
					Debug.LogError(e.Message + "\n" + e.StackTrace);
				}
				m_LoadedScreens.Add(screenPrefab, screen);
			}

			return (T)screen;
		}

		public void CloseScreen<T>(T screenOrPrefab) where T : ScreenView
		{
			var screen = m_ScreenStack.Find(x => screenOrPrefab);
			if (screen == null)
			{
				screen = GetInstantiatedScreen<T>(screenOrPrefab, false);
			}

			if (screen == null)
			{
				Debug.LogError("Screen wasn't opened yet.");
				return;
			}
			
			CloseScreen(screen);
		}

		public void CloseScreen(ScreenView screen) 
		{
			if (screen.CanBeClosed == false)
				return;
			
			var wasLastOnStack = GetTopOnStack() == screen;

			m_ScreenStack.Remove(screen);
			screen.OnHide();
			
			if (wasLastOnStack)
			{
				screen.gameObject.SetActive(false);
				
				var newTopScreen = m_ScreenStack.LastOrDefault();

				if (newTopScreen != null)
				{
					newTopScreen.gameObject.SetActive(true);
					newTopScreen.OnStackVisibilityChanged(true);
				}
				else
				{
					Debug.LogError("Closed last screen!");
				}
			}
		}
		
		private void OnBackAction(InputAction.CallbackContext ctx)
		{
			CloseLastScreen();
		}
	}
}