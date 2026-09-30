namespace Quiz_Web.Services.IServices
{
    public interface IStorageService
    {
        /// <summary>
        /// Uploads a file stream to cloud object storage (Cloudflare R2) or local storage as fallback.
        /// </summary>
        /// <param name="fileStream">Stream of the file content</param>
        /// <param name="fileName">Original or suggested file name</param>
        /// <param name="contentType">MIME type (e.g. video/mp4, image/jpeg)</param>
        /// <param name="folder">Subfolder category (e.g. "videos", "covers")</param>
        /// <returns>Public absolute URL to the uploaded file</returns>
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder = "videos");

        /// <summary>
        /// Deletes a file from storage by its public URL or object key.
        /// </summary>
        /// <param name="fileUrlOrKey">The URL or key of the file to remove</param>
        Task<bool> DeleteFileAsync(string fileUrlOrKey);
    }
}
