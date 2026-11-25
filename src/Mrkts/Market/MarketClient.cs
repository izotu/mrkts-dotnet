namespace Mrkts.Market;

public sealed class MarketClient
{
    internal MarketClient(MrktClient client)
    {
        Gifts = new Gifts(client);
        Orders = new Orders(client);
        Offers = new Offers(client);
    }

    public Gifts Gifts { get; }
    public Orders Orders { get; }
    public Offers Offers { get; }
}
