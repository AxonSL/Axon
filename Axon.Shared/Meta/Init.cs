using System;

namespace Axon.Shared.Meta;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class Init : Attribute { }
