using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.TestTools;
using PlayerLoopHelper;

public class EditorTests
{
    [Test]
	public void OnRegister_ReturnTrueIfAnchorTypeIsRegistered()
	{
		Assert.IsFalse(PlayerLoopSystemHelper.Register(
			typeof(EditorTests),
			InsertPosition.FirstChildOf,
			typeof(MonoBehaviour),
			Callback
		));

		Assert.IsTrue(PlayerLoopSystemHelper.Register(
			typeof(EditorTests),
			InsertPosition.FirstChildOf,
			typeof(UnityEngine.PlayerLoop.Update),
			Callback
		));
	}

    [Test]
	public void IsRegistered_ShouldReturnTrueIfTypeIsRegistered()
	{
		Assert.True(PlayerLoopSystemHelper.IsRegistered(typeof(UnityEngine.PlayerLoop.Update)));
		Assert.True(PlayerLoopSystemHelper.IsRegistered(typeof(UnityEngine.PlayerLoop.PreUpdate)));
	}

	[Test]
	public void IsRegistered_ShouldReturnFalseIfTypeIsNotRegistered()
	{
		Assert.False(PlayerLoopSystemHelper.IsRegistered(typeof(EditorTests)));
	}

    PlayerLoopSystem previousLoop;

    [SetUp]
    public void SaveCurrentLoop()
    {
        previousLoop = PlayerLoop.GetCurrentPlayerLoop();
    }

	[TearDown]
	public void ResetPlayerLoop()
	{
		PlayerLoop.SetPlayerLoop(previousLoop);
	}

    void Callback()
	{
	}
}
