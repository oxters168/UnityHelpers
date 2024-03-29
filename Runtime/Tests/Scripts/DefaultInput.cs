﻿using UnityEngine;
#if UNITY_EDITOR
using UnityHelpers.Editor;
#endif

namespace UnityHelpers.Tests
{
    public class DefaultInput : MonoBehaviour
    {
        #if UNITY_EDITOR
        [RequireInterface(typeof(IValueManager))]
        #endif
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