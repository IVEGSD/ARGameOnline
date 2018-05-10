using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinBorner : MonoBehaviour
{
	public float BornSpeed = 1f;
	public float DestorySpeed = 3f;
	public GameObject CoinManager;
	
	private float timer, timer2;
	private GameObject prefab;
	
	// Use this for initialization
	void Start () {
		prefab = Resources.Load<GameObject>("Coin");
		DestorySpeed = BornSpeed * 2;
	}
	
	// Update is called once per frame
	void Update ()
	{
		timer += Time.deltaTime;
		timer2 += Time.deltaTime;

		if (timer > BornSpeed)
		{
			float x = Random.Range(-5f, 5f);
			float z = Random.Range(-5f, 5f);

			var coin = Instantiate(prefab, new Vector3(x, 0, z), Quaternion.identity);
			coin.transform.parent = CoinManager.transform;
			coin.transform.position = new Vector3(x, 5f, z);
			coin.transform.Rotate(90, 0, 0);
			timer = 0;
			BornSpeed = Random.Range(1f, 5f);
		}

		if (timer2 > DestorySpeed)
		{
			Destroy(GameObject.FindWithTag("Coin"));
			timer2 = 0;
			DestorySpeed = Random.Range(5f, 10f);
		}
		
	}
}
