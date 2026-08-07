namespace AgizDisSaglikTakip.Core.Utilities.FileStorage;

public class ContactFileStorageService : LocalFileStorageService, IContactFileStorageService
{
    public ContactFileStorageService(FileStorageSettings settings) : base(settings)
    {
    }
}
