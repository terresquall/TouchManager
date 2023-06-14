using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Terresquall.FruitSlicer
{
    public class Balloon : MonoBehaviour
    {
        Vector2 sliceStartPosition;

        [SerializeField] GameObject balloonPopped;

        Rigidbody rb;
        Camera camera;
        [SerializeField] bool onScreen = false;

        FruitNinjaGameManager fruitNinjaGameManager;

        // Start is called before the first frame update
        void Start()
        {
            fruitNinjaGameManager = FindObjectOfType<FruitNinjaGameManager>();
            rb = GetComponent<Rigidbody>();
            camera = FindObjectOfType<Camera>();
            rb.velocity = Quaternion.Euler(0, 0, 0) * transform.up * 2;
        }

        // Update is called once per frame
        void Update()
        {
            CheckIfOffscreen();
            transform.rotation = Quaternion.identity;
        }
        void OnSwipeEnter(Touch t)
        {
            if (sliceStartPosition.sqrMagnitude > 0)
            {
                sliceStartPosition = Camera.main.ScreenToWorldPoint(t.position);

                Vector3 spawnPoint = transform.position;
                Instantiate(balloonPopped, spawnPoint, Quaternion.identity, null);

                fruitNinjaGameManager.Score += 5;
                Destroy(gameObject);
            }
        }

        void OnSwipeExit(Touch t)
        {
            Vector2 sliceEnd = Camera.main.ScreenToWorldPoint(t.position);

            Vector3 spawnPoint = transform.position;
            Instantiate(balloonPopped, spawnPoint, Quaternion.identity, null);

            fruitNinjaGameManager.Score += 5;
            Destroy(gameObject);
        }

        void CheckIfOffscreen()
        {
            Vector3 viewportPosition = camera.WorldToViewportPoint(transform.position);
            if (onScreen && (viewportPosition.x < 0 || viewportPosition.x > 1 || viewportPosition.y < 0 || viewportPosition.y > 1))
            {
                Destroy(gameObject, 1);
            }
            else
            {
                StartCoroutine(DelayOnScreen());
            }
        }
        IEnumerator DelayOnScreen()
        {
            yield return new WaitForSeconds(0.1f);
            onScreen = true;
        }
    }
}
