﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2018 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using ShareX.HelpersLib;
using ShareX.UploadersLib.Properties;
using System;
using System.IO;
using System.Windows.Forms;

namespace ShareX.UploadersLib.ImageUploaders
{
    public class CustomImageUploaderService : ImageUploaderService
    {
        public override ImageDestination EnumValue { get; } = ImageDestination.CustomImageUploader;

        public override bool CheckConfig(UploadersConfig config)
        {
            return config.CustomUploadersList != null && config.CustomUploadersList.IsValidIndex(config.CustomImageUploaderSelected);
        }

        public override GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo)
        {
            int index;

            if (taskInfo.OverrideCustomUploader)
            {
                index = taskInfo.CustomUploaderIndex.BetweenOrDefault(0, config.CustomUploadersList.Count - 1);
            }
            else
            {
                index = config.CustomImageUploaderSelected;
            }

            CustomUploaderItem customUploader = config.CustomUploadersList.ReturnIfValidIndex(index);

            if (customUploader != null)
            {
                return new CustomImageUploader(customUploader);
            }

            return null;
        }

        public override TabPage GetUploadersConfigTabPage(UploadersConfigForm form) => form.tpCustomUploaders;
    }

    public sealed class CustomImageUploader : ImageUploader
    {
        private CustomUploaderItem uploader;

        public CustomImageUploader(CustomUploaderItem customUploaderItem)
        {
            uploader = customUploaderItem;
        }

        public override UploadResult Upload(Stream stream, string fileName)
        {
            CustomUploaderArgumentInput input = new CustomUploaderArgumentInput(fileName, "");

            CustomUploaderRequestFormat requestFormat = uploader.GetRequestFormat(CustomUploaderDestinationType.ImageUploader);

            if (requestFormat == CustomUploaderRequestFormat.FormData)
            {
                UploadResult result = SendRequestFile(uploader.GetRequestURL(), stream, fileName, uploader.GetFileFormName(),
                    uploader.GetArguments(input), uploader.GetHeaders(input), null, uploader.ResponseType, uploader.GetHttpMethod());

                if (result.IsSuccess)
                {
                    try
                    {
                        uploader.ParseResponse(result);
                    }
                    catch (Exception e)
                    {
                        Errors.Add(Resources.CustomFileUploader_Upload_Response_parse_failed_ + Environment.NewLine + e);
                    }
                }

                return result;
            }
            else
            {
                throw new Exception("Unsupported request format.");
            }
        }
    }
}