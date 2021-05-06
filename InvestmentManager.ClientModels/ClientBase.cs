namespace InvestmentManager.ClientModels
{
    public class ClientBase
    {
        public long Id { get; set; }
        public virtual string Name { get; set; }
        public string Description { get; set; }
    }
}
