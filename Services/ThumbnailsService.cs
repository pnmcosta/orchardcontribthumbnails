using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard;
using Orchard.Caching;
using Orchard.Environment.Configuration;
using Orchard.Environment.Extensions;
using Orchard.FileSystems.Media;
using Orchard.Media.Services;
using ImageResizer;
using System.Drawing;
using Orchard.Mvc.Extensions;
namespace Contrib.Thumbnails.Services
{
    public interface IThumbnailsService : IDependency
    {
        string GetThumbnail(string url, int width, int height);
        IStorageFile CreateThumbnail(string mediaPath, string name, int width, int height, bool force = false);
    }
    [OrchardFeature("Nublr.Orchard.Features.Thumbnails")]
    public class ThumbnailsService : IThumbnailsService
    {
        private readonly IMediaService _mediaService;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly string _publicPath;
        private readonly IWorkContextAccessor _wca;
        private readonly IStorageProvider _storageProvider;

        private const string ThumbnailsCacheMediaPath = "assets/cache";

        public ThumbnailsService(ShellSettings settings, IWorkContextAccessor wca, ICacheManager cacheManager, IMediaService mediaService, ISignals signals, IStorageProvider storageProvider)
        {
            _wca = wca;
            _cacheManager = cacheManager;
            _mediaService = mediaService;
            _signals = signals;
            _storageProvider = storageProvider;
            var appPath = "";
            if (HostingEnvironment.IsHosted)
            {
                appPath = HostingEnvironment.ApplicationVirtualPath;
            }
            if (!appPath.EndsWith("/"))
                appPath = appPath + '/';
            if (!appPath.StartsWith("/"))
                appPath = '/' + appPath;

            _publicPath = appPath + "Media/" + settings.Name + "/";

            var physPath = ThumbnailsCacheMediaPath.Replace('/', Path.DirectorySeparatorChar);
            var parent = Path.GetDirectoryName(physPath);
            var folder = Path.GetFileName(physPath);
            if (_mediaService.GetMediaFolders(parent).All(f => f.Name != folder))
            {
                _mediaService.CreateFolder(parent, folder);
            }
        }
        public string GetThumbnail(string url, int width, int height)
        {
            if (url.IndexOf(_publicPath, StringComparison.InvariantCultureIgnoreCase) == -1)
                return url;

            string relativePath = url.Substring(url.IndexOf(_publicPath, StringComparison.InvariantCultureIgnoreCase) + _publicPath.Length);
            string mediaPath = Fix(Path.GetDirectoryName(relativePath));
            string name = Path.GetFileName(relativePath);
            string fileHash = CreateMd5Hash(string.Concat(mediaPath, name, width, height));
            string fileExtension = Path.GetExtension(name);
            string thumbnailFileName = fileHash + fileExtension;

            return _cacheManager.Get(
                        "Nublr.Thumbnails." + fileHash,
                        ctx =>
                        {
                            ctx.Monitor(_signals.When("Nublr.Thumbnails." + fileHash + ".Changed"));
                            WorkContext workContext = _wca.GetContext();
                            if (_mediaService.GetMediaFiles(ThumbnailsCacheMediaPath).Any(i => i.Name == thumbnailFileName))
                                return _mediaService.GetPublicUrl(Combine(ThumbnailsCacheMediaPath, thumbnailFileName));
                            UrlHelper urlHelper = new UrlHelper(new RequestContext(workContext.HttpContext, new RouteData()));
                            return urlHelper.Action("Create", "Thumbnails", new
                                    {
                                        area = "Nublr.Orchard.Features",
                                        mediaPath = mediaPath,
                                        name = name,
                                        width = width,
                                        height = height
                                    });
                        });
        }

       
        public IStorageFile CreateThumbnail(string mediaPath, string name, int width, int height, bool force = false)
        {
            if (!_mediaService.GetMediaFiles(mediaPath).Any(i => i.Name == name))
                return null;

            string fileHash = CreateMd5Hash(string.Concat(mediaPath, name, width, height));
            string fileExtension = Path.GetExtension(name);
            string thumbnailFileName = fileHash + fileExtension;
            string relativePath = Combine(mediaPath, name);
            string thumbnailRelativePath = Combine(ThumbnailsCacheMediaPath, thumbnailFileName);

            if (_mediaService.GetMediaFiles(ThumbnailsCacheMediaPath).Any(i => i.Name == thumbnailFileName))
            {
                if (force)
                    _mediaService.DeleteFile(ThumbnailsCacheMediaPath, thumbnailFileName);
                else
                {
                    return _storageProvider.GetFile(thumbnailRelativePath);
                }
            }

            var imageFile = _storageProvider.GetFile(relativePath);
            var outputFile = imageFile;
            using (var imageStream = imageFile.OpenRead())
            {
                Image image = Image.FromStream(imageStream);
                if (MustBeScaledDown(image, width, height))
                {
                    var thumbnailFile = _storageProvider.CreateFile(thumbnailRelativePath);
                    using (var thumbnailStream = thumbnailFile.CreateFile())
                        ImageBuilder.Current.Build(image, thumbnailStream, new ResizeSettings() { Width = width, Height = height });
                    outputFile = thumbnailFile;
                }
            }

            if (outputFile != imageFile)
                _signals.Trigger("Nublr.Thumbnails." + fileHash + ".Changed");

            return outputFile;
        }
        private static bool MustBeScaledDown(Image image, int width, int height)
        {
            return (width > 0 || height > 0) && (image.Width > width || image.Height > height);
        }
        private string Combine(params string[] paths)
        {
            return Fix(Path.Combine(paths));
        }
        private string Fix(string path)
        {
            return path.Replace(Path.DirectorySeparatorChar, '/');
        }
        /// <summary>
        /// http://en.csharp-online.net/Create_a_MD5_Hash_from_a_string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string CreateMd5Hash(string input)
        {
            // Use input string to calculate MD5 hash
            MD5 md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            var sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
                // To force the hex string to lower-case letters instead of
                // upper-case, use he following line instead:
                // sb.Append(hashBytes[i].ToString("x2")); 
            }
            return sb.ToString();
        }
    }
}