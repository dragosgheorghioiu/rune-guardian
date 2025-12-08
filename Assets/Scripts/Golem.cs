using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuneGuardian
{
    public class Golem : MonoBehaviour, IEnemy
    {
        [Header("Movement")]
        public Transform targetPoint;
        public float baseSpeed = 2f;
        private float speed;

        [Header("Health")]
        public int baseMaxHealth = 2;
        private int maxHealth;
        private int currentHealth;

        private Animator anim;
        private bool isDead = false;
        private bool isMoving = false;
        private bool isHitAnimationPlaying = false;
        private RuneDifficulty difficulty;

        void Start()
        {
            anim = GetComponent<Animator>();
            
            // Use base values if not set by WaveController
            if (maxHealth == 0)
                maxHealth = baseMaxHealth;
            if (speed == 0)
                speed = baseSpeed;
                
            currentHealth = maxHealth;

            Invoke(nameof(StartWalking), 1f);
        }

        // IEnemy interface implementation
        public void SetHealth(int health)
        {
            maxHealth = health;
            currentHealth = health;
        }

        public void SetSpeed(int speedValue)
        {
            speed = speedValue;
        }

        public void SetDifficulty(RuneDifficulty difficulty)
        {
            this.difficulty = difficulty;
        }

        void StartWalking()
        {
            if (isDead) return;
            isMoving = true;
            anim.SetBool("IsWalking", true);
        }

        void Update()
        {
            if (isDead || !isMoving || isHitAnimationPlaying) return;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                TakeDamage(1);
            }

            if (targetPoint != null)
            {
                Vector3 dir = (targetPoint.position - transform.position).normalized;

                transform.position += dir * speed * Time.deltaTime;
                transform.forward = Vector3.Lerp(transform.forward, dir, 10f * Time.deltaTime);
            }
        }

        public void TakeDamage(int damage)
        {
            if (isDead) return;

            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                PlayGetHit();
            }
        }

        void PlayGetHit()
        {
            isHitAnimationPlaying = true;
            anim.SetTrigger("GetHit");
            anim.SetBool("IsWalking", false);
            Invoke(nameof(EndGetHit), 2f);
        }

        void EndGetHit()
        {
            isHitAnimationPlaying = false;
            anim.SetBool("IsWalking", true);
        }

        void Die()
        {
            isDead = true;
            isMoving = false;

            anim.SetTrigger("Die");
            anim.SetBool("IsWalking", false);

            Destroy(gameObject, 2f);
        }
    }
}