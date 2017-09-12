using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public class RepeatableComparer<T> : IComparer<T>
    where T: IComparable<T>
{
    public int Compare(T a, T b)
    {
        return a.CompareTo(b)<=0? -1 : 1;//这样保证了键值重复的情况下不会报错，如果有等于0的情况，那么就SortedList就不能add相同的键值
    }
}

public class ValueObject<T> {
    T _value;
    public T Value{
        get { return _value; }
        set { _value = value; }
        }
    public ValueObject(T v)
    {
        _value = v;
    }

}


public static class ClassExtend  {
    public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
    {
        Rect result = rect;
        result.x -= pivotPoint.x;
        result.y -= pivotPoint.y;
        result.xMin *= scale;
        result.xMax *= scale;
        result.yMin *= scale;
        result.yMax *= scale;
        result.x += pivotPoint.x;
        result.y += pivotPoint.y;
        return result;
    }

    public static Vector3 HorizontalLeft(this Vector3 v)
    {
        return new Vector3(-v.z, 0, v.x);
    }

    public static Vector3 HorizontalRight(this Vector3 v)
    {
        return new Vector3(v.z, 0, -v.x);
    }

    public static T AddComponentIfNoExist<T>(this Component c) where T : Component
    {
        return c.gameObject.AddComponentIfNoExist<T>();
    }

    public static T AddComponentIfNoExist<T>(this GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp == null)
            return go.AddComponent<T>();
        else
            return comp;
    }

    public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) 
    {
        TValue val;
        if (dict.TryGetValue(key, out val) == false)
            return default(TValue);
        return val;

    }

    public static TValue GetNewIfNo<TKey, TValue>(this Dictionary<TKey, TValue> dict,TKey key)where TValue:new()
    {
        TValue val;
        if (dict.TryGetValue(key, out val) == false)
        {
            val = new TValue();
            dict.Add(key, val);
        }
        return val;
    }

    public static T Get<T>(this List<T> l, int idx)
    {
        if(idx<0 || idx>=l.Count)
            return default(T);
        
        return l[idx];
    }

    

    public static TValue Get<TKey, TValue>(this SortedDictionary<TKey, TValue> dict, TKey key, TValue def = default(TValue)) where TValue : new()
    {
        TValue val;
        if (dict.TryGetValue(key, out val) == false)
            return default(TValue);
        return val;

    }

    public static TValue GetNewIfNo<TKey, TValue>(this SortedDictionary<TKey, TValue> dict, TKey key) where TValue : new()
    {
        TValue val;
        if (dict.TryGetValue(key, out val) == false)
        {
            val = new TValue();
            dict.Add(key, val);
        }
        return val;

    }

  
    

    //默认的sort同优先级会乱序，所以使用自己写的插入排序实现
    public static void SortEx<T>(this List<T> list, System.Comparison<T> comparison) 
    {
        int count = list.Count;
        T tem;
        for (int j = 1; j < count; j++)
        {
            tem = list[j];

            int i = j - 1;
            for (; i >= 0 && comparison(list[i], tem) > 0; i--)
            {
                list[i + 1] = list[i];
            }
            list[i + 1] = tem;
        }
    }

    public static void Shuffle<T>(this List<T> list)
    {
        var rand = new System.Random();
        for (int i = 0; i < list.Count; ++i)
        {
            int idx = rand.Next(0, list.Count - i);
            T tmp = list[idx];
            list[idx] = list[list.Count - 1 - i];
            list[list.Count - 1 - i] = tmp;
        }
    }

    //默认的sort同优先级会乱序，所以使用自己写的插入排序实现
    public static void SortEx<T>(this LinkedList<T> list, System.Comparison<T> comparison)
    {
        if (list.Count < 2)
            return;

        LinkedListNode<T> tem;
        LinkedListNode<T> last = list.First;//已经排好序的那部分的队尾,在它的后面是等待排序的节点
        LinkedListNode<T> cur = last;
        do
        {
            do
            {
                if (comparison(last.Next.Value,cur.Value)>=0)
                {
                    if (last == cur)//不用插入了
                    {
                        last = last.Next;
                        cur = last;
                    }
                    else//插入
                    {
                        tem = last.Next;
                        list.Remove(tem);
                        list.AddAfter(cur, tem);
                        cur = last;
                    }
                    break;
                }
                if (cur.Previous == null)
                {
                    tem = last.Next;
                    list.Remove(tem);
                    list.AddBefore(cur, tem);
                    cur = last;
                    break;
                }
                cur = cur.Previous;
            }
            while (true);
        } while (last.Next != null);
        
    }

    public static string[] GetNames(this Animation ani)
    {
        List<string> l = new List<string>();
        if (ani == null)
            return l.ToArray();
        

        foreach (var v in ani)
        {
            if (v == null || !(v is AnimationState))
                continue;
            AnimationState st = (AnimationState)v;
            if (string.IsNullOrEmpty(st.name))
                continue;
            l.Add(st.name);
        }
        return l.ToArray();
    }
}
