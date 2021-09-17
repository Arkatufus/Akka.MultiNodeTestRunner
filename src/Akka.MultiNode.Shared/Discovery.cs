//-----------------------------------------------------------------------
// <copyright file="Discovery.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2019 Lightbend Inc. <http://www.lightbend.com>
//     Copyright (C) 2013-2019 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System.Reflection;
using Akka.Remote.TestKit;

namespace Akka.MultiNode.Shared
{
    public class Discovery
    {
        public Discovery(
            Assembly assembly,
            TypeInfo typeInfo,
            MethodInfo methodInfo, MultiNodeFactAttribute attribute)
        {
            Assembly = assembly;
            TypeInfo = typeInfo;
            MethodInfo = methodInfo;
            Attribute = attribute;
        }
        
        public Assembly Assembly { get; }
        public TypeInfo TypeInfo { get; }
        public MethodInfo MethodInfo { get; }
        public MultiNodeFactAttribute Attribute { get; }
    }
}
