using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FBHolder : MonoBehaviour
{

    void Awake()
    {
        FB.Init(SetInit, OnHideUnity);
    }

    void Start()
    {
       //gameObject.GetComponent<Image>()
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

    public void FBLogin()
    {
            FB.Login("email", AuthCallback);
    }

    void AuthCallback(FBResult result)
    {
        if (FB.IsLoggedIn)
        {
            Debug.Log("FB login worked.");
            FB.API(Util.GetPictureURL("me", 128, 128), Facebook.HttpMethod.GET, DealWithProfilePicture);
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
        Image userAvatar = GameObject.Find("Facebook").GetComponent<Image>();
        userAvatar.sprite = temporaryAvatar;
        //Debug.Log(userAvatar.sprite.name);
        //userAvatar.sprite = Sprite.Create(result.Texture, new Rect(0, 0, 128, 128), new Vector2(0, 0));
    }
}
