namespace Core.Utility
{
	public interface IRandom
	{
		// gets value in interval 0,1
		float Value { get; }
        
		int Next();

		/// <summary>
		/// Returns random int from specified range. From included and To excluded!
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		int Range(int from, int to);
        
		/// <summary>
		/// Returns random int from specified range. From included and To excluded!
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		float Range(float from, float to);
	}}