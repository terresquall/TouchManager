using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Terresquall.FruitSlicer
{
    public class Bomb : MonoBehaviour
    {
        public float lifespan = 2f;
        public float defaultSpawnFacing;
        [Range(0, 180)] public float spawnRange = 45f;
        Vector2 sliceStartPosition;

        [SerializeField] GameObject explosion;

        Rigidbody rb;
        Camera camera;
        [SerializeField] bool onScreen = false;

        FruitNinjaGameManager fruitNinjaGameManager;
        // Start is called before the first frame update
        void Start()
        {
            fruitNinjaGameManager = FindObjectOfType<FruitNinjaGameManager>();
            float a = GetSpawnFacing();
            rb = GetComponent<Rigidbody>();
            camera = FindObjectOfType<Camera>();
            rb.velocity = Quaternion.Euler(0, 0, a) * transform.up * Random.Range(10, 15);
            rb.angularVelocity = new Vector3(0, 0, Random.Range(0, 10));

            transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        }

        // Update is called once per frame
        void Update()
        {
            CheckIfOffscreen();
        }
        float GetSpawnFacing()
        {
            return defaultSpawnFacing + Random.Range(-spawnRange, spawnRange);
        }

        void OnSwipeEnter(Touch t)
        {
            if (sliceStartPosition.sqrMagnitude > 0)
            {
                sliceStartPosition = Camera.main.ScreenToWorldPoint(t.position);
                Vector3 spawnPoint = transform.position;
                Instantiate(explosion, spawnPoint, Quaternion.identity, null);
                fruitNinjaGameManager.Penalty += 3;
                Destroy(gameObject);
            }
        }

        void OnSwipeExit(Touch t)
        {
            Vector2 sliceEnd = Camera.main.ScreenToWorldPoint(t.position);
            Vector3 spawnPoint = transform.position;
            Instantiate(explosion, spawnPoint, Quaternion.identity, null);
            fruitNinjaGameManager.Penalty += 3;
            Destroy(gameObject);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Vector2 pos = transform.position,
                    dir = Quaternion.Euler(0, 0, defaultSpawnFacing) * transform.up,
                    end = pos + dir * 2,
                    lDir = Quaternion.Euler(0, 0, 135) * dir,
                    rDir = Quaternion.Euler(0, 0, -135) * dir;

            Gizmos.DrawLine(pos, end);
            Gizmos.DrawLine(end, end + lDir);
            Gizmos.DrawLine(end, end + rDir);

            // Draw the direction range.
            Vector2 dirLeft = Quaternion.Euler(0, 0, spawnRange) * dir,
                    dirRight = Quaternion.Euler(0, 0, -spawnRange) * dir;

            Gizmos.color = Color.white;
            Gizmos.DrawLine(pos, pos + dirLeft * 3);
            Gizmos.DrawLine(pos, pos + dirRight * 3);
        }
        void CheckIfOffscreen()
        {
            Vector3 viewportPosition = camera.WorldToViewportPoint(transform.position);
            if (onScreen && (viewportPosition.x < 0 || viewportPosition.x > 1 || viewportPosition.y < 0 || viewportPosition.y > 1))
            {
                Destroy(gameObject, 1f);
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
