﻿// (c) Copyright Microsoft Corporation.
// This source is subject to [###LICENSE_NAME###].
// Please see [###LICENSE_LINK###] for details.
// All other rights reserved.

using System;
using System.Windows.Controls;

namespace Microsoft.Silverlight.Testing.Controls
{
    /// <summary>
    /// Represents the control that redistributes space between columns or rows
    /// of a Grid control.
    /// </summary>
    /// <QualityBand>Mature</QualityBand>
    public partial class GridSplitter : Control
    {
        /// <summary>
        /// Inherited code: Requires comment.
        /// </summary>
        /// <QualityBand>Mature</QualityBand>
        internal enum SplitBehavior
        {
            /// <summary>
            /// Inherited code: Requires comment.
            /// </summary>
            Split,

            /// <summary>
            /// Inherited code: Requires comment.
            /// </summary>
            ResizeDefinition1,

            /// <summary>
            /// Inherited code: Requires comment.
            /// </summary>
            ResizeDefinition2
        }
    }
}