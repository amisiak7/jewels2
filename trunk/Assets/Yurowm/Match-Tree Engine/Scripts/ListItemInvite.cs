using UnityEngine;
using UnityEngine.UI;
namespace FBHandler
{
    public class ListItemInvite : MonoBehaviour
    {
        //Class that Holds items of our dynamic custom Invitable ListView
        public Toggle tglBtn; // toggle button to select item
        public string fId, picUrl; //holds id and pic URL returned from server.
        public Text txtName; //Used to Store and Display Name from the server
        public Image imgPic; // Image View to show image of the specified ID

        private GameObject FBHolderReference;

        void Start()
        {
            tglBtn.GetComponent<Toggle>().onValueChanged.AddListener(ToggleClicked);
            FBHolderReference = GameObject.FindGameObjectWithTag("FBHolder");
            Debug.Log(FBHolderReference.name);
        }

        private void ToggleClicked(bool state)
        {
            FBHolderReference.GetComponent<FBHolder>().ChangeToggleState(FBHolder.ToggleState.Partial);
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