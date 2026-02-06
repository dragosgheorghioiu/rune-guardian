using UnityEngine;

namespace RuneGuardian
{
    public class MagicSphere : MonoBehaviour
    {
        private MagicSphereSystem magicSystem;
        private int sphereIndex;

        public void Initialize(MagicSphereSystem system, int index)
        {
            magicSystem = system;
            sphereIndex = index;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("PlayerHand"))
            {
                magicSystem.UpdatePatternColors(sphereIndex);
            }
        }

        private void OnMouseDown()
        {
            Debug.Log("SPHERE CLICK");
            magicSystem.UpdatePatternColors(sphereIndex);
        }
    }

}