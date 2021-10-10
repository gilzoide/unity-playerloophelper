# PlayerLoopHelper
Single file helper class for registering/unregistering systems in [Unity](https://unity.com/)'s
[PlayerLoop](https://docs.unity3d.com/ScriptReference/LowLevel.PlayerLoop.html).


## How to install
Either:

- In [Package Manager Window](https://docs.unity3d.com/Manual/upm-ui.html)
  install using this repository's [Git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html):
  `https://github.com/gilzoide/unity-playerloophelper.git`
- Copy [PlayerLoopHelper.cs](Runtime/PlayerLoopHelper.cs) anywhere inside your project.


## Usage example
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
            typeof(MainThreadDispatcher),
            // "FirstChildOf Update": this system will run as the first step
            // in the Update phase, before other components
            // For more phases, check out UnityEngine.PlayerLoop subclasses
            // (e.g.: https://docs.unity3d.com/ScriptReference/PlayerLoop.Update.html)
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


## API
`enum InsertPosition`
  - `Before`: insert new system before specified one, as its sibling
  - `After`: insert new system after specified one, as its sibling
  - `FirstChildOf`: insert new system as the first child of specified one
  - `LastChildOf`: insert new system as the last child of specified one

`class PlayerLoopSystemHelper`
  - `static bool Register(Type type, InsertPosition position, Type anchorType, PlayerLoopSystem.UpdateFunction action)`

    Registers a `PlayerLoopSystem` with the given `type` and `action` in
    the specified `position` relative to `anchorType`.

    Returns `true` if `anchorType` was found and system was inserted successfully.

  - `static bool Unregister(Type type)`

    Unregisters a `PlayerLoopSystem`.

    Returns `true` if `type` was found and removed successfully.


## Other projects for injecting callbacks in Unity's PlayerLoop
- https://github.com/Refsa/PlayerLoopInjector
- https://github.com/sotanmochi/PlayerLooper