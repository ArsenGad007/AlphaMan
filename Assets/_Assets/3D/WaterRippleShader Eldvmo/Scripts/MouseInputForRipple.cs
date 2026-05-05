using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Eldvmo.Ripples
{
    public class MouseInputForRipple : MonoBehaviour
    {
        [SerializeField] private MeshRenderer ripplePlane;
        private Vector4[] ripplePoints = new Vector4[100];
        private int rippleIndex = 0;
        private Vector2 _oldInputCentre;
        private int waterLayerMask;

        void Start()
        {
            waterLayerMask = LayerMask.GetMask("Water");
        }

        void FixedUpdate()
        {
            if (Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, waterLayerMask))
                {
                    // Don't add to ripple if mouse position is too closed to the old mouse position
                    if (_oldInputCentre == null || Vector2.Distance(_oldInputCentre, hit.textureCoord) < 0.05f) return;

                    ripplePoints[rippleIndex] = new Vector4(hit.textureCoord.x, hit.textureCoord.y, Time.time, 0);
                    rippleIndex = (rippleIndex + 1) % ripplePoints.Length;
                    _oldInputCentre = hit.textureCoord;
                }
                ripplePlane.material.SetVectorArray("_InputCentre", ripplePoints);
            }
        }
    }
}