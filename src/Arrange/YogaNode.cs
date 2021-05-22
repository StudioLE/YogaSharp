/*
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Arrange
{
    public partial class YogaNode : IEnumerable<YogaNode>
    {
        private readonly YGNodeHandle _ygNode;
        private readonly YogaConfig _config;
        private WeakReference _parent;
        private List<YogaNode> _children;
        private MeasureFunction _measureFunction;
        private BaselineFunction _baselineFunction;
        private YogaMeasureFunc _managedMeasure;
        private YogaBaselineFunc _managedBaseline;
        private object _data;

        public YogaNode(YogaConfig config = null)
        {
            this._config = config == null ? YogaConfig.Default : config;
            this._ygNode = Native.YGNodeNewWithConfig(this._config.Handle);
            if (this._ygNode.IsInvalid)
            {
                throw new InvalidOperationException("Failed to allocate native memory");
            }

            this._ygNode.SetContext(this);
        }

        public YogaNode(YogaNode srcNode)
            : this(srcNode._config)
        {
            CopyStyle(srcNode);
        }

        public void Reset()
        {
            this._measureFunction = null;
            this._baselineFunction = null;
            this._data = null;

            this._ygNode.ReleaseManaged();
            Native.YGNodeReset(this._ygNode);
            this._ygNode.SetContext(this);
        }

        public bool IsDirty
        {
            get
            {
                return Native.YGNodeIsDirty(this._ygNode);
            }
        }

        public virtual void MarkDirty()
        {
            Native.YGNodeMarkDirty(this._ygNode);
        }

        public bool HasNewLayout
        {
            get
            {
                return Native.YGNodeGetHasNewLayout(this._ygNode);
            }
        }

        public void MarkHasNewLayout()
        {
            Native.YGNodeSetHasNewLayout(this._ygNode, true);
        }

        public YogaNode Parent
        {
            get
            {
                return this._parent != null ? this._parent.Target as YogaNode : null;
            }
        }

        public bool IsMeasureDefined
        {
            get
            {
                return this._measureFunction != null;
            }
        }

        public bool IsBaselineDefined
        {
            get
            {
                return this._baselineFunction != null;
            }
        }

        public void CopyStyle(YogaNode srcNode)
        {
            Native.YGNodeCopyStyle(this._ygNode, srcNode._ygNode);
        }

        public YogaDirection StyleDirection
        {
            get
            {
                return Native.YGNodeStyleGetDirection(this._ygNode);
            }

            set
            {
                Native.YGNodeStyleSetDirection(this._ygNode, value);
            }
        }

        public YogaFlexDirection FlexDirection
        {
            get
            {
                return Native.YGNodeStyleGetFlexDirection(this._ygNode);
            }

            set
            {
                Native.YGNodeStyleSetFlexDirection(this._ygNode, value);
            }
        }

        public YogaJustify JustifyContent
        {
            get
            {
                return Native.YGNodeStyleGetJustifyContent(this._ygNode);
            }

            set
            {
                Native.YGNodeStyleSetJustifyContent(this._ygNode, value);
            }
        }

        public YogaDisplay Display
        {
            get
            {
                return Native.YGNodeStyleGetDisplay(this._ygNode);
            }

            set
            {
                Native.YGNodeStyleSetDisplay(this._ygNode, value);
            }
        }

        public YogaAlign AlignItems
        {
            get
            {
                return Native.YGNodeStyleGetAlignItems(this._ygNode);
            }

            set
            {
                Native.YGNodeStyleSetAlignItems(this._ygNode, value);
            }
        }

        public YogaAlign AlignSelf
        {
            get
            {
                return Native.YGNodeStyleGetAlignSelf(this._ygNode);
            }

            set
            {
                Native.YGNodeStyleSetAlignSelf(this._ygNode, value);
            }
        }

        public YogaAlign AlignContent
        {
            get
            {
                return Native.YGNodeStyleGetAlignContent(this._ygNode);
            }

            set
            {
                Native.YGNodeStyleSetAlignContent(this._ygNode, value);
            }
        }

        public YogaPositionType PositionType
        {
            get
            {
                return Native.YGNodeStyleGetPositionType(this._ygNode);
            }

            set
            {
                Native.YGNodeStyleSetPositionType(this._ygNode, value);
            }
        }

        public YogaWrap Wrap
        {
            get
            {
                return Native.YGNodeStyleGetFlexWrap(this._ygNode);
            }

            set
            {
                Native.YGNodeStyleSetFlexWrap(this._ygNode, value);
            }
        }

        public float Flex
        {
            set
            {
                Native.YGNodeStyleSetFlex(this._ygNode, value);
            }
        }

        public float FlexGrow
        {
            get
            {
                return Native.YGNodeStyleGetFlexGrow(this._ygNode);
            }

            set
            {
                Native.YGNodeStyleSetFlexGrow(this._ygNode, value);
            }
        }

        public float FlexShrink
        {
            get
            {
                return Native.YGNodeStyleGetFlexShrink(this._ygNode);
            }

            set
            {
                Native.YGNodeStyleSetFlexShrink(this._ygNode, value);
            }
        }

        public YogaValue FlexBasis
        {
            get
            {
                return Native.YGNodeStyleGetFlexBasis(this._ygNode);
            }

            set
            {
                if (value.Unit == YogaUnit.Percent)
                {
                    Native.YGNodeStyleSetFlexBasisPercent(this._ygNode, value.Value);
                }
                else if (value.Unit == YogaUnit.Auto)
                {
                    Native.YGNodeStyleSetFlexBasisAuto(this._ygNode);
                }
                else
                {
                    Native.YGNodeStyleSetFlexBasis(this._ygNode, value.Value);
                }
            }
        }

        public YogaValue Width
        {
            get
            {
                return Native.YGNodeStyleGetWidth(this._ygNode);
            }

            set
            {
                if (value.Unit == YogaUnit.Percent)
                {
                    Native.YGNodeStyleSetWidthPercent(this._ygNode, value.Value);
                }
                else if (value.Unit == YogaUnit.Auto)
                {
                    Native.YGNodeStyleSetWidthAuto(this._ygNode);
                }
                else
                {
                    Native.YGNodeStyleSetWidth(this._ygNode, value.Value);
                }
            }
        }

        public YogaValue Height
        {
            get
            {
                return Native.YGNodeStyleGetHeight(this._ygNode);
            }

            set
            {
                if (value.Unit == YogaUnit.Percent)
                {
                    Native.YGNodeStyleSetHeightPercent(this._ygNode, value.Value);
                }
                else if (value.Unit == YogaUnit.Auto)
                {
                    Native.YGNodeStyleSetHeightAuto(this._ygNode);
                }
                else
                {
                    Native.YGNodeStyleSetHeight(this._ygNode, value.Value);
                }
            }
        }

        public YogaValue MaxWidth
        {
            get
            {
                return Native.YGNodeStyleGetMaxWidth(this._ygNode);
            }

            set
            {
                if (value.Unit == YogaUnit.Percent)
                {
                    Native.YGNodeStyleSetMaxWidthPercent(this._ygNode, value.Value);
                }
                else
                {
                    Native.YGNodeStyleSetMaxWidth(this._ygNode, value.Value);
                }
            }
        }

        public YogaValue MaxHeight
        {
            get
            {
                return Native.YGNodeStyleGetMaxHeight(this._ygNode);
            }

            set
            {
                if (value.Unit == YogaUnit.Percent)
                {
                    Native.YGNodeStyleSetMaxHeightPercent(this._ygNode, value.Value);
                }
                else
                {
                    Native.YGNodeStyleSetMaxHeight(this._ygNode, value.Value);
                }
            }
        }

        public YogaValue MinWidth
        {
            get
            {
                return Native.YGNodeStyleGetMinWidth(this._ygNode);
            }

            set
            {
                if (value.Unit == YogaUnit.Percent)
                {
                    Native.YGNodeStyleSetMinWidthPercent(this._ygNode, value.Value);
                }
                else
                {
                    Native.YGNodeStyleSetMinWidth(this._ygNode, value.Value);
                }
            }
        }

        public YogaValue MinHeight
        {
            get
            {
                return Native.YGNodeStyleGetMinHeight(this._ygNode);
            }

            set
            {
                if (value.Unit == YogaUnit.Percent)
                {
                    Native.YGNodeStyleSetMinHeightPercent(this._ygNode, value.Value);
                }
                else
                {
                    Native.YGNodeStyleSetMinHeight(this._ygNode, value.Value);
                }
            }
        }

        public float AspectRatio
        {
            get
            {
                return Native.YGNodeStyleGetAspectRatio(this._ygNode);
            }

            set
            {
                Native.YGNodeStyleSetAspectRatio(this._ygNode, value);
            }
        }

        public float LayoutX
        {
            get
            {
                return Native.YGNodeLayoutGetLeft(this._ygNode);
            }
        }

        public float LayoutY
        {
            get
            {
                return Native.YGNodeLayoutGetTop(this._ygNode);
            }
        }

        public float LayoutWidth
        {
            get
            {
                return Native.YGNodeLayoutGetWidth(this._ygNode);
            }
        }

        public float LayoutHeight
        {
            get
            {
                return Native.YGNodeLayoutGetHeight(this._ygNode);
            }
        }

        public YogaDirection LayoutDirection
        {
            get
            {
                return Native.YGNodeLayoutGetDirection(this._ygNode);
            }
        }

        public YogaOverflow Overflow
        {
            get
            {
                return Native.YGNodeStyleGetOverflow(this._ygNode);
            }

            set
            {
                Native.YGNodeStyleSetOverflow(this._ygNode, value);
            }
        }

        public object Data
        {
            get
            {
                return this._data;
            }

            set
            {
                this._data = value;
            }
        }

        public YogaNode this[int index]
        {
            get
            {
                return this._children[index];
            }
        }

        public int Count
        {
            get
            {
                return this._children != null ? this._children.Count : 0;
            }
        }

        public void MarkLayoutSeen()
        {
            Native.YGNodeSetHasNewLayout(this._ygNode, false);
        }

        public bool IsReferenceBaseline
        {
            get
            {
                return Native.YGNodeIsReferenceBaseline(this._ygNode);
            }

            set
            {
                Native.YGNodeSetIsReferenceBaseline(this._ygNode, value);
            }
        }

        public bool ValuesEqual(float f1, float f2)
        {
            if (float.IsNaN(f1) || float.IsNaN(f2))
            {
                return float.IsNaN(f1) && float.IsNaN(f2);
            }

            return Math.Abs(f2 - f1) < float.Epsilon;
        }

        public void Insert(int index, YogaNode node)
        {
            if (this._children == null)
            {
                this._children = new List<YogaNode>(4);
            }
            this._children.Insert(index, node);
            node._parent = new WeakReference(this);
            Native.YGNodeInsertChild(this._ygNode, node._ygNode, (uint)index);
        }

        public void RemoveAt(int index)
        {
            var child = this._children[index];
            child._parent = null;
            this._children.RemoveAt(index);
            Native.YGNodeRemoveChild(this._ygNode, child._ygNode);
        }

        public void AddChild(YogaNode child)
        {
            Insert(this.Count, child);
        }

        public void RemoveChild(YogaNode child)
        {
            int index = IndexOf(child);
            if (index >= 0)
            {
                RemoveAt(index);
            }
        }

        public void Clear()
        {
            if (this._children != null)
            {
                while (this._children.Count > 0)
                {
                    RemoveAt(this._children.Count-1);
                }
            }
        }

        public int IndexOf(YogaNode node)
        {
            return this._children != null ? this._children.IndexOf(node) : -1;
        }

        public void SetMeasureFunction(MeasureFunction measureFunction)
        {
            this._measureFunction = measureFunction;
            this._managedMeasure = measureFunction != null ? MeasureInternal : (YogaMeasureFunc)null;
            Native.YGNodeSetMeasureFunc(this._ygNode, this._managedMeasure);
        }

        public void SetBaselineFunction(BaselineFunction baselineFunction)
        {
            this._baselineFunction = baselineFunction;
            this._managedBaseline =
                baselineFunction != null ? BaselineInternal : (YogaBaselineFunc)null;
            Native.YGNodeSetBaselineFunc(this._ygNode, this._managedBaseline);
        }

        public void CalculateLayout(
            float width = YogaConstants.Undefined,
            float height = YogaConstants.Undefined)
        {
            Native.YGNodeCalculateLayout(
                this._ygNode,
                width,
                height,
                Native.YGNodeStyleGetDirection(this._ygNode));
        }

        private static YogaSize MeasureInternal(
            IntPtr unmanagedNodePtr,
            float width,
            YogaMeasureMode widthMode,
            float height,
            YogaMeasureMode heightMode)
        {
            var node = YGNodeHandle.GetManaged(unmanagedNodePtr);
            if (node == null || node._measureFunction == null)
            {
                throw new InvalidOperationException("Measure function is not defined.");
            }
            return node._measureFunction(node, width, widthMode, height, heightMode);
        }

        private static float BaselineInternal(
            IntPtr unmanagedNodePtr,
            float width,
            float height)
        {
            var node = YGNodeHandle.GetManaged(unmanagedNodePtr);
            if (node == null || node._baselineFunction == null)
            {
                throw new InvalidOperationException("Baseline function is not defined.");
            }
            return node._baselineFunction(node, width, height);
        }

        public string Print(YogaPrintOptions options =
            YogaPrintOptions.Layout|YogaPrintOptions.Style|YogaPrintOptions.Children)
        {
            StringBuilder sb = new StringBuilder();
            Logger orig = this._config.Logger;
            this._config.Logger = (config, node, level, message) => {sb.Append(message);};
            Native.YGNodePrint(this._ygNode, options);
            this._config.Logger = orig;
            return sb.ToString();
        }

        public IEnumerator<YogaNode> GetEnumerator()
        {
            return this._children != null ? ((IEnumerable<YogaNode>)this._children).GetEnumerator() :
                System.Linq.Enumerable.Empty<YogaNode>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._children != null ? ((IEnumerable<YogaNode>)this._children).GetEnumerator() :
                System.Linq.Enumerable.Empty<YogaNode>().GetEnumerator();
        }
    }
}
