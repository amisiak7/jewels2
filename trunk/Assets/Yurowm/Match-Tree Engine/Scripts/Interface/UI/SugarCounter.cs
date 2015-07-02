using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Indicator of count of remaining sugards in SugarDrop mode.
[RequireComponent(typeof(Text))]
public class SugarCounter : MonoBehaviour {

    Text label;

    void Awake() {
        label = GetComponent<Text>();
    }

    void Update() {
        label.text = SessionAssistant.main.targetSugarDropsCount.ToString();
    }
}