using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Akka.Remote.TestKit;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using DiagnosticMessage = Xunit.Sdk.DiagnosticMessage;
using NullMessageSink = Xunit.Sdk.NullMessageSink;
using TestMethodDisplay = Xunit.Sdk.TestMethodDisplay;
using TestMethodDisplayOptions = Xunit.Sdk.TestMethodDisplayOptions;

namespace Akka.MultiNode.NodeRunner
{
    [DebuggerDisplay("\\{ class = {TestMethod.TestClass.Class.Name}, method = {TestMethod.Method.Name}, display = {DisplayName}, skip = {SkipReason} \\}")]
    public class MultiNodeTestCase : TestMethodTestCase, IXunitTestCase
    {
        private int _timeout;

        /// <summary />
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public MultiNodeTestCase() => DiagnosticMessageSink = new NullMessageSink();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Xunit.Sdk.XunitTestCase" /> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        /// <param name="defaultMethodDisplay">Default method display to use (when not customized).</param>
        /// <param name="defaultMethodDisplayOptions">Default method display options to use (when not customized).</param>
        /// <param name="testMethod">The test method this test case belongs to.</param>
        /// <param name="testMethodArguments">The arguments for the test method.</param>
        public MultiNodeTestCase(
            IMessageSink diagnosticMessageSink,
            TestMethodDisplay defaultMethodDisplay,
            TestMethodDisplayOptions defaultMethodDisplayOptions,
            ITestMethod testMethod,
            object[] testMethodArguments = null)
            : base(defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments)
        {
            DiagnosticMessageSink = diagnosticMessageSink;
        }

        /// <summary>
        /// Gets the message sink used to report <see cref="T:Xunit.Abstractions.IDiagnosticMessage" /> messages.
        /// </summary>
        protected IMessageSink DiagnosticMessageSink { get; }

        /// <inheritdoc />
        public int Timeout
        {
            get
            {
                EnsureInitialized();
                return _timeout;
            }
            protected set
            {
                EnsureInitialized();
                _timeout = value;
            }
        }

        /// <summary>
        /// Gets the display name for the test case. Calls <see cref="M:Xunit.Sdk.TypeUtility.GetDisplayNameWithArguments(Xunit.Abstractions.IMethodInfo,System.String,System.Object[],Xunit.Abstractions.ITypeInfo[])" />
        /// with the given base display name (which is itself either derived from <see cref="P:Xunit.FactAttribute.DisplayName" />,
        /// falling back to <see cref="P:Xunit.Sdk.TestMethodTestCase.BaseDisplayName" />.
        /// </summary>
        /// <param name="factAttribute">The fact attribute the decorated the test case.</param>
        /// <param name="displayName">The base display name from <see cref="P:Xunit.Sdk.TestMethodTestCase.BaseDisplayName" />.</param>
        /// <returns>The display name for the test case.</returns>
        protected virtual string GetDisplayName(IAttributeInfo factAttribute, string displayName) => 
            TestMethod.Method.GetDisplayNameWithArguments(displayName, TestMethodArguments, MethodGenericTypes);

        /// <summary>
        /// Gets the skip reason for the test case. By default, pulls the skip reason from the
        /// <see cref="P:Xunit.FactAttribute.Skip" /> property.
        /// </summary>
        /// <param name="factAttribute">The fact attribute the decorated the test case.</param>
        /// <returns>The skip reason, if skipped; <c>null</c>, otherwise.</returns>
        protected virtual string GetSkipReason(IAttributeInfo factAttribute) => 
            factAttribute.GetNamedArgument<string>("Skip");

        /// <summary>
        /// Gets the timeout for the test case. By default, pulls the skip reason from the
        /// <see cref="P:Xunit.FactAttribute.Timeout" /> property.
        /// </summary>
        /// <param name="factAttribute">The fact attribute the decorated the test case.</param>
        /// <returns>The timeout in milliseconds, if set; 0, if unset.</returns>
        protected virtual int GetTimeout(IAttributeInfo factAttribute) => 
            factAttribute.GetNamedArgument<int>("Timeout");

        /// <inheritdoc />
        protected override void Initialize()
        {
            base.Initialize();
            var factAttribute = TestMethod.Method.GetCustomAttributes(typeof (MultiNodeFactAttribute)).First();
            var displayName = factAttribute.GetNamedArgument<string>("DisplayName") ?? BaseDisplayName;
            DisplayName = GetDisplayName(factAttribute, displayName);
            SkipReason = GetSkipReason(factAttribute);
            Timeout = GetTimeout(factAttribute);
        }

        /// <inheritdoc />
        public virtual Task<RunSummary> RunAsync(
            IMessageSink diagnosticMessageSink,
            IMessageBus messageBus,
            object[] constructorArguments,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            return new XunitTestCaseRunner(this, DisplayName, SkipReason, constructorArguments, TestMethodArguments, messageBus, aggregator, cancellationTokenSource).RunAsync();
        }

        /// <inheritdoc />
        public override void Serialize(IXunitSerializationInfo data)
        {
            base.Serialize(data);
            data.AddValue("Timeout", Timeout);
        }

        /// <inheritdoc />
        public override void Deserialize(IXunitSerializationInfo data)
        {
            base.Deserialize(data);
            Timeout = data.GetValue<int>("Timeout");
        }
    }
}