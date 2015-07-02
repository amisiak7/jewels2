using UnityEngine;
using System.Collections;

public abstract class BlockInterface : MonoBehaviour {
	public Slot slot;

	abstract public void BlockCrush ();
	abstract public bool CanBeCrushedByNearSlot ();
}
