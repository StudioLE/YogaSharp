/*
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;
using System.Runtime.InteropServices;

namespace Arrange
{
    internal class YGNodeHandle : SafeHandle
    {
        private GCHandle _managedNodeHandle;

        private YGNodeHandle() : base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get
            {
                return this.handle == IntPtr.Zero;
            }
        }

        protected override bool ReleaseHandle()
        {
            ReleaseManaged();
            if (!this.IsInvalid)
            {
                Native.YGNodeFree(this.handle);
                GC.KeepAlive(this);
            }
            return true;
        }

        public void SetContext(YogaNode node)
        {
            if (!this._managedNodeHandle.IsAllocated)
            {
                this._managedNodeHandle = GCHandle.Alloc(node, GCHandleType.Weak);
                var managedNodePtr = GCHandle.ToIntPtr(this._managedNodeHandle);
                Native.YGNodeSetContext(this.handle, managedNodePtr);
            }
        }

        public void ReleaseManaged()
        {
            if (this._managedNodeHandle.IsAllocated)
            {
                this._managedNodeHandle.Free();
            }
        }

        public static YogaNode GetManaged(IntPtr unmanagedNodePtr)
        {
            if (unmanagedNodePtr != IntPtr.Zero)
            {
                var managedNodePtr = Native.YGNodeGetContext(unmanagedNodePtr);
                var node = GCHandle.FromIntPtr(managedNodePtr).Target as YogaNode;
                if (node == null)
                {
                    throw new InvalidOperationException("YogaNode is already deallocated");
                }
                return node;
            }
            return null;
        }
    }
}
