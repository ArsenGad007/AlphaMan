using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Eldvmo.Ripples
{
    public class BoatControl : MonoBehaviour
    {
        [SerializeField] private float _speed = 0.05f;
        [SerializeField] private float _rotateSpeed = 0.5f;

        void Update()
        {
            if(Input.GetKey("w"))
            {
                transform.Translate(Vector3.forward * _speed);
            }
            else if(Input.GetKey("s"))
            {
                transform.Translate(Vector3.back * _speed);
            }
            else if(Input.GetKey("a"))
            {
                transform.Translate(Vector3.forward * _speed);
                transform.Rotate(Vector3.up * -_rotateSpeed);
            }
            else if(Input.GetKey("d"))
            {
                transform.Translate(Vector3.forward * _speed);
                transform.Rotate(Vector3.up * _rotateSpeed);
            }
        }
    }
}
