using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/*
 * *********************************************************
 * 名称：渲染器的材质管理
 
 * 日期：2015.8.28
 * 描述：由于可能有多个材质特效会叠加在同一个render上。
 *       这个类保证了对于render上的同一个材质一个时间只能有一个东西改变它。
 *       比如vu滚动或者改颜色。
 * *********************************************************
 */

 [RequireComponent(typeof(Renderer))]
public class MaterialMgr : MonoBehaviour {
    public class MaterialHandle : IShareThingHandle
    {
        public MaterialFx fx;
        public Material mat;
        public MaterialMgr mgr;
        public MaterialHandle(MaterialFx fx, Material mat,MaterialMgr mgr)
        {
            this.fx = fx;
            this.mat =mat;
            this.mgr = mgr;
        }

        public override void OnLast(IShareThingHandle prev){
            
        }
        public override void OnOverlay() { }
        public override void OnEmpty(){}
    }

    #region Fields
    List<ShareThing> m_materials = new List<ShareThing>();//每个材质球就是一个共享事物,通过shareThing可以很好的修改和复位材质

    bool m_cache=false;
    Renderer m_render;
    bool m_isVisible = true;//优化，可见的时候才UpdateMatrial
    #endregion

    #region Properties
    public Renderer Renderer { get { return m_render; } }
    #endregion

    #region Mono Frame
    // Update is called once per frame
    void LateUpdate()
    {
        if (!m_isVisible)
            return;

        MaterialHandle h;
        for (int i = m_materials.Count - 1; i >= 0; --i)
        {
            h = m_materials[i].Get<MaterialHandle>();
            if (h != null)
                h.fx.UpdateMaterial();
        }
    }

    void OnBecameVisible()
    {
        m_isVisible = true;
    }

    void OnBecameInvisible()
    {
        m_isVisible = false;
    }

    void OnEnable()
    {
        Cache();
        m_isVisible = true;
    }

    void OnDisable()
    {
        Cache();
        Clear();
    }
    #endregion

    #region Private Methods
    
    #endregion
    public void Cache(bool force=false)//force表明原始材质可能改变了，重新设置下原始材质
    {
        if (m_cache)
        {
            if(force)//这里改下主材质就可以了
            {
                m_materials[0].m_param = m_render.sharedMaterial;
            }
            return;
        }

        gameObject.isStatic = false;
        m_render = GetComponent<Renderer>();
        Material[] ms = m_render.sharedMaterials;
        m_materials.Clear();
        if (ms != null)
        {
            for (int i = 0; i < ms.Length; ++i)
            {
                ShareThing st = new ShareThing();
                st.m_param = ms[i];//原始材质记录下来
                st.m_param2 = true;//表明是角色创建的时候就有的材质
                m_materials.Add(st);
                if (ms[i] == null)
                {
                    Debuger.LogError("可能材质丢失");
                }
            }
        }

        m_cache = true;
    }
    

	
    public MaterialHandle Add(MaterialFx fx)
    {
        Cache();
        
        Material startMat=null;
        //获取原始材质
        if (fx.m_type == MaterialFx.enType.replace || fx.m_type == MaterialFx.enType.modify)
        {
            if (fx.m_matIndex >= m_materials.Count || m_materials[fx.m_matIndex].m_param == null)
            {
                Debuger.LogError("找不到对应材质，或者要改变材质的render材质丢失 fx:{0} index:{1}", fx.name, fx.m_matIndex);
                return null;
            }
            startMat = (Material)m_materials[fx.m_matIndex].m_param;
        }
        

        //创建新材质
        Material newMat;
        if(fx.m_type == MaterialFx.enType.add || fx.m_type == MaterialFx.enType.replace ){
            if (fx.m_mat == null)
            {
                Debuger.LogError("材质没有填 fx:{0} ", fx.name);
                return null;
            }
            if (fx.m_anis.Count == 0 && (fx.m_type == MaterialFx.enType.add || !fx.m_useOldTex))//不需要渐变的情况下材质不需要复制
                newMat = fx.m_mat;
            else
            {
                newMat = new Material(fx.m_mat);
                newMat.name = fx.m_mat.name+ "(Clone)";
            }
                
        }
        else if (fx.m_type == MaterialFx.enType.modify)
        {
            newMat = new Material(startMat);
            newMat.name = startMat.name + "(Clone)";
        }
        else
        {
            Debuger.LogError("未知的类型:{0}",fx.m_type);
            return null;
        }

        //如果有多个(Clone)后缀，那只保留一下
        //if (newMat.name.EndsWith("(Clone)(Clone)"))
            //newMat.name = newMat.name.Replace("(Clone)", "") + "(Clone)";

        //替换新材质的贴图
        if (fx.m_useOldTex&&startMat!=null)
        {
            Texture tex;
            if (startMat.HasProperty("_MainTex"))
                tex = startMat.GetTexture("_MainTex");
            else if (startMat.HasProperty("_MainTexAlpha"))
                tex = startMat.GetTexture("_MainTexAlpha");
            else
            {
                Debuger.LogError("找不到老材质的贴图 fx:{0} index:{1}", fx.name, fx.m_matIndex);
                return null;
            }

            if (newMat.HasProperty("_MainTex"))
                newMat.SetTexture("_MainTex",tex)  ;
            else if (newMat.HasProperty("_MainTexAlpha"))
                newMat.SetTexture("_MainTexAlpha", tex);
            else
            {
                Debuger.LogError("找不到新材质的贴图 fx:{0} index:{1}", fx.name, fx.m_matIndex);
                return null;
            }
        }
        
        //添加到共享事物里进行管理
        MaterialHandle handle = new MaterialHandle(fx, newMat,this);
        ShareThing st;
        int idx;
        if (fx.m_type == MaterialFx.enType.add)
        {
            st = new ShareThing();
            st.m_param = null;//原始材质记录下来,在这里没有原始材质
            st.m_param2 = false;//表明不是是角色创建的时候就有的材质
            m_materials.Add(st);
            st.Add(handle);
            idx = m_materials.Count-1;
        }
        else
        {
            st = m_materials[fx.m_matIndex];
            st.Add(handle);
            idx = fx.m_matIndex;
        }
            

        //真正把材质放到Render上
        bool needChange=handle == st.Get();
        if (needChange)
        {
            Material[] mats = m_render.sharedMaterials;
            int length = Mathf.Max(idx + 1, mats.Length);
            Material[] newMats = new Material[length];
            for (int i = 0; i < length; ++i)
            {
                if (i == idx)
                    newMats[i] =newMat;
                else
                    newMats[i] = mats[i];
            }
            m_render.sharedMaterials = newMats;
        }

        return handle;
    }

    public void Remove(MaterialHandle h)
    {
        if (this == null) return;//可能已经被销毁
        if (h.m_shareThing == null)//防止死锁
            return;

        Cache();

        int idx = m_materials.IndexOf(h.m_shareThing);
        if (idx == -1)
        {
            Debuger.LogError("逻辑错误，找不到shareThing");
            h.m_shareThing =null;//防止死锁
            return;
        }
        

        //从shareThing中删除
        ShareThing st= m_materials[idx];
        bool needChange = st.Get() == h;//如果最顶的不是h，那么肯定是不用改变render的材质的
        st.Remove(h);//remove里执行了h.m_shareThing = null，必须保证在h.fx.Stop()之前执行，不然可能造成死锁

        //Remove和stop可能会互相调用,要判断下内部已经做了判断
        h.fx.Stop();

        //不需要改材质
        if(!needChange)
            return;

        //计算要改的材质
        bool remove = false;
        Material newMat = null;
        MaterialHandle newH = st.Get<MaterialHandle>();
        if (newH == null)//已经没有特效材质的情况，这个时候有原始材质就换回原始材质，没有就删除
        {
            if ((bool)st.m_param2)
                newMat = (Material)st.m_param;
            else
            {
                m_materials.RemoveAt(idx);
                remove = true;
            }
                
        }
        else//有别的特效材质的情况
            newMat = newH.mat;

        //真正把材质放到Render上
        Material[] mats = m_render.sharedMaterials;
        int length = remove ? mats.Length - 1 : mats.Length;
        Material[] newMats = new Material[length];
        int curIdx = 0;
        for (int i = 0; i < mats.Length; ++i)
        {
            if (i == idx){
                if(remove == false)
                    newMats[curIdx] = newMat;
            }
            else
                newMats[curIdx] = mats[i];

            if (i != idx || remove == false)
                ++curIdx;
        }
        m_render.sharedMaterials = newMats;
    }

    void Clear(){
        ShareThing st;
        List<Material> ms = new List<Material>();
        for (int i = m_materials.Count - 1; i >= 0; --i)
        {
            st =m_materials[i];

            foreach (IShareThingHandle h in st.m_handles)
            {
                h.m_shareThing =null;//注意一定先设为null，不然会造成死锁            
                ((MaterialHandle)h).fx.Stop();
            }
            st.Clear();

            if (((bool)st.m_param2)==false)
                m_materials.RemoveAt(i);
            else {
                Material m =(Material)st.m_param;
                if(m!=null)
                    ms.Insert(0, m);
                else
                {
                    Debuger.LogError("材质在运行过程中丢失了");
                }
            }
        }
        
        //还原回原来的材质
        m_render.sharedMaterials =ms.ToArray();
        
    }
}
