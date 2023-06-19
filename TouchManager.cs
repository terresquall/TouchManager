using System.Collections.Generic;
using UnityEngine;

namespace Terresquall {
    [RequireComponent(typeof(Camera))]
    public class TouchManager:MonoBehaviour {

        const string VERSION = "0.2.0";
        const string DATE = "18 June 2023";

        List<int> touchIds = new List<int>();
        public new Camera camera;
        public LayerMask affectedLayers = ~0;

        public enum DetectionMode {
            both, physics2D, physics
        }
        public DetectionMode detectionMode;

        public float precision = 0.2f; // The lower this is, the more precise swipe detection will be.

        [SerializeField] GameObject trailPrefab; // Attach a trail prefab here and you will get trails when you swipe.
        GameObject trail; // The trail we are using currently.

        List<GameObject> swiped = new List<GameObject>(),
                         swiped2D = new List<GameObject>();

        // Records the position of the last input, regardless of touch or mouse.
        // For simulating the TouchPhase.Stationary state.
        Vector2 lastMousePosition;

        static List<TouchManager> instances = new List<TouchManager>();

        void Awake() {
            // Track all instances of Touch Manager.
            instances.Add(this);
        }

        // Start is called before the first frame update
        void Start() {
            // Check the assigned trail prefab, and instantiate it if it passes the checks.
            if(trailPrefab) SetTrail(trailPrefab);
        }

        // Delete the current trail and replace it with a new one.
        // This is static because you should only have one TouchManager.
        public static void SetTrail(GameObject prefab,int index = 0) {
            // If trying to retrieve an invalid index, print an error.
            if(index >= instances.Count) {
                Debug.LogError(string.Format("Touch Manager of index {0} does not exist.",index));
                return;
            }

            instances[index].SetTrail(prefab);
        }

        // Set trail destroys the existing trail and adds a new trail.
        public void SetTrail(GameObject prefab) {
            if(trail) Destroy(trail);
            trailPrefab = prefab;
            trail = Instantiate(prefab,camera.transform);
            trail.SetActive(false);
        }

        void Reset() {
            camera = GetComponentInChildren<Camera>();
        }

        // Determines whether to process touch or mouse input for this frame.
        // If there is a touch on screen, any mouse input will be ignored.
        void Update() {
            if(Input.touchCount > 0)
                ReceiveTouchInput();
            else
                ReceiveMouseInput();
        }

        // Loops through all touches and processes each of them.
        void ReceiveTouchInput() {
            for(int i = 0;i < Input.touchCount;i++) {
                ProcessInput(Input.GetTouch(i));
            }
        }

        // Makes the mouse input simulate a touch.
        // Simulates all 4 touch states.
        void ReceiveMouseInput() {
            // Terminate if there is no mouse.
            if(!Input.mousePresent) return;

            Touch t = new Touch();

            // Only process if a mouse button is being used, or just being released.
            if(Input.GetMouseButton(0)) {
                t.fingerId = -1;
           
                // Determines the phase of the touch.
                if(Input.GetMouseButtonDown(0)) {
                    t.phase = TouchPhase.Began;
                } else {
                    t.deltaPosition = (Vector2)Input.mousePosition - lastMousePosition;
                    if(t.deltaPosition.sqrMagnitude < 0.1f)
                        t.phase = TouchPhase.Stationary;
                    else
                        t.phase = TouchPhase.Moved;
                }

                // Record the touch position.
                t.position = lastMousePosition = Input.mousePosition;
                ProcessInput(t);
            } else if(Input.GetMouseButtonUp(0)) {
                t.phase = TouchPhase.Ended;

                // Record the touch position.
                t.position = lastMousePosition = Input.mousePosition;
                ProcessInput(t);
            }
        }

        // Process a single touch input.
        void ProcessInput(Touch t) {

            // Get the ray to track the swipes.
            Ray r = camera.ScreenPointToRay(t.position);

            switch(t.phase) {
                case TouchPhase.Began:

                    // Call OnTouchTap2D on 2D objects.
                    if(detectionMode != DetectionMode.physics) {
                        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(r,Mathf.Infinity,affectedLayers);
                        foreach(RaycastHit2D h in hits) {
                            h.collider.gameObject.SendMessage("OnTouchTap2D",t,SendMessageOptions.DontRequireReceiver);
                        }
                    }

                    // Call OnTouchTap on 3D objects.
                    if(detectionMode != DetectionMode.physics) {
                        RaycastHit[] hits = Physics.RaycastAll(r,Mathf.Infinity,affectedLayers);
                        foreach(RaycastHit h in hits) {
                            h.collider.gameObject.SendMessage("OnTouchTap",t,SendMessageOptions.DontRequireReceiver);
                        }
                    }

                    touchIds.Add(t.fingerId);
                    break;

                case TouchPhase.Stationary:

                    // Call OnTouchHold2D on 2D objects.
                    if(detectionMode != DetectionMode.physics) {
                        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(r,Mathf.Infinity,affectedLayers);
                        foreach(RaycastHit2D h in hits) {
                            h.collider.gameObject.SendMessage("OnTouchHold2D",t,SendMessageOptions.DontRequireReceiver);
                        }
                    }

                    // Call OnTouchHold on 3D objects.
                    if(detectionMode != DetectionMode.physics) {
                        RaycastHit[] hits = Physics.RaycastAll(r,Mathf.Infinity,affectedLayers);
                        foreach(RaycastHit h in hits) {
                            h.collider.gameObject.SendMessage("OnTouchHold",t,SendMessageOptions.DontRequireReceiver);
                        }
                    }
                    trail.SetActive(true);

                    break;

                case TouchPhase.Moved:

                    // Detect swipes with 2D colliders.
                    if(detectionMode != DetectionMode.physics) {

                        // Track which swiped objects haven't been swiped in this cycle.
                        List<GameObject> unswiped = new List<GameObject>(swiped2D);

                        // Do a raycast and check the swipe status of all objects.
                        RaycastHit2D[] hits = Physics2D.LinecastAll(lastMousePosition,t.position,affectedLayers);
                        foreach(RaycastHit2D h in hits) {
                            if(swiped2D.Contains(h.collider.gameObject)) {
                                h.collider.gameObject.SendMessage("OnSwipeStay2D",t,SendMessageOptions.DontRequireReceiver);
                            } else {
                                swiped2D.Add(h.collider.gameObject);
                                h.collider.gameObject.SendMessage("OnSwipeEnter2D",t,SendMessageOptions.DontRequireReceiver);
                            }

                            // Remove the object if it has been swiped already.
                            unswiped.Remove(h.collider.gameObject);
                        }

                        // If an object has not been swiped despite being swiped in the last frame,
                        // that means it is no longer being swiped.
                        foreach(GameObject go in unswiped) {
                            go.SendMessage("OnSwipeExit2D",t,SendMessageOptions.DontRequireReceiver);
                            swiped2D.Remove(go);
                        }
                    }

                    // Detect swipes with 3D colliders.
                    if(detectionMode != DetectionMode.physics2D) {

                        // Track which swiped objects haven't been swiped in this cycle.
                        List<GameObject> unswiped = new List<GameObject>(swiped);

                        // Grab all objects that are at the finger's position.
                        List<RaycastHit> hits = new List<RaycastHit>(
                            Physics.RaycastAll(r,Mathf.Infinity,affectedLayers)
                        );

                        // If the touch displacement from the last frame to current is large,
                        // we check for some points along the way to our new position too.
                        float displacement = t.deltaPosition.magnitude;
                        if(displacement > precision) {
                            Vector2 interval = t.deltaPosition * 0.2f / displacement;
                            float total = 0;

                            while(total < displacement) {
                                hits.AddRange(
                                    Physics.RaycastAll(
                                        camera.ScreenPointToRay(t.position - interval),
                                        Mathf.Infinity,affectedLayers
                                    )
                                );
                                total += precision;
                            }
                        }

                        foreach(RaycastHit h in hits) {
                            if(swiped.Contains(h.collider.gameObject)) {
                                h.collider.gameObject.SendMessage("OnSwipeStay",t,SendMessageOptions.DontRequireReceiver);
                            } else {
                                swiped.Add(h.collider.gameObject);
                                h.collider.gameObject.SendMessage("OnSwipeEnter",t,SendMessageOptions.DontRequireReceiver);
                            }

                            // Remove the object if it has been swiped already.
                            unswiped.Remove(h.collider.gameObject);
                        }

                        // If an object has not been swiped despite being swiped in the last frame,
                        // that means it is no longer being swiped.
                        foreach(GameObject go in unswiped) {
                            go.SendMessage("OnSwipeExit",t,SendMessageOptions.DontRequireReceiver);
                            swiped.Remove(go);
                        }
                    }

                    trail.SetActive(true);

                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:

                    // Call OnSwipeExit2D / OnTouchUntap2D on 2D objects.
                    if(detectionMode != DetectionMode.physics) {
                        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(r,Mathf.Infinity,affectedLayers);
                        foreach(RaycastHit2D h in hits) {
                            if(swiped2D.Contains(h.collider.gameObject)) {
                                swiped2D.Remove(h.collider.gameObject);
                                h.collider.gameObject.SendMessage("OnSwipeExit2D",t,SendMessageOptions.DontRequireReceiver);
                            } else {
                                h.collider.gameObject.SendMessage("OnTouchUntap2D",t,SendMessageOptions.DontRequireReceiver);
                            }
                        }
                    }

                    // Call OnSwipeExit / OnTouchUntap on 3D objects.
                    if(detectionMode != DetectionMode.physics) {
                        RaycastHit[] hits = Physics.RaycastAll(r,Mathf.Infinity,affectedLayers);
                        foreach(RaycastHit h in hits) {
                            if(swiped.Contains(h.collider.gameObject)) {
                                swiped.Remove(h.collider.gameObject);
                                h.collider.gameObject.SendMessage("OnSwipeExit",t,SendMessageOptions.DontRequireReceiver);
                            } else {
                                h.collider.gameObject.SendMessage("OnTouchUntap",t,SendMessageOptions.DontRequireReceiver);
                            }
                        }
                    }
                    touchIds.Remove(t.fingerId);

                    trail.SetActive(false);
	
                    break;
            }
            
            // Always move the trail to the latest position.
            trail.transform.position = camera.ScreenToWorldPoint(t.position) - new Vector3(0,0,transform.position.z);
        }
    }
}