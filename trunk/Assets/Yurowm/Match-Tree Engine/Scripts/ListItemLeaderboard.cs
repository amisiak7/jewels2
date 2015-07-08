using UnityEngine;
using System.Collections;
using UnityEngine.UI;
namespace FBHandler
{
    public class ListItemLeaderboard : MonoBehaviour
    {
        //Class that holds items for Our Custom dynamic Leaderboard ListView
        public string fId, picUrl; // holds id and pic URL returned from server.
        public Text txtName, txtScore; //Used to Store and Display Name and Score from the server
        public Image imgPic; // Image View to show image of the specified ID

        public void AssignValues(string fId, string picUrl, string txtName, string txtScore, Sprite imgPic)
        {
            this.fId = fId;
            this.picUrl = picUrl;
            this.txtName.text = txtName;
            this.txtScore.text = txtScore;
            this.imgPic.sprite = imgPic;
        }
    }
}
