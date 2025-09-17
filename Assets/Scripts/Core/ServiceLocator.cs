namespace Core
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Sirenix.OdinInspector;

	public interface IInitializable
	{
		public void Initialize(ServiceLocator locator);
		public void Deinitialize();
	}

	public interface IUpdatable
	{
		public void Update(float deltaTime);
	}
	
	public class ServiceLocator
	{
		[ShowInInspector]
		private Dictionary<Type, object> m_Services = new Dictionary<Type, object>();
		
		private List<IInitializable> m_Initializables = new List<IInitializable>();
		private List<IUpdatable> m_Updatables = new List<IUpdatable>();

		public void Register<T>(T service)
		{
			m_Services.Add(service.GetType(), service);
			
			if (service is IInitializable initializable)
			{
				m_Initializables.Add(initializable);
			}
			
			if (service is IUpdatable updatable)
			{
				m_Updatables.Add(updatable);
			}
		}

		public T Get<T>()
		{
			var serviceType = typeof(T);
			if (m_Services.TryGetValue(serviceType, out var service) == false)
			{
				throw new Exception($"No registered service of type {serviceType.Name}!");
			}

			return (T)service;
		}

		public void InitializeAll()
		{
			foreach (var initializable in m_Initializables)
			{
				initializable.Initialize(this);
			}
		}
		
		public void DeinitializeAll()
		{
			foreach (var initializable in m_Initializables)
			{
				initializable.Deinitialize();
			}
			
			m_Initializables.Clear();
			m_Services.Clear();
		}

		public void UpdateAll(float deltaTime)
		{
			foreach (var updatable in m_Updatables)
			{
				updatable.Update(deltaTime);
			}
		}
	}
}