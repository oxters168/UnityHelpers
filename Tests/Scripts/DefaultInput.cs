using UnityEngine;

namespace UnityHelpers
{
    public class DefaultInput : MonoBehaviour
    {
        [RequireInterface(typeof(IValueManager))]
        public MonoBehaviour inputObject;
        private IValueManager _inputObject;

        public OrbitCameraController orbitCamera;

        void Update()
        {
            orbitCamera.target = inputObject.transform;
            _inputObject = inputObject.GetComponent<IValueManager>();
            _inputObject.SetAxis("Horizontal", Input.GetAxis("Horizontal"));
            _inputObject.SetAxis("Vertical", Input.GetAxis("Vertical"));
        }
    }
}