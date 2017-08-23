using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TouchType
{
    TouchDown,
    TouchUp,
    Frick,
    Drag,
    None
}

public class TouchController : MonoBehaviour {

	
	public static Vector3 touchStartPos{get;set;}
	public static Vector3 touchEndPos{get;set;}
	public static Vector3 currentDragPos{get;set;}
	public static Vector3 prevDragPos{get;set;}

	private static bool isFlick;
	private static bool isTouchUp;
	private static bool isDrag;
	private static bool isPlanetTouch;

	private static float touchTimer;
	private static float FLICK_SUCCESS_TIME = 1.0f;//Mathf.Infinity;

	// void OnGUI(){
	// 	Event e = Event.current;
	// 	if (e.button == 0 && e.isMouse){
	// 		if (Input.GetKeyDown(KeyCode.Mouse0)){
	// 			touchStartPos = new Vector3(Input.mousePosition.x,
	// 										Input.mousePosition.y,
	// 										Input.mousePosition.z);

	// 			isFlick = true;
	// 		}
	// 	}
	// }

	void Awake(){
		isFlick = false;
		isTouchUp = false;
		isDrag = false;
		touchTimer = 0.0f;
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {

		Ray ray = Camera.main.ScreenPointToRay(touchStartPos);
		RaycastHit hit;
		isPlanetTouch = Physics.Raycast(ray, out hit, Mathf.Infinity, (int)Layer.PLANET);

		if (isFlick)
			isFlick = false;
		if (isTouchUp)
			isTouchUp = false;
		
		DragUpdate();

		TouchBegin();
		TouchEnd();
	}

	public void TouchBegin()
	{
#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.Mouse0)){
				touchStartPos = new Vector3(Input.mousePosition.x,
											Input.mousePosition.y,
											0.0f);
				currentDragPos = touchStartPos;
											
				isDrag = true;
		}
#else
		if (Input.touchCount > 0){
			Touch t = Input.GetTouch(0);
			if (t.phase == TouchPhase.Began){
				touchStartPos = new Vector3(t.position.x,
											t.position.y,
											0.0f);
				currentDragPos = touchStartPos;
				
				isDrag = true;
			}
		}
#endif
	}

	public void TouchEnd(){
#if UNITY_EDITOR
		if (Input.GetKeyUp(KeyCode.Mouse0)){
				touchEndPos = new Vector3(Input.mousePosition.x,
											Input.mousePosition.y,
											0.0f);

				if (IsFlickSuccess())
					isFlick = true;
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
				if (IsFlickSuccess())
					isFlick = true;

				TouchReset();

				isTouchUp = true;
			}
		}
#endif
	}

	void DragUpdate(){
#if UNITY_EDITOR
		if (isDrag){
			if (Input.GetKey(KeyCode.Mouse0))
			{
                touchTimer += Time.deltaTime;
                prevDragPos = currentDragPos;
				currentDragPos = new Vector3(Input.mousePosition.x,
										Input.mousePosition.y,
										0.0f);
			}
		}
#else
		if (isDrag){
			if (Input.touchCount > 0){
			    touchTimer += Time.deltaTime;
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

    public static Vector3 GetFlickVelocity(){

		if (isFlick){
			return new Vector3( touchEndPos.x - touchStartPos.x,
								0,
								touchEndPos.y - touchStartPos.y);
		}
		 

		return Vector3.zero;
	}
	
	public static Vector3 GetDragVelocity(){

		if (isDrag && touchTimer > 0 && !IsFlickSuccess()){
			return new Vector3 (currentDragPos.x - prevDragPos.x,
								0.0f,
								currentDragPos.y - prevDragPos.y);
		}

		return Vector3.zero;
	}

    public static Vector3 GetVelocity(TouchType type)
    {
        switch (type)
        {
            case TouchType.Drag: return GetDragVelocity();
            case TouchType.Frick: return GetFlickVelocity();
        }

        return Vector3.zero;
    }

	public static bool IsFlickSuccess(){
		return (touchTimer < FLICK_SUCCESS_TIME);
	}

	static void TouchReset(){
		isDrag = false;
		touchTimer = 0.0f;
	}

	public static bool IsFirstTouch(){
		return (isDrag && touchTimer == 0);
	}

	public static float GetTouchTimer(){
		return touchTimer;
	}

	public static TouchType GetTouchType(){
		// タッチ開始
		if (!isTouchUp && isDrag && touchTimer == 0)
			return TouchType.TouchDown;

		// タッチ終了
		if (!isFlick && isTouchUp)
			return TouchType.TouchUp;

		// フリック
		if (GetFlickVelocity().magnitude > UtilityMath.epsilon)
			return TouchType.Frick;

		// ドラッグ
		if (GetDragVelocity().magnitude > UtilityMath.epsilon)
			return TouchType.Drag;

		return TouchType.None;
	}

	public static bool IsPlanetTouch(){
		return isPlanetTouch;
	}
}
