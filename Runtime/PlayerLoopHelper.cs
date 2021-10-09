using System;
using UnityEngine.LowLevel;

namespace PlayerLoopHelper
{
	public enum InsertPosition
	{
		Before,
		After,
		FirstChildOf,
		LastChildOf,
	}

	public static class PlayerLoopSystemHelper
	{
		public static bool Register(Type type, InsertPosition position, Type anchorType, PlayerLoopSystem.UpdateFunction action)
		{
			PlayerLoopSystem rootPlayerLoopSystem = PlayerLoop.GetCurrentPlayerLoop();
			var insertParams = new InsertParameters
			{
				type = type,
				position = position,
				anchorType = anchorType,
				action = action
			};
			if (TryInsertSystemInList(ref rootPlayerLoopSystem.subSystemList, ref insertParams))
			{
				PlayerLoop.SetPlayerLoop(rootPlayerLoopSystem);
				return true;
			}
			return false;
		}

		public static bool Unregister(Type type)
		{
			PlayerLoopSystem rootPlayerLoopSystem = PlayerLoop.GetCurrentPlayerLoop();
			if (TryRemoveSystemFromList(ref rootPlayerLoopSystem.subSystemList, type))
			{
				PlayerLoop.SetPlayerLoop(rootPlayerLoopSystem);
				return true;
			}
			return false;
		}

		#region Private Implementation
		private struct InsertParameters
		{
			public Type type;
			public InsertPosition position;
			public Type anchorType;
			public PlayerLoopSystem.UpdateFunction action;
		}

		private static bool TryInsertSystemInList(ref PlayerLoopSystem[] subSystemList, ref InsertParameters parameters)
		{
			for (int i = 0; i < subSystemList.Length; i++)
			{
				ref PlayerLoopSystem it = ref subSystemList[i];
				if (it.type == parameters.anchorType)
				{
					var newSystem = new PlayerLoopSystem
					{
						type = parameters.type,
						updateDelegate = parameters.action,
					};
					switch (parameters.position)
					{
						case InsertPosition.Before:
							ArrayUtils.InsertInto(ref subSystemList, i, newSystem);
							break;

						case InsertPosition.After:
							ArrayUtils.InsertInto(ref subSystemList, i + 1, newSystem);
							break;

						case InsertPosition.FirstChildOf:
							ArrayUtils.PushFrontInto(ref subSystemList[i].subSystemList, newSystem);
							break;
						
						case InsertPosition.LastChildOf:
							ArrayUtils.PushBackInto(ref subSystemList[i].subSystemList, newSystem);
							break;
					}
					return true;
				}
				else if (!ArrayUtils.IsNullOrEmpty(it.subSystemList) && TryInsertSystemInList(ref it.subSystemList, ref parameters))
				{
					return true;
				}
			}
			return false;
		}

		private static bool TryRemoveSystemFromList(ref PlayerLoopSystem[] subSystemList, Type anchorType)
		{
			for (int i = 0; i < subSystemList.Length; i++)
			{
				ref PlayerLoopSystem it = ref subSystemList[i];
				if (it.type == anchorType)
				{
					ArrayUtils.RemoveFrom(ref subSystemList, i);
					return true;
				}
				else if (!ArrayUtils.IsNullOrEmpty(it.subSystemList) && TryRemoveSystemFromList(ref it.subSystemList, anchorType))
				{
					return true;
				}
			}
			return false;
		}
		#endregion
	}

	public static class ArrayUtils
	{
		public static bool IsNullOrEmpty<T>(T[] array)
		{
			return array == null || array.Length == 0;
		}

		public static void InsertInto<T>(ref T[] array, int index, T value)
		{
			T[] newArray;
			if (array == null)
			{
				newArray = new T[] { value };
			}
			else
			{
				newArray = new T[array.Length + 1];
				if (index > 0)
				{
					Array.Copy(array, newArray, index);
				}
				newArray[index] = value;
				if (index < array.Length)
				{
					Array.Copy(array, index, newArray, index + 1, array.Length - index);
				}
			}
			array = newArray;
		}

		public static void PushFrontInto<T>(ref T[] array, T value)
		{
			InsertInto(ref array, 0, value);
		}
		public static void PushBackInto<T>(ref T[] array, T value)
		{
			InsertInto(ref array, array?.Length ?? 0, value);
		}

		public static void RemoveFrom<T>(ref T[] array, int index)
		{
			int newArrayLength = array.Length - 1;
			var newArray = new T[newArrayLength];
			if (index > 0)
			{
				Array.Copy(array, newArray, index);
			}
			if (index < newArrayLength)
			{
				Array.Copy(array, index + 1, newArray, index, newArrayLength - index);
			}
			array = newArray;
		}
	}
}
