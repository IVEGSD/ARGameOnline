using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Database : MonoBehaviour
{
	public Transform CharacterTransform;
	private float posX, posZ;

	// player game data & profile
	private string email;
	
	// for storing all player who is online
	private Dictionary<string, object> players = new Dictionary<string, object>();
	private Dictionary<string, GameObject> playersGameObject = new Dictionary<string, GameObject>(); 

	// for check dependencies is available for database 
	private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

	private void Start ()
	{
		// initialize player's position
		posX = CharacterTransform.position.x;
		posZ = CharacterTransform.position.z;
		
		// get current user
		Debug.Log("Current User: " + FirebaseLoginControl.Useremail);
		// debug email address
		email = FirebaseLoginControl.Useremail ?? "abc@gmail.com";

		// check dependencies is available for database 
		FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
		{
			dependencyStatus = task.Result;
			if (dependencyStatus == DependencyStatus.Available)
			{
				InitializeFirebase();
			}
			else
			{
				Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
			}
		});
	}

	private void Update()
	{
		// update character position
		posX = CharacterTransform.position.x;
		posZ = CharacterTransform.position.z;
		
		// if user is inputting, update position
		if (JoystickController.IsMoving)
		{
			_UpdatePosition();
		}
	}

	// initialize online database
	private void InitializeFirebase()
	{
		// get database instance by firebase link
		var app = FirebaseApp.DefaultInstance;
		app.SetEditorDatabaseUrl("https://argamedemo.firebaseio.com/");
		if (app.Options.DatabaseUrl != null)
		{
			app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);
		}

		// initialize player's record
		_InitializePlayerRecord();
		
		// value change listener
		_PlayerPositionListener();
	}

	// before game start, initialize player record in the database
	public void _InitializePlayerRecord()
	{
		Debug.Log("Initializing Player Record...");
		Debug.Log("Attempting to initialize Player Record...");

		FirebaseDatabase.DefaultInstance.
			GetReference("Users").RunTransaction(InitializePlayerTransaction).ContinueWith(task =>
		{
			if (task.Exception != null)
			{
				Debug.LogError(task.Exception);
			}
			else if (task.IsCompleted)
			{
				Debug.Log("Player Record initialization completed.");
			}
		});
	}
	
	// continue process of InitializePlayerRecord();
	private TransactionResult InitializePlayerTransaction(MutableData mutableData)
	{
		var users = mutableData.Value as List<object>;

		// if no user in the database, create new record
		if (users == null)
		{
			Debug.Log("users table is null, create new list...");
			users = new List<object>();
		}

		long tempScore = 0;
		
		// reset current user record
		foreach (var user in users)
		{
			if ((string) ((Dictionary<string, object>) user)["email"] != email) continue;
			users.Remove(user);
			tempScore = (long) ((Dictionary<string, object>) user)["score"];
			Debug.Log(tempScore);
			Debug.Log("Current user is removed...");
			break;
		}
		
		// create new user record
		Dictionary<string, object> newUserRecord = new Dictionary<string, object>();
		newUserRecord["email"] = email;
		newUserRecord["online"] = true;
		newUserRecord["score"] = tempScore;
		newUserRecord["posX"] = 0;
		newUserRecord["posZ"] = 0;
		newUserRecord["type"] = 1;
		users.Add(newUserRecord);
		
		// add to firebase database
		mutableData.Value = users;
		Debug.Log("New user record is created.");
		return TransactionResult.Success(mutableData);
	}
	
	// get players position on database when position changed in database
	public void _PlayerPositionListener()
	{
		Debug.Log("Attempting to change player status...");
		FirebaseDatabase.DefaultInstance.
			GetReference("Users").ValueChanged += (object sender, ValueChangedEventArgs e1) =>
		{
			if (e1.DatabaseError != null)
			{
				Debug.LogError(e1.DatabaseError.Message);
				return;
			}

			Debug.Log("Received values for players.");
			if (e1.Snapshot != null && e1.Snapshot.ChildrenCount > 0)
			{
				foreach (var child in e1.Snapshot.Children)
				{
					String childEmailKey = child.Child("email").Value.ToString();
					// if player is online, add to list players
					if ((bool) child.Child("online").Value)
					{
						// if contains player, remove it and add updated list:players
						if (players.ContainsKey(childEmailKey))
						{
							players.Remove(childEmailKey);
						}
						players.Add(childEmailKey, child.Value as Dictionary<string, object>);

						// create other player game object and add to list playersGameObject
						if (child.Child("email").Value.ToString().Equals(email)) continue;
						
						// get updated other player position
//						var posX = child.Child("posX").Value.ToString();
//						var posZ = child.Child("posZ").Value.ToString();
//						long x = long.Parse(posX);
//						long z = long.Parse(posZ);
//						
//						
//						Debug.Log( "" + x);
//						Debug.Log( "" + z);
						float x = float.Parse(child.Child("posX").Value.ToString());
						float z = float.Parse(child.Child("posZ").Value.ToString());
						Debug.Log(child.Child("posX").Value.ToString());
						Debug.Log("PosX:" + x + ", PosZ:" + z);
						
						/*if (playersGameObject.ContainsKey(childEmailKey))
						{
							Vector3 newPosition = new Vector3((float)x, 0, (float)z);
							playersGameObject[childEmailKey].transform.position = newPosition;
						}
						else
						{
							// instantiate other player if player is not exist in playersGameObject
							var player = Resources.Load<GameObject>("Sphere");
							Instantiate(player, new Vector3(x, 0, z), Quaternion.identity);
							playersGameObject.Add(childEmailKey, player);
						}*/
					}
					else
					{
						// if players contains player is offline, remove it with game object
						if (players.ContainsKey(childEmailKey))
						{
							players.Remove(childEmailKey);							
						}
						if (playersGameObject.ContainsKey(childEmailKey))
						{
							playersGameObject.Remove(childEmailKey);
							Destroy(playersGameObject[childEmailKey], 0f);
						}
					}
					//if (!(bool) child.Child("online").Value || child.Child("email").Value.Equals(email)) continue;
					/*DebugLog(child.Child("email").Value.ToString() + ", Position: X=" +
					          child.Child("posX").Value.ToString() + ", z=" +
					          child.Child("posZ").Value.ToString());*/
				}
				//Debug.Log("Players.Count: " + players.Count);
//				Dictionary<string, object> myPlayerDictionary = (Dictionary<string, object>) players[email];
				
				/*foreach (var player in players)
				{
					DebugLog("Email: " + ((Dictionary<string, object>) player.Value)["email"] + 
					          ", type: " + ((Dictionary<string, object>) player.Value)["type"] + 
					          ", posX: " + ((Dictionary<string, object>) player.Value)["posX"] + 
					          ", posZ: " + ((Dictionary<string, object>) player.Value)["posZ"] + 
					          ", score: " + ((Dictionary<string, object>) player.Value)["score"]);
				}*/
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

		DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("Users");

		DebugLog("Running Transaction...");
		reference.RunTransaction(AddScoreTransaction)
			.ContinueWith(task =>
			{
				if (task.Exception != null)
				{
					DebugLog(task.Exception.ToString());
				}
				else if (task.IsCompleted)
				{
					DebugLog("Transaction complete.");
				}
			});
	}

	private TransactionResult AddScoreTransaction(MutableData mutableData)
	{
		var users = mutableData.Value as List<object>;

		if (users == null)
		{
			users = new List<object>();
		}

		// delete user if existed in database
		foreach (var user in users)
		{
			if ((string) ((Dictionary<string, object>) user)["email"] != email) continue;
			users.Remove(user);
			break;
		}

		Dictionary<string, object> newScoreMap = (Dictionary<string, object>) players[email];
	
		newScoreMap["score"] = (long)newScoreMap["score"] + 1; 
		users.Add(newScoreMap);
		

		mutableData.Value = users;
		
		return TransactionResult.Success(mutableData);
	}
	
	// update player transform
	public void _UpdatePosition()
	{
		Debug.Log("Start to Update Position");
		DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("Users");
		//DebugLog("Running Transaction...");
		
		reference.RunTransaction(UpdatePositionTransaction)
			.ContinueWith(task =>
			{
				if (task.Exception != null)
				{
					Debug.Log(task.Exception.ToString());
				}
				else if (task.IsCompleted)
				{
					Debug.Log("Transaction complete.");
				}
			});
	}
	
	private TransactionResult UpdatePositionTransaction(MutableData mutableData)
	{
		var users = mutableData.Value as List<object>;

		if (users == null)
		{
			users = new List<object>();
		}

		// delete user if existed in database
		foreach (var user in users)
		{
			if ((string) ((Dictionary<string, object>) user)["email"] != email) continue;
			users.Remove(user);
			break;
		}

		Dictionary<string, object> newScoreMap = (Dictionary<string, object>) players[email];
		newScoreMap["posX"] = posX * 0.2; 
		newScoreMap["posZ"] = posZ * 0.2;
		users.Add(newScoreMap);

		mutableData.Value = users;
		
		return TransactionResult.Success(mutableData);
	}
	
	// update online status to false (offline)
	public void _UpdateOnlineStatus()
	{
		Debug.Log("Start to Update Online Status");
		DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("Users");
		//DebugLog("Running Transaction...");
		
		reference.RunTransaction(UpdateOnlineStatusTransaction)
			.ContinueWith(task =>
			{
				if (task.Exception != null)
				{
					Debug.Log(task.Exception.ToString());
				}
				else if (task.IsCompleted)
				{
					Debug.Log("Transaction complete.");
				}
			});
	}
	
	private TransactionResult UpdateOnlineStatusTransaction(MutableData mutableData)
	{
		var users = mutableData.Value as List<object>;

		if (users == null)
		{
			users = new List<object>();
		}

		// delete user if existed in database
		foreach (var user in users)
		{
			if ((string) ((Dictionary<string, object>) user)["email"] != email) continue;
			users.Remove(user);
			break;
		}

		Dictionary<string, object> newScoreMap = (Dictionary<string, object>) players[email];
		newScoreMap["online"] = false;
		users.Add(newScoreMap);

		mutableData.Value = users;
		
		return TransactionResult.Success(mutableData);
	}

	// Debug
	private void DebugLog(string s)
	{
		Debug.LogError(s);
		//DebugText.text += s + "\n";
	}
	
	// Sign out and reset player online status
	private void OnDestroy()
	{
		// log off the firebase login server
		FirebaseAuth.DefaultInstance.SignOut();
		_UpdateOnlineStatus();

	}
}
