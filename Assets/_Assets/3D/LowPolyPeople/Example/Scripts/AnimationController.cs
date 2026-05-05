using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DavidJalbert.LowPolyPeople
{
    public class AnimationController : MonoBehaviour
    {
        public Animator[] characters;

        void Start()
        {
            setAnimation("idle");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) setAnimation("idle");
            if (Input.GetKeyDown(KeyCode.Alpha2)) setAnimation("walk");
            if (Input.GetKeyDown(KeyCode.Alpha3)) setAnimation("run");
            if (Input.GetKeyDown(KeyCode.Alpha4)) setAnimation("wave");
        }

        public void setAnimation(string tag)
        {
            Debug.Log("Current animation: " + tag);
            foreach (Animator animator in characters) animator.SetTrigger(tag);
        }
    }
}