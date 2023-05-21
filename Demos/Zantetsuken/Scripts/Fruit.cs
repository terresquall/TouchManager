using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Terresquall.Zantetsuken {
    public class Fruit : MonoBehaviour {
    
        Vector2 sliceStartPosition;

        void OnSwipeEnter2D(Touch t) {
            if(sliceStartPosition.sqrMagnitude > 0) {
                sliceStartPosition = Camera.main.ScreenToWorldPoint(t.position);
            }
        }

        void OnSwipeExit2D(Touch t) {
            Vector2 sliceEnd = Camera.main.ScreenToWorldPoint(t.position);
            Destroy(gameObject);
        }

    }
}
