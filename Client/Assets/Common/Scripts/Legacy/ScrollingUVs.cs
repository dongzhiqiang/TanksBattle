using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScrollingUVs : MonoBehaviour 
{
    [SerializeField]
	int materialIndex = 0;

    public Vector2 uvAnimationRate = new Vector2(1.0f, 0.0f);
	public string textureName = "_MainTex";

    Renderer mRenderer;

    Material material = null;

    void Start()
    {
        gameObject.isStatic = false;

        mRenderer = GetComponent<Renderer>();

        if (mRenderer == null)
        {
            this.enabled = false;
            Debuger.LogError("该对象没有render组件，不应该挂脚本ScrollingUVs  go=" + this.gameObject.name);
            return;
        }

        if(materialIndex == 0)
        {
            material = GetComponent<Renderer>().material;
        }
        else
        {
            Material[] materials = GetComponent<Renderer>().materials;
            if (materials == null || materialIndex >= materials.Length || materialIndex < 0 || ((material = materials[materialIndex]) == null))
            {
                this.enabled = false;
                materials = null;
                Debuger.LogError("ScrollingUVs 纹理超了!" + this.gameObject.name);
            }
        }

    }

    Vector2 uvOffset = Vector2.zero;
	
	void LateUpdate() 
	{
        if (!isVisible)
            return;

        uvOffset += uvAnimationRate * Time.unscaledDeltaTime;
        if (uvOffset.x > 1.0f || uvOffset.x < -1.0f)
            uvOffset.x = 0;

        if (uvOffset.y > 1.0f || uvOffset.y < -1.0f)
            uvOffset.y = 0;

        if (material != null && mRenderer != null && mRenderer.enabled)
        {
            material.SetTextureOffset(textureName, uvOffset);
        }
	}

    bool isVisible = true;

    void OnBecameVisible()
    {
        isVisible = true;
    }

    void OnBecameInvisible()
    {
        isVisible = false;
    }


	void OnDisable()
	{
		uvOffset = Vector2.zero; 
	}

    void OnEnable()
    {
        isVisible = true;
    }
}