namespace Axon.Shared.Auth;

public class ClientLoginAttempt
{
    public byte[] signature; // Normally [80], not sure if this stays the same always after encryption

    public byte[] Encode()
    {
        return signature;
    }

    public static ClientLoginAttempt Decode(byte[] buf)
    {
        var attempt = new ClientLoginAttempt();
        attempt.signature = buf;
        return attempt;
    }
}