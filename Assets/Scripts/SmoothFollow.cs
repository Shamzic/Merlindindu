using UnityEngine;

namespace UnityStandardAssets.Utility
{
    public class SmoothFollow : MonoBehaviour
    {

        // The target we are following
        [SerializeField]
        private Transform target;
        // the height we want the camera to be above the target
        [SerializeField]
        private float height = 30f;
        public float maxHeight = 55f;
        public float minHeight = 25f;
        [SerializeField]
        private float xpos = 0;
        [SerializeField]
        private float zpos = 0;
        // Use this for initialization
        void Start() { }

        void LateUpdate()
        {

            Vector3 newPosition = target.position;
            newPosition.y = height;
            newPosition.x += xpos;
            newPosition.z += zpos;
            transform.position = newPosition;
        
            if (height <= maxHeight && height >= minHeight) height -= 5*Input.GetAxis("Mouse ScrollWheel");
            if (height <= minHeight) height = minHeight + (float)0.2;
            if (height >= maxHeight) height = maxHeight - (float)0.2;
        }


    }
}