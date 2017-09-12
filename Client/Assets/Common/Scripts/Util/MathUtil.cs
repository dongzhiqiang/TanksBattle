using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MathUtil
{
    public static bool IsEqual(float a, float b)
    {
        if (a >= b - Mathf.Epsilon && a <= b + Mathf.Epsilon)
            return true;
        else
            return false;
    }

    public static bool IsZero(float f)
    {
        if (f >= -Mathf.Epsilon && f <= Mathf.Epsilon)
            return true;
        else
            return false;
    }

    public static bool CanRect(Vector2 p1, Vector2 p2)
    {
        return p1.x != p2.x && p1.y != p2.y;
    }

    //知道两个点，求矩形
    public static Rect GetRectByTwoPoint(Vector2 p1,Vector2 p2)
    {
        return new Rect(Mathf.Min(p1.x, p2.x), Mathf.Min(p1.y, p2.y), Mathf.Abs(p1.x - p2.x), Mathf.Abs(p1.y - p2.y));
    }

    //知道中点，求矩形
    public static Rect GetRectByCenter(float centerX,float centerY,float width,float height)
    {
        return new Rect(centerX - width/2,centerY - height/2,width,height);
    }

    public static Rect GetRectInWorldSpace(RectTransform rt)
    {
        Vector3 vMin = rt.TransformPoint(rt.rect.min);
        Vector3 vMax = rt.TransformPoint(rt.rect.max);
        return GetRectByTwoPoint(vMin, vMax);
    }
    public static Vector3 GetCenterInWorldSpace(RectTransform rt)
    {
        Vector3 vMin = rt.TransformPoint(rt.rect.min);
        Vector3 vMax = rt.TransformPoint(rt.rect.max);
        return GetRectByTwoPoint(vMin, vMax).center;
    }

    static int[] idxs = new int[] { 1, 2, 3, 0 };
    static Vector2[] cornerRate =new Vector2[] { new Vector2(-1f,-1f), new Vector2(-1f, 1f), new Vector2(1f, 1f), new Vector2(1f, -1f) };
    static Vector2[] cornerPivot = new Vector2[] { new Vector2(1f, 1f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 1f) };
    /// <summary>
    /// 已知内外框，和要移动的框的大小，把框移动到内框四个角上，自动找到不会超出边界的那个角
    /// </summary>
    /// <param name="outerBox">外限制框</param>
    /// <param name="innerBox">内围绕框</param>
    /// <param name="moveBox">要移动位置的框</param>
    /// <param name="padding">移动框跟内围绕框的距离</param>
    public static void CalcAlignPos(RectTransform outerBox, RectTransform innerBox, RectTransform moveBox, Vector2 padding)
    {
        
        Vector3[] cornerMax = new Vector3[4];
        Vector3[] cornerCtrl = new Vector3[4];
        Vector3[] cornerSize= new Vector3[4];
        outerBox.GetWorldCorners(cornerMax);//从左下角开始，顺时针
        innerBox.GetWorldCorners(cornerCtrl);//从左下角开始，顺时针
        moveBox.GetWorldCorners(cornerSize);//从左下角开始，顺时针
        Rect rMax = GetRectByTwoPoint(cornerMax[0], cornerMax[2]);
        Rect rSize = GetRectByTwoPoint(cornerSize[0], cornerSize[2]);

        //找到适合的角
        int cur = idxs[0];
        foreach (int i in idxs)//这里从左上角开始检查
        {
            //控件的这一角超出了界面
            if (!rMax.Contains(cornerCtrl[i]))
                continue;

            //控件的这一角到界面同一角的范围内，不够容纳size
            Rect r = GetRectByTwoPoint(cornerCtrl[i], cornerMax[i]);
            if (rSize.width > r.width || rSize.height > r.height)
                continue;

            cur = i;
            break;
        }

        //设置轴心，注意这里是特地设置的，为了缩放从角落开始
        moveBox.pivot = cornerPivot[cur];

        //设置位置，加上padding
        Vector3 moveBoxScale = moveBox.lossyScale;
        Vector2 p1 = (Vector2)cornerCtrl[cur] + new Vector2(cornerRate[cur].x * padding.x * moveBoxScale.x, cornerRate[cur].y * padding.y * moveBoxScale.y);
        moveBox.position = new Vector3(p1.x, p1.y, cornerSize[0].z);
    }

    //显示一个RectTransforms的四个角
    public static void TestWorldCorners(RectTransform r)
    {
        Vector3[] fourCornersArray = new Vector3[4];
        r.GetWorldCorners(fourCornersArray);//从左下角开始，顺时针
        for (int i = 0; i < fourCornersArray.Length; ++i)
        {
            RectTransform tt = new GameObject(i.ToString(), typeof(RectTransform)).GetComponent<RectTransform>();
            tt.SetParent(r, false);
            tt.localScale = Vector3.one;
            tt.anchoredPosition3D = Vector3.zero;
            tt.position = fourCornersArray[i];
        }
    }
    
    /// <summary>
    /// 已知内外框，和要移动的框的大小，把框移动到内框四个边上，自动找到最合适的边
    /// </summary>
    /// <param name="outerBox">外限制框</param>
    /// <param name="innerBox">内围绕框</param>
    /// <param name="moveBox">要移动位置的框</param>
    /// <param name="padding">移动框跟内围绕框参考边的距离</param>
    public static void AlignCtrl(RectTransform outerBox, RectTransform innerBox, RectTransform moveBox, Vector2 padding, bool allowRotate = false)
    {
        //获取世界空间的框
        Rect outerBoxRect = GetRectInWorldSpace(outerBox);
        Rect innerBoxRect = GetRectInWorldSpace(innerBox);
        Rect moveBoxRect = GetRectInWorldSpace(moveBox);
        Vector3 moveBoxScale = moveBox.lossyScale;

        //获取内框与外框的距离，找到最小值，并把移动框放到另一面
        float minDist = float.MaxValue;
        int targetSide = 0;
        float distVal = innerBoxRect.xMin - outerBoxRect.xMin;  //左
        if (distVal < minDist) { minDist = distVal; targetSide = 2; } //右
        distVal = outerBoxRect.yMax - innerBoxRect.yMax;        //上
        if (distVal < minDist) { minDist = distVal; targetSide = 3; } //下
        distVal = outerBoxRect.xMax - innerBoxRect.xMax;        //右
        if (distVal < minDist) { minDist = distVal; targetSide = 0; } //左
        distVal = innerBoxRect.yMin - outerBoxRect.yMin;        //下
        if (distVal < minDist) { minDist = distVal; targetSide = 1; } //上

        switch (targetSide)
        {
            case 0: //左
                moveBox.position = new Vector3(innerBoxRect.xMin - moveBoxRect.width * (1 - moveBox.pivot.x) - padding.x * moveBoxScale.x, innerBoxRect.yMin + innerBoxRect.height / 2, moveBox.position.z);
                if (allowRotate)
                    moveBox.localRotation = Quaternion.Euler(0, 0, -90.0f);
                break;
            case 1: //上
                moveBox.position = new Vector3(innerBoxRect.xMin + innerBoxRect.width / 2, innerBoxRect.yMax + moveBoxRect.height * moveBox.pivot.y + padding.y * moveBoxScale.y, moveBox.position.z);
                if (allowRotate)
                    moveBox.localRotation = Quaternion.Euler(0, 0, 180.0f);
                break;
            case 2: //右
                moveBox.position = new Vector3(innerBoxRect.xMax + moveBoxRect.width * moveBox.pivot.x + padding.x * moveBoxScale.x, innerBoxRect.yMin + innerBoxRect.height / 2, moveBox.position.z);
                if (allowRotate)
                    moveBox.localRotation = Quaternion.Euler(0, 0, 90.0f);
                break;
            case 3: //下
                moveBox.position = new Vector3(innerBoxRect.xMin + innerBoxRect.width / 2, innerBoxRect.yMin - moveBoxRect.height * (1 - moveBox.pivot.y) - padding.y * moveBoxScale.y, moveBox.position.z);
                if (allowRotate)
                    moveBox.localRotation = Quaternion.Euler(0, 0, 0.0f);
                break;
        }
    }

    /// <summary>
    /// 让两个UI框中心对齐，并可以带上偏移
    /// </summary>
    /// <param name="targetBox"></param>
    /// <param name="moveBox"></param>
    /// <param name="offsetFromCenter"></param>
    public static void CenterCtrl(RectTransform targetBox, RectTransform moveBox, Vector2 offsetFromCenter)
    {
        //获取世界空间的框
        Rect targetBoxRect = GetRectInWorldSpace(targetBox);
        Rect moveBoxRect = GetRectInWorldSpace(moveBox);
        Vector3 moveBoxScale = moveBox.lossyScale;
        moveBox.position = new Vector3(targetBoxRect.xMin + targetBoxRect.width / 2 + offsetFromCenter.x * moveBoxScale.x, targetBoxRect.yMin + targetBoxRect.height / 2 + offsetFromCenter.y * moveBoxScale.y, moveBox.position.z);
    }

    public enum CornerType
    {
        none,
        leftTop,
        rightTop,
        rightBottom,
        leftBottom,
    }

    /// <summary>
    /// 在一个外框的限定范围内，移动一个框到另一个正方形内框的四个角落，具体哪个角落，自动选择
    /// 如果这个不动的内框不是正方形，那我们就假设它是以短边为长度的正方形，也就是最终可能不在四个边角上，而是虚拟的正方形的四个角落里
    /// </summary>
    /// <param name="outerBox">外限制框</param>
    /// <param name="innerBox">内围绕框</param>
    /// <param name="moveBox">要移动位置的框</param>
    /// <param name="padding">移动框跟内围绕框参考角落点的距离</param>
    /// <returns>返回放置的方位</returns>
    public static CornerType AlignCtrlToCornerOfSquare(RectTransform outerBox, RectTransform innerBox, RectTransform moveBox, Vector2 padding, bool allowRotate = false)
    {
        //获取世界空间的框
        Rect outerBoxRect = GetRectInWorldSpace(outerBox);
        Rect innerBoxRect = GetRectInWorldSpace(innerBox);
        Rect moveBoxRect = GetRectInWorldSpace(moveBox);
        //内框中心点
        Vector2 inBoxCenter = innerBoxRect.center;
        //内固定框的宽、高中的小值的一半
        float squareHalfWidth = Mathf.Min(innerBoxRect.width, innerBoxRect.height) / 2;

        //获取内框中心跟外框四角的距离
        //为什么用中心点？我们假设它就是正方形了，用中心点算出来的距离做比较，更省事，只是用来比较而已
        float minDist = float.MaxValue;
        CornerType targetCorner = CornerType.none;
        float distVal = Vector3.SqrMagnitude(new Vector2(outerBoxRect.xMax - inBoxCenter.x, inBoxCenter.y - outerBoxRect.yMin));  //右下
        if (distVal < minDist) { minDist = distVal; targetCorner = CornerType.leftTop; } //左上
        distVal = Vector3.SqrMagnitude(new Vector2(outerBoxRect.xMax - inBoxCenter.x, outerBoxRect.yMax - inBoxCenter.y));        //右上
        if (distVal < minDist) { minDist = distVal; targetCorner = CornerType.leftBottom; } //左下
        distVal = Vector3.SqrMagnitude(new Vector2(inBoxCenter.x - outerBoxRect.xMin, outerBoxRect.yMax - inBoxCenter.y));        //左上
        if (distVal < minDist) { minDist = distVal; targetCorner = CornerType.rightBottom; } //右下
        distVal = Vector3.SqrMagnitude(new Vector2(inBoxCenter.x - outerBoxRect.xMin, inBoxCenter.y - outerBoxRect.yMin));        //左下
        if (distVal < minDist) { minDist = distVal; targetCorner = CornerType.rightTop; } //右上

        Vector2 offsetDir = Vector2.zero;
        Vector2 usedPadding = Vector2.zero;
        Quaternion rotate = Quaternion.identity;
        switch (targetCorner)
        {
            case CornerType.rightBottom: //右下
                offsetDir = new Vector2(1, -1);
                usedPadding = new Vector2(padding.x, padding.y);
                rotate = Quaternion.Euler(0, 0, 180.0f);
                break;
            case CornerType.rightTop: //右上
                offsetDir = new Vector2(1, 1);
                usedPadding = new Vector2(padding.y, padding.x);
                rotate = Quaternion.Euler(0, 0, 270.0f);
                break;
            case CornerType.leftTop: //左上
                offsetDir = new Vector2(-1, 1);
                usedPadding = new Vector2(padding.x, padding.y);
                rotate = Quaternion.Euler(0, 0, 0.0f);
                break;
            case CornerType.leftBottom: //左下
                offsetDir = new Vector2(-1, -1);
                usedPadding = new Vector2(padding.y, padding.x);
                rotate = Quaternion.Euler(0, 0, 90.0f);
                break;            
        }
        Vector2 posIn2D = inBoxCenter + offsetDir * squareHalfWidth;
        Vector3 moveBoxScale = moveBox.lossyScale;
        moveBox.position = new Vector3(posIn2D.x + offsetDir.x * usedPadding.x * moveBoxScale.x, posIn2D.y + offsetDir.y * usedPadding.y * moveBoxScale.y, moveBox.position.z);
        if (allowRotate)
            moveBox.localRotation = rotate;
        return targetCorner;
    }

    #region Easing Curves
    delegate float EasingFunction(float start, float end, float Value);
    public enum EaseType
    {
        min,
        easeInQuad,
        easeOutQuad,
        easeInOutQuad,
        easeInCubic,
        easeOutCubic,
        easeInOutCubic,
        easeInQuart,
        easeOutQuart,
        easeInOutQuart,
        easeInQuint,
        easeOutQuint,
        easeInOutQuint,
        easeInSine,
        easeOutSine,
        easeInOutSine,
        easeInExpo,
        easeOutExpo,
        easeInOutExpo,
        easeInCirc,
        easeOutCirc,
        easeInOutCirc,
        linear,
        spring,
        easeInBounce,
        easeOutBounce,
        easeInOutBounce,
        easeInBack,
        easeOutBack,
        easeInOutBack,
        easeInElastic,
        easeOutElastic,
        easeInOutElastic,
        max,
    }

    //instantiates a cached ease equation refrence:
    public static float Curve(EaseType easeType, float start, float end, float value)
    {
        switch (easeType)
        {
            case EaseType.easeInQuad:return easeInQuad(start, end, value);
            case EaseType.easeOutQuad:return easeOutQuad(start, end, value);
            case EaseType.easeInOutQuad:return easeInOutQuad(start, end, value);
            case EaseType.easeInCubic:return easeInCubic(start, end, value);
            case EaseType.easeOutCubic:return easeOutCubic(start, end, value);
            case EaseType.easeInOutCubic:return easeInOutCubic(start, end, value);
            case EaseType.easeInQuart:return easeInQuart(start, end, value);
            case EaseType.easeOutQuart:return easeOutQuart(start, end, value);
            case EaseType.easeInOutQuart:return easeInOutQuart(start, end, value);
            case EaseType.easeInQuint:return easeInQuint(start, end, value);
            case EaseType.easeOutQuint:return easeOutQuint(start, end, value);
            case EaseType.easeInOutQuint:return easeInOutQuint(start, end, value);
            case EaseType.easeInSine:return easeInSine(start, end, value);
            case EaseType.easeOutSine:return easeOutSine(start, end, value);
            case EaseType.easeInOutSine:return easeInOutSine(start, end, value);
            case EaseType.easeInExpo:return easeInExpo(start, end, value);
            case EaseType.easeOutExpo:return easeOutExpo(start, end, value);
            case EaseType.easeInOutExpo:return easeInOutExpo(start, end, value);
            case EaseType.easeInCirc:return easeInCirc(start, end, value);
            case EaseType.easeOutCirc:return easeOutCirc(start, end, value);
            case EaseType.easeInOutCirc:return easeInOutCirc(start, end, value);
            case EaseType.linear:return linear(start, end, value);
            case EaseType.spring:return spring(start, end, value);
            case EaseType.easeInBounce:return easeInBounce(start, end, value);
            case EaseType.easeOutBounce:return easeOutBounce(start, end, value);
            case EaseType.easeInOutBounce:return easeInOutBounce(start, end, value);
            case EaseType.easeInBack:return easeInBack(start, end, value);
            case EaseType.easeOutBack:return easeOutBack(start, end, value);
            case EaseType.easeInOutBack:return easeInOutBack(start, end, value);
            case EaseType.easeInElastic:return easeInElastic(start, end, value);
            case EaseType.easeOutElastic:return easeOutElastic(start, end, value);
            case EaseType.easeInOutElastic:return easeInOutElastic(start, end, value);
            default:Debuger.LogError("未知的类型:{0}",easeType);return end;
        }
    }

    public static float linear(float start, float end, float value)
    {
        return Mathf.Lerp(start, end, value);
    }

    public static float clerp(float start, float end, float value)
    {
        float min = 0.0f;
        float max = 360.0f;
        float half = Mathf.Abs((max - min) * 0.5f);
        float retval = 0.0f;
        float diff = 0.0f;
        if ((end - start) < -half)
        {
            diff = ((max - start) + end) * value;
            retval = start + diff;
        }
        else if ((end - start) > half)
        {
            diff = -((max - end) + start) * value;
            retval = start + diff;
        }
        else retval = start + (end - start) * value;
        return retval;
    }

    public static float spring(float start, float end, float value)
    {
        value = Mathf.Clamp01(value);
        value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
        return start + (end - start) * value;
    }

    public static float easeInQuad(float start, float end, float value)
    {
        end -= start;
        return end * value * value + start;
    }

    public static float easeOutQuad(float start, float end, float value)
    {
        end -= start;
        return -end * value * (value - 2) + start;
    }

    public static float easeInOutQuad(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * value * value + start;
        value--;
        return -end * 0.5f * (value * (value - 2) - 1) + start;
    }

    public static float easeInCubic(float start, float end, float value)
    {
        end -= start;
        return end * value * value * value + start;
    }

    public static float easeOutCubic(float start, float end, float value)
    {
        value--;
        end -= start;
        return end * (value * value * value + 1) + start;
    }

    public static float easeInOutCubic(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * value * value * value + start;
        value -= 2;
        return end * 0.5f * (value * value * value + 2) + start;
    }

    public static float easeInQuart(float start, float end, float value)
    {
        end -= start;
        return end * value * value * value * value + start;
    }

    public static float easeOutQuart(float start, float end, float value)
    {
        value--;
        end -= start;
        return -end * (value * value * value * value - 1) + start;
    }

    public static float easeInOutQuart(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * value * value * value * value + start;
        value -= 2;
        return -end * 0.5f * (value * value * value * value - 2) + start;
    }

    public static float easeInQuint(float start, float end, float value)
    {
        end -= start;
        return end * value * value * value * value * value + start;
    }

    public static float easeOutQuint(float start, float end, float value)
    {
        value--;
        end -= start;
        return end * (value * value * value * value * value + 1) + start;
    }

    public static float easeInOutQuint(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * value * value * value * value * value + start;
        value -= 2;
        return end * 0.5f * (value * value * value * value * value + 2) + start;
    }

    public static float easeInSine(float start, float end, float value)
    {
        end -= start;
        return -end * Mathf.Cos(value * (Mathf.PI * 0.5f)) + end + start;
    }

    public static float easeOutSine(float start, float end, float value)
    {
        end -= start;
        return end * Mathf.Sin(value * (Mathf.PI * 0.5f)) + start;
    }

    public static float easeInOutSine(float start, float end, float value)
    {
        end -= start;
        return -end * 0.5f * (Mathf.Cos(Mathf.PI * value) - 1) + start;
    }

    public static float easeInExpo(float start, float end, float value)
    {
        end -= start;
        return end * Mathf.Pow(2, 10 * (value - 1)) + start;
    }

    public static float easeOutExpo(float start, float end, float value)
    {
        end -= start;
        return end * (-Mathf.Pow(2, -10 * value) + 1) + start;
    }

    public static float easeInOutExpo(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * Mathf.Pow(2, 10 * (value - 1)) + start;
        value--;
        return end * 0.5f * (-Mathf.Pow(2, -10 * value) + 2) + start;
    }

    public static float easeInCirc(float start, float end, float value)
    {
        end -= start;
        return -end * (Mathf.Sqrt(1 - value * value) - 1) + start;
    }

    public static float easeOutCirc(float start, float end, float value)
    {
        value--;
        end -= start;
        return end * Mathf.Sqrt(1 - value * value) + start;
    }

    public static float easeInOutCirc(float start, float end, float value)
    {
        value /= .5f;
        end -= start;
        if (value < 1) return -end * 0.5f * (Mathf.Sqrt(1 - value * value) - 1) + start;
        value -= 2;
        return end * 0.5f * (Mathf.Sqrt(1 - value * value) + 1) + start;
    }

    /* GFX47 MOD START */
    public static float easeInBounce(float start, float end, float value)
    {
        end -= start;
        float d = 1f;
        return end - easeOutBounce(0, end, d - value) + start;
    }
    /* GFX47 MOD END */

    /* GFX47 MOD START */
    //public static float bounce(float start, float end, float value){
    public static float easeOutBounce(float start, float end, float value)
    {
        value /= 1f;
        end -= start;
        if (value < (1 / 2.75f))
        {
            return end * (7.5625f * value * value) + start;
        }
        else if (value < (2 / 2.75f))
        {
            value -= (1.5f / 2.75f);
            return end * (7.5625f * (value) * value + .75f) + start;
        }
        else if (value < (2.5 / 2.75))
        {
            value -= (2.25f / 2.75f);
            return end * (7.5625f * (value) * value + .9375f) + start;
        }
        else
        {
            value -= (2.625f / 2.75f);
            return end * (7.5625f * (value) * value + .984375f) + start;
        }
    }
    /* GFX47 MOD END */

    /* GFX47 MOD START */
    public static float easeInOutBounce(float start, float end, float value)
    {
        end -= start;
        float d = 1f;
        if (value < d * 0.5f) return easeInBounce(0, end, value * 2) * 0.5f + start;
        else return easeOutBounce(0, end, value * 2 - d) * 0.5f + end * 0.5f + start;
    }
    /* GFX47 MOD END */

    public static float easeInBack(float start, float end, float value)
    {
        end -= start;
        value /= 1;
        float s = 1.70158f;
        return end * (value) * value * ((s + 1) * value - s) + start;
    }

    public static float easeOutBack(float start, float end, float value)
    {
        float s = 1.70158f;
        end -= start;
        value = (value) - 1;
        return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
    }

    public static float easeInOutBack(float start, float end, float value)
    {
        float s = 1.70158f;
        end -= start;
        value /= .5f;
        if ((value) < 1)
        {
            s *= (1.525f);
            return end * 0.5f * (value * value * (((s) + 1) * value - s)) + start;
        }
        value -= 2;
        s *= (1.525f);
        return end * 0.5f * ((value) * value * (((s) + 1) * value + s) + 2) + start;
    }
    
    /* GFX47 MOD START */
    public static float easeInElastic(float start, float end, float value)
    {
        end -= start;

        float d = 1f;
        float p = d * .3f;
        float s = 0;
        float a = 0;

        if (value == 0) return start;

        if ((value /= d) == 1) return start + end;

        if (a == 0f || a < Mathf.Abs(end))
        {
            a = end;
            s = p / 4;
        }
        else
        {
            s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
        }

        return -(a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
    }
    /* GFX47 MOD END */

    /* GFX47 MOD START */
    //public static float elastic(float start, float end, float value){
    public static float easeOutElastic(float start, float end, float value)
    {
        /* GFX47 MOD END */
        //Thank you to rafael.marteleto for fixing this as a port over from Pedro's UnityTween
        end -= start;

        float d = 1f;
        float p = d * .3f;
        float s = 0;
        float a = 0;

        if (value == 0) return start;

        if ((value /= d) == 1) return start + end;

        if (a == 0f || a < Mathf.Abs(end))
        {
            a = end;
            s = p * 0.25f;
        }
        else
        {
            s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
        }

        return (a * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) + end + start);
    }

    /* GFX47 MOD START */
    public static float easeInOutElastic(float start, float end, float value)
    {
        end -= start;

        float d = 1f;
        float p = d * .3f;
        float s = 0;
        float a = 0;

        if (value == 0) return start;

        if ((value /= d * 0.5f) == 2) return start + end;

        if (a == 0f || a < Mathf.Abs(end))
        {
            a = end;
            s = p / 4;
        }
        else
        {
            s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
        }

        if (value < 1) return -0.5f * (a * Mathf.Pow(2, 10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
        return a * Mathf.Pow(2, -10 * (value -= 1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
    }
    /* GFX47 MOD END */

    #endregion
}