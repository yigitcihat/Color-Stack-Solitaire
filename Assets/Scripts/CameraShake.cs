using System.Collections;
using DG.Tweening;
using UnityEngine;

public class CameraShake : Singleton<CameraShake>
{
    private Vector3 originalPos;

    private void Start() => originalPos = transform.position;

    internal IEnumerator Shake(float duration, float magnitude)
    {
        Taptic.Heavy();
        var startTime = Time.time;
        var elapsed = 0.0f;

        while (elapsed < duration)
        {
            var x = Random.Range(-1f, 1f) * magnitude;
            var y = Random.Range(-1f, 1f) * magnitude;

            transform.DOMove(new (originalPos.x + x, originalPos.y + y, originalPos.z), duration / 5);

            elapsed = Time.time - startTime;
            yield return new WaitForSeconds(duration / 5);
        }

        transform.DOMove(originalPos, Time.deltaTime);
    }
}