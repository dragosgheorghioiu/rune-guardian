using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuneGuardian
{
    public class MagicSphereSystem : MonoBehaviour
    {
        [SerializeField] private List<GameObject> spheres;
        [SerializeField] private Color greyColor = Color.grey;
        [SerializeField] private Color greenColor = Color.green;
        [SerializeField] private Color yellowColor = Color.yellow;
        [SerializeField] private Transform shootSource;
        [SerializeField] private ShootSpell shooter;

        private List<int> currentSession;
        private int currentIndex;
        private bool patternComplete;

        private void OnEnable()
        {
            RuneGuardianController.OnRuneGuardianInit += Init;
            MakeAllSpheresGrey();
        }
        private void OnDisable()
        {
            RuneGuardianController.OnRuneGuardianInit -= Init;
        }

        public void Init(InputData inputData)
        {
            if (inputData.gameMode != GameMode.SPHERE) return;
            
            for (int i = 0; i < spheres.Count; i++)
            {
                MagicSphere magicSphere = spheres[i].GetComponent<MagicSphere>();
                if (magicSphere == null)
                {
                    magicSphere = spheres[i].AddComponent<MagicSphere>();
                }
                magicSphere.Initialize(this, i);
            }
        }
        
        public void SetupPattern()
        {
            for (int i = 0; i < spheres.Count; i++)
            {
                MagicSphere magicSphere = spheres[i].GetComponent<MagicSphere>();
                if (magicSphere == null)
                {
                    magicSphere = spheres[i].AddComponent<MagicSphere>();
                }
                magicSphere.Initialize(this, i);
            }

            currentSession = new List<int>();
            currentIndex = 0;
            patternComplete = false;

            int randomCount = Random.Range(1, spheres.Count + 1);

            List<int> availableIndices = new List<int>();
            for (int i = 0; i < spheres.Count; i++)
            {
                availableIndices.Add(i);
            }

            for (int i = 0; i < randomCount; i++)
            {
                int randomIndex = Random.Range(0, availableIndices.Count);
                currentSession.Add(availableIndices[randomIndex]);
                availableIndices.RemoveAt(randomIndex);
            }

            for (int i = currentSession.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                int temp = currentSession[i];
                currentSession[i] = currentSession[randomIndex];
                currentSession[randomIndex] = temp;
            }

            UpdateAllSphereColors();
        }

        public void UpdatePatternColors(int index)
        {
            if (patternComplete) return;
            
            if (currentSession[currentIndex] == index)
            {
                Debug.Log("Correct sphere: " + index);
                currentIndex++;
                
                if (currentIndex >= currentSession.Count)
                {
                    patternComplete = true;
                    shooter?.FireProjectile(0, shootSource.position);
                    MakeAllSpheresGrey();
                }
                else
                {
                    UpdateAllSphereColors();
                }
            }
            else
            {
                Debug.Log("Wrong sphere! Expected: " + currentSession[currentIndex] + ", Got: " + index);
            }
        }

        private void UpdateAllSphereColors()
        {
            for (int i = 0; i < spheres.Count; i++)
            {
                Renderer renderer = spheres[i].GetComponent<Renderer>();
                if (renderer != null)
                {
                    int sessionIndex = currentSession.IndexOf(i);

                    if (sessionIndex == -1)
                    {
                        renderer.material.color = greyColor;
                    }
                    else if (sessionIndex < currentIndex)
                    {
                        renderer.material.color = greenColor;
                    }
                    else if (sessionIndex == currentIndex)
                    {
                        renderer.material.color = yellowColor;
                    }
                    else
                    {
                        renderer.material.color = greyColor;
                    }
                }
            }
        }
        
        private void MakeAllSpheresGrey()
        {
            for (int i = 0; i < spheres.Count; i++)
            {
                Renderer renderer = spheres[i].GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = greyColor;
                }
            }
        }
    }
}