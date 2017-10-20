namespace Database.Repositories.Interfaces
{
    public interface IApiRepository
    {
        bool ValidateAuthKey(string authKey);
    }
}
