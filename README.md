# PlayerLoopHelper
Simple helper class for registering/unregistering systems in [Unity](https://unity.com/)'s
[PlayerLoop](https://docs.unity3d.com/ScriptReference/LowLevel.PlayerLoop.html).


## How to install
Either:

1. In [Package Manager Window](https://docs.unity3d.com/Manual/upm-ui.html)
   install using this repository's [Git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html):
   `https://github.com/gilzoide/unity-playerloophelper.git`
2. Copy [PlayerLoopHelper.cs](Runtime/PlayerLoopHelper.cs) anywhere inside your project.


## Example
```cs
using System;
using System.Collections.Concurrent;
using PlayerLoopHelper;

// Class for scheduling tasks that will run in Unity's Main Thread.
// 
// Using a PlayerLoopSystem avoids needing a living singleton GameObject
// with a MonoBehaviour that overrides `Update`, which is the usual way
// of implementing such functionality in Unity.
//
// Usage:
//     MainThreadDispatcher.Dispatch(() =>
//     {
//         Debug.Log("Some action that runs on Unity's Main Thread"));
//     }
public static class MainThreadDispatcher
{
    static readonly ConcurrentQueue<Action> _taskQueue;
    static MainThreadDispatcher()
    {
        _taskQueue = new ConcurrentQueue<Action>();
        Enable(); 
    }

    public static bool Enable()
    {
        return PlayerLoopSystemHelper.Register(
            // PlayerLoop systems are identified by their Type
            // To unregister this system, pass the same Type to
            // `Unregister`
            typeof(MainThreadDispatcher),
            // "FirstChildOf Update": this system will run as the first
            // step in the Update loop, before other components
            InsertPosition.FirstChildOf,
            typeof(UnityEngine.PlayerLoop.Update),
            // Callback that will run once per frame
            UpdateCallback
        );
    }

    public static bool Disable()
    {
        return PlayerLoopSystemHelper.Unregister(typeof(MainThreadDispatcher));
    }

    public static void Dispatch(Action action)
    {
        _taskQueue.Enqueue(action);
    }

    static void UpdateCallback()
    {
        while (_taskQueue.TryDequeue(out Action task))
        {
            task();
        }
    }
}
```