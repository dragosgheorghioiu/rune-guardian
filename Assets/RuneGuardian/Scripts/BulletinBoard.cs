using UnityEngine;

namespace RuneGuardian
{
    public class BulletinBoard : MonoBehaviour
    {
        public static Vector3 DefaultPositionOffset = new Vector3(0f, 2.3f, 0.127f);

        private GameObject SpawnChild(GameObject prefab, Vector3 positionOffset)
        {
            GameObject instance = Instantiate(prefab, transform);
            instance.transform.localPosition = positionOffset;
            instance.transform.localRotation = Quaternion.Euler(90.0f, 0, 0);

            return instance;
        }

        public GameObject[] SpawnShapeWithSpell(GameObject spellTypePrefab, GestureRecognizerExample.ShapeTemplate shapeTemplate, int objectTypeIndex)
        {
            Vector3 positionOffset = DefaultPositionOffset + new Vector3(-0.4f, -objectTypeIndex * 0.5f, 0);
            Vector3 spellOffset = DefaultPositionOffset + new Vector3(0.4f, -objectTypeIndex * 0.5f, 0);

            GameObject spellObject = SpawnChild(spellTypePrefab, positionOffset);
            GameObject shapeObject = SpawnChild(shapeTemplate.BulletinDrawing, spellOffset);
            
            return new GameObject[] { spellObject, shapeObject };
        }
    }
}