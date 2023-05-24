using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Terresquall.Zantetsuken {
    public class Fruit : MonoBehaviour {
        
        public float speed = 5f;
        public float defaultSpawnFacing;
        [Range(0,180)] public float spawnRange = 45f;
        Vector2 sliceStartPosition;

        Rigidbody2D rb;

        void Start() {
            transform.rotation = Quaternion.Euler(0,0,Random.Range(0,360));
            rb = GetComponent<Rigidbody2D>();
            rb.velocity = Quaternion.Euler(0,0,GetSpawnFacing()) * transform.up * speed;
            rb.angularVelocity = Random.Range(0,180);
        }

        float GetSpawnFacing() {
            return defaultSpawnFacing + Random.Range(-spawnRange, spawnRange);
        }

        void OnSwipeEnter2D(Touch t) {
            if(sliceStartPosition.sqrMagnitude > 0) {
                sliceStartPosition = Camera.main.ScreenToWorldPoint(t.position);
            }
        }

        void OnSwipeExit2D(Touch t) {
            Vector2 sliceEnd = Camera.main.ScreenToWorldPoint(t.position);
            Destroy(gameObject);
        }

        void OnDrawGizmosSelected() {
            Gizmos.color = Color.green;
            Vector2 pos = transform.position,
                    dir = Quaternion.Euler(0,0,defaultSpawnFacing) * transform.up,
                    end = pos + dir * 2,
                    lDir = Quaternion.Euler(0,0,135) * dir,
                    rDir = Quaternion.Euler(0,0,-135) * dir;
            
            Gizmos.DrawLine(pos, end);
            Gizmos.DrawLine(end, end + lDir);
            Gizmos.DrawLine(end, end + rDir);

            // Draw the direction range.
            Vector2 dirLeft = Quaternion.Euler(0,0,spawnRange) * dir,
                    dirRight = Quaternion.Euler(0,0,-spawnRange) * dir;

            Gizmos.color = Color.white;
            Gizmos.DrawLine(pos, pos + dirLeft * 3);
            Gizmos.DrawLine(pos, pos + dirRight * 3);
        }
    }
}