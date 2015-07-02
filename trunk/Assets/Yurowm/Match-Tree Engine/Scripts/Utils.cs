using System.Collections;
using System.Text;
using System;
using UnityEngine;

// A set of useful functions
public class Utils {

	public static readonly Side[] allSides = {Side.Top, Side.Bottom, Side.Right, Side.Left,
											Side.TopLeft, Side.TopRight, Side.BottomRight ,Side.BottomLeft};
	public static readonly Side[] straightSides = {Side.Top, Side.Bottom, Side.Right, Side.Left};
	public static readonly Side[] slantedSides = {Side.TopLeft, Side.TopRight, Side.BottomRight ,Side.BottomLeft};

	public static Side MirrorSide(Side s) {
		switch (s) {
		case Side.Bottom: return Side.Top;
		case Side.Top: return Side.Bottom;
		case Side.Left: return Side.Right;
		case Side.Right: return Side.Left;
		case Side.BottomLeft: return Side.TopRight;
		case Side.BottomRight: return Side.TopLeft;
		case Side.TopLeft: return Side.BottomRight;
		case Side.TopRight: return Side.BottomLeft;
		}
		return Side.Null;
	}

	public static int SideOffsetX (Side s) {
		switch (s) {
		case Side.Top:
		case Side.Bottom: 
			return 0;
		case Side.TopLeft:
		case Side.BottomLeft:
		case Side.Left: 
			return -1;
		case Side.BottomRight:
		case Side.TopRight:
		case Side.Right: 
			return 1;
		}
		return 0;
	}
	
	public static int SideOffsetY (Side s) {
		switch (s) {
		case Side.Left: 
		case Side.Right: 
			return 0;
		case Side.Bottom: 
		case Side.BottomRight:
		case Side.BottomLeft:
			return -1;
		case Side.TopLeft:
		case Side.TopRight:
		case Side.Top:
			return 1;
		}
		return 0;
	}

	public static Side SideHorizontal (Side s) {
		switch (s) {
		case Side.Left: 
		case Side.TopLeft:
		case Side.BottomLeft:
			return Side.Left;
		case Side.Right:
		case Side.TopRight:
		case Side.BottomRight:
			return Side.Right;
		default:
			return Side.Null;
		}
	}

	public static Side SideVertical (Side s) {
		switch (s) {
		case Side.Top: 
		case Side.TopLeft:
		case Side.TopRight:
			return Side.Top;
		case Side.Bottom:
		case Side.BottomLeft:
		case Side.BottomRight:
			return Side.Bottom;
		default:
			return Side.Null;
		}
	}

	public static string StringReplaceAt(string value, int index, char newchar)
	{
		if (value.Length <= index)
			return value;
		StringBuilder sb = new StringBuilder(value);
		sb[index] = newchar;
		return sb.ToString();
	}

	// Coroutine wait until the function "Action" will be true for a "delay" seconds
	public static IEnumerator WaitFor (Func<bool> Action, float delay) {
		float time = 0;
		while (time <= delay) {
			if (Action())
				time += Time.unscaledDeltaTime;
			else
				time = 0;
			yield return 0;
		}
		yield break;
	}
}


// Directions. Used as an index for links to neighboring slots.
public enum Side {
	Null, Top, Bottom, Right, Left,
	TopRight, TopLeft,
	BottomRight, BottomLeft
}

public struct int2 {
	public int x;
	public int y;
}