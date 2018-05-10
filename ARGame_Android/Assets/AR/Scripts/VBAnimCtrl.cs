using UnityEngine;
using Vuforia;

public class VBAnimCtrl : MonoBehaviour, IVirtualButtonEventHandler
{
    public Animator characterAnim;

    public GameObject VBBtn;


    public void OnButtonPressed(VirtualButtonBehaviour vb)
    {
        characterAnim.SetBool("isWalk", true);
        characterAnim.SetBool("isIdle", false);
        Debug.Log("Pressed");
    }

    public void OnButtonReleased(VirtualButtonBehaviour vb)
    {
        characterAnim.SetBool("isIdle", true);
        characterAnim.SetBool("isWalk", false);
        Debug.Log("Released");
    }

    // Use this for initialization
    private void Start()
    {
        VBBtn = GameObject.Find("Btn_Rotation");
        VBBtn.GetComponent<VirtualButtonBehaviour>().RegisterEventHandler(this);
        characterAnim.GetComponent<Animator>();
    }
}