using System.Collections.Generic;
using Entities.Core.Scripts.GUI;
using Entities.Core.Scripts.User;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entities.Core.Scripts
{
	public class GameApplication : MonoBehaviour
	{
		[FormerlySerializedAs("m_Settings")]
		[Header("Setup")]
		[SerializeField] private MasterSettings m_MasterSettings;
		[SerializeField] private ScreenStack m_ScreenStack;
		
		private ServiceLocator m_ServiceLocator;

		[Header("Runtime Debug View")]
		[ShowInInspector, ListDrawerSettings(DefaultExpandedState = true)]
		private List<object> m_DebugViews = new List<object>(); 
		
        private const string KEY = "user_save_data";
		
        
		private void Awake()
		{
			Debug.Log("Starting application");
			InitGame();
		}

		private void OnDestroy()
		{
			m_ServiceLocator.DeinitializeAll();
			Debug.Log("Ending application");
		}

		private void Update()
		{
			m_ServiceLocator.UpdateAll(Time.deltaTime);
		}

		private void InitGame()
		{
			var userService = new UserService(new PlayerPrefsSaveRep(KEY));
			
			m_ServiceLocator = new ServiceLocator();
			m_ServiceLocator.Register(m_MasterSettings);
			m_ServiceLocator.Register(m_ScreenStack);
			m_ServiceLocator.Register(userService);
			
			m_ServiceLocator.InitializeAll();

			m_DebugViews.Add(userService.UserData);
			m_DebugViews.Add(m_ServiceLocator);
			
			m_ScreenStack.OpenScreen(m_MasterSettings.ScreenSettings.StartScreen);
			userService.UserData.AppStartCount++;
		}
	}
}