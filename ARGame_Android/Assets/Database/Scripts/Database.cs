using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;
using UnityEngine;
using UnityEngine.UI;

namespace Database.Scripts
{
    public class Database : MonoBehaviour
    {
        // for storing all player who is online
        private readonly Dictionary<string, object> players = new Dictionary<string, object>();
        private readonly Dictionary<string, GameObject> playersGameObject = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Vector3> playersTargetPosition = new Dictionary<string, Vector3>();
        private GameObject currentUserGameObject;
        public Transform CharacterTransform;
        public GameObject OnlinePlayer;
        

        // for check dependencies is available for database 
        private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

        // player's profile
        private string email;
        private string username;
        private long score;

        // update logic
        private int count;    // if user data updated, initialize its prefab
        private int initCount;    // if user data updated, initialize its prefab
        private float posX, posZ;    // track user position and update to database
        private string prefabType;    // player prefab type (control character)
        private bool prefabInitCompleted;    // if prefab initialization completed, set component


        public Text ScoreText;    //for show score
        public float Speed = 0.1f;    // tune


        private void Start()
        {
            // initialize player's position
            posX = CharacterTransform.position.x;
            posZ = CharacterTransform.position.z;
            prefabInitCompleted = false;
            initCount = 0;
            username = FirebaseLoginControl.UserID ?? "QWERTY";

            // get current user
            Debug.Log("Current User: " + FirebaseLoginControl.Useremail);
            // debug email address
            email = FirebaseLoginControl.Useremail ?? "abc@gmail.com";

            // check dependencies is available for database 
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                    InitializeFirebase();
                else
                    Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            });

            // initialize player's record
            _InitializePlayerRecord();

            // value change listener
            _PlayerPositionListener();
        }

        private void InitializePlayerComponent()
        {
            DebugLog("InitializePlayerComponent" + prefabType);
            var prefab = Resources.Load<GameObject>(prefabType);
            var player = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            player.GetComponent<Collider>().enabled = true;
            ;
            player.name = email ?? "No Name";
            player.transform.parent = CharacterTransform;
        }

        private void Update()
        {
            ScoreText.text = "Score : " + score;
            if (prefabInitCompleted && count == 0)
            {
                InitializePlayerComponent();
                prefabInitCompleted = false;
                count++;
            }

            // keep track character position
            posX = CharacterTransform.position.x;
            posZ = CharacterTransform.position.z;


            // if user is inputting, update position
            if (JoystickController.IsMoving) _UpdatePosition();

            // update online player's position
            foreach (var player in playersGameObject)
            {
                if (player.Key.Equals(email)) continue;
                if (player.Value.transform.position == playersTargetPosition[player.Key]) continue;
                var move = Vector3.Lerp(
                    player.Value.transform.position,
                    playersTargetPosition[player.Key],
                    Speed);
                player.Value.transform.position = move;
                Debug.LogError("Target Position: " + playersTargetPosition[player.Key]);
                Debug.LogError("Online Player Position Updated.");
            }
        }

        // initialize online database
        private void InitializeFirebase()
        {
            // get database instance by firebase link
            var app = FirebaseApp.DefaultInstance;
            app.SetEditorDatabaseUrl("https://argamedemo.firebaseio.com/");
            if (app.Options.DatabaseUrl != null) app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);
        }

        // before game start, initialize player record in the database
        public void _InitializePlayerRecord()
        {
            Debug.Log("Initializing Player Record...");
            Debug.Log("Attempting to initialize Player Record...");

            FirebaseDatabase.DefaultInstance.GetReference("Users").Child(username)
                .RunTransaction(InitializePlayerTransaction).ContinueWith(task =>
                {
                    if (task.Exception != null)
                        Debug.LogError(task.Exception);
                    else if (task.IsCompleted) Debug.Log("Player Record initialization completed.");
                });
        }

        // continue process of InitializePlayerRecord();
        private TransactionResult InitializePlayerTransaction(MutableData mutableData)
        {
            var users = mutableData.Value as List<object> ?? new List<object>();

            var isRemove = false; // determine user exist in database or not
            var newUserRecord = new Dictionary<string, object>();

            // Update player's records
            foreach (var user in users)
            {
                if ((string) ((Dictionary<string, object>) user)["email"] != email) continue;

                // reset current user record
                newUserRecord = (Dictionary<string, object>) user;
                users.Remove(user);
                Debug.Log("Current user is exist in database...");
                Debug.LogError("Current user is removed...");
                isRemove = true;
                break;
            }

            // update user record
            newUserRecord["online"] = true;
            newUserRecord["posX"] = 0;
            newUserRecord["posZ"] = 0;

            // player not exist in database, initialize player records
            if (!isRemove)
            {
                newUserRecord["email"] = email;
                newUserRecord["type"] = "Sphere";
                newUserRecord["score"] = 0;
                Debug.Log("Current user is new user.");
            }

            initCount++;
            // get the current player's prefab type, for game object instantiation
            if (initCount == 2)
            {
                prefabType = newUserRecord["type"].ToString();
                Debug.LogError(prefabType);
                prefabInitCompleted = true;
            }

            // add to firebase database
            users.Add(newUserRecord);
            mutableData.Value = users;
            Debug.Log("New user record is created.");
            return TransactionResult.Success(mutableData);
        }

        // get players position on database when position changed in database
        private void _PlayerPositionListener()
        {
            Debug.Log("Attempting to listen player status change...");
            FirebaseDatabase.DefaultInstance.GetReference("Users").ValueChanged += (sender, e1) =>
            {
                if (e1.DatabaseError != null)
                {
                    Debug.LogError(e1.DatabaseError.Message);
                    return;
                }

                Debug.Log("Received values for players.");
                if (e1.Snapshot == null || e1.Snapshot.ChildrenCount <= 0) return;
                foreach (var childs in e1.Snapshot.Children)
                foreach (var child in childs.Children)
                {
                    var childEmailKey = child.Child("email").Value.ToString();
//						Debug.LogError(childEmailKey);
                    // if player is online, add to list players
                    if ((bool) child.Child("online").Value)
                    {
                        // if contains player, remove it and add updated list:players
                        if (players.ContainsKey(childEmailKey)) players.Remove(childEmailKey);
                        players.Add(childEmailKey, child.Value as Dictionary<string, object>);

                        // create other player game object and add to list playersGameObject
                        if (child.Child("email").Value.ToString().Equals(email)) continue;

                        // get updated online player position
                        var x = float.Parse(child.Child("posX").Value.ToString());
                        var z = float.Parse(child.Child("posZ").Value.ToString());
                        Debug.Log("PosX:" + x + ", PosZ:" + z);
                        
                        if (playersGameObject.ContainsKey(childEmailKey))
                        {
                            var newPosition = new Vector3(x, 0, z);
                            playersTargetPosition[childEmailKey] = newPosition;
                            Debug.LogError("New Position Set: " + newPosition);
                        }
                        else
                        {
                            // instantiate other player if player is not exist in playersGameObject
                            var prefab = Resources.Load<GameObject>((child.Child("type").Value.ToString() + "Online"));
                            var player = Instantiate(prefab, new Vector3(x, 0, z), Quaternion.identity);
                            player.name = childEmailKey;
							player.transform.parent = OnlinePlayer.transform;
                            playersGameObject.Add(childEmailKey, player);
                            playersTargetPosition.Add(childEmailKey, prefab.transform.position);
                        }
                    }
                    else
                    {
                        // if players contains player is offline, remove it with game object
                        if (players.ContainsKey(childEmailKey)) players.Remove(childEmailKey);

                        if (!playersGameObject.ContainsKey(childEmailKey)) continue;
                        Destroy(playersGameObject[childEmailKey], 0f);
                        playersGameObject.Remove(childEmailKey);
                        playersTargetPosition.Remove(childEmailKey);
                    }
                }
            };
        }


        // add player score
        public void _AddScore()
        {
            if (string.IsNullOrEmpty(email))
            {
                DebugLog("invalid email.");
                return;
            }

            DebugLog("Attempting to add score...");

            var reference = FirebaseDatabase.DefaultInstance.GetReference("UsersScore").Child(username);

            DebugLog("Running Transaction...");
            reference.RunTransaction(AddScoreTransaction)
                .ContinueWith(task =>
                {
                    if (task.Exception != null)
                        DebugLog(task.Exception.ToString());
                    else if (task.IsCompleted) DebugLog("Transaction complete.");
                });
        }

        private TransactionResult AddScoreTransaction(MutableData mutableData)
        {
            var users = mutableData.Value as List<object>;

            if (users == null)
            {
                users = new List<object>();
            }
            else
            {
                object targetUser = null;
                foreach (var user in users)
                {
                    if (!(user is Dictionary<string, object>))
                        continue;
                    if (!((Dictionary<string, object>) user)["email"].ToString().Equals(email))
                        continue;
                    var userScore = (long) ((Dictionary<string, object>) user)["score"];
                    if (score == long.MaxValue) score = 0;
                    score = ++userScore;
                    targetUser = user;
                }

                users.Remove(targetUser);
            }

            /*Dictionary<string, object> newScoreMap = new Dictionary<string, object>();
    
    // delete user if existed in database
    foreach (var user in users)
    {
        if ((string) ((Dictionary<string, object>) user)["email"] != email) continue;
        users.Remove(user);
        newScoreMap = (Dictionary<string, object>)user;
        break;
    }

    newScoreMap["email"] = email;
    newScoreMap["score"] = (long) newScoreMap["score"] + 1;
    users.Add(newScoreMap);*/
            var newScoreMap = new Dictionary<string, object>();
            newScoreMap["score"] = score;
            newScoreMap["email"] = email;

            users.Add(newScoreMap);

            mutableData.Value = users;
            return TransactionResult.Success(mutableData);
        }

        // update player transform
        public void _UpdatePosition()
        {
            Debug.Log("Start to Update Position");
            var reference = FirebaseDatabase.DefaultInstance.GetReference("Users").Child(username);
            //DebugLog("Running Transaction...");

            reference.RunTransaction(UpdatePositionTransaction)
                .ContinueWith(task =>
                {
                    if (task.Exception != null)
                        Debug.Log(task.Exception.ToString());
                    else if (task.IsCompleted) Debug.Log("Transaction complete.");
                });
        }

        private TransactionResult UpdatePositionTransaction(MutableData mutableData)
        {
            var users = mutableData.Value as List<object>;

            if (users == null) users = new List<object>();

            // delete user if existed in database
            foreach (var user in users)
            {
                if ((string) ((Dictionary<string, object>) user)["email"] != email) continue;
                users.Remove(user);
                break;
            }

            var newScoreMap = (Dictionary<string, object>) players[email];
            newScoreMap["posX"] = posX;
            newScoreMap["posZ"] = posZ;
            users.Add(newScoreMap);

            mutableData.Value = users;

            return TransactionResult.Success(mutableData);
        }

        // update online status to false (offline)
        private void _UpdateOnlineStatus()
        {
            Debug.Log("Start to Update Online Status");
            var reference = FirebaseDatabase.DefaultInstance.GetReference("Users").Child(username);
            //DebugLog("Running Transaction...");

            reference.RunTransaction(UpdateOnlineStatusTransaction)
                .ContinueWith(task =>
                {
                    if (task.Exception != null)
                        Debug.Log(task.Exception.ToString());
                    else if (task.IsCompleted) Debug.Log("Transaction complete.");
                });
        }

        private TransactionResult UpdateOnlineStatusTransaction(MutableData mutableData)
        {
            var users = mutableData.Value as List<object>;

            if (users == null) users = new List<object>();

            // delete user if existed in database
            var newScoreMap = new Dictionary<string, object>();
            foreach (var user in users)
            {
                if ((string) ((Dictionary<string, object>) user)["email"] != email) continue;
                newScoreMap = (Dictionary<string, object>) user;
                users.Remove(user);
                break;
            }

            newScoreMap["online"] = false;
            users.Add(newScoreMap);
            mutableData.Value = users;

            return TransactionResult.Success(mutableData);
        }

        // Debug
        private void DebugLog(string s)
        {
            Debug.LogError(s);
        }

        // Sign out and reset player online status
        private void OnDestroy()
        {
            // log off the firebase login server
            FirebaseAuth.DefaultInstance.SignOut();
            _UpdateOnlineStatus();
        }
    }
}