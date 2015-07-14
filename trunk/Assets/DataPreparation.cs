using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Parse;

public class DataPreparation : MonoBehaviour {

    public ParseObject gameScore = new ParseObject("GameScore");
    public Text levelCounter;
    public GameObject FBHolderReference;

    void OnEnable()
    {
        FBHolderReference.GetComponent<FBHandler.FBHolder>().UploadResultToParse(levelCounter.text,SessionAssistant.main.score);
    }
}
