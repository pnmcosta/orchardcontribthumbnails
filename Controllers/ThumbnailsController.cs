using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.Environment.Extensions;
using Contrib.Thumbnails.Services;
using Orchard.FileSystems.Media;
using System.Text;

namespace Contrib.Thumbnails.Controllers
{
    public class ThumbnailsController : Controller
    {
        private readonly IThumbnailsService _thumbnailsService;

        public ThumbnailsController(IThumbnailsService thumbnailsService)
        {
            _thumbnailsService = thumbnailsService;
        }
        public FileResult Create(string name, string mediaPath, int width, int height, bool force = false)
        {
            IStorageFile thumbnailFile = _thumbnailsService.CreateThumbnail(mediaPath, name, width, height, force);
            if(thumbnailFile!=null)
                return File(thumbnailFile.OpenRead(), thumbnailFile.GetFileType());
            return File(Encoding.UTF8.GetBytes("Invalid media"), "text/plain");
        }
    }
}