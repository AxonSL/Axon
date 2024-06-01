namespace Axon.Client.NetworkMessages;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CustomNetworkMessageAttribute : Attribute
{
    public Type MessageHelper { get; set; }
}
