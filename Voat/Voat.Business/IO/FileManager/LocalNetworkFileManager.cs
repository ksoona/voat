﻿using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Voat.Common;
using Voat.Common.Components;
using Voat.Configuration;
using Voat.Utilities;

namespace Voat.IO
{
    public class LocalNetworkFileManager : FileManager
    {
        protected virtual string ContentPath(FileType type)
        {
            var path = "";
            switch (type)
            {
                case FileType.Avatar:
                    path = VoatSettings.Instance.DestinationPathAvatars;
                    break;
                case FileType.Badge:
                    path = "~/images/Badges/";
                    break;
                case FileType.Thumbnail:
                    path = VoatSettings.Instance.DestinationPathThumbs;
                    break;
            }
            return path;
        }
        protected override string Domain { get => VoatSettings.Instance.SiteDomain; }

        protected void EnsureLocalDirectoryExists(FileType type)
        {
            var dir = FilePather.Instance.LocalPath(ContentPath(type));
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
        public override void Delete(FileKey key)
        {
            if (Exists(key))
            {
                File.Delete(FilePather.Instance.LocalPath(ContentPath(key.FileType), key.ID));
            }
        }

        public override bool Exists(FileKey key)
        {
            return File.Exists(FilePather.Instance.LocalPath(ContentPath(key.FileType), key.ID));
        }

        public override async Task Upload(FileKey key, Uri contentPath, HttpResourceOptions options = null, Func < Stream, Task<Stream>> preProcessor = null)
        {
            if (Regex.IsMatch(contentPath.Scheme, "http(s)?", RegexOptions.IgnoreCase))
            {
                EnsureLocalDirectoryExists(key.FileType);

                using (var httpResource = new HttpResource(contentPath.ToString(), options))
                {
                    await httpResource.Execute();

                    if (!httpResource.Response.IsSuccessStatusCode)
                    {
                        throw new Exception("Mega major issues");
                    }

                    var processedStream = httpResource.Stream;
                    if (preProcessor != null)
                    {
                        processedStream = await preProcessor(httpResource.Stream);
                    }
                    await Upload(key, processedStream);
                }
        }
            else if (Regex.IsMatch(contentPath.Scheme, "file", RegexOptions.IgnoreCase) || File.Exists(contentPath.ToString()))
            {
                EnsureLocalDirectoryExists(key.FileType);

                using (var sourceStream = (Stream)File.OpenRead(contentPath.ToString()))
                {
                    var processedStream = sourceStream;
                    if (preProcessor != null)
                    {
                        processedStream = await preProcessor(sourceStream);
                    }
                    await Upload(key, processedStream);
            }
        }
            else
            {
                throw new ArgumentException("Can not determine resource location", nameof(contentPath));
            }
        }

        public override string Uri(FileKey key, PathOptions options = null)
        {
            if (key == null || String.IsNullOrEmpty(key.ID))
            {
                return null;
            }

            return VoatUrlFormatter.BuildUrlPath(null, options, (new string[] { ContentPath(key.FileType), key.ID }).ToPathParts());
        }

        public override async Task Upload(FileKey key, Stream stream)
        {
            using (var destinationStream = new FileStream(FilePather.Instance.LocalPath(ContentPath(key.FileType), key.ID), FileMode.Create, FileAccess.Write, FileShare.None, 1048576, true))
            {
                await stream.CopyToAsync(destinationStream);
            }
        }
    }
}