using System.Collections;
using UnityEngine;

// Slash-tillägg: vrider svärdet i ett svep från vänster till höger
public class SwordSwing : MonoBehaviour
{
    public float swingTime = 0.22f;   // hur lång tid svepet tar (sekunder)
    public float startAngle = -80f;   // vinkel där svepet börjar (vänster)
    public float endAngle = 80f;      // vinkel där svepet slutar (höger)
    public float restAngle = -100f;   // vinkel när svärdet vilar vid sidan
    public TrailRenderer trail;       // ljusstrimman efter klingan

    private bool swinging = false;

    void Start()
    {
        transform.localRotation = Quaternion.Euler(0, restAngle, 0);
        if (trail != null) trail.emitting = false;
    }

    public void Swing()
    {
        if (!swinging)
        {
            StartCoroutine(SwingRoutine());
        }
    }

    IEnumerator SwingRoutine()
    {
        swinging = true;
        if (trail != null) { trail.Clear(); trail.emitting = true; }

        float t = 0;
        while (t < swingTime)
        {
            t += Time.deltaTime;
            float angle = Mathf.Lerp(startAngle, endAngle, t / swingTime);
            transform.localRotation = Quaternion.Euler(0, angle, 0);
            yield return null;
        }

        if (trail != null) trail.emitting = false;
        yield return new WaitForSeconds(0.1f);
        transform.localRotation = Quaternion.Euler(0, restAngle, 0);
        swinging = false;
    }
}
