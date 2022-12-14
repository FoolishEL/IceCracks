using Lean;
using UnityEngine;

namespace IceCracks.Interactions
{
    public class LeanTouchBlocker : MonoBehaviour
    {
        private void OnMouseEnter() => LeanTouch.IsBlocked = true;

        private void OnMouseExit() => LeanTouch.IsBlocked = false;
    }
}