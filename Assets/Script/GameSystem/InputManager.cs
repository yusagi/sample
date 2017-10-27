using System.Collections;
using UnityEngine;

public class InputManager : SingletonBefaviour<InputManager>
{
    public enum TouchType
    {
        TouchDown,
        TouchUp,
        Touch,
        Frick,
        Drag,
        None,
    }

    public Vector3 touchStartPos { get; set; }
    public Vector3 touchEndPos { get; set; }
    public Vector3 currentDragPos { get; set; }
    public Vector3 prevDragPos { get; set; }

    private bool isTouchUp;
    private bool isDrag;
    private bool isPlanetTouch;

    private float touchTimer;
    private float FLICK_SUCCESS_TIME = 10.0f;

    protected virtual void OnCreateSingleTon()
    {
        isTouchUp = false;
        isDrag = false;
        touchTimer = 0.0f;
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(touchStartPos);
        RaycastHit hit;
        isPlanetTouch = Physics.Raycast(ray, out hit, Mathf.Infinity, (int)LayerMask.PLANET);

        if (isTouchUp)
        {
            isTouchUp = false;
        }

        DragUpdate();
        TouchBegin();
        TouchEnd();
        
    }

    void TouchBegin()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            touchStartPos = currentDragPos = new Vector3(   Input.mousePosition.x,
                                                            Input.mousePosition.y,
                                                            0.0f);
            isDrag = true;
        }
#else
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                touchStartPos = currentDragPos = new Vector3(t.position.x,
                                                              t.position.y,
                                                              0.0f);
                isDrag = true;
            }
        }
#endif
    }

    void TouchEnd()
    {
#if UNITY_EDITOR
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            touchEndPos = new Vector3(Input.mousePosition.x,
                                        Input.mousePosition.y,
                                        0.0f);

            TouchReset();

            isTouchUp = true;
        }
#else
		if (Input.touchCount > 0){
			Touch t = Input.GetTouch(0);
			if (t.phase == TouchPhase.Ended){
				touchEndPos = new Vector3(t.position.x,
											t.position.y,
											0.0f);
				TouchReset();

				isTouchUp = true;
			}
		}
#endif
    }

    void DragUpdate()
    {
#if UNITY_EDITOR
        if (isDrag)
        {
            touchTimer += Time.deltaTime;
            if (Input.GetKey(KeyCode.Mouse0))
            {
                prevDragPos = currentDragPos;
                currentDragPos = new Vector3(Input.mousePosition.x,
                                                Input.mousePosition.y,
                                                0.0f);
            }
        }
#else
		if (isDrag){
			touchTimer += Time.deltaTime;
			if (Input.touchCount > 0){
				Touch t = Input.GetTouch(0);
				if (t.phase == TouchPhase.Moved)
				{
					prevDragPos = currentDragPos;
					currentDragPos = new Vector3(t.position.x,
											t.position.y,
											0.0f);
				}
			}
		}
#endif
    }

    void TouchReset()
    {
        isDrag = false;
        touchTimer = 0.0f;
    }



    public static Vector3 GetFlickVelocity()
    {
        if (instance.isTouchUp && IsFlickSuccess())
        {
            return new Vector3(instance.touchEndPos.x - instance.touchStartPos.x,
                                0.0f,
                                instance.touchEndPos.y - instance.touchStartPos.y);
        }

        return Vector3.zero;
    }

    public static Vector3 GetDragVelocity()
    {
        if (instance.isDrag && instance.touchTimer > 0)
        {
            return new Vector3(instance.currentDragPos.x - instance.prevDragPos.x,
                               0.0f,
                               instance.currentDragPos.y - instance.prevDragPos.y);
        }

        return Vector3.zero;
    }

    public static bool IsFlickSuccess()
    {
        return (instance.touchTimer < instance.FLICK_SUCCESS_TIME);
    }

    public static bool IsPlanetTouch()
    {
        return instance.isPlanetTouch;
    }

    public static float GetTouchTimer()
    {
        return instance.touchTimer;
    }

    public static TouchType GetTouchType()
    {
        // タッチ開始
        if (!instance.isTouchUp && instance.isDrag && instance.touchTimer == 0)
        {
            return TouchType.TouchDown;
        }

        // タッチ終了
        if (instance.isTouchUp)
        {
            if (IsFlickSuccess())
            {
                return TouchType.Frick;
            }
            else
            {
                return TouchType.TouchUp;
            }
        }

        if (instance.isDrag){
            if (GetDragVelocity().magnitude > UtilityMath.epsilon)
            {
                return TouchType.Drag;
            }
            else{
                return TouchType.Touch;
            }
        }

        return TouchType.None;
    }
}