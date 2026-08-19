namespace AgizDisSaglikTakip.Core.Utilities.FileStorage;

// StatusNote görselleriyle aynı klasörü paylaşmasın diye ayrı bir tip — DI'da
// IFileStorageService'ten bağımsız olarak, kendi FileStorageSettings'iyle kaydediliyor.
public interface IContactFileStorageService : IFileStorageService
{
}
