namespace AgizDisSaglikTakip.Core.Utilities.FileStorage;

public class FileStorageSettings
{
    // Dosyanın diskte fiziksel olarak kaydedileceği tam klasör yolu.
    public string UploadFolderPath { get; set; } = string.Empty;

    // Aynı dosyaya tarayıcıdan erişmek için kullanılacak URL öneki (örn. "/uploads/status-notes").
    public string UploadUrlPath { get; set; } = string.Empty;
}
