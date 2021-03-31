using System.Collections;
using UnityEngine;

namespace Zenject
{
    public class CoroutineRunner : MonoBehaviour
    {
        public void RunCoroutine(IEnumerator routine, float delay = 0)
        {
            StartCoroutine(RunCoroutineCoroutineWithDelay(routine, delay));
        }

        private IEnumerator RunCoroutineCoroutineWithDelay(IEnumerator routine, float delay = 0)
        {
            if (delay > 0)
                yield return new WaitForSeconds(delay);

            StartCoroutine(routine);
        }
    }
}