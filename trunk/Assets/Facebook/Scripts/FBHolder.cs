using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FBHolder : MonoBehaviour
{

    public GameObject FBIsLoggedIn;
    public GameObject FBIsNotLoggedIn;
    public GameObject FacebookButton;
    public GameObject FacebookLoginPopup;

    private Dictionary<string, string> profile = null;

    void Awake()
    {
        FB.Init(SetInit, OnHideUnity);
    }

    void Update()
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
            FB.Login("email", AuthCallback);
        }
        else
        {
            //FBIsNotLoggedIn.SetActive(true);
            //FBIsLoggedIn.SetActive(false);
        }

    }

    void AuthCallback(FBResult result)
    {
        if (FB.IsLoggedIn)
        {
            Debug.Log("FB login worked.");
            FB.API(Util.GetPictureURL("me", 128, 128), Facebook.HttpMethod.GET, DealWithProfilePicture);
            FB.API("/me?fields=id,first_name", Facebook.HttpMethod.GET, DealWithUserName);
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

            FB.API(Util.GetPictureURL("me", 128, 128), Facebook.HttpMethod.GET, DealWithProfilePicture);
            return;
        }

        Sprite temporaryAvatar = Sprite.Create(result.Texture, new Rect(0, 0, 128, 128), new Vector2(0, 0));
        Texture2D popupIconTexture = new Texture2D( (int)temporaryAvatar.rect.width, (int)temporaryAvatar.rect.height );
     
        var pixels = temporaryAvatar.texture.GetPixels(  (int)temporaryAvatar.textureRect.x, 
                                                (int)temporaryAvatar.textureRect.y, 
                                                (int)temporaryAvatar.textureRect.width, 
                                                (int)temporaryAvatar.textureRect.height );
 
        popupIconTexture.SetPixels( pixels );
        popupIconTexture.Apply();

        
        FacebookLoginPopup.GetComponent<AchievementPopup>().icon.GetComponent<Renderer>().material.mainTexture = popupIconTexture;
        popupIconTexture = CalculateTexture(128, 128, 64, 64, 64, popupIconTexture);
        temporaryAvatar = Sprite.Create(popupIconTexture, new Rect(0, 0, 128, 128), new Vector2(0, 0));
        Image userAvatar = FacebookButton.GetComponent<Image>();
        userAvatar.sprite = temporaryAvatar;
    }

    void DealWithUserName(FBResult result)
    {
        if (result.Error != null)
        {
            Debug.Log("Problem with Username");
        }

        profile = Util.DeserializeJSONProfile(result.Text);

        string popupLoginMessage;

        popupLoginMessage = profile["first_name"] + " has logged in.";
        FacebookLoginPopup.GetComponent<AchievementPopup>().title.text = popupLoginMessage;
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
        FB.Feed(
            linkCaption:"I'm playing this awesome game right now!!!",
            picture:"https://lh3.googleusercontent.com/AJM4lHx7XgTO1S9fpIhCaiF4emgGnU_KMBW3jvnwv-hX0UhlNLTsLxFgFg1JIcu9lF4=w300",
            linkName:"Check out this Game!",
            link:"https://apps.facebook.com/" + FB.AppId + "/?challenge_brag=" + (FB.IsLoggedIn ? FB.UserId : "guest"));
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

}
