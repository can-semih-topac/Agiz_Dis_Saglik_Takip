namespace AgizDisSaglikTakip.Core.Utilities.FileStorage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageSettings _settings;

    public LocalFileStorageService(FileStorageSettings settings)
    {
        _settings = settings;
        Directory.CreateDirectory(_settings.UploadFolderPath);
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileExtension)
    {
        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        var fullPath = Path.Combine(_settings.UploadFolderPath, fileName);

        await using var output = new FileStream(fullPath, FileMode.Create);
        await fileStream.CopyToAsync(output);

        return $"{_settings.UploadUrlPath}/{fileName}";
    }
}
