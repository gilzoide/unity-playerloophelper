using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.LowLevel;
using PlayerLoopHelper;

public class RuntimeTests
{
	[UnityTest]
	public IEnumerator OnRegister_CallbackIsCalledOncePerFrame()
	{
		PlayerLoopSystemHelper.Register(
			typeof(RuntimeTests),
			InsertPosition.FirstChildOf,
			typeof(UnityEngine.PlayerLoop.Update),
			Callback
		);
		yield return null;
		Assert.AreEqual(timesCallbackRan, 1);
		yield return null;
		Assert.AreEqual(timesCallbackRan, 2);
	}

	[UnityTest]
	public IEnumerator OnUnregister_CallbackIsNotCalledAnymore()
	{
		PlayerLoopSystemHelper.Register(
			typeof(RuntimeTests),
			InsertPosition.FirstChildOf,
			typeof(UnityEngine.PlayerLoop.Update),
			Callback
		);
		yield return null;
		Assert.AreEqual(timesCallbackRan, 1);
		PlayerLoopSystemHelper.Unregister(typeof(RuntimeTests));
		yield return null;
		Assert.AreEqual(timesCallbackRan, 1);
		yield return null;
		Assert.AreEqual(timesCallbackRan, 1);
		yield return null;
		Assert.AreEqual(timesCallbackRan, 1);
	}

	PlayerLoopSystem previousLoop;

	[SetUp]
	public void SaveCurrentLoop()
    {
        previousLoop = PlayerLoop.GetCurrentPlayerLoop();
    }

	[TearDown]
	public void ResetPlayerLoopAndVariables()
	{
		timesCallbackRan = 0;
		PlayerLoop.SetPlayerLoop(previousLoop);
	}

	int timesCallbackRan;
	void Callback()
	{
		timesCallbackRan++;
	}
}
