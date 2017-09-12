using UnityEngine;
using UnityEditor;
using System.Collections;

public class EditorStyleEx
{


    private static GUISkin skin;
    public static GUISkin mSkin
    {
        get
        {
            if (skin == null)
            {
                var skinPath = "Assets/ThridPart/AstarPathfindingProject/Editor/EditorAssets/AstarEditorSkinDark.guiskin";
                skin = AssetDatabase.LoadAssetAtPath(skinPath, typeof(GUISkin)) as GUISkin;
            }
            return skin;
        }
    }

    private static GUIStyle GetStyle(string styleName)
    {
        GUIStyle error = GUI.skin.FindStyle(styleName);
        if (error == null)
        {
            error = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
        }
        if (error == null)
        {
            Debug.LogError("Missing built-in guistyle " + styleName);
        }
        return error;
    }

    private static GUIStyle graphDeleteButtonStyle;
    public static GUIStyle GraphDeleteButtonStyle
    {
        get
        {
            if (graphDeleteButtonStyle == null)
            {
                graphDeleteButtonStyle = mSkin.FindStyle("PixelButton");
            }
            return graphDeleteButtonStyle;
        }
    }
    private static GUIStyle graphInfoButtonStyle;
    public static GUIStyle GraphInfoButtonStyle
    {
        get
        {
            if (graphInfoButtonStyle == null)
            {
                graphInfoButtonStyle = mSkin.FindStyle("InfoButton");
            }
            return graphInfoButtonStyle;
        }
    }
    private static GUIStyle graphGizmoButtonStyle;
    public static GUIStyle GraphGizmoButtonStyle
    {
        get
        {
            if (graphGizmoButtonStyle == null)
            {
                graphGizmoButtonStyle = mSkin.FindStyle("GizmoButton");
            }
            return graphGizmoButtonStyle;
        }
    }
    private static GUIStyle boxStyle;
    public static GUIStyle BoxStyle
    {
        get
        {
            if (boxStyle == null)
            {
                boxStyle = mSkin.FindStyle("box");
            }
            return boxStyle;
        }
    }

    private static GUIStyle graphBoxStyle;
    public static GUIStyle GraphBoxStyle
    {
        get
        {
            if (graphBoxStyle == null)
            {
                graphBoxStyle = mSkin.FindStyle("PixelBox3");
            }
            return graphBoxStyle;
        }
    }

    
    private static GUIStyle buttonStyle;
    public static GUIStyle ButtonStyle
    {
        get
        {
            if (buttonStyle == null)
            {
                buttonStyle = mSkin.FindStyle("button");
            }
            return buttonStyle;
        }
    }
    private static GUIStyle toggleStyle;
    public static GUIStyle ToggleStyle
    {
        get
        {
            if (toggleStyle == null)
            {
                toggleStyle = mSkin.FindStyle("toggle");
            }
            return toggleStyle;
        }
    }
    private static GUIStyle labelStyle;
    public static GUIStyle LabelStyle
    {
        get
        {
            if (labelStyle == null)
            {
                labelStyle = mSkin.FindStyle("label");
            }
            return labelStyle;
        }
    }
    private static GUIStyle textfieldStyle;
    public static GUIStyle TextfieldStyle
    {
        get
        {
            if (textfieldStyle == null)
            {
                textfieldStyle = mSkin.FindStyle("textfield");
            }
            return textfieldStyle;
        }
    }
    private static GUIStyle textareaStyle;
    public static GUIStyle TextareaStyle
    {
        get
        {
            if (textareaStyle == null)
            {
                textareaStyle = mSkin.FindStyle("textarea");
            }
            return textareaStyle;
        }
    }
    private static GUIStyle horizontalsliderStyle;
    public static GUIStyle HorizontalsliderStyle
    {
        get
        {
            if (horizontalsliderStyle == null)
            {
                horizontalsliderStyle = mSkin.FindStyle("horizontalslider");
            }
            return horizontalsliderStyle;
        }
    }
    private static GUIStyle horizontalsliderthumbStyle;
    public static GUIStyle HorizontalsliderthumbStyle
    {
        get
        {
            if (horizontalsliderthumbStyle == null)
            {
                horizontalsliderthumbStyle = mSkin.FindStyle("horizontalsliderthumb");
            }
            return horizontalsliderthumbStyle;
        }
    }
    private static GUIStyle verticalsliderStyle;
    public static GUIStyle VerticalsliderStyle
    {
        get
        {
            if (verticalsliderStyle == null)
            {
                verticalsliderStyle = mSkin.FindStyle("verticalslider");
            }
            return verticalsliderStyle;
        }
    }
    private static GUIStyle horizontalscrollbarStyle;
    public static GUIStyle HorizontalscrollbarStyle
    {
        get
        {
            if (horizontalscrollbarStyle == null)
            {
                horizontalscrollbarStyle = mSkin.FindStyle("horizontalscrollbar");
            }
            return horizontalscrollbarStyle;
        }
    }
    private static GUIStyle verticalscrollbarStyle;
    public static GUIStyle VerticalscrollbarStyle
    {
        get
        {
            if (verticalscrollbarStyle == null)
            {
                verticalscrollbarStyle = mSkin.FindStyle("verticalscrollbar");
            }
            return VerticalscrollbarStyle;
        }
    }

    private static GUIStyle horizontalscrollbarthumbStyle;
    public static GUIStyle HorizontalscrollbarthumbStyle
    {
        get
        {
            if (horizontalscrollbarthumbStyle == null)
            {
                horizontalscrollbarthumbStyle = mSkin.FindStyle("horizontalscrollbarthumb");
            }
            return horizontalscrollbarthumbStyle;
        }
    }
    private static GUIStyle horizontalscrollbarleftbuttonStyle;
    public static GUIStyle HorizontalscrollbarleftbuttonStyle
    {
        get
        {
            if (horizontalscrollbarleftbuttonStyle == null)
            {
                horizontalscrollbarleftbuttonStyle = mSkin.FindStyle("horizontalscrollbarleftbutton");
            }
            return horizontalscrollbarleftbuttonStyle;
        }
    }
    private static GUIStyle pixelBoxStyle;
    public static GUIStyle PixelBoxStyle
    {
        get
        {
            if (pixelBoxStyle == null)
            {
                pixelBoxStyle = mSkin.FindStyle("PixelBox");
            }
            return pixelBoxStyle;
        }
    }
    private static GUIStyle colorInterpolationBoxStyle;
    public static GUIStyle ColorInterpolationBoxStyle
    {
        get
        {
            if (colorInterpolationBoxStyle == null)
            {
                colorInterpolationBoxStyle = mSkin.FindStyle("ColorInterpolationBox");
            }
            return colorInterpolationBoxStyle;
        }
    }
    private static GUIStyle stretchWidthStyle;
    public static GUIStyle StretchWidthStyle
    {
        get
        {
            if (stretchWidthStyle == null)
            {
                stretchWidthStyle = mSkin.FindStyle("StretchWidth");
            }
            return stretchWidthStyle;
        }
    }
    private static GUIStyle boxHeaderStyle;
    public static GUIStyle BoxHeaderStyle
    {
        get
        {
            if (boxHeaderStyle == null)
            {
                boxHeaderStyle = mSkin.FindStyle("BoxHeader");
            }
            return boxHeaderStyle;
        }
    }
    private static GUIStyle topBoxHeaderStyle;
    public static GUIStyle TopBoxHeaderStyle
    {
        get
        {
            if (topBoxHeaderStyle == null)
            {
                topBoxHeaderStyle = mSkin.FindStyle("TopBoxHeader");
            }
            return topBoxHeaderStyle;
        }
    }
    private static GUIStyle pixelBox3Style;
    public static GUIStyle PixelBox3Style
    {
        get
        {
            if (pixelBox3Style == null)
            {
                pixelBox3Style = mSkin.FindStyle("PixelBox3");
            }
            return pixelBox3Style;
        }
    }
    private static GUIStyle pixelButtonStyle;
    public static GUIStyle PixelButtonStyle
    {
        get
        {
            if (pixelButtonStyle == null)
            {
                pixelButtonStyle = mSkin.FindStyle("PixelButton");
            }
            return pixelButtonStyle;
        }
    }
    private static GUIStyle linkButtonStyle;
    public static GUIStyle LinkButtonStyle
    {
        get
        {
            if (linkButtonStyle == null)
            {
                linkButtonStyle = mSkin.FindStyle("LinkButton");
            }
            return linkButtonStyle;
        }
    }
    private static GUIStyle closeButtonStyle;
    public static GUIStyle CloseButtonStyle
    {
        get
        {
            if (closeButtonStyle == null)
            {
                closeButtonStyle = mSkin.FindStyle("CloseButton");
            }
            return closeButtonStyle;
        }
    }
    private static GUIStyle gridPivotSelectButtonStyle;
    public static GUIStyle GridPivotSelectButtonStyle
    {
        get
        {
            if (gridPivotSelectButtonStyle == null)
            {
                gridPivotSelectButtonStyle = mSkin.FindStyle("GridPivotSelectButton");
            }
            return gridPivotSelectButtonStyle;
        }
    }
    private static GUIStyle gridPivotSelectBackgroundStyle;
    public static GUIStyle GridPivotSelectBackgroundStyle
    {
        get
        {
            if (gridPivotSelectBackgroundStyle == null)
            {
                gridPivotSelectBackgroundStyle = mSkin.FindStyle("GridPivotSelectBackground");
            }
            return gridPivotSelectBackgroundStyle;
        }
    }
    private static GUIStyle collisionHeaderStyle;
    public static GUIStyle CollisionHeaderStyle
    {
        get
        {
            if (collisionHeaderStyle == null)
            {
                collisionHeaderStyle = mSkin.FindStyle("CollisionHeader");
            }
            return collisionHeaderStyle;
        }
    }
    private static GUIStyle infoButtonStyle;
    public static GUIStyle InfoButtonStyle
    {
        get
        {
            if (infoButtonStyle == null)
            {
                infoButtonStyle = mSkin.FindStyle("InfoButton");
            }
            return infoButtonStyle;
        }
    }
    private static GUIStyle pixelBox3SeparatorStyle;
    public static GUIStyle PixelBox3SeparatorStyle
    {
        get
        {
            if (pixelBox3SeparatorStyle == null)
            {
                pixelBox3SeparatorStyle = mSkin.FindStyle("PixelBox3Separator");
            }
            return pixelBox3SeparatorStyle;
        }
    }
    private static GUIStyle gridSizeLockStyle;
    public static GUIStyle GridSizeLockStyle
    {
        get
        {
            if (gridSizeLockStyle == null)
            {
                gridSizeLockStyle = mSkin.FindStyle("GridSizeLock");
            }
            return gridSizeLockStyle;
        }
    }
    private static GUIStyle upArrowStyle;
    public static GUIStyle UpArrowStyle
    {
        get
        {
            if (upArrowStyle == null)
            {
                upArrowStyle = mSkin.FindStyle("UpArrow");
            }
            return upArrowStyle;
        }
    }
    private static GUIStyle downArrowStyle;
    public static GUIStyle DownArrowStyle
    {
        get
        {
            if (downArrowStyle == null)
            {
                downArrowStyle = mSkin.FindStyle("DownArrow");
            }
            return downArrowStyle;
        }
    }
    private static GUIStyle smallResetStyle;
    public static GUIStyle SmallResetStyle
    {
        get
        {
            if (smallResetStyle == null)
            {
                smallResetStyle = mSkin.FindStyle("SmallReset");
            }
            return smallResetStyle;
        }
    }
    private static GUIStyle gizmoButtonStyle;
    public static GUIStyle GizmoButtonStyle
    {
        get
        {
            if (gizmoButtonStyle == null)
            {
                gizmoButtonStyle = mSkin.FindStyle("GizmoButton");
            }
            return gizmoButtonStyle;
        }
    }


    private static GUIStyle helpBox;
    public static GUIStyle HelpBox
    {
        get
        {
            if (helpBox == null)
            {
                helpBox = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("HelpBox");
            }
            return helpBox;
        }
    }


    private static GUIStyle thinHelpBox;
    public static GUIStyle ThinHelpBox
    {
        get
        {
            if (thinHelpBox == null)
            {
                thinHelpBox = new GUIStyle(HelpBox);
                thinHelpBox.contentOffset = new Vector2(0, -2);
                thinHelpBox.stretchWidth = false;
                thinHelpBox.clipping = TextClipping.Overflow;
                thinHelpBox.overflow.top += 1;
            }
            return thinHelpBox;
        }
    }


    private static GUIStyle upArrow;
    public static GUIStyle UpArrow 
    {
        get
        {
            if (upArrow == null)
            {
                upArrow = mSkin.FindStyle("UpArrow");
            }
            return upArrow;
        }
    }

    private static GUIStyle downArrow;
    public static GUIStyle DownArrow
    {
        get
        {
            if (downArrow == null)
            {
                downArrow = mSkin.FindStyle("DownArrow");
            }
            return downArrow;
        }
    }

    private static GUIStyle labelWordWrap;
    public static GUIStyle LabelWordWrap
    {
        get
        {
            if (labelWordWrap == null)
            {
                labelWordWrap = new GUIStyle(EditorStyles.label);
                labelWordWrap.wordWrap = true;
            }
            return labelWordWrap;
        }
    }

    private static GUIStyle textAreaWordWrap;
    public static GUIStyle TextAreaWordWrap
    {
        get
        {
            if (textAreaWordWrap == null)
            {
                textAreaWordWrap = new GUIStyle(EditorStyles.textArea);
                textAreaWordWrap.wordWrap = true;
            }
            return textAreaWordWrap;
        }
    }

    private static GUIStyle toolBarButtonEx;
    public static GUIStyle ToolBarButtonEx
    {
        get
        {
            if (toolBarButtonEx == null)
            {
                toolBarButtonEx = new GUIStyle(EditorStyles.toolbarButton);
                toolBarButtonEx.alignment = TextAnchor.MiddleCenter;
                toolBarButtonEx.padding = new RectOffset(0, 0, 0, 0);
                toolBarButtonEx.fontSize = 12;
            }
            return toolBarButtonEx;
        }
    }

    private static GUIStyle miniButtonEx;
    public static GUIStyle MiniButtonEx
    {
        get
        {
            if (miniButtonEx == null)
            {
                miniButtonEx = new GUIStyle(EditorStyles.miniButton);
                miniButtonEx.alignment = TextAnchor.MiddleCenter;
                miniButtonEx.padding = new RectOffset(0, 0, 0, 0);
            }
            return miniButtonEx;
        }
    }

    private static GUIStyle olTitleAlignCenter;
    public static GUIStyle OLTitleAlignCenter
    {
        get
        {
            if (olTitleAlignCenter == null)
            {
                olTitleAlignCenter = new GUIStyle(GetStyle("OL title"));
                olTitleAlignCenter.alignment = TextAnchor.MiddleCenter;
                olTitleAlignCenter.padding = new RectOffset(0, 0, 0, 0);
            }
            return olTitleAlignCenter;
        }
    }

    private static GUIStyle toolbarPopupEx;
    public static GUIStyle ToolbarPopupEx
    {
        get
        {
            if (toolbarPopupEx == null)
            {
                toolbarPopupEx = new GUIStyle(EditorStyles.toolbarPopup);
                toolbarPopupEx.alignment = TextAnchor.MiddleCenter;
                toolbarPopupEx.padding = new RectOffset(0, 0, 0, 0);
                toolbarPopupEx.fontSize = 12;
            }
            return toolbarPopupEx;
        }
    }

    private static Texture timelineMarker;
    public static Texture TimelineMarker
    {
        get
        {
            if (timelineMarker == null)
                timelineMarker = Resources.Load("TimelineMarker") as Texture;
            return timelineMarker;
        }
        set {; }
    }


    private static Texture timelineScrubHead;
    public static Texture TimelineScrubHead
    {
        get
        {
            if (timelineScrubHead == null)
                timelineScrubHead = Resources.Load("TimelineScrubHead") as Texture;
            return timelineScrubHead;
        }
        set {; }
    }


    private static Texture timelineScrubTail;
    public static Texture TimelineScrubTail
    {
        get
        {
            if (timelineScrubTail == null)
                timelineScrubTail = Resources.Load("TimelineScrubTail") as Texture;
            return timelineScrubTail;
        }
        set {; }
    }

    private static GUIStyle labelExRight;
    public static GUIStyle LabelExRight
    {
        get
        {
            if (labelExRight == null)
            {
                labelExRight = new GUIStyle(EditorStyles.label);
                labelExRight.alignment = TextAnchor.MiddleRight;
                labelExRight.padding = new RectOffset(5, 5, 2, 2);
                labelExRight.margin = new RectOffset(0, 0, 0, 0);
            }
            return labelExRight;
        }
    }

    private static GUIStyle labelExLeft;
    public static GUIStyle LabelExLeft
    {
        get
        {
            if (labelExLeft == null)
            {
                labelExLeft = new GUIStyle(EditorStyles.label);
                labelExLeft.alignment = TextAnchor.MiddleLeft;
                labelExLeft.padding = new RectOffset(5, 5, 2, 2);
                labelExLeft.margin = new RectOffset(0, 0, 0, 0);
            }
            return labelExLeft;
        }
    }

    private static GUIStyle labelExLeftItalic;
    public static GUIStyle LabelExLeftItalic
    {
        get
        {
            if (labelExLeftItalic == null)
            {
                labelExLeftItalic = new GUIStyle(EditorStyles.label);
                labelExLeftItalic.alignment = TextAnchor.MiddleLeft;
                labelExLeftItalic.padding = new RectOffset(5, 5, 2, 2);
                labelExLeftItalic.margin = new RectOffset(0, 0, 0, 0);
                labelExLeftItalic.fontStyle = FontStyle.Italic;
            }
            return labelExLeftItalic;
        }
    }
}
