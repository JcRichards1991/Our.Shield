using System;

namespace Our.Shield.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class SingleEnvironmentAttribute : Attribute
    {
        
    }
}
