namespace Core.Utility
{
	using System.Collections.Generic;
	using UnityEngine;

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
			// TODO: Implement Weighted random selection instead of Pure Random selection.
			return SelectRandom(list, random);
		}
	}
}