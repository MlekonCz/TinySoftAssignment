using System;

namespace Entities.Core.Scripts.Utility
{
	// Helper class for easy seed-based randomization
	public class SeedRandom : IRandom
	{
		// PRIVATE MEMBERS

		private static System.Random m_SeedProvider;
		
		private System.Random m_Random;

		// PUBLIC PROPERTIES

		public int Seed;
		
		/// <summary>
		/// Random float value from interval [0..1] with inclusive border values
		/// </summary>
		public float Value => (float) m_Random.Next(0, int.MaxValue) / (int.MaxValue - 1);

		// CTOR
		/// <summary>
		/// Randomization class based around System.Random with seed-based generation.
		/// </summary>
		/// <param name="seed">When you specify seed, you'll get same results for same random sequence. If not specified, the seed itself will be random.</param>
		public SeedRandom(int? seed = null)
		{
			if (seed.HasValue)
			{
				Seed = seed.Value;
			}
			else
			{
				if (m_SeedProvider == null)
				{
					m_SeedProvider = new System.Random((int)DateTime.Now.Ticks);
				}

				Seed = m_SeedProvider.Next();
			}
			
			m_Random = new Random(Seed);
		}

		public int Next()
		{
			return m_Random.Next();
		}

		/// <summary>
		/// Returns random int from specified range. From included and To excluded!
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public int Range(int from, int to)
		{
			return m_Random.Next(from, to);
		}

		/// <summary>
		/// Returns random int from specified range. From included and To included!
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public float Range(float from, float to)
		{
			float diff = to - from;
			return from + (diff * Value);
		}

	}}