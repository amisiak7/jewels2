using UnityEngine;
using System.Collections;
using UnityEngine.UI;
namespace GameSlyce
{
    public class SplashUIManager : MonoBehaviour
    {
        public Text statusTxt;
        public void LoginNContinue()
        {
            if (!FB.IsInitialized)
            {
                FB.Init(onInitComplete, onHideUnity);
            }
            else
            {
                if (FB.IsLoggedIn)
                {
                    statusTxt.text = "Successfully Logged In.";
                    Application.LoadLevel(1);
                }
                else
                {
                    LoginFB();
                }
            }
        }

        private void onInitComplete()
        {
            LoginFB();
        }

        private void onHideUnity(bool isUnityShown)
        {
            if (isUnityShown)
            {
                Time.timeScale = 1;
                statusTxt.text = "Facebook Initialized.";
            }
            else
            {
                statusTxt.text = "Initializing Facebook!!!";
                Time.timeScale = 0;
            }
        }

        void LoginFB()
        {
            statusTxt.text = "Logging In!";
            FB.Login("public_profile,user_friends,publish_actions", LoginCallback);
        }
        //Callback method of login
        void LoginCallback(FBResult result)
        {
            if (result.Error != null)
            {
                statusTxt.text = "Something messed Up Trying Again!!!";
                LoginFB();
            }
            else if (!FB.IsLoggedIn)
            {
                statusTxt.text = "Login cancelled by User";
            }
            else
            {
                statusTxt.text = "Successfully Logged In.";
                Application.LoadLevel(1);
            }
        }
    }
}