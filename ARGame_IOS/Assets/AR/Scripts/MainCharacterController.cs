using JetBrains.Annotations;
using UnityEngine;

public class MainCharacterController : MonoBehaviour
{
    private Transform cameraTransform;
    private JoystickController Joystick;
    public float MoveSpeed = 2.5f;
    public Vector3 MoveVector;

    private void Start()
    {
        Joystick = GameObject.FindWithTag("Joystick").GetComponent<JoystickController>();
    }

    // Update is called once per frame
    private void Update()
    {
        // get the original input direction
        MoveVector = Input();

        // convert the input depends on the camera view
        MoveVector = RotateWithView();

        if (MoveVector != Vector3.zero)
        {
            // change character looking direciton with joystick input
            transform.rotation = Quaternion.LookRotation(MoveVector);

            Move();
        }
    }

    private void Move()
    {
        transform.Translate(Vector3.forward * MoveSpeed * Time.deltaTime);
    }

    private Vector3 Input()
    {
        var direction = Vector3.zero;

        direction.x = Joystick.Horizontal();
        direction.z = Joystick.Vertical();

        if (direction.magnitude > 1)
            direction.Normalize();

        return direction;
    }

    private Vector3 RotateWithView()
    {
        if (cameraTransform != null)
        {
            var dir = cameraTransform.TransformDirection(MoveVector);
            dir.Set(dir.x /* * -1*/, 0, dir.z /* * -1*/);
            return dir.normalized * MoveVector.magnitude;
        }

        cameraTransform = Camera.main.transform;
        return MoveVector;
    }
}