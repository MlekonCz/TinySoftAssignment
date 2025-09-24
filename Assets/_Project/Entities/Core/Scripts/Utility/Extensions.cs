using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Entities.Core.Scripts.Utility
{
	public static class Extensions
	{
		public static T SelectRandom<T>(this List<T> list, IRandom random = null)
		{
			if (list.Count == 0)
				return default(T);
			int index = random?.Range(0, list.Count) ?? Random.Range(0, list.Count);
			return list[index];
		}
		
		public static T SelectWeightedRandom<T>(this List<T> list, IRandom random = null) where T : IWeightRandomItem
		{
			if (list == null || list.Count == 0)
				return default(T);
    
			if (list.Count == 1)
				return list[0];
    
			var validItems = list.Where(x => x != null && x.Weight > 0).ToList();
    
			if (validItems.Count == 0)
				return SelectRandom(list, random);
    
			var totalWeight = validItems.Sum(x => x.Weight);
			var randomValue = (random?.Value ?? Random.value) * totalWeight;
    
			var currentWeight = 0f;
			foreach (var item in validItems)
			{
				currentWeight += item.Weight;
				if (randomValue <= currentWeight)
					return item;
			}
    
			return validItems.Last();
		}
	}
}