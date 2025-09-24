using UnityEngine;

namespace Entities.Core.Scripts.GUI
{
	public class ScreenWidget : MonoBehaviour
	{
		protected ScreenStack	m_ScreenStack;
		protected ScreenView	m_OwnerScreen;

		public virtual void Initialize(ScreenStack stack, ScreenView owner, ServiceLocator locator)
		{
			m_ScreenStack = stack;
			m_OwnerScreen = owner;
		}

		public virtual void Deinitialize()
		{
			
		}

		public virtual void OnShow() { }
		public virtual void OnHide() { }
	}
}