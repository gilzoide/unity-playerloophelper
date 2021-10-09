using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.LowLevel;
using PlayerLoopHelper;

public class RuntimeTests
{
	[Test]
	public void OnRegister_ReturnTrueIfAnchorTypeIsRegistered()
	{
		Assert.IsFalse(PlayerLoopSystemHelper.Register(
			typeof(RuntimeTests),
			InsertPosition.FirstChildOf,
			typeof(MonoBehaviour),
			Callback
		));

		Assert.IsTrue(PlayerLoopSystemHelper.Register(
			typeof(RuntimeTests),
			InsertPosition.FirstChildOf,
			typeof(UnityEngine.PlayerLoop.Update),
			Callback
		));
	}

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

	[TearDown]
	public void ResetVariables()
	{
		timesCallbackRan = 0;
		PlayerLoop.SetPlayerLoop(PlayerLoop.GetDefaultPlayerLoop());
	}

	int timesCallbackRan;
	void Callback()
	{
		timesCallbackRan++;
	}
}
