using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class CollisionController : MonoBehaviour
{
	public static int score;
	public GameObject Database;
	
	// Use this for initialization
	private void Start ()
	{
		Database = GameObject.Find("DatabaseManager");
	}

	private void OnCollisionEnter(Collision col)
	{
		if (!col.gameObject.CompareTag("Coin")) return;
		Destroy(col.gameObject);
		Database.GetComponent<Database.Scripts.Database>()._AddScore();
	}
}
