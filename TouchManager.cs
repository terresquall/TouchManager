using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Terresquall {
	[RequireComponent(typeof(Camera))]
	public class TouchManager : MonoBehaviour {

		List<int> touchIds = new List<int>();
		public new Camera camera;
		public LayerMask affectedLayers = ~0;

		public enum DetectionMode { both, physics2D, physics }
		public DetectionMode detectionMode;

		List<GameObject> swiped = new List<GameObject>(),
						 swiped2D = new List<GameObject>();

		const string VERSION = "0.1.0";
		const string DATE = "1 May 2023";

		// Start is called before the first frame update
		void Start() 
		{

		}

		void Reset() 
		{
			camera = GetComponentInChildren<Camera>();
		}

		// Update is called once per frame
		void Update() 
		{
			InputHandler();           
        }
			
		void InputHandler() // to handle both the touch and the mouse input
		{
			Touch t;
            //touch input
			if(Input.touchCount > 0)
			{
				for(int i = 0; i < Input.touchCount; i++)
				{
                    t = Input.GetTouch(i); // Current touch data.
                    AfterInput(t);
                }               
            }
            else if (Input.GetMouseButtonDown(0)) //mouse input
            {
                Touch mouseTouch = new Touch();
                mouseTouch.fingerId = -1;
                mouseTouch.position = Input.mousePosition;
                mouseTouch.phase = TouchPhase.Began;

                AfterInput(mouseTouch);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                Touch mouseTouch = new Touch();
                mouseTouch.fingerId = -1;
                mouseTouch.position = Input.mousePosition;
                mouseTouch.phase = TouchPhase.Ended;

                AfterInput(mouseTouch);
            }
            else if (Input.GetMouseButton(0))
            {
                Touch mouseTouch = new Touch();
                mouseTouch.fingerId = -1;
                mouseTouch.position = Input.mousePosition;
                mouseTouch.phase = TouchPhase.Moved;

                AfterInput(mouseTouch);
            }


        }
		void AfterInput(Touch t) // called after the input has been detected
		{
			// Get the ray to track the swipes.
			Ray r = camera.ScreenPointToRay(t.position);

			switch (t.phase)
			{
				case TouchPhase.Began:

					// Call OnTouchTap2D on 2D objects.
					if (detectionMode != DetectionMode.physics)
					{
						RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(r, Mathf.Infinity, affectedLayers);
						foreach (RaycastHit2D h in hits)
						{
							h.collider.gameObject.SendMessage("OnTouchTap2D", t, SendMessageOptions.DontRequireReceiver);
						}
					}

					// Call OnTouchTap on 3D objects.
					if (detectionMode != DetectionMode.physics)
					{
						RaycastHit[] hits = Physics.RaycastAll(r, Mathf.Infinity, affectedLayers);
						foreach (RaycastHit h in hits)
						{
							h.collider.gameObject.SendMessage("OnTouchTap", t, SendMessageOptions.DontRequireReceiver);
						}
					}

					touchIds.Add(t.fingerId);
					break;

				case TouchPhase.Stationary:

					// Call OnTouchHold2D on 2D objects.
					if (detectionMode != DetectionMode.physics)
					{
						RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(r, Mathf.Infinity, affectedLayers);
						foreach (RaycastHit2D h in hits)
						{
							h.collider.gameObject.SendMessage("OnTouchHold2D", t, SendMessageOptions.DontRequireReceiver);
						}
					}

					// Call OnTouchHold on 3D objects.
					if (detectionMode != DetectionMode.physics)
					{
						RaycastHit[] hits = Physics.RaycastAll(r, Mathf.Infinity, affectedLayers);
						foreach (RaycastHit h in hits)
						{
							h.collider.gameObject.SendMessage("OnTouchHold", t, SendMessageOptions.DontRequireReceiver);
						}
					}

					break;

				case TouchPhase.Moved:

					// Detect swipes with 2D colliders.
					if (detectionMode != DetectionMode.physics)
					{

						// Track which swiped objects haven't been swiped in this cycle.
						List<GameObject> unswiped = new List<GameObject>(swiped2D);

						// Do a raycast and check the swipe status of all objects.
						RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(r, Mathf.Infinity, affectedLayers);
						foreach (RaycastHit2D h in hits)
						{
							if (swiped2D.Contains(h.collider.gameObject))
							{
								h.collider.gameObject.SendMessage("OnSwipeStay2D", t, SendMessageOptions.DontRequireReceiver);
							}
							else
							{
								swiped2D.Add(h.collider.gameObject);
								h.collider.gameObject.SendMessage("OnSwipeEnter2D", t, SendMessageOptions.DontRequireReceiver);
							}

							// Remove the object if it has been swiped already.
							unswiped.Remove(h.collider.gameObject);
						}

						// If an object has not been swiped despite being swiped in the last frame,
						// that means it is no longer being swiped.
						foreach (GameObject go in unswiped)
						{
							go.SendMessage("OnSwipeExit2D", t, SendMessageOptions.DontRequireReceiver);
							swiped2D.Remove(go);
						}
					}

					// Detect swipes with 3D colliders.
					if (detectionMode != DetectionMode.physics2D)
					{

						// Track which swiped objects haven't been swiped in this cycle.
						List<GameObject> unswiped = new List<GameObject>(swiped);

						// Do a raycast and check the swipe status of all objects.
						RaycastHit[] hits = Physics.RaycastAll(r, Mathf.Infinity, affectedLayers);
						foreach (RaycastHit h in hits)
						{
							if (swiped.Contains(h.collider.gameObject))
							{
								h.collider.gameObject.SendMessage("OnSwipeStay", t, SendMessageOptions.DontRequireReceiver);
							}
							else
							{
								swiped.Add(h.collider.gameObject);
								h.collider.gameObject.SendMessage("OnSwipeEnter", t, SendMessageOptions.DontRequireReceiver);
							}

							// Remove the object if it has been swiped already.
							unswiped.Remove(h.collider.gameObject);
						}

						// If an object has not been swiped despite being swiped in the last frame,
						// that means it is no longer being swiped.
						foreach (GameObject go in unswiped)
						{
							go.SendMessage("OnSwipeExit", t, SendMessageOptions.DontRequireReceiver);
							swiped.Remove(go);
						}
					}
					break;

				case TouchPhase.Ended:
				case TouchPhase.Canceled:

					// Call OnSwipeExit2D / OnTouchUntap2D on 2D objects.
					if (detectionMode != DetectionMode.physics)
					{
						RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(r, Mathf.Infinity, affectedLayers);
						foreach (RaycastHit2D h in hits)
						{
							if (swiped2D.Contains(h.collider.gameObject))
							{
								swiped2D.Remove(h.collider.gameObject);
								h.collider.gameObject.SendMessage("OnSwipeExit2D", t, SendMessageOptions.DontRequireReceiver);
							}
							else
							{
								h.collider.gameObject.SendMessage("OnTouchUntap2D", t, SendMessageOptions.DontRequireReceiver);
							}
						}
					}

					// Call OnSwipeExit / OnTouchUntap on 3D objects.
					if (detectionMode != DetectionMode.physics)
					{
						RaycastHit[] hits = Physics.RaycastAll(r, Mathf.Infinity, affectedLayers);
						foreach (RaycastHit h in hits)
						{
							if (swiped.Contains(h.collider.gameObject))
							{
								swiped.Remove(h.collider.gameObject);
								h.collider.gameObject.SendMessage("OnSwipeExit", t, SendMessageOptions.DontRequireReceiver);
							}
							else
							{
								h.collider.gameObject.SendMessage("OnTouchUntap", t, SendMessageOptions.DontRequireReceiver);
							}
						}
					}

					touchIds.Remove(t.fingerId);
					break;
			}
		}
	}
}