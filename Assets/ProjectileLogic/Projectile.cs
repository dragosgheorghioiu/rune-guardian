using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuneGuardian
{
    public class Projectile : MonoBehaviour
    {
        public float speed = 10f;
        public int damage = 1;
        public float lifeTime = 5f;

        private void Start()
        {
            Destroy(gameObject, lifeTime);
        }

        void Update()
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Golem"))
            {
                other.GetComponent<Golem>().TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}