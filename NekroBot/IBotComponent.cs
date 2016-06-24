namespace NekroBot
{
    using System.Threading.Tasks;

    public interface IBotComponent
    {
        Task Run();
    }
}