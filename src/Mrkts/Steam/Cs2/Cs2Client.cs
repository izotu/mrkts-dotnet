namespace Mrkts.Steam.Cs2;

public sealed class Cs2Client
{
    internal Cs2Client(MrktClient client) => Skins = new Skins(client);

    public Skins Skins { get; }
}
