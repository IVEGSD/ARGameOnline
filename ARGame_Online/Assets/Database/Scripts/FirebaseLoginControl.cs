using System;
using Firebase.Auth;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FirebaseLoginControl : MonoBehaviour
{
    [Header("Registration")] 
    public InputField EmailAddress;
    public InputField Password;
    public InputField UserName;
    
    [Header("Login")] 
    public InputField LoginEmail;
    public InputField  LoginPassword;
    
    [Header("Main Menu Control")]
    public MainMenuController MainMenuController;
    
    [Header("Debug")]
    public Text DebugLogText;
    
    

    public static string Useremail;
    
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void Start()
    {
        DebugLogText.text = "";
    }
    
    public void Login()
    {
        FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(LoginEmail.text, LoginPassword.text).ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("Login was canceled.");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError("Login encountered an aerror: " + task.Exception);
                return;
            }
            
            // Firebase user has been created.
            
            // debug
            FirebaseUser newUser = task.Result;
            Debug.LogFormat("Firebase user Login successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
            // store user email for database management
            Useremail = newUser.Email;
            // scene transition
            SceneManager.LoadSceneAsync("ARScene");
        });
    }

    public void LoginAnonymous()
    {
        FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync().ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("LoginAnonymous was canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("LoginAnonymous encountered an aerror: " + task.Exception);
                    return;
                }
                
                // debug
                FirebaseUser newUser = task.Result;
                Debug.LogFormat("Firebase Anonymous created successfully: {0} ({1})",
                    newUser.DisplayName, newUser.UserId);
                // scene transition
                SceneManager.LoadSceneAsync("ARScene");
            });
    }

    public void CreateUser()
    {
        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(EmailAddress.text, Password.text).ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithIDAsync was canceled.");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError("CreateUser encountered an aerror: " + task.Exception);
                return;
            }
            
            //Firebase user has been created.
            FirebaseUser newUser = task.Result;
            DebugLogText.text = "User Created.";
            UpdateUserProfile(UserName.text);
            DebugLogText.text += "User Profile Updated.";
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
           
            // back to menu if user is created.
            MainMenuController.Press_Cancel();
        });
    }

    // Update Player's username
    private void  UpdateUserProfile([NotNull] string username)
    {
        if (string.IsNullOrEmpty(username)) throw new ArgumentException("Value cannot be null or empty.", "username");
        // get current user
        var user = FirebaseAuth.DefaultInstance.CurrentUser;

        if (user == null)
        {
            Debug.LogError("Cannot get current user. It is null.");
        }
        else
        {
            // create user profile : name
            var profile = new UserProfile()
            {
                DisplayName = username
            };
            
            // udpate user profile
            user.UpdateUserProfileAsync(profile).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateUserProfile was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfile encontered an error: " + task.Exception);
                    return;
                }
                Debug.Log("User profile updated successfully.");
            });
        }
    }
}