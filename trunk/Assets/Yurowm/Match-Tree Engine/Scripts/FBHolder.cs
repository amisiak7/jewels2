using UnityEngine;
using UnityEngine.UI;
using Facebook.MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Parse;

namespace FBHandler
{
    [ThreadSafe]
    public class FBHolder : MonoBehaviour
    {
        public struct LeaderboardListItem
        {
            public string id;
            public string playerName;
            public string score;
        }

        public delegate void CreateItems();
        CreateItems createItems;

        public GameObject FBIsLoggedIn;
        public GameObject FBIsNotLoggedIn;
        public GameObject FacebookButton;
        public GameObject FacebookLoginPopup;
        public GameObject ShareScoreCounter;
        public GameObject ShareLevelCounter;
        public GameObject NoInternetConnectionScreen;
        public GameObject InternetConnectionScreen;
        public GameObject InviteView;
        public GameObject GiftView;
        

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
        static List<LeaderboardListItem> leaderboardCheatList = new List<LeaderboardListItem>();
        // List containers that list Items - (Dynamically Increasing ListView <Custom>)
        public GameObject listInviteContainer, listLeaderboardContainer;
        //Prefabs that holds items that will be places in the containers.
        public ListItemInvite itemInvitePref;
        public ListItemLeaderboard itemLeaderPref;
        //strings that let you get JSON from the Facebook API calls.
        string loadLeaderboardString = "app/scores",
            loadInvitableFriendsString = "v2.0/me/invitable_friends";
        // Start is called once at startup
        public static bool inviteLoaded, leaderboardLoaded, giftLoaded;
        public GameObject leaderboardView;
        public GameObject notLoggedInPopUp;
        public Text levelNumberText;
        public Text levelCounterTextWinPopup;
        public Text levelCounterTextSelectLevel;

        public GameObject listGiftContainer;
        static List<ListItemGift> listGifts = new List<ListItemGift>();
        public ListItemGift itemGiftPref;
        public GameObject btnSendGift, btnSlctAllGift;

        private bool isConnectedToInternet;
        private float delay = 5;
        private float nextUsage;
        private bool isFBInitialized = false;
        private string playerName = "";
        private string playerPictureURL = "";
        private string playerID = "";
        private static bool parseProcessFinished = false;
        


        private Dictionary<string, string> profile = null;
        //private List<object> ScoresList = null;

        void Awake()
        {
            Time.timeScale = 1;
            CheckForInternetConnection();
            if(isConnectedToInternet)
            {
                FB.Init(SetInit, OnHideUnity);
            }
        }

        void Start()
        {
            nextUsage = Time.time + delay;
            GiftView.SetActive(false);
            InviteView.SetActive(true);
        }

        void CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    if(!isConnectedToInternet)
                    { 
                        NoInternetConnectionScreen.SetActive(false);
                        InternetConnectionScreen.SetActive(true);
                        isConnectedToInternet = true;
                        //FB.Logout();
                    }
                }
            }
            catch
            {
                if (isConnectedToInternet)
                {
                    NoInternetConnectionScreen.SetActive(true);
                    InternetConnectionScreen.SetActive(false);
                    isConnectedToInternet = false;
                }
            }
        }

        void Update()
        {
            if (Time.time > nextUsage)
            {
                CheckForInternetConnection();
                nextUsage = Time.time + delay;
            }
            if (isConnectedToInternet)
            {
                if (isFBInitialized)
                {
                    if (FB.IsLoggedIn)
                    {
                        FBIsNotLoggedIn.SetActive(false);
                        FBIsLoggedIn.SetActive(true);

                    }
                    else
                    {
                        FBIsNotLoggedIn.SetActive(true);
                        FBIsLoggedIn.SetActive(false);
                    }
                }
                else
                {
                    isFBInitialized = true;
                    FB.Init(SetInit, OnHideUnity);
                }
            }
        }
        private void SetInit()
        {
            Debug.Log("FB init done.");

            if (FB.IsLoggedIn)
            {
                Debug.Log("FB logged in");
            }
            else
            {
            }

        }

        private void OnHideUnity(bool isGameShown)
        {
            if (!isGameShown)
            {
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 1;
            }
        }

        public void OnButtonClick()
        {
            if (!FB.IsLoggedIn)
            {
                Debug.Log("Try Login");
                FB.Login("email,publish_actions,user_friends", AuthCallback);
            }
            else
            {
                FBIsNotLoggedIn.SetActive(true);
                FBIsLoggedIn.SetActive(false);
            }


        }

        void AuthCallback(FBResult result)
        {
            if (FB.IsLoggedIn)
            {
                Debug.Log("FB login worked.");
                FB.API(Util.GetPictureURLB("me", 128, 128), Facebook.HttpMethod.GET, DealWithProfilePicture);
                FB.API("/me?fields=id,first_name,last_name", Facebook.HttpMethod.GET, DealWithUserName);
            }
            else
            {
                Debug.Log("FB login failed.");
            }
        }

        void DealWithProfilePicture(FBResult result)
        {
            if (result.Error != null)
            {
                Debug.Log("Problem with getting the profile picture");

                FB.API(Util.GetPictureURLB("me", 128, 128), Facebook.HttpMethod.GET, DealWithProfilePicture);
                return;
            }

            Sprite temporaryAvatar = Sprite.Create(result.Texture, new Rect(0, 0, 128, 128), new Vector2(0, 0));
            Texture2D popupIconTexture = new Texture2D((int)temporaryAvatar.rect.width, (int)temporaryAvatar.rect.height);

            var pixels = temporaryAvatar.texture.GetPixels((int)temporaryAvatar.textureRect.x,
                                                    (int)temporaryAvatar.textureRect.y,
                                                    (int)temporaryAvatar.textureRect.width,
                                                    (int)temporaryAvatar.textureRect.height);

            popupIconTexture.SetPixels(pixels);
            popupIconTexture.Apply();


           // FacebookLoginPopup.GetComponent<AchievementPopup>().icon.GetComponent<Renderer>().material.mainTexture = popupIconTexture;
            popupIconTexture = CalculateTexture(128, 128, 64, 64, 64, popupIconTexture);
            temporaryAvatar = Sprite.Create(popupIconTexture, new Rect(0, 0, 128, 128), new Vector2(0, 0));
            Image userAvatar = FacebookButton.GetComponent<Image>();
            userAvatar.sprite = temporaryAvatar;
            LoadInvitableFriends();
        }

        void DealWithUserName(FBResult result)
        {
            if (result.Error != null)
            {
                Debug.Log("Problem with Username");
            }
            Debug.Log(result.Text);

            profile = Util.DeserializeJSONProfile(result.Text);
            playerName = profile["first_name"] + " " + profile["last_name"];
            Debug.LogWarning(playerName);
            playerPictureURL = "https://graph.facebook.com/" + profile["id"] + "/picture";
            playerID = profile["id"];
            Debug.Log(playerPictureURL);

            //string popupLoginMessage;
            //Debug.Log("Popup trial");
            //popupLoginMessage = profile["first_name"] + " has logged in.";
            //FacebookLoginPopup.GetComponent<AchievementPopup>().title.text = popupLoginMessage;
        }

        public void InviteFriends()
        {
            FB.AppRequest(
                message: "Join me in this great Berry Adventure!",
                title: "Invite Your friends to join You in game!"
                );
        }

        public void ShareWithFriends()
        {
            string shareMessage = "I just scored " + ShareScoreCounter.GetComponent<Text>().text + "points on " + ShareLevelCounter.GetComponent<Text>().text + ". You think You can beat me?";

            FB.Feed(
                linkCaption: shareMessage,
                picture: "https://lh3.googleusercontent.com/AJM4lHx7XgTO1S9fpIhCaiF4emgGnU_KMBW3jvnwv-hX0UhlNLTsLxFgFg1JIcu9lF4=w300",
                linkName: "Join me on this epic Berry Adventure",
                link: "https://apps.facebook.com/" + FB.AppId + "/?challenge_brag=" + (FB.IsLoggedIn ? FB.UserId : "guest"));
        }

        Texture2D CalculateTexture(
         int h, int w, float r, float cx, float cy, Texture2D sourceTex
         )
        {
            Color[] c = sourceTex.GetPixels(0, 0, sourceTex.width, sourceTex.height);
            Texture2D b = new Texture2D(h, w);
            for (int i = 0; i < (h * w); i++)
            {
                int y = Mathf.FloorToInt(((float)i) / ((float)w));
                int x = Mathf.FloorToInt(((float)i - ((float)(y * w))));
                if (r * r >= (x - cx) * (x - cx) + (y - cy) * (y - cy))
                {
                    b.SetPixel(x, y, c[i]);
                }
                else
                {
                    b.SetPixel(x, y, Color.clear);
                }
            }
            b.Apply();
            return b;
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

        delegate void LoadPictureCallback(Texture2D texture, int index);
        //Method to load leaderboard
        public void LoadLeaderboard()
        {
            if (!FB.IsLoggedIn || !FB.IsInitialized)
            {
                notLoggedInPopUp.gameObject.GetComponent<CPanel>().SetActive(true);
            }
            else
            {
                if (levelCounterTextSelectLevel.IsActive())
                {
                    leaderboardView.SetActive(true);
                    levelNumberText.text = levelCounterTextSelectLevel.text;
                    //FB.API(loadLeaderboardString, Facebook.HttpMethod.GET, CallBackLoadLeaderboard);
                    CallBackLoadLeaderboard();

                }
                else if(levelCounterTextWinPopup.IsActive())
                {
                    leaderboardView.SetActive(true);
                    levelNumberText.text = levelCounterTextWinPopup.text;
                    CallBackLoadLeaderboard();
                    //FB.API(loadLeaderboardString, Facebook.HttpMethod.GET, CallBackLoadLeaderboard);
                }
            }
        }

        public void CloseLeaderboard()
        {
            leaderboardView.SetActive(false);
            listLeaderboard = new List<ListItemLeaderboard>();
            for (int i = 0; i < listLeaderboardContainer.gameObject.transform.childCount; i++)
            {
                GameObject.Destroy(listLeaderboardContainer.transform.GetChild(0).gameObject);
            }
        }
        //callback of from Facebook API when the leaderboard data from the server is loaded.
        void CallBackLoadLeaderboard()
        {
            leaderboardLoaded = false;
            parseProcessFinished = false;
            leaderboardCheatList = new List<LeaderboardListItem>();
            StartCoroutine(GetDataFromCloud());
            StartCoroutine(LoadLeaderboardItemsFromList());
            leaderboardLoaded = true;
            
        }

        IEnumerator GetDataFromCloud()
        {
            var query = ParseObject.GetQuery("GameScore").WhereEqualTo("levelNumber", levelNumberText.text);
            query.FindAsync().ContinueWith(t =>
            {
                if (!t.IsFaulted)
                {
                    IEnumerable<ParseObject> gameScores = t.Result;
                    foreach (ParseObject gameScore in gameScores)
                    {
                        LeaderboardListItem tmpItem = new LeaderboardListItem();
                        tmpItem.score = gameScore["scoreCount"].ToString();
                        tmpItem.playerName = gameScore["playerName"].ToString();
                        tmpItem.id = gameScore["playerID"].ToString();
                        leaderboardCheatList.Add(tmpItem);
                        //Debug.Log("Item: " + ", Id: " + tmpItem.id + ", Name: " + tmpItem.playerName + ", Score: " + tmpItem.score);
                    }
                    parseProcessFinished = true;
                }
            });
            yield return new WaitForSeconds(0.5f);
        }

        IEnumerator LoadLeaderboardItemsFromList()
        {
            while (!parseProcessFinished)
            {
                yield return new WaitForSeconds(0.5f);
            }
            if (parseProcessFinished)
            {
                int i = 0;
                foreach (LeaderboardListItem item in leaderboardCheatList)
                {
                    CreateListItemLeaderboard(item.id, item.playerName, item.score);
                    LoadFriendsAvatar(i);
                    i++;
                }
            }
        } 

        // Method to load Friends Profile Pictures
        void LoadFriendsAvatar(int index)
        {
            Debug.Log(listLeaderboard.Count);
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
            FB.API("me/permissions", Facebook.HttpMethod.GET, delegate(FBResult result)
            {
                Debug.Log(result.Text);
            });

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
                string id = System.Convert.ToString(((Dictionary<string, object>)data[i])["id"]);
                string name = System.Convert.ToString(((Dictionary<string, object>)data[i])["name"]);
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
        public void UploadResultToParse(string levelNumber, int scoreCount)
        {
            var query = ParseObject.GetQuery("GameScore").WhereEqualTo("playerName", playerName).WhereEqualTo("levelNumber", levelNumber);
            query.FirstAsync().ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Debug.Log("Adding new score");
                        ParseObject gameScore = new ParseObject("GameScore");
                        gameScore["playerID"] = playerID;
                        gameScore["playerName"] = playerName;
                        gameScore["levelNumber"] = levelNumber;
                        gameScore["scoreCount"] = scoreCount;
                        gameScore["playerPictureURL"] = playerPictureURL;
                        Debug.Log(playerPictureURL);
                        gameScore.SaveAsync();
                    }
                    else
                    {
                        Debug.Log("Updating score");
                        ParseObject gameScore = t.Result;
                        if ((int)gameScore["scoreCount"] < scoreCount)
                        {
                            gameScore["scoreCount"] = scoreCount;
                            gameScore.SaveAsync();
                        }
                    }
                    
                }
            );
        }

        public void CloseGiftView()
        {
            GiftView.SetActive(false);
            InviteView.SetActive(true);
        }

        public void OpenGiftView()
        {
            InviteView.SetActive(false);
            GiftView.SetActive(true);
            FB.API(loadLeaderboardString, Facebook.HttpMethod.GET, SendGiftCallback);
        }

        public void SendGiftCallback(FBResult result)
        {
            //Deserializing JSON returned from server
            Dictionary<string, object> JSON = Json.Deserialize(result.Text) as Dictionary<string, object>;
            List<object> data = JSON["data"] as List<object>;

            if (result.Error != null)
            {
                FB.API(loadLeaderboardString, Facebook.HttpMethod.GET, SendGiftCallback);
                return;
            }
            //Loop to traverse and process all the items returned from the server.
            for (int i = 0; i < data.Count; i++)
            {
                Dictionary<string, object> UserInfo = ((Dictionary<string, object>)data[i])["user"] as Dictionary<string, object>;
                string name = System.Convert.ToString(((Dictionary<string, object>)UserInfo)["name"]);
                string id = System.Convert.ToString(((Dictionary<string, object>)UserInfo)["id"]);
                if (!name.Equals(playerName))
                {
                    CreateListItemGift(id, name);
                    LoadGiftAvatar(i);
                }
            }
            giftLoaded = true;
        }

        public void CreateListItemGift(string id, string fName)
        {
            ListItemGift tempItem = Instantiate(itemGiftPref) as ListItemGift;
            tempItem.fId = id;
            tempItem.txtName.text = fName;
            tempItem.transform.SetParent(listGiftContainer.transform, false);
            listGifts.Add(tempItem);
        }

        private void LoadGiftAvatar(int index)
        {
            FB.API(Util.GetPictureURLA(listGifts[index].fId), Facebook.HttpMethod.GET, result =>
            {
                if (result.Error != null)
                {
                    Util.LogError(result.Error);
                    return;
                }
                listGifts[index].picUrl = Util.DeserializePictureURLString(result.Text);
                StartCoroutine(LoadFPicRoutine(listGifts[index].picUrl, PicCallBackGift, index));
            });
        }

        private void PicCallBackGift(Texture2D texture, int index)
        {
            if (texture == null)
            {
                StartCoroutine(LoadFPicRoutine(listGifts[index].picUrl, PicCallBackGift, index));
                return;
            }
            listGifts[index].imgPic.sprite = Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
                );
        }

        public void SendGift()
        {

        }
    }
}