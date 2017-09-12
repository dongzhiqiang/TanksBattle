using UnityEngine;
using UnityEngine.UI;
using System.Collections;

class UIScrollTips : MonoBehaviour
{
    public GameObject leftOrTop;
    public GameObject rightOrDown;

    ScrollRect mScrollView;
    RectTransform mScrollRect;
    RectTransform mContentRect;

    void Awake()
    {
        mScrollView = this.GetComponent<ScrollRect>();
        if (mScrollView == null)
        {
            Debug.LogError("UIScrollTips脚本没有在ScrollRect上");
            return;
        }

        mScrollRect = mScrollView.GetComponent<RectTransform>();
        mContentRect = mScrollView.content;
    }
    void Update()
    {
        if (mScrollView == null) return;

        //水平滚动
        if (mScrollView.horizontal)
        {
            if (leftOrTop != null)
            {
                if (mContentRect.offsetMin.x < -1)
                    leftOrTop.gameObject.SetActive(true);
                else
                    leftOrTop.gameObject.SetActive(false);
            }

            if (rightOrDown != null)
            {
                if (mContentRect.offsetMax.x > mScrollRect.sizeDelta.x + 1)
                    rightOrDown.gameObject.SetActive(true);
                else
                    rightOrDown.gameObject.SetActive(false);
            }
        }


        //竖直滚动
        if (mScrollView.vertical)
        {
            if (leftOrTop != null)
            {
                if (mContentRect.offsetMax.y > 1)
                    leftOrTop.gameObject.SetActive(true);
                else
                    leftOrTop.gameObject.SetActive(false);
            }

            if (rightOrDown != null)
            {
                if (Mathf.Abs(mContentRect.offsetMin.y) > mScrollRect.sizeDelta.y + 1)
                    rightOrDown.gameObject.SetActive(true);
                else
                    rightOrDown.gameObject.SetActive(false);
            }
        }
    }

    public static void ScrollPos(ScrollRect scrollView, int index)
    {
        if (scrollView == null || scrollView.content == null)
            return;

        int childCnt = scrollView.content.childCount;
        if (childCnt <= 0)
            return;
        //下面代码有效的前提是：不考虑item和content缩放、旋转，item的锚点一定是在content的左上角
        index = Mathf.Clamp(index, 0, childCnt - 1);
        RectTransform curItem = (RectTransform)scrollView.content.GetChild(index);
        ScrollPos(scrollView, curItem);
    }

    public static void ScrollPos(ScrollRect scrollView, RectTransform curItem)
    {
        if (scrollView == null || scrollView.content == null || curItem == null)
            return;

        //判断要显示的Item是否scrollView.content的子孙节点
        var directChild = true;
        var tempItem = (Transform)curItem;
        do
        {
            tempItem = tempItem.parent;
            if (tempItem == scrollView.content)
                break;
            else if (tempItem == null)
                return;
            directChild = false;
        } while (true);

        Vector2 itemPivot = curItem.pivot; //0~1，左下角到右上角
        Vector2 itemPos = curItem.anchoredPosition; //幸好Layout Group会把item的锚点设置到左上角，所以这个值正好是item的pivot点相对于父节点左上角的偏移
        //如果不是content的直接子节点，要把anchoredPosition转换到content空间下
        if (!directChild)
        {
            var temp = curItem.parent.TransformPoint(curItem.anchoredPosition);
            itemPos = scrollView.content.InverseTransformPoint(temp);
        }
        Rect itemRect = curItem.rect; //这里我们只用它的width和height，x,y是左下角相对于pivot的偏移        
        Rect contentRect = scrollView.content.rect; //这里获取父节点的width和height
        Rect viewportRect = scrollView.viewport == null ? ((RectTransform)scrollView.transform).rect : scrollView.viewport.rect; //这里获取视口的width和height，一般是父节点的父节点

        if (scrollView.horizontal)
        {
            float rate = (itemPos.x - itemPivot.x * itemRect.width + itemRect.width / 2 - viewportRect.width / 2) / (contentRect.width - viewportRect.width);
            rate = Mathf.Clamp01(rate);
            scrollView.horizontalNormalizedPosition = rate;
        }

        if (scrollView.vertical)
        {
            //itemPos.y一般是负数，contentRect.height + itemPos.y就是item的pivot相对于父节点下边线的距离，就是求出item在父节点坐标系的Y坐标值（Y从下向上增长）
            float rate = (contentRect.height + itemPos.y - itemPivot.y * itemRect.height + itemRect.height / 2 - viewportRect.height / 2) / (contentRect.height - viewportRect.height);
            rate = Mathf.Clamp01(rate);
            scrollView.verticalNormalizedPosition = rate;
        }
    }
}
