using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
#endif

namespace Terresquall
{
    [RequireComponent(typeof(Camera))]
    public class TouchManager : MonoBehaviour
    {

        const string VERSION = "0.2.0";
        const string DATE = "18 June 2023";

        List<int> touchIds = new List<int>();
        public new Camera camera;

        [Header("Detection")]
        public LayerMask affectedLayers = ~0;

        public enum DetectionMode
        {
            both, physics2D, physics
        }
        public DetectionMode detectionMode;

        public float precision = 0.2f; // Lower value = more precise swipe detection
        public bool supportsMouse = true;

        [Header("UI")]
        [SerializeField] GameObject trailPrefab; // Assign a trail prefab for visual swipe effects
        GameObject trail;

        List<GameObject> swiped = new List<GameObject>(),
                         swiped2D = new List<GameObject>();


        // Track the last known mouse position
        Vector2 lastMousePosition;

        static List<TouchManager> instances = new List<TouchManager>();
        void Awake()
        {
            instances.Add(this);
        }

        void Start()
        {
            if (trailPrefab) SetTrail(trailPrefab);
        }

        public static void SetTrail(GameObject prefab, int index = 0)
        {
            if (index >= instances.Count)
            {
                UnityEngine.Debug.LogError($"Touch Manager of index {index} does not exist.");
                return;
            }
            instances[index].SetTrail(prefab);
        }

        public void SetTrail(GameObject prefab)
        {
            if (trail) Destroy(trail);
            trailPrefab = prefab;
            trail = Instantiate(prefab, camera.transform);
            trail.SetActive(false);
        }

        void Reset()
        {
            camera = GetComponentInChildren<Camera>();
        }

        void Update()
        {
            if (Input.touchCount > 0)
                ReceiveTouchInput();
            else
                ReceiveMouseInput();

        }

        void ReceiveTouchInput()
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                ProcessInput(Input.GetTouch(i));
            }
        }

        void ReceiveMouseInput()
        {
#if ENABLE_INPUT_SYSTEM //if new input system is enabled
            if (Mouse.current == null || !supportsMouse) return;

            UnityEngine.Touch t = new UnityEngine.Touch();

            if (Mouse.current.leftButton.isPressed)
            {
                t.fingerId = -1;

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    t.phase = UnityEngine.TouchPhase.Began;
                }
                else
                {
                    Vector2 currentPosition = Mouse.current.position.ReadValue();
                    t.deltaPosition = currentPosition - lastMousePosition;
                    t.phase = t.deltaPosition.sqrMagnitude < 0.1f ? UnityEngine.TouchPhase.Stationary : UnityEngine.TouchPhase.Moved;
                    lastMousePosition = currentPosition;
                }

                t.position = lastMousePosition;
                ProcessInput(t);
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                t.phase = UnityEngine.TouchPhase.Ended;
                t.position = Mouse.current.position.ReadValue();
                ProcessInput(t);
            }
#else       //else use the old input system

            if (!Input.mousePresent || !supportsMouse) return;

            UnityEngine.Touch t = new UnityEngine.Touch();

            if (Input.GetMouseButton(0))
            {
                t.fingerId = -1;

                if (Input.GetMouseButtonDown(0))
                {
                    t.phase = UnityEngine.TouchPhase.Began;
                }
                else
                {
                    t.deltaPosition = (Vector2)Input.mousePosition - lastMousePosition;
                    if (t.deltaPosition.sqrMagnitude < 0.1f)
                        t.phase = TouchPhase.Stationary;
                    else
                        t.phase = TouchPhase.Moved;
                }

                t.position = lastMousePosition = Input.mousePosition;
                ProcessInput(t);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                t.phase = UnityEngine.TouchPhase.Ended;
                t.position = lastMousePosition = Input.mousePosition;
                ProcessInput(t);
            }
#endif
        }

        void ProcessInput(UnityEngine.Touch t)
        {

            // Get the ray to track the swipes.
            Ray r = camera.ScreenPointToRay(t.position);

            switch (t.phase)
            {
                case UnityEngine.TouchPhase.Began:

                    // Call OnTouchTap2D on 2D objects.
                    if (detectionMode != DetectionMode.physics)
                    {
                        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(r, Mathf.Infinity, affectedLayers);
                        if (hits != null)
                        {
                            foreach (RaycastHit2D h in hits)
                            {
                                h.collider.gameObject.SendMessage("OnTouchTap2D", t, SendMessageOptions.DontRequireReceiver);
                            }
                        }
                    }

                    // Call OnTouchTap on 3D objects.
                    if (detectionMode != DetectionMode.physics)
                    {
                        RaycastHit[] hits = Physics.RaycastAll(r, Mathf.Infinity, affectedLayers);
                        if (hits != null)
                        {
                            foreach (RaycastHit h in hits)
                            {
                                h.collider.gameObject.SendMessage("OnTouchTap", t, SendMessageOptions.DontRequireReceiver);
                            }
                        }
                    }

                    touchIds.Add(t.fingerId);
                    break;

                case UnityEngine.TouchPhase.Stationary:

                    // Call OnTouchHold2D on 2D objects.
                    if (detectionMode != DetectionMode.physics)
                    {
                        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(r, Mathf.Infinity, affectedLayers);
                        if (hits != null)
                        {
                            foreach (RaycastHit2D h in hits)
                            {
                                h.collider.gameObject.SendMessage("OnTouchHold2D", t, SendMessageOptions.DontRequireReceiver);
                            }
                        }
                    }

                    // Call OnTouchHold on 3D objects.
                    if (detectionMode != DetectionMode.physics)
                    {
                        RaycastHit[] hits = Physics.RaycastAll(r, Mathf.Infinity, affectedLayers);
                        if (hits != null)
                        {
                            foreach (RaycastHit h in hits)
                            {
                                h.collider.gameObject.SendMessage("OnTouchHold", t, SendMessageOptions.DontRequireReceiver);
                            }
                        }
                    }
                    trail.SetActive(true);

                    break;

                case UnityEngine.TouchPhase.Moved:

                    // Detect swipes with 2D colliders.
                    if (detectionMode != DetectionMode.physics)
                    {

                        // Track which swiped objects haven't been swiped in this cycle.
                        List<GameObject> unswiped = new List<GameObject>(swiped2D);

                        // Do a raycast and check the swipe status of all objects.
                        RaycastHit2D[] hits = Physics2D.LinecastAll(lastMousePosition, t.position, affectedLayers);
                        if (hits != null)
                        {
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

                    }

                    // Detect swipes with 3D colliders.
                    if (detectionMode != DetectionMode.physics2D)
                    {

                        // Track which swiped objects haven't been swiped in this cycle.
                        List<GameObject> unswiped = new List<GameObject>(swiped);

                        // Grab all objects that are at the finger's position.
                        List<RaycastHit> hits = new List<RaycastHit>(
                            Physics.RaycastAll(r, Mathf.Infinity, affectedLayers)
                        );

                        // If the touch displacement from the last frame to current is large,
                        // we check for some points along the way to our new position too.
                        float displacement = t.deltaPosition.magnitude;
                        if (displacement > precision)
                        {
                            Vector2 interval = t.deltaPosition * 0.2f / displacement;
                            float total = 0;

                            while (total < displacement)
                            {
                                hits.AddRange(
                                    Physics.RaycastAll(
                                        camera.ScreenPointToRay(t.position - interval),
                                        Mathf.Infinity, affectedLayers
                                    )
                                );
                                total += precision;
                            }
                        }

                        if (hits != null)
                        {
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
                    }

                    trail.SetActive(true);

                    break;

                case UnityEngine.TouchPhase.Ended:
                case UnityEngine.TouchPhase.Canceled:

                    // Call OnSwipeExit2D / OnTouchUntap2D on 2D objects.
                    if (detectionMode != DetectionMode.physics)
                    {
                        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(r, Mathf.Infinity, affectedLayers);
                        if (hits != null)
                        {
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
                    }

                    // Call OnSwipeExit / OnTouchUntap on 3D objects.
                    if (detectionMode != DetectionMode.physics)
                    {
                        RaycastHit[] hits = Physics.RaycastAll(r, Mathf.Infinity, affectedLayers);
                        if (hits != null)
                        {
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

                    }
                    touchIds.Remove(t.fingerId);

                    trail.SetActive(false);

                    break;
            }

            // Always move the trail to the latest position.
            trail.transform.position = camera.ScreenToWorldPoint(t.position) - new Vector3(0, 0, transform.position.z);
        }
    }
}
