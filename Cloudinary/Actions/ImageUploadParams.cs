﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace CloudinaryDotNet.Actions
{
    /// <summary>
    /// Parameters for uploading image to cloudinary
    /// </summary>
    public class ImageUploadParams : RawUploadParams
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageUploadParams"/> class.
        /// </summary>
        public ImageUploadParams()
        {
            Overwrite = null;
            UniqueFilename = null;
        }

        /// <summary>
        /// An optional format to convert the uploaded image to before saving in the cloud. For example: "jpg".
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// A transformation to run on the uploaded image before saving it in the cloud. For example: limit the dimension of the uploaded image to 512x512 pixels.
        /// </summary>
        public Transformation Transformation { get; set; }

        /// <summary>
        /// A list of transformations to create for the uploaded image during the upload process, instead of lazily creating them when being accessed by your site's visitors.
        /// </summary>
        public List<Transformation> EagerTransforms { get; set; }

        /// <summary>
        /// Gets or sets privacy mode of the image. Valid values: 'private', 'upload' and 'authenticated'. Default: 'upload'.
        /// </summary>
        public new string Type
        {
            get { return base.Type; }
            set { base.Type = value; }
        }

        /// <summary>
        /// Whether to retrieve the Exif metadata of the uploaded photo. Default: false.
        /// </summary>
        public bool? Exif { get; set; }

        /// <summary>
        /// Whether to retrieve predominant colors and color histogram of the uploaded image. Default: false.
        /// </summary>
        public bool? Colors { get; set; }

        /// <summary>
        /// Whether to retrieve a list of coordinates of automatically detected faces in the uploaded photo. Default: false.
        /// </summary>
        public bool? Faces { get; set; }

        /// <summary>
        /// Sets the face coordinates. Use plain string (x,y,w,h|x,y,w,h) or <see cref="FaceCoordinates"> object</see>/>.
        /// </summary>
        public object FaceCoordinates { get; set; }

        /// <summary>
        /// Whether to retrieve IPTC and detailed Exif metadata of the uploaded photo. Default: false.
        /// </summary>
        public bool? Metadata { get; set; }

        /// <summary>
        /// Whether to generate the eager transformations asynchronously in the background after the upload request is completed rather than online as part of the upload call. Default: false.
        /// </summary>
        public bool? EagerAsync { get; set; }

        /// <summary>
        /// An HTTP URL to send notification to (a webhook) when the generation of eager transformations is completed.
        /// </summary>
        public string EagerNotificationUrl { get; set; }

        /// <summary>
        /// Set to "rekognition_scene" to automatically detect scene categories of photos using the ReKognition Scene Categorization add-on.
        /// </summary>
        public string Categorization { get; set; }

        /// <summary>
        /// Set to "rekognition_scene" to automatically detect scene categories of photos using the ReKognition Scene Categorization add-on.
        /// </summary>
        public float? AutoTagging { get; set; }

        /// <summary>
        /// Set to "rekognition_face" to automatically extract advanced face attributes of photos using the ReKognition Detect Face Attributes add-on.
        /// </summary>
        public string Detection { get; set; }

        public string SimilaritySearch { get; set; }

        public string Ocr { get; set; }

        /// <summary>
        /// Optional. Allows to use an upload preset for setting parameters of this upload.
        /// </summary>
        public string UploadPreset { get; set; }

        /// <summary>
        /// Optional. Allows to send unsigned request. Requires setting appropriate <see cref="UploadPreset"/>.
        /// </summary>
        public bool? Unsigned { get; set; }

        /// <summary>
        /// Gets or sets the phash flag.
        /// </summary>
        public bool? Phash { get; set; }

        /// <summary>
        /// Maps object model to dictionary of parameters in cloudinary notation.
        /// </summary>
        /// <returns>Sorted dictionary of parameters.</returns>
        public override SortedDictionary<string, object> ToParamsDictionary()
        {
            SortedDictionary<string, object> dict = base.ToParamsDictionary();

            AddParam(dict, "format", Format);
            AddParam(dict, "exif", Exif);
            AddParam(dict, "faces", Faces);
            AddParam(dict, "colors", Colors);
            AddParam(dict, "image_metadata", Metadata);
            AddParam(dict, "eager_async", EagerAsync);
            AddParam(dict, "eager_notification_url", EagerNotificationUrl);
            AddParam(dict, "categorization", Categorization);
            AddParam(dict, "detection", Detection);
            AddParam(dict, "ocr", Ocr);
            AddParam(dict, "similarity_search", SimilaritySearch);
            AddParam(dict, "upload_preset", UploadPreset);
            AddParam(dict, "unsigned", Unsigned);
            AddParam(dict, "phash", Phash);

            if (AutoTagging.HasValue)
                AddParam(dict, "auto_tagging", AutoTagging.Value);

            if (FaceCoordinates != null)
                AddParam(dict, "face_coordinates", FaceCoordinates.ToString());

            if (Transformation != null)
                AddParam(dict, "transformation", Transformation.Generate());

            if (EagerTransforms != null && EagerTransforms.Count > 0)
            {
                AddParam(dict, "eager",
                    String.Join("|", EagerTransforms.Select(t => t.Generate()).ToArray()));
            }

            return dict;
        }
    }

    public class FaceCoordinates : List<Rectangle>
    {
        public override string ToString()
        {
            return String.Join("|",
                    this.Select(r => String.Format("{0},{1},{2},{3}", r.X, r.Y, r.Width, r.Height)).ToArray());
        }
    }
}
