/**
 * PlayerLoopHelper: Simple helper class for registering/unregistering systems in Unity's PlayerLoop. 
 *
 * Project URL: https://github.com/gilzoide/unity-playerloophelper
 * 
 * This is free and unencumbered software released into the public domain.
 * For more information, please refer to <http://unlicense.org/>
 */
using System;
using UnityEngine.LowLevel;
using static UnityEngine.LowLevel.PlayerLoopSystem;

namespace PlayerLoopHelper
{
	/// <summary>Relative position for inserting a PlayerLoopSystem in the PlayerLoop tree</summary>
	public enum InsertPosition
	{
		/// <summary>Insert system right before the specified one, as a sibling</summary>
		Before,
		/// <summary>Insert system right after the specified one, as a sibling</summary>
		After,
		/// <summary>Insert system as the first child of the specified one</summary>
		FirstChildOf,
		/// <summary>Insert system as the last child of the specified one</summary>
		LastChildOf,
	}

	/// <summary>Helper class for registering/unregistering systems into Unity's PlayerLoop</summary>
	public static class PlayerLoopSystemHelper
	{
		/// <summary>
		/// Registers a PlayerLoopSystem with specified <paramref name="type"/> that will run the given
		/// <paramref name="action"/>. The new system will be inserted in the given <paramref name="position"/>
		/// relative to the PlayerLoopSystem associated with <paramref name="anchorType"/>.
		/// </summary>
		/// <param name="type">Type associated with new PlayerLoopSystem</param>
		/// <param name="position">Position relative to <paramref name="anchorType"/> to insert new system</param>
		/// <param name="anchorType">Type of the PlayerLoopSystem where new system should be inserted relative to</param>
		/// <param name="action">Delegate that new system will run</param>
		/// <returns>Whether PlayerLoopSystem associated with <paramref name="anchorType"/> was found 
		/// in current PlayerLoop and the new system was registered successfully</returns>
		public static bool Register(Type type, InsertPosition position, Type anchorType, UpdateFunction action)
		{
			PlayerLoopSystem rootPlayerLoopSystem = PlayerLoop.GetCurrentPlayerLoop();
			if (TryInsertSystemInList(ref rootPlayerLoopSystem.subSystemList, type, position, anchorType, action))
			{
				PlayerLoop.SetPlayerLoop(rootPlayerLoopSystem);
				return true;
			}
			return false;
		}

		/// <returns>Whether PlayerLoopSystem associated with <paramref name="anchorType"/> was found 
		/// in current PlayerLoop and unregistered successfully</returns>
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

		/// <returns>Whether a PlayerLoopSystem associated with <paramref name="type"/> was found in current PlayerLoop</returns>
		public static bool IsRegistered(Type type)
		{
			PlayerLoopSystem rootPlayerLoopSystem = PlayerLoop.GetCurrentPlayerLoop();
			return FindSystemInList(ref rootPlayerLoopSystem.subSystemList, type, out int index) != NOT_FOUND;
		}

		#region Private Implementation
		private static PlayerLoopSystem[] NOT_FOUND = null;

		private static bool TryInsertSystemInList(ref PlayerLoopSystem[] subSystemList, Type type,
				InsertPosition position, Type anchorType, UpdateFunction action)
		{
			ref PlayerLoopSystem[] foundList = ref FindSystemInList(ref subSystemList, anchorType, out int index);
			if (foundList != NOT_FOUND)
			{
				var newSystem = new PlayerLoopSystem { type = type, updateDelegate = action };
				switch (position)
				{
					case InsertPosition.Before:
						ArrayUtils.InsertInto(ref foundList, index, newSystem);
						break;

					case InsertPosition.After:
						ArrayUtils.InsertInto(ref foundList, index + 1, newSystem);
						break;

					case InsertPosition.FirstChildOf:
						ArrayUtils.PushFrontInto(ref foundList[index].subSystemList, newSystem);
						break;
					
					case InsertPosition.LastChildOf:
						ArrayUtils.PushBackInto(ref foundList[index].subSystemList, newSystem);
						break;
				}
				return true;
			}
			return false;
		}

		private static bool TryRemoveSystemFromList(ref PlayerLoopSystem[] subSystemList, Type anchorType)
		{
			ref PlayerLoopSystem[] foundList = ref FindSystemInList(ref subSystemList, anchorType, out int index);
			if (foundList != NOT_FOUND)
			{
				ArrayUtils.RemoveFrom(ref foundList, index);
				return true;
			}
			return false;
		}

		private static ref PlayerLoopSystem[] FindSystemInList(ref PlayerLoopSystem[] subSystemList, Type anchorType, out int index)
		{
			for (int i = 0; i < subSystemList.Length; i++)
			{
				ref PlayerLoopSystem it = ref subSystemList[i];
				if (it.type == anchorType)
				{
					index = i;
					return ref subSystemList;
				}
				else if (!ArrayUtils.IsNullOrEmpty(it.subSystemList))
				{
					ref PlayerLoopSystem[] foundList = ref FindSystemInList(ref it.subSystemList, anchorType, out index);
					if (foundList != NOT_FOUND) return ref foundList;
				}
			}
			index = 0;
			return ref NOT_FOUND;
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
