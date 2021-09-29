//-----------------------------------------------------------------------
// <copyright file="Discovery.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2019 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2019 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Akka.MultiNode.TestAdapter.Internal;
using Akka.Remote.TestKit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Akka.MultiNode.TestAdapter
{
    public class Discovery : IMessageSink, IDisposable
    {
        // There can be multiple fact attributes in a single class, but our convention
        // limits them to 1 fact attribute per test class
        public List<MultiNodeTest> MultiNodeTests { get; }
        public List<ITestCase> TestCases { get; }
        public List<ErrorMessage> Errors { get; } = new List<ErrorMessage>();
        public bool WasSuccessful => Errors.Count == 0;

        private readonly string _assemblyPath;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Discovery"/> class.
        /// </summary>
        public Discovery(string assemblyPath)
        {
            _assemblyPath = assemblyPath;
            MultiNodeTests = new List<MultiNodeTest>();
            TestCases = new List<ITestCase>();
            Finished = new ManualResetEvent(false);
        }

        public ManualResetEvent Finished { get; }

        public virtual bool OnMessage(IMessageSinkMessage message)
        {
            switch (message)
            {
                case ITestCaseDiscoveryMessage discovery:
                    var testClass = discovery.TestClass.Class;
                    if (testClass.IsAbstract) 
                        break;
                    
                    if (!discovery.TestMethod.Method.GetCustomAttributes(typeof(MultiNodeFactAttribute)).Any())
                        break;
                    
                    MultiNodeTests.Add(new MultiNodeTest(discovery, _assemblyPath));
                    TestCases.Add(discovery.TestCase);
                    break;
                case IDiscoveryCompleteMessage _:
                    Finished.Set();
                    break;
                case ErrorMessage err:
                    Errors.Add(err);
                    break;
            }

            return true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Finished.Dispose();
        }
    }
}