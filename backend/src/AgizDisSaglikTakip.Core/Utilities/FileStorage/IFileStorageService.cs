namespace AgizDisSaglikTakip.Core.Utilities.FileStorage;

public interface IFileStorageService
{
    // fileExtension ".jpg" gibi noktayla birlikte verilir. Dönen değer DB'ye yazılacak URL yoludur.
    Task<string> SaveFileAsync(Stream fileStream, string fileExtension);
}
