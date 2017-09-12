using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundTestRoleMove : MonoBehaviour 
{
#if !ART_DEBUG
	public float m_speed = 3.0F;
    public Camera m_camera;
    public Dictionary<float,GameObject> m_gos=new Dictionary<float,GameObject>();
    
    Vector3 offset;
    List<float> removes = new List<float>();

    const string m_sound3d = "fx_sound_3d";
    const string m_bullet = "fx_gjb_jineng_zuzhouzhijian";
    
    void Start()
    {
        GameObjectPool.GetPool(GameObjectPool.enPool.Fx).PreLoad(m_sound3d, false);
        GameObjectPool.GetPool(GameObjectPool.enPool.Fx).PreLoad(m_bullet, false);
        offset = m_camera.transform.position - this.transform.position;
    }
	void Update () 
    {
        if (Input.GetKey(KeyCode.W))
            transform.position+= transform.forward * m_speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S))
            transform.position-= transform.forward * m_speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.D))
            transform.position+= transform.right * m_speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.A))
              transform.position-= transform.right * m_speed * Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space))
            Shoot();
        m_camera.transform.position = this.transform.position + offset;

        if (m_gos.Count != 0)
        {
            foreach (float key in m_gos.Keys)
            {
                GameObject go = m_gos.Get(key);
                go.transform.position += go.transform.forward * 10 * Time.deltaTime;
                if (Time.unscaledTime - key >= 3.0f)
                {
                    GameObjectPool.GetPool(GameObjectPool.enPool.Fx).Put(go);
                    removes.Add(key);
                }
            }
            int len = removes.Count;
            if (len == 0)
                return;
            for (int i = 0; i < len; i++)
                m_gos.Remove(removes[i]);

            if (removes.Count >= 0)
                removes.Clear();
        }
      
    }
    
    void Shoot()
    {
        GameObject go = GameObjectPool.GetPool(GameObjectPool.enPool.Fx).GetImmediately(m_bullet);
        go.transform.position = this.transform.TransformPoint(Vector3.zero);
        go.transform.localRotation = this.transform.localRotation;
        m_gos.Add(Time.unscaledTime, go);
        SoundMgr.instance.Play3DSound(1, go.transform);
    }

#endif
}
