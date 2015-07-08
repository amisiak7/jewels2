using Facebook.MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FBHandler
{

    public class _UIManager : MonoBehaviour
    {
        public GameObject loadingPanel;
        public Sprite[] stateSprites;
        public enum ToggleState
        {
            Unchecked, Partial, Checked
        };
        public ToggleState tglStateSlctAll = ToggleState.Unchecked;
        //Reference to Invite Button and select All- toggler
        public GameObject btnInvite, btnSlctAll;
        // List of the invite and leaderboard list items
        static List<ListItemInvite> listInvites = new List<ListItemInvite>();
        static List<ListItemLeaderboard> listLeaderboard = new List<ListItemLeaderboard>();
        // List containers that list Items - (Dynamically Increasing ListView <Custom>)
        public GameObject listInviteContainer, listLeaderboardContainer;
        //Prefabs that holds items that will be places in the containers.
        public ListItemInvite itemInvitePref;
        public ListItemLeaderboard itemLeaderPref;
        //strings that let you get JSON from the Facebook API calls.
        string loadLeaderboardString = "app/scores",
            loadInvitableFriendsString = "me/invitable_friends";
        // Start is called once at startup
        public static bool inviteLoaded, leaderboardLoaded;
        void Start()
        {
            btnSlctAll.GetComponent<Image>().sprite = stateSprites[0];
            btnInvite.SetActive(false);

            if (inviteLoaded && leaderboardLoaded)
            {
                LoadData();
            }
            else if (FB.IsLoggedIn)
            {
                LoadLeaderboard();
                LoadInvitableFriends();
            }
        }
        void Update()
        {
            if (inviteLoaded && leaderboardLoaded)
            {
                loadingPanel.SetActive(false);
            }
        }

        //Click Handler of Select ALl Button
        public void TglSelectAllClickHandler()
        {
            switch (tglStateSlctAll)
            {
                case ToggleState.Partial:
                case ToggleState.Unchecked:
                    foreach (var item in listInvites)
                    {
                        item.tglBtn.isOn = true;
                    }
                    tglStateSlctAll = ToggleState.Checked;
                    ChangeToggleState(ToggleState.Checked);
                    break;
                case ToggleState.Checked:
                    foreach (var item in listInvites)
                    {
                        item.tglBtn.isOn = false;
                    }
                    ChangeToggleState(ToggleState.Unchecked);
                    break;
            }
        }
        //Method to change Toggle State On the Fly
        public void ChangeToggleState(ToggleState state)
        {
            switch (state)
            {
                case ToggleState.Unchecked:
                    tglStateSlctAll = state;
                    btnSlctAll.GetComponent<Image>().sprite = stateSprites[0];
                    break;
                case ToggleState.Partial:
                    bool flagOn = false, flagOff = false;
                    foreach (var item in listInvites)
                    {
                        if (item.tglBtn.isOn)
                        {
                            flagOn = true;
                        }
                        else
                        {
                            flagOff = true;
                        }
                    }
                    if (flagOn && flagOff)
                    {
                        tglStateSlctAll = state;
                        btnSlctAll.GetComponent<Image>().sprite = stateSprites[1];
                        //Debug.Log("Partial");
                    }
                    else if (flagOn && !flagOff)
                    {
                        ChangeToggleState(ToggleState.Checked);
                        //Debug.Log("Checked");
                    }
                    else if (!flagOn && flagOff)
                    {
                        ChangeToggleState(ToggleState.Unchecked);
                        //Debug.Log("Unchecked");
                    }
                    break;
                case ToggleState.Checked:
                    tglStateSlctAll = state;
                    btnSlctAll.GetComponent<Image>().sprite = stateSprites[2];
                    break;
            }
        }

        //method to logout facebook
        public void LogoutFB()
        {
            Application.LoadLevel(0);
            FB.Logout();
        }
        //Method to post score to the server.
        public void PostScore(string score)
        {
            var query = new Dictionary<string, string>();
            query["score"] = score;
            FB.API("/me/scores", Facebook.HttpMethod.POST, delegate(FBResult r) { Util.Log("Result: " + r.Text); }, query);
        }
        //helpfull delegate that helps in loading Picture from URL
        delegate void LoadPictureCallback(Texture2D texture, int index);
        //Method to load leaderboard
        public void LoadLeaderboard()
        {
            FB.API(loadLeaderboardString, Facebook.HttpMethod.GET, CallBackLoadLeaderboard);
        }
        //callback of from Facebook API when the leaderboard data from the server is loaded.
        void CallBackLoadLeaderboard(FBResult result)
        {
            //Deserializing JSON returned from server
            Dictionary<string, object> JSON = Json.Deserialize(result.Text) as Dictionary<string, object>;
            List<object> data = JSON["data"] as List<object>;

            if (result.Error != null)
            {
                FB.API(loadLeaderboardString, Facebook.HttpMethod.GET, CallBackLoadLeaderboard);
                return;
            }
            //Loop to traverse and process all the items returned from the server.
            for (int i = 0; i < data.Count; i++)
            {
                string fScore = System.Convert.ToString(((Dictionary<string, object>)data[i])["score"]);
                Dictionary<string, object> UserInfo = ((Dictionary<string, object>)data[i])["user"] as Dictionary<string, object>;
                string name = System.Convert.ToString(((Dictionary<string, object>)UserInfo)["name"]);
                string id = System.Convert.ToString(((Dictionary<string, object>)UserInfo)["id"]);
                CreateListItemLeaderboard(id, name, fScore);
                LoadFriendsAvatar(i);
            }
            leaderboardLoaded = true;
        }
        // Method to load Friends Profile Pictures
        void LoadFriendsAvatar(int index)
        {
            FB.API(Util.GetPictureURLA(listLeaderboard[index].fId), Facebook.HttpMethod.GET, result =>
            {
                if (result.Error != null)
                {
                    Util.LogError(result.Error);
                    return;
                }
                listLeaderboard[index].picUrl = Util.DeserializePictureURLString(result.Text);
                StartCoroutine(LoadFPicRoutine(listLeaderboard[index].picUrl, PicCallBackLeaderboard, index));
            });
        }
        // Method that Proceeds with the Invitable Friends
        public void LoadInvitableFriends()
        {
            FB.API(loadInvitableFriendsString, Facebook.HttpMethod.GET, CallBackLoadInvitableFriends);

        }
        //Callback of Invitable Friends API Call
        void CallBackLoadInvitableFriends(FBResult result)
        {
            //Deserializing JSON returned from server
            Debug.Log("Data from JSON");
            Debug.Log(result.Text);
            Dictionary<string, object> JSON = Json.Deserialize(result.Text) as Dictionary<string, object>;
            Debug.Log(JSON["data"]);
            List<object> data = JSON["data"] as List<object>;
            //Loop to traverse and process all the items returned from the server.
            for (int i = 0; i < data.Count; i++)
            {
                Debug.Log("ID key trial");
                string id = System.Convert.ToString(((Dictionary<string, object>)data[i])["id"]);
                Debug.Log("Name key trial");
                string name = System.Convert.ToString(((Dictionary<string, object>)data[i])["name"]);
                Debug.Log("Picture key trial");
                Dictionary<string, object> picInfo = ((Dictionary<string, object>)data[i])["picture"] as Dictionary<string, object>;
                string url = Util.DeserializePictureURLObject(picInfo);
                CreateListItemInvite(id, name, url);
                StartCoroutine(LoadFPicRoutine(url, PicCallBackInvitable, i));
            }
            btnInvite.SetActive(true);
            inviteLoaded = true;
        }
        //Method to add item to the custom invitable dynamically scrollable list
        void CreateListItemInvite(string id, string fName, string url = "")
        {
            ListItemInvite tempItem = Instantiate(itemInvitePref) as ListItemInvite;
            tempItem.fId = id;
            tempItem.picUrl = url;
            tempItem.txtName.text = fName;
            tempItem.transform.SetParent(listInviteContainer.transform, false);
            listInvites.Add(tempItem);
        }
        //Method to all items to the leaderboard dynamically scrollable list
        void CreateListItemLeaderboard(string id, string fName, string fScore = "")
        {
            ListItemLeaderboard tempItem = Instantiate(itemLeaderPref) as ListItemLeaderboard;
            tempItem.fId = id;
            tempItem.txtName.text = fName;
            tempItem.txtScore.text = fScore;
            tempItem.transform.SetParent(listLeaderboardContainer.transform, false);
            listLeaderboard.Add(tempItem);
        }
        //Coroutine to load Picture from the specified URL
        IEnumerator LoadFPicRoutine(string url, LoadPictureCallback Callback, int index)
        {
            WWW www = new WWW(url);
            yield return www;
            Callback(www.texture, index);
        }
        //Callback of Invitable Friend API call
        private void PicCallBackInvitable(Texture2D texture, int index)
        {
            if (texture == null)
            {
                StartCoroutine(LoadFPicRoutine(listInvites[index].picUrl, PicCallBackInvitable, index));
                return;
            }
            listInvites[index].imgPic.sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
                );
        }
        //Callback to Score API Call
        private void PicCallBackLeaderboard(Texture2D texture, int index)
        {
            if (texture == null)
            {
                StartCoroutine(LoadFPicRoutine(listLeaderboard[index].picUrl, PicCallBackLeaderboard, index));
                return;
            }
            listLeaderboard[index].imgPic.sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
                );
        }
        //ClickHandling Method that Sends Backend Facebook Native App request (Invitable)Calls
        public void SendInvites()
        {
            List<string> lstToSend = new List<string>();
            foreach (var item in listInvites)
            {
                if (item.tglBtn.isOn)
                {
                    lstToSend.Add(item.fId);
                }
            }
            int dialogCount = (int)Mathf.Ceil(lstToSend.Count / 50f);
            CallInvites(lstToSend, dialogCount);
        }
        //Helping method that will be recursive if you'll have to sent invites to more than 50 Friends.
        public string invMessage, invTitle;
        private void CallInvites(List<string> lstToSend, int dialogCount)
        {
            if (dialogCount > 0)
            {
                string[] invToSend = (lstToSend.Count >= 50) ? new string[50] : new string[lstToSend.Count];

                for (int i = 0; i < invToSend.Length; i++)
                {
                    try
                    {
                        if (lstToSend[i] != null)
                        {
                            invToSend[i] = lstToSend[i];
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message);
                    }
                }
                lstToSend.RemoveRange(0, invToSend.Length);
                FB.AppRequest(
                    invMessage,
                    invToSend,
                    null,
                    null,
                    null,
                    "",
                    invTitle,
                    FBResult =>
                    {
                        if (--dialogCount > 0)
                        {
                            CallInvites(lstToSend, dialogCount);
                        }
                    }
                );
            }
        }
        public void LoadData()
        {
            for (int i = 0; i < listLeaderboard.Count; i++)
            {
                ListItemLeaderboard tmp = Instantiate(itemLeaderPref) as ListItemLeaderboard;
                tmp.AssignValues(listLeaderboard[i].fId, listLeaderboard[i].picUrl, listLeaderboard[i].txtName.text,
                listLeaderboard[i].txtScore.text, listLeaderboard[i].imgPic.sprite);
                tmp.transform.SetParent(listLeaderboardContainer.transform, false);
                listLeaderboard[i] = tmp;

            }
            for (int j = 0; j < listInvites.Count; j++)
            {
                ListItemInvite tmp = Instantiate(itemInvitePref) as ListItemInvite;
                tmp.AssignValues(listInvites[j].fId, listInvites[j].picUrl,
                    listInvites[j].txtName.text, listInvites[j].imgPic.sprite);
                tmp.transform.SetParent(listInviteContainer.transform, false);
                listInvites[j] = tmp;
            }
        }
        //Click Handler for Back Buttons
        public void BackMain()
        {
            Application.LoadLevel(0);
        }

        //public void BackInvite()
        //{
        //    _animListViewInv.Play(Animator.StringToHash("InvMoveDown"));
        //}
        //public void BackLeaderboard()
        //{
        //    _animListViewLeader.Play(Animator.StringToHash("InvMoveDown"));
        //}

        //Method to Change Value of LoadingProgressBar
    }
}