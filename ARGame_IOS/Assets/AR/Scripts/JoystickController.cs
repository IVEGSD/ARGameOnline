using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoystickController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private Image BGImg;
    private Vector3 inputVector;
    private Image joyImg;
    public static bool IsMoving;

    public void OnDrag(PointerEventData eventData)
    {
        IsMoving = true;
        // calculate the joystick position on the control area
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(BGImg.rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pos))
        {
            pos.x = pos.x / BGImg.rectTransform.sizeDelta.x;
            pos.y = pos.y / BGImg.rectTransform.sizeDelta.y;
            Debug.Log("Position" + pos);
            inputVector = new Vector3(pos.x * 2 - 1, 0, pos.y * 2 - 1);
            Debug.Log("inputVector" + inputVector);
            inputVector = inputVector.magnitude > 1.0f ? inputVector.normalized : inputVector;

            joyImg.rectTransform.anchoredPosition = new Vector3(
                inputVector.x * BGImg.rectTransform.sizeDelta.x / 2.15f,
                inputVector.z * BGImg.rectTransform.sizeDelta.y / 2.15f);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector3.zero;
        joyImg.rectTransform.anchoredPosition = Vector3.zero;
        IsMoving = false;
    }

    private void Start()
    {
        BGImg = GetComponent<Image>();
        joyImg = transform.GetChild(0).GetComponent<Image>();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public float Horizontal()
    {
        if (inputVector.x != 0)
            return inputVector.x;
        return Input.GetAxis("Horizontal");
    }

    public float Vertical()
    {
        if (inputVector.z != 0)
            return inputVector.z;
        return Input.GetAxis("Vertical");
    }
}