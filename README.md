Contrib.Thumbnails
========================

Thumbnails Generator Module for Orchard Project CMS using the ImageResizer.net library

Main features:
- Simple and easy to use extension method
- Compatible with Azure Storage or local file system
- Uses the media folder to store cache of images (assets/cache)

Usage 1 (extension):
- Reference Contrib.Thumbnails on your theme
- Add @using Contrib.Thumbnails.Extensions; to your overriding view
- Use extension method, @Html.Thumbnail("{mediaimageurl}", "Alternate Text", 100, 100, null)

Usage 2 (URL):
- Use url /Thumbnails?mediaPath={mediapathfordirectory}&name={filename}&width=100&height=100

Futures:
- Implement diferent resizing methods from the ImageResizer.net library