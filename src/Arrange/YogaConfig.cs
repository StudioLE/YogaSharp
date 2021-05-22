/*
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the MIT license found in the
 * LICENSE file in the root directory of this source tree.
 */

using System;

namespace Arrange
{
    public class YogaConfig
    {
        internal static readonly YogaConfig Default = new YogaConfig(YGConfigHandle.Default);
        private static YogaLogger _managedLogger;

        private YGConfigHandle _ygConfig;
        private Logger _logger;

        private YogaConfig(YGConfigHandle ygConfig)
        {
            this._ygConfig = ygConfig;
            if (this._ygConfig.IsInvalid)
            {
                throw new InvalidOperationException("Failed to allocate native memory");
            }

            this._ygConfig.SetContext(this);

            if (this._ygConfig == YGConfigHandle.Default)
            {
                _managedLogger = LoggerInternal;
                Native.YGInteropSetLogger(_managedLogger);
            }
        }

        public YogaConfig()
            : this(Native.YGConfigNew())
        {
        }

        internal YGConfigHandle Handle
        {
            get {
                return this._ygConfig;
            }
        }

        private static void LoggerInternal(
            IntPtr unmanagedConfigPtr,
            IntPtr unmanagedNodePtr,
            YogaLogLevel level,
            string message)
        {
            var config = YGConfigHandle.GetManaged(unmanagedConfigPtr);
            if (config == null || config._logger == null)
            {
                // Default logger
                System.Diagnostics.Debug.WriteLine(message);
            }
            else
            {
                var node = YGNodeHandle.GetManaged(unmanagedNodePtr);
                config._logger(config, node, level, message);
            }

            if (level == YogaLogLevel.Error || level == YogaLogLevel.Fatal)
            {
                throw new InvalidOperationException(message);
            }
        }

        public Logger Logger
        {
            get {
                return this._logger;
            }

            set {
                this._logger = value;
            }
        }

        public void SetExperimentalFeatureEnabled(
            YogaExperimentalFeature feature,
            bool enabled)
        {
            Native.YGConfigSetExperimentalFeatureEnabled(this._ygConfig, feature, enabled);
        }

        public bool IsExperimentalFeatureEnabled(YogaExperimentalFeature feature)
        {
            return Native.YGConfigIsExperimentalFeatureEnabled(this._ygConfig, feature);
        }

        public bool UseWebDefaults
        {
            get
            {
                return Native.YGConfigGetUseWebDefaults(this._ygConfig);
            }

            set
            {
                Native.YGConfigSetUseWebDefaults(this._ygConfig, value);
            }
        }

        public bool UseLegacyStretchBehaviour
        {
            get
            {
                return Native.YGConfigGetUseLegacyStretchBehaviour(this._ygConfig);
            }

            set
            {
                Native.YGConfigSetUseLegacyStretchBehaviour(this._ygConfig, value);
            }
        }

        public float PointScaleFactor
        {
            set
            {
                Native.YGConfigSetPointScaleFactor(this._ygConfig, value);
            }
        }

        public static int GetInstanceCount()
        {
            return Native.YGConfigGetInstanceCount();
        }

        public static void SetDefaultLogger(Logger logger)
        {
            Default.Logger = logger;
        }
    }
}
