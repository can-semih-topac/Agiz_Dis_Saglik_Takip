namespace AgizDisSaglikTakip.Core.Utilities.Security.Hashing;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
