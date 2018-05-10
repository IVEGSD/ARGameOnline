using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{

	public GameObject Menu;
	public GameObject Register;
	public GameObject Login;

	public void Press_Login()
	{
		Menu.SetActive(false);
		Login.SetActive(true);
	}

	public void Press_Cancel()
	{
		Menu.SetActive(true);
		Register.SetActive(false);
		Login.SetActive(false);
	}

	public void Press_Register()
	{
		Menu.SetActive(false);
		Register.SetActive(true);
	}
}
