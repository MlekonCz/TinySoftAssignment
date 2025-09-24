using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Entities.Core.Scripts.GUI
{
	// ScreenView with custom data binding
	public abstract class ScreenView<TData> : ScreenView
	{
		public abstract void BindScreenData(TData data);
	}
	
	
	// Basic ScreenView with no data binding
	public abstract class ScreenView : MonoBehaviour
	{
		public virtual bool CanBeClosed => true;

		protected ScreenStack m_ScreenStack;
		protected ServiceLocator m_Locator;
		private ScreenWidget[] m_Widgets;
		private bool m_ScreenBusy;

		public bool IsActiveInStack { get; private set; }
		public bool IsTopOnStack { get; private set; }
		public bool IsScreenBusy { get; private set; }

		public Action<bool> SignalScreenBusyChanged;

		internal virtual void Initialize(ScreenStack stack, ServiceLocator locator)
		{
			m_ScreenStack = stack;
			m_Locator = locator;
			
			m_Widgets = GetComponentsInChildren<ScreenWidget>();
			foreach (var widget in m_Widgets)
			{
				widget.Initialize(m_ScreenStack, this, m_Locator);
			}
		}

		internal virtual void Deinitialize()
		{
			foreach (var widget in m_Widgets)
			{
				widget.Deinitialize();
			}
		}
		
		internal virtual void OnShow()
		{
			Debug.Log("GUI - Screen added to stack: " + this.GetType().Name);
			IsActiveInStack = true;
			IsTopOnStack = true;

			foreach (var widget in m_Widgets)
			{
				widget.OnShow();
			}
		}

		internal virtual void OnHide()
		{
			Debug.Log("GUI - Screen removed from stack: " + this.GetType().Name);
			IsTopOnStack = false;
			IsActiveInStack = false;
			
			foreach (var widget in m_Widgets)
			{
				widget.OnHide();
			}
		}

		internal virtual void OnStackVisibilityChanged(bool isTop)
		{
			Debug.Log("GUI - Screen visibility changed on stack: " + this.GetType().Name + " | " + isTop);
			IsTopOnStack = isTop;
		}

		protected void SetScreenBusy(bool busy)
		{
			IsScreenBusy = busy;
			if (SignalScreenBusyChanged != null)
			{
				SignalScreenBusyChanged.Invoke(busy);
			}
		}

		public void CloseSelf()
		{
			m_ScreenStack.CloseScreen(this);			
		}
		
		public async UniTask WaitUntilClose(CancellationToken cancellationToken)
		{
			while (IsActiveInStack)
			{
				await UniTask.Yield();
				
				if (cancellationToken.IsCancellationRequested)
					break;
			}
		}
	}
}