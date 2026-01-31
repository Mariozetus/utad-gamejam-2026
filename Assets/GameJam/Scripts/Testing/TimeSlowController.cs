using UnityEngine;
using System.Collections;

public class TimeSlowController : MonoBehaviour
{
    [SerializeField] private float slowScale = 0.2f;
    private bool _running;

    public void TriggerSlowTime(float durationSeconds)
    {
        if (_running) return;
        StartCoroutine(SlowRoutine(durationSeconds));
    }

    private IEnumerator SlowRoutine(float duration)
    {
        _running = true;
        float old = Time.timeScale;
        Time.timeScale = slowScale;

        float end = Time.unscaledTime + Mathf.Max(0f, duration);
        while (Time.unscaledTime < end) yield return null;

        Time.timeScale = old;
        _running = false;
    }
}