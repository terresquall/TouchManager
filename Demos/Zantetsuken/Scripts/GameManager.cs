using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Terresquall.Zantetsuken {
    [RequireComponent(typeof(Camera))]
    public class GameManager : MonoBehaviour {
        
        public Camera camera;
        public Rect spawnArea;

        void OnDrawGizmosSelected() {
            if(spawnArea.size.sqrMagnitude > 0) {
                // Draw the lines of the bounds.
                Gizmos.color = Color.cyan;

                Vector2 pos = (Vector2)transform.position + spawnArea.position - spawnArea.size * 0.5f,
                        size = spawnArea.size;

                // Get the 4 points in the bounds.
                Vector3 a = new Vector3(pos.x, pos.y),
                        b = new Vector3(pos.x, pos.y + size.y),
                        c = new Vector2(pos.x + size.x, pos.y + size.y),
                        d = new Vector3(pos.x + size.x, pos.y);
                
                Gizmos.DrawLine(a, b);
                Gizmos.DrawLine(b, c);
                Gizmos.DrawLine(c, d);
                Gizmos.DrawLine(d, a);
            }
        }

        void Reset() {
            camera = GetComponent<Camera>();
            spawnArea.y = camera.orthographicSize + ;
            spawnArea.size = new Vector2(camera.orthographicSize * 2f * camera.aspect, 1);
        }

    }
}