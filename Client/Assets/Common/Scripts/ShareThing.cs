/*
 * ********************************************************************
 * 名称：共享资源管理器
 
 * 日期：2015.8.28
 * 描述：多个地方需要操作同一个资源的时候（如层、相机等），操作完成还原
 *       的时候可能会覆盖掉别的操作。用这个类可以方便的管理。
 * 规则：插入时如果没有优先级更高的handle则调用Handle的onLast
 *       当最后一个handle被删除的时候调用Handle的OnEmpty
 * 这里有个建议:
 *       1.如果对ShareThing操作是一样的，那么建议从ShareThingHandle继承
 *       2.如果对ShareThing的操作是多种的，比如相机位置的控制可能有多种
 *        多样)(拉近、看别人、沿着某个路径)。建议用ShareThingDelegate，
 *        原因见ShareThingDelegate.OnEmpty()的描述
 *       3.操作内容比较短，除了共享资源外没什么变量的话，那么也建议用
 *        ShareThingDelegate 
 * 例子:
    ShareThing shareThing = new ShareThing();
    int i =0;
    ShareThingDelegate d1=shareThing.Add((object p) => { Debuger.LogError("1"); }, (object p) => { Debuger.LogError("1" + i); }, 1, null, i);
    i++;
    ShareThingDelegate d3 = shareThing.Add((object p) => { Debuger.LogError("3"); }, (object p) => { Debuger.LogError("3" + i); }, 3, null, i);
    i++;
    ShareThingDelegate d2 = shareThing.Add((object p) => { Debuger.LogError("2"); }, (object p) => { Debuger.LogError("2" + i); }, 2, null, i);
    i++;
    ShareThingDelegate d4 = shareThing.Add((object p) => { Debuger.LogError("4"); }, (object p) => { Debuger.LogError("4" + i); }, 4, null, i);

    d3.Release();
    d4.Release();
    d1.Release();
    d2.Release();
 * 打印:1 3 4 2 13
 * ********************************************************************
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class IShareThingHandle{
    public ShareThing m_shareThing ;//这个变量是给ShareThing用的，外部不要赋值
    public int m_priority;//优先级，决定了被放置的顺序
    public bool m_removeIfOverlay;//如果被重叠则删除

    public bool IsHandling { get { return m_shareThing.Get() == this; } }//是不是正在运行中的那一个

    public IShareThingHandle()
    {
        
    }
    //当自己为最后的时候
    public abstract void OnLast(IShareThingHandle prev);

    //当自己从最后被别人覆盖
    public abstract void OnOverlay();

    //当因自己被最后一个删除的时候
    public abstract void OnEmpty();

    public void Release()
    {
        if (m_shareThing != null)
            m_shareThing.Remove(this);
    }

    
}

public class ShareThingDelegate : IShareThingHandle
{
     public Action<object> onPush;//操作资源
     public object pushParam;
     public Action<object> onEmpty;//释放资源
     public object emptyParam;

     public ShareThingDelegate(Action<object> onPush, Action<object> onEmpty, object pushParam, object emptyParam)
     {
         this.onPush = onPush;
         this.onEmpty = onEmpty;
         this.pushParam = pushParam;
         this.emptyParam = emptyParam;
     }

     public override void OnLast(IShareThingHandle prev)
     {
         if (onPush!=null)
            onPush(pushParam);
         if (m_shareThing.m_param == null)
         {
             m_shareThing.m_param = onEmpty;//emptyParam是真正的共享资源的初始值
             m_shareThing.m_param2 = emptyParam;
         }       
     }

     public override void OnOverlay() { }

     public override void OnEmpty()
     {
         //这里的empty函数和empty参数可能不是当前对象的
         //因为写ShareThingDelegate的本意就是为了不同操作可以对同一个共享资源进行处理
         //那么必须保证第一个操作者传进来的empty参数才是复位共享资源的关键
         Action<object> onTrueEmpty = (Action<object>)m_shareThing.m_param;
         if (onTrueEmpty!=null)
             onTrueEmpty(m_shareThing.m_param2);
     }
}


public class ShareThing
{
    const int Check_Leak_Count = 100 ;//泄露检测，超过这个数量就报错

    public object m_param =null;//这个值可以理解为共享资源。可以参考ShareThingDelegate的实现
    public object m_param2 = null;
    public object m_param3 = null;

    public LinkedList<IShareThingHandle> m_handles = new LinkedList<IShareThingHandle>();

    //增加操作
    public ShareThingDelegate Add(Action<object> onPush, Action<object> onEmpty, int priority = 0, object pushParam = null, object emptyParam = null, bool removeIfOverlay = false)
    {        
        ShareThingDelegate d =new ShareThingDelegate(onPush, onEmpty, pushParam, emptyParam);
        Add(d, priority, false, removeIfOverlay);
        return d;
    }

    //增加操作
    public T Add<T>(int priority = 0, bool changeIfExist = false, bool removeIfOverlay = false) where T : IShareThingHandle, new()
    {
        T handle = new T();
        Add(handle, priority, changeIfExist, removeIfOverlay);
        return handle;
    }

    //增加操作，如果已经在里面了，那么changeIfExist=true时Change()，否则报个错
    public void Add(IShareThingHandle handle, int priority = 0,bool changeIfExist=false,bool removeIfOverlay=false) 
    {
        //
        if (handle.m_shareThing != null)
        {
            if(handle.m_shareThing != this)
            {
                Debuger.LogError("逻辑错误handle的shareThing不一样");
                return;
            }
            if(!changeIfExist)
            {
                Debuger.LogError("已经在shareThing里面了");
                return;
            }
            Change(handle, priority);
            return;
        }

        IShareThingHandle oldLast = Get();

        //这里检错下，如果太多，说明可能有忘记释放的地方
        if (m_handles.Count > Check_Leak_Count)
            Debuger.LogError("共享资源管理器中操作数太多。type=" + handle.GetType().ToString());
        
        handle.m_shareThing = this;
        handle.m_priority = priority;
        handle.m_removeIfOverlay = removeIfOverlay;

        //按照插入同优先级的最后面
        LinkedListNode<IShareThingHandle> node = m_handles.Last;
        if(node == null )
            m_handles.AddLast(handle);    
        else {
            do{
                if (node.Value.m_priority <= priority){
                    m_handles.AddAfter(node,handle);
                    break;
                }
                else if (node.Previous==null)
                {
                    m_handles.AddBefore(node, handle);
                    break;
                }
                node= node.Previous;
            }while(true);
        }

        //处理回调,一定要放在位置操作之后
        if (Get()== handle)
        {
            handle.OnLast(oldLast);
            if (oldLast != null)
            {
                oldLast.OnOverlay();
                if (oldLast.m_removeIfOverlay)
                    m_handles.Remove(oldLast);
            }
            if (m_handles.Count == 0)
                handle.OnEmpty();   
        }
        
    }

    //修改操作,如果外部修改了优先级，可以用这个函数重新排列
    public void Sort()
    {
        IShareThingHandle oldLast = Get();

        m_handles.SortEx(Compare);

        //处理回调,一定要放在位置操作之后
        IShareThingHandle last = Get();
        if (oldLast != last)
        {
            last.OnLast(oldLast);
            oldLast.OnOverlay();
        }
    }

    static public int Compare(IShareThingHandle a,IShareThingHandle b)
    {
        return a.m_priority.CompareTo(b.m_priority);
    }


    //修改操作,默认是放在同优先级后面(先出)，addAfter =false那么就放前(慢出)
    public void Change(IShareThingHandle handle, int priority,bool after = true)
    {
        IShareThingHandle oldLast = Get();
        if (!m_handles.Remove(handle))
        {
            Debuger.LogError("改变不了优先级，不在管理器中");
            return;
        }

        //按照插入同优先级的最后面
        handle.m_priority = priority;
        LinkedListNode<IShareThingHandle> node = m_handles.Last;
        if (node == null)
            m_handles.AddLast(handle);
        else
        {
            do
            {
                if (after == true && node.Value.m_priority <= priority)//插入到优先级后面
                {
                    m_handles.AddAfter(node, handle);
                    break;
                }
                else if (after == false && node.Value.m_priority < priority)//插入到优先级前
                {
                    m_handles.AddAfter(node, handle);
                    break;
                }
                else if (node.Previous == null)
                {
                    m_handles.AddBefore(node, handle);
                    break;
                }
                
                node = node.Previous;
            } while (true);
        }

        //处理回调,一定要放在位置操作之后
        IShareThingHandle last = Get();
        if (oldLast != last)
        {
            last.OnLast(oldLast); 
            oldLast.OnOverlay();
            if (oldLast.m_removeIfOverlay)
                m_handles.Remove(oldLast);
        }
        if (m_handles.Count == 0)
            handle.OnEmpty();    
    }

    //删除操作
    public void Remove(IShareThingHandle handle)
    {
        IShareThingHandle oldLast = Get();
        handle.m_shareThing = null;
        if (!m_handles.Remove(handle))
        {
            Debuger.LogError("删除不了，不在管理器中");
            return;
        }

        //处理回调,一定要放在位置操作之后
        IShareThingHandle last = Get();
        if (oldLast != last)
        {
            if (last!=null)
                last.OnLast(oldLast);
            if (oldLast.m_shareThing!= null)//注意这里要判断下，有可能就是被删除的那个
                oldLast.OnOverlay();
            if (oldLast.m_removeIfOverlay)
                m_handles.Remove(oldLast);
        }
        if (m_handles.Count == 0)
            handle.OnEmpty();    
    }

    //删除操作
    public void Remove(List<LinkedListNode<IShareThingHandle>> removes)
    {
        IShareThingHandle oldLast = Get();

        foreach (LinkedListNode<IShareThingHandle> node in removes)
        {
            node.Value.m_shareThing = null;
            m_handles.Remove(node);
        }

        //处理回调,一定要放在位置操作之后
        IShareThingHandle last = Get();
        if (oldLast != last)
        {
            if (last != null)
                last.OnLast(oldLast);
            if (oldLast.m_shareThing != null)//注意这里要判断下，有可能就是被删除的那个
                oldLast.OnOverlay();
            if (oldLast.m_removeIfOverlay)
                m_handles.Remove(oldLast);
        }
        if (m_handles.Count == 0)
            oldLast.OnEmpty();
    }   

    //删除操作，onCheckRemove返回true则删除，注意onCheckRemove中不能有其他的增删改操作，这里是从前往后遍历，这样可以防止这个过程中多次OnLast
    public void Remove(Func<IShareThingHandle,bool> onCheckRemove )
    {
        if(m_handles.Count == 0)
            return;
        IShareThingHandle oldLast =Get();
        
        LinkedListNode<IShareThingHandle>  node = m_handles.First;
        LinkedListNode<IShareThingHandle> temp;
        
        while (node != null)
        {
            temp = node.Next;
            if(onCheckRemove(node.Value))
            {
                m_handles.Remove(node);
                node.Value.m_shareThing = null;
            }
            node = temp;
        }

        //处理回调,一定要放在位置操作之后
        IShareThingHandle last = Get();
        if (oldLast != last)
        {
            if (last != null)
                last.OnLast(oldLast);
            if (oldLast.m_shareThing != null)//注意这里要判断下，有可能就是被删除的那个
                oldLast.OnOverlay();
            if (oldLast.m_removeIfOverlay)
                m_handles.Remove(oldLast);
        }
        if (m_handles.Count == 0)
            oldLast.OnEmpty();
            
    }

    //清空操作
    public void Clear()
    {
        if (m_handles.Count == 0)
            return;

        IShareThingHandle last = Get();
        LinkedListNode<IShareThingHandle> node = m_handles.First;
        while (node != null)
        {
            node.Value.m_shareThing = null;
            node = node.Next;
        }
        m_handles.Clear();

        //处理回调,一定要放在位置操作之后
        if (last != null)
            last.OnEmpty();
    }

    //获取当前的handle
    public IShareThingHandle Get()     {
        return m_handles.Count != 0 ? m_handles.Last.Value : null;
    }

    //获取当前的handle
    public T Get<T>() where T : IShareThingHandle
    {
        return m_handles.Count != 0 ? m_handles.Last.Value as T : null;
    }
}
