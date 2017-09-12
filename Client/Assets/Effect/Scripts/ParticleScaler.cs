//This script will only work in editor mode. You cannot adjust the scale dynamically in-game!
using UnityEngine;
using System.Collections;

#if UNITY_EDITOR 
using UnityEditor;
#endif

[ExecuteInEditMode]
public class ParticleScaler : MonoBehaviour 
{
    [Range(0.01f,10f)]
	public float particleScale = 1.0f;
	public bool alsoScaleGameobject = true;
    
	float prevScale;

    public static void SetScale( GameObject go,float scale )
    {
        ParticleScaler par = go.GetComponent<ParticleScaler>();
        if (par == null)
            par = go.AddComponent<ParticleScaler>();
        par.SetScaleEffect(scale);
        //par.particleScale = scale;
    }

    public void AutoScaleByScene (float localScale)
    {
        int w = Screen.width;
        int h = Screen.height;
        particleScale = (((float)w / h) / (1136.0f / 640.0f)) * localScale;
        SetScaleEffect(particleScale);
    }

	void Awake()
	{
		prevScale = particleScale;
	}


 #if UNITY_EDITOR
    void Update()
    {
        SetScaleEffect(particleScale);
	}
#endif

    static public bool HasParticles(GameObject go)
    {
        ParticleSystem[] systems = go.GetComponentsInChildren<ParticleSystem>();
        if (systems != null && systems.Length > 0)
            return true;

        ParticleEmitter[] emitters = go.GetComponentsInChildren<ParticleEmitter>();
        if (emitters != null && emitters.Length > 0)
            return true;
        
        return false;
    }

    //设置特效大小，现在可以动态修改
    public void SetScaleEffect( float scale )
    {
        //check if we need to update
        if ( !Mathf.Approximately(prevScale, scale) && scale > 0)
        {
            if (alsoScaleGameobject)
                transform.localScale = new Vector3(scale, scale, scale);

            float scaleFactor = scale / prevScale;

            //scale legacy particle systems
            //ScaleLegacySystems(scaleFactor);//提升性能，考虑到运行时也要缩放

            //scale shuriken particle systems
            ScaleShurikenSystems(scaleFactor);

            //scale trail renders
            ScaleTrailRenderers(scaleFactor);

            prevScale = scale;
            particleScale = scale;
        }
    }

	void ScaleShurikenSystems(float scaleFactor)
	{
		//get all shuriken systems we need to do scaling on
		ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>();

		foreach (ParticleSystem system in systems)
		{
			system.startSpeed *= scaleFactor;
			system.startSize *= scaleFactor;
			system.gravityModifier *= scaleFactor;

#if UNITY_EDITOR
            //some variables cannot be accessed through regular script, we will acces them through a serialized object
            SerializedObject so = new SerializedObject(system);
            so.FindProperty("VelocityModule.x.scalar").floatValue *= scaleFactor;
            so.FindProperty("VelocityModule.y.scalar").floatValue *= scaleFactor;
            so.FindProperty("VelocityModule.z.scalar").floatValue *= scaleFactor;
            so.FindProperty("ClampVelocityModule.magnitude.scalar").floatValue *= scaleFactor;
            so.FindProperty("ClampVelocityModule.x.scalar").floatValue *= scaleFactor;
            so.FindProperty("ClampVelocityModule.y.scalar").floatValue *= scaleFactor;
            so.FindProperty("ClampVelocityModule.z.scalar").floatValue *= scaleFactor;
            so.FindProperty("ForceModule.x.scalar").floatValue *= scaleFactor;
            so.FindProperty("ForceModule.y.scalar").floatValue *= scaleFactor;
            so.FindProperty("ForceModule.z.scalar").floatValue *= scaleFactor;
            so.FindProperty("ColorBySpeedModule.range").vector2Value *= scaleFactor;
            so.FindProperty("SizeBySpeedModule.range").vector2Value *= scaleFactor;
            so.FindProperty("RotationBySpeedModule.range").vector2Value *= scaleFactor;

            so.ApplyModifiedProperties();
#endif
		}
	}

	void ScaleLegacySystems(float scaleFactor)
	{
		//get all emitters we need to do scaling on
		ParticleEmitter[] emitters = GetComponentsInChildren<ParticleEmitter>();

		//get all animators we need to do scaling on
		ParticleAnimator[] animators = GetComponentsInChildren<ParticleAnimator>();

		//apply scaling to emitters
		foreach (ParticleEmitter emitter in emitters)
		{
			emitter.minSize *= scaleFactor;
			emitter.maxSize *= scaleFactor;
			emitter.worldVelocity *= scaleFactor;
			emitter.localVelocity *= scaleFactor;
			emitter.rndVelocity *= scaleFactor;

#if UNITY_EDITOR
            //some variables cannot be accessed through regular script, we will acces them through a serialized object
            SerializedObject so = new SerializedObject(emitter);

            so.FindProperty("m_Ellipsoid").vector3Value *= scaleFactor;
            so.FindProperty("tangentVelocity").vector3Value *= scaleFactor;
            so.ApplyModifiedProperties();
#endif
		}

		//apply scaling to animators
		foreach (ParticleAnimator animator in animators)
		{
			animator.force *= scaleFactor;
			animator.rndForce *= scaleFactor;
		}
	}

	void ScaleTrailRenderers(float scaleFactor)
	{
		//get all animators we need to do scaling on
		TrailRenderer[] trails = GetComponentsInChildren<TrailRenderer>();

		//apply scaling to animators
		foreach (TrailRenderer trail in trails)
		{
			trail.startWidth *= scaleFactor;
			trail.endWidth *= scaleFactor;
		}
    }
}
