namespace Quiz_Web.Models.Settings
{
    public class CloudflareR2Settings
    {
        public string AccountId { get; set; } = string.Empty;
        public string AccessKeyId { get; set; } = string.Empty;
        public string SecretAccessKey { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;
        public string PublicUrl { get; set; } = string.Empty;

        /// <summary>
        /// True if all necessary credentials to connect to Cloudflare R2 are present.
        /// </summary>
        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(AccountId) &&
            !string.IsNullOrWhiteSpace(AccessKeyId) &&
            !string.IsNullOrWhiteSpace(SecretAccessKey) &&
            !string.IsNullOrWhiteSpace(BucketName);
    }
}
