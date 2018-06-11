using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLook : MonoBehaviour
{

    //	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    //	public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    public float minimumX = -360F;
    public float maximumX = 360F;

    public float minimumY = -60F;
    public float maximumY = 60F;


    public Vector2 mOld;
    public Vector2 mNew;
    public Vector2 mDv;

    public bool onMouse;

    float sizeW;
    float sizeH;

    public float oldXr;
    public float oldYr;

    public Touch tch;

    public int fingerID;

    public bool isOn;

    public Text tt;

    public void SetOn(bool on)
    {
        isOn = on;
        //		onMouse = on;
        //		mDv = Vector2.zero;

        //		if (on) {
        //			mDv = Vector2.zero;
        onMouse = false;
        //		} else {
        //			mDv = Vector2.zero;
        //			onMouse = false;
        //		}
    }

    public Transform trX;
    public Transform trY;

    public float oldRy;

    public float rotationX;
    public float rotationY;

    //	void FixedUpdate ()
    void Update()
    {
        if (isOn)
        {
            if (!onMouse)
            {
                //Touch myTouch = Input.GetTouch(0);
#if UNITY_EDITOR
                //				if ( Input.GetMouseButton(0) )
                //				{
                mNew = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                onMouse = true;
                oldXr = trX.eulerAngles.y;
                oldYr = trY.localEulerAngles.x;
                if (oldYr > 180)
                    oldYr = oldYr - 360;
                //				}
#else
				Touch[] myTouches = Input.touches;
				for (int i = 0; i < Input.touchCount; i++) {
					if (myTouches [i].phase == TouchPhase.Began) {
						mNew = new Vector2 (myTouches [i].position.x, myTouches [i].position.y);
						onMouse = true;
						oldXr = trX.eulerAngles.y;
						oldYr = trY.localEulerAngles.x;
						if (oldYr > 180)
							oldYr = oldYr - 360;

						fingerID = myTouches [i].fingerId;
					}
				}
#endif
            }
            else
            {
#if UNITY_EDITOR
                //				if ( Input.GetMouseButton(0) )
                //				{

                mOld = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                mDv = new Vector2(-(mOld.x - mNew.x) / sizeW, (mOld.y - mNew.y) / sizeH);

                rotationX = oldXr - mDv.x * sensitivityX;
                rotationY = oldYr - mDv.y * sensitivityY;

                rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
                //					transform.localEulerAngles = new Vector3 ( rotationY, rotationX, 0);

                trX.eulerAngles = new Vector3(0, rotationX, 0);
                trY.localEulerAngles = new Vector3(rotationY, 0, 0);

                //				}

                //				trX.rotation = Quaternion.Euler( new Vector3(0,rotationX,0) );
                //				trY.localEulerAngles = new Vector3 (rotationY, 0, 0);

#else
				Touch[] myTouches = Input.touches;
				bool hasTouch = false;
				for (int i = 0; i < Input.touchCount; i++) {
					if (myTouches [i].fingerId == fingerID) {
						hasTouch = true;

						mOld = new Vector2 (myTouches [i].position.x, myTouches [i].position.y);
						mDv = new Vector2 (-(mOld.x - mNew.x) / sizeW, (mOld.y - mNew.y) / sizeH);

						float rotationX = oldXr - mDv.x * sensitivityX;
						float rotationY = oldYr - mDv.y * sensitivityY;

//						bool s = true;


					rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
//					transform.localEulerAngles = new Vector3 (rotationY, rotationX, 0);
						//trX.localEulerAngles = new Vector3 (0, rotationX, 0);

						trX.eulerAngles = new Vector3(0, rotationX, 0);
						trY.localEulerAngles = new Vector3 (rotationY, 0, 0);

						//tt.text = 
					}
				}
				if (!hasTouch) {
					mDv = Vector2.zero;
					onMouse = false;
				}
#endif
            }

            /*

		if (axes == RotationAxes.MouseXAndY)
		{
			float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
			
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
			
			transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
		}
		else if (axes == RotationAxes.MouseX)
		{
			transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
		}
		else
		{
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
			
			transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
		}
		*/
        }
    }

    void Start()
    {
        sizeW = Screen.width;
        sizeH = Screen.height;
        // Make the rigid body not change rotation
        //if (GetComponent<Rigidbody>())
        //	GetComponent<Rigidbody>().freezeRotation = true;
    }
}

