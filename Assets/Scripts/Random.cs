using UnityEngine;
public class RandomEx
{
	private Random.State state;
	public RandomEx(int _seed)
	{
		SetSeed(_seed);
	}
	public void SetSeed(int _seed)
	{
		var prev_state = Random.state;
		Random.InitState(_seed);
		state = Random.state;
		Random.state = prev_state;
	}
	public int Range(int min, int max)
	{
		var prev_state = Random.state;
		Random.state = state;
		var result = Random.Range(min, max);
		state = Random.state;
		Random.state = prev_state;
		return result;
	}
	public float Range(float min, float max)
	{
		var prev_state = Random.state;
		Random.state = state;
		var result = Random.Range(min, max);
		state = Random.state;
		Random.state = prev_state;
		return result;
	}
}