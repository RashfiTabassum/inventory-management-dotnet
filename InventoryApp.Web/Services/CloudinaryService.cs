using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;

namespace InventoryApp.Web.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {
            var account = new Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]);

            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<string?> UploadImageAsync(
            Stream imageStream, string fileName)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, imageStream),
                Folder = "inventory-app",
                Transformation = new Transformation()
                    .Width(800)
                    .Height(600)
                    .Crop("limit")
                    .Quality("auto")
                    .FetchFormat("auto")
            };

            var result = await _cloudinary
                .UploadAsync(uploadParams);

            if (result.Error != null) return null;

            return result.SecureUrl?.ToString();
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            var publicId = ExtractPublicId(imageUrl);
            if (publicId == null) return false;

            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            return result.Result == "ok";
        }

        private string? ExtractPublicId(string imageUrl)
        {
            try
            {
                var uri = new Uri(imageUrl);
                var path = uri.AbsolutePath;
                var uploadIndex = path.IndexOf("/upload/");
                if (uploadIndex < 0) return null;

                var afterUpload = path
                    .Substring(uploadIndex + 8);

                if (afterUpload.StartsWith("v"))
                {
                    var slashIndex = afterUpload.IndexOf('/');
                    if (slashIndex >= 0)
                        afterUpload = afterUpload
                            .Substring(slashIndex + 1);
                }

                var dotIndex = afterUpload.LastIndexOf('.');
                if (dotIndex >= 0)
                    afterUpload = afterUpload
                        .Substring(0, dotIndex);

                return afterUpload;
            }
            catch
            {
                return null;
            }
        }
    }
}