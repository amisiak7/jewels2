using UnityEngine;
using UnityEngine.UI;
namespace FBHandler
{
    public class ListItemGift : MonoBehaviour
    {
        //Class that Holds items of our dynamic custom Invitable ListView
        public Button sendGiftButton; // toggle button to select item
        public string fId, picUrl; //holds id and pic URL returned from server.
        public Text txtName; //Used to Store and Display Name from the server
        public Image imgPic; // Image View to show image of the specified ID

        void Start()
        {
            sendGiftButton.onClick.RemoveAllListeners();
            UnityEngine.Events.UnityAction action = () => { GameObject.FindGameObjectWithTag("FBHolder").GetComponent<FBHolder>().SendGift(); };
            Debug.Log(action.Method.ToString());
            sendGiftButton.onClick.AddListener(action);
        }

        public void AssignValues(string fId, string picUrl, string txtName, Sprite imgPic)
        {
            this.fId = fId;
            this.picUrl = picUrl;
            this.txtName.text = txtName;
            this.imgPic.sprite = imgPic;
        }
    }
}