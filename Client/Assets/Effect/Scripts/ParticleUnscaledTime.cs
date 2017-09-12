using UnityEngine;

public class ParticleUnscaledTime : MonoBehaviour
{
    private float m_lastTime;
    private ParticleSystem m_particle;

    bool isEqual(float a, float b)
    {
        if (a >= b - Mathf.Epsilon && a <= b + Mathf.Epsilon)
            return true;
        else
            return false;
    }

    void Awake()
    {
        m_particle = GetComponent<ParticleSystem>();
    }

    void Start()
    {
    }

    void OnEnable()
    {
        m_lastTime = Time.realtimeSinceStartup;
        m_particle.Play();
    }

    void OnDisable()
    {
        m_particle.Stop();
    }

    void Update()
    {
        float curTime = Time.realtimeSinceStartup;
        float deltaTime = curTime - m_lastTime;
        m_lastTime = curTime;

        if (!isEqual(Time.timeScale, 1.0f))
            m_particle.Simulate(deltaTime, true, false);
    }
}
