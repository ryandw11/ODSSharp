using System;

namespace ODS.Serializer
{
    /**
     * <summary>This attribute tells ODS that the field can be serialized.</summary>
     */
    [AttributeUsage(AttributeTargets.Field)]
    public class ODSSerializeable : Attribute
    {}
}
