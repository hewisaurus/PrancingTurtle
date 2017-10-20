namespace Database.Repositories.Interfaces
{
    public interface INpcRepository
    {
        string GetName(string npcId, int encounterId);
    }
}
