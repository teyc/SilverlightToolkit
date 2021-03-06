// (c) Copyright Microsoft Corporation.
// This source is subject to [###LICENSE_NAME###].
// Please see [###LICENSE_LINK###] for details.
// All other rights reserved.
//
// <autogenerated />
// The Visual Studio metadata is not subjected to same source analysis

//*****************************************************************************
// Assert Failed Exception class
// Copyright(c) Microsoft Corporation, 2003
//*****************************************************************************

namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
    using System;
    using System.Diagnostics;
//  using Microsoft.VisualStudio.TestTools.Resources;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Base class for Framework Exceptions, provides localization trick so that messages are in HA locale.
    /// </summary>
    public abstract partial class UnitTestAssertException: Exception
    {
        private string m_message;

        /// <summary>
        /// Initializes a new UnitTestAssertException.
        /// </summary>
        protected UnitTestAssertException()
        {
        }

        internal UnitTestAssertException(string message, Exception inner) : base(message, inner)
        {
            System.Diagnostics.Debug.Assert(message != null);
            // inner can be null.
            m_message = message;
        }

        /// <summary>
        /// Initializes UnitTestAssertException.
        /// </summary>
        /// <param name="msg">The message.</param>
        protected UnitTestAssertException(string msg) : base(msg)
        {
        }

        /// <summary>
        /// Gets the Message string.
        /// </summary>
        public override string Message
        {
            get
            {
                return m_message == null ? base.Message : m_message.ToString();
            }
        }
    }
    
    /// <summary>
    /// AssertFailedException class. Used to indicate failure for a test case
    /// </summary>
    public partial class AssertFailedException : UnitTestAssertException
    {
        /// <summary>
        /// Initializes a new AssertFailedException.
        /// </summary>
        /// <param name="message">The message.</param>
        public AssertFailedException(string message) : base(message) { }
        /// <summary>
        /// AssertFailedException
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public AssertFailedException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// The AssertInconclusiveException class.
    /// </summary>
    public partial class AssertInconclusiveException : UnitTestAssertException
    {
        /// <summary>
        /// Initializes a new AssertInconclusiveException.
        /// </summary>
        /// <param name="message">The message.</param>
        public AssertInconclusiveException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new AssertInconclusiveException.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public AssertInconclusiveException(string message, Exception inner) : base(message, inner) { }
        /// <summary>
        /// Initializes a new AssertInconclusiveException.
        /// </summary>
        public AssertInconclusiveException() : base() { }
    }

    /// <summary>
    /// InternalTestFailureException class. Used to indicate internal failure 
    /// for a test case.
    /// </summary>
    public partial class InternalTestFailureException : UnitTestAssertException
    {
        /// <summary>
        /// Initializes a new InternalTestFailureException.
        /// </summary>
        /// <param name="message">The message.</param>
        public InternalTestFailureException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new InternalTestFailureException.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public InternalTestFailureException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Initializes a new InternalTestFailureException.
        /// </summary>
        public InternalTestFailureException()
            : base()
        {
        }
    }
}