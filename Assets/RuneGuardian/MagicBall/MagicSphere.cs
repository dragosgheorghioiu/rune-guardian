using UnityEngine;

namespace RuneGuardian
{
    public class MagicSphere : MonoBehaviour
    {
        private MagicSphereSystem magicSystem;
        private int sphereIndex;
        [SerializeField] public GameObject green;
        [SerializeField] public GameObject yellow;
        [SerializeField] public GameObject grey;
        public void Initialize(MagicSphereSystem system, int index)
        {
            magicSystem = system;
            sphereIndex = index;
        }

        public void Green()
        {
            green.SetActive(true);
            yellow.SetActive(false);
            grey.SetActive(false);
        }

        public void Yellow()
        {
            green.SetActive(false);
            yellow.SetActive(true);
            grey.SetActive(false);

        }

        public void Grey() {
            green.SetActive(false);
            yellow.SetActive(false);
            grey.SetActive(true);
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log(other.tag);
               magicSystem.UpdatePatternColors(sphereIndex);

        }

        private void OnMouseDown()
        {
            Debug.Log("SPHERE CLICK");
            magicSystem.UpdatePatternColors(sphereIndex);
        }


    }

}