using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Tesseract;
using UtilitiesCS.Extensions;

namespace UtilitiesCS.EmailIntelligence
{
    public class ImageStripper
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Constructors and private fields

        public ImageStripper() 
        { 
            //_globals = appGlobals; 
        }

        public ImageStripper(string cachefile)
        {
            //_globals = appGlobals;
            _cachefile = cachefile;
        }

        //private IApplicationGlobals _globals;
        private string _cachefile;

        #endregion Constructors and private fields

        //int misses, hits = 0;
                
        internal (string text, HashSet<string> tokens) analyze(
            string engine_name, List<object> parts)
        {
            if (engine_name != "Tesseract")
            {
                logger.Error($"Engine {engine_name} has not been implemented. " +
                    $"invalid engine name {engine_name} - OCR disabled\n" +
                    $"Only Tesseract OCR engine has been implemented. ");
                return ("", new HashSet<string>());
            }

            if (parts.Count == 0)
            {
                //logger.Debug("No images to analyze");
                return ("", new HashSet<string>());
            }

            var (images, tokens) = PIL_decode_parts(parts);

            if (images.Count > 0)
            {
                var (text, new_tokens) = this.extract_ocr_info(images);
                return (text, new HashSet<string>(tokens.Concat(new_tokens)));
            }
            else
            {
                return ("", tokens);
            }
        }

        internal (List<Bitmap> image, HashSet<string> tokens) PIL_decode_parts(List<object> parts)
        {
            var tokens = new HashSet<string>();
            var rows = new LinkedList<Bitmap>();
            var max_image_size = SpamBayesOptions.max_image_size;

            var attachmentParts = parts.Where(x => x is IAttachment)?.Cast<IAttachment>()?.ToList() ?? [];
            var htmlParts = parts.Where(x => x is string)?.Cast<string>()?.ToList() ?? [];
            foreach (var part in attachmentParts)
            {
                // See 'image_large_size_attribute' above - the provider may have seen
                // an image, but optimized the fact we don't bother processing large
                // images.
                var nbytes = part.Size;
                byte[] bytes = null;
                if (nbytes < max_image_size)
                {
                    try
                    {
                        bytes = part.AttachmentData;
                        if (bytes.IsNullOrEmpty()) 
                        {
                            tokens.Add($"invalid-image:{part.Type}");
                        }
                        //bytes = GetBytes(part);
                    }
                    catch (System.Exception)
                    {
                        tokens.Add($"invalid-image:{part.Type}");
                    }
                }
                else
                {
                    // assume it's just a picture for now
                    tokens.Add("image:big");
                }

                // We're dealing with spammers and virus writers here.  Who knows
                // what garbage they will call a GIF image to entice you to open
                // it?
                
                Image image = null;
                Bitmap bitmap = null;
                
                try
                {
                    image = GetImage(GetStream(bytes));
                    //image = GetImage(part);
                }
                catch (System.Exception)
                {
                    tokens.Add($"invalid-image:{part.Type}");
                }
                if (image is not null)
                {
                    if (IsMultiFrameImage(image))
                    {
                        try
                        {
                            // Assume the pixel with the largest value is the
                            // background.
                            bitmap = GetFrameWithText(image);
                        }
                        catch (System.Exception)
                        {
                            tokens.Add($"invalid-image:{part.Type}");
                        }
                    }
                    else
                    {
                        bitmap = (Bitmap)image;
                    }

                    // image = image.convert("RGB")
                    bitmap = bitmap?.ToRGB();

                    if (rows.Count == 0)
                    {
                        // first image
                        rows.AddLast(bitmap);
                    }
                    else if (rows.Last().Height != bitmap.Height)
                    {
                        // new image, different height => start new row
                        rows.AddLast(bitmap);
                    }
                    else
                    {
                        // new image, same height => extend current row
                        rows.Last.Value = imconcatlr(rows.Last.Value, bitmap);
                    }
                }                
            }

            if (rows.Count == 0)
            {
                // no images
                return (new List<Bitmap>(), tokens);
            }

            // now concatenate the resulting row images top-to-bottom
            var full_image = rows.First.Value;
            rows.RemoveFirst();

            while (rows.Count > 0)
            {
                full_image = imconcattb(full_image, rows.First.Value);
                rows.RemoveFirst();
            }

            return (new List<Bitmap> { full_image }, tokens);
        }
                
        internal (string text, HashSet<string> tokens) extract_ocr_info(List<Bitmap> images)
        {
            var textbits = new List<string>();
            var tokens = new HashSet<string>();

            foreach (var image in images)
            {
                string ctext = "";
                try
                {
                    ctext = this.extract_text(image).ToLower();
                }
                catch (System.Exception e)
                {
                    logger.Error(e.Message);
                }
                
                var ctokens = new HashSet<string>();
                
                if (ctext.Trim().IsNullOrEmpty())
                {
                    ctokens.Add("image-text:no text found");
                    // # Lots of spam now contains images in which it is
                    // difficult or impossible (using ocrad) to find any
                    // text.  Make a note of that.
                }
                else
                {
                    var nlines = ctext.Trim().Split('\n').Length;
                    if (nlines > 0)
                        ctokens.Add($"image-text-lines:{Math.Round(Math.Log(nlines, 2))}");
                }

                textbits.Add(ctext);
                tokens = new HashSet<string>(tokens.Concat(ctokens));
            }

            var texts = string.Join("\n", textbits);
            
            return (texts, tokens);
        }

        internal Bitmap imconcattb(Bitmap top, Bitmap bottom)
        {
            var w1 = top.Width;
            var h1 = top.Height;
            var w2 = bottom.Width;
            var h2 = bottom.Height;

            if (w1 * w2 * h1 * h2 == 0)
            {
                //logger.Debug($"Invalid image dimensions: w1: {w1}, h1: {h1}, w2: {w2}, h2: {h2}");
                return top;
            }

            Bitmap bitmap = new(Math.Max(w1,w2), h1 + h2, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Black);
                try
                {
                    g.DrawImage(top, 0, 0);
                    g.DrawImage(bottom, 0, top.Height);
                }
                catch (System.OutOfMemoryException e)
                {
                    //logger.Debug($"Variables before exception: w1: {w1}, h1: {h1}, w2: {w2}, h2: {h2}");
                    logger.Error(e.Message,e);
                    bitmap = top;
                }
                
            }
            return bitmap;
        }

        internal Bitmap imconcatlr(Bitmap left, Bitmap right)
        {
            var w1 = left.Width;
            var h1 = right.Height;
            var w2 = right.Width;
            var h2 = right.Height;

            Bitmap bitmap = new(w1 + w2, Math.Max(h1,h2), PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Black);
                g.DrawImage(left, 0, 0);
                g.DrawImage(right, left.Width, 0);
            }
            return bitmap;
        }
        
        /// <summary>
        /// Spammers are now using GIF image sequences.  From examining a
        /// miniscule set of multi-frame GIFs it appears the frame with
        /// the fewest number of background pixels is the one with the
        /// text content.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        internal Bitmap GetFrameWithText(Image image)
        {
            var frames = SeperateMultiFrame(image);
            Bitmap imageWithText = null;
            var bgpix = 1e17;
            foreach (var frame in frames) 
            {
                // Generate Histogram of image
                var bg = frame.GenerateHistogram().Max(x => x.Value);
                if (bg < bgpix)
                {
                    imageWithText = frame;
                    bgpix = bg;
                }
            }
            return imageWithText;
        }

        internal bool IsMultiFrameImage(Image image)
        {
            var dimension = new FrameDimension(image.FrameDimensionsList[0]);
            int frameCount = image.GetFrameCount(dimension);
            return frameCount > 1;
        }

        internal IEnumerable<Bitmap> SeperateMultiFrame(Image image)
        {
            var dimension = new FrameDimension(image.FrameDimensionsList[0]);
            var frameCount = image.GetFrameCount(dimension);
            for (var i = 0; i < frameCount; i++)
            {
                image.SelectActiveFrame(dimension, i);
                yield return (Bitmap)image.Clone();
            }
            
        }

        internal Image GetImage(Attachment attachment)
        {
            var bytes = GetBytes(attachment);
            var stream = GetStream(bytes);
            var image = GetImage(stream);
            return image;
        }

        internal Image GetImage(MemoryStream stream)
        {
            var image = Image.FromStream(stream);
            return image;
        }

        internal MemoryStream GetStream(byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            return stream;
        }
        
        internal byte[] GetBytes(Attachment attachment)
        {
            const string PR_ATTACH_DATA_BIN = "http://schemas.microsoft.com/mapi/proptag/0x37010102";
            var bytes = attachment.PropertyAccessor.GetProperty(PR_ATTACH_DATA_BIN);
            return bytes;
        }

        public string extract_text(Bitmap bitmap)
        {
            // Get byte array of image
            byte[] data = bitmap.ToByte();
            
            string tessdataPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}{Path.DirectorySeparatorChar}TaskMaster{Path.DirectorySeparatorChar}tessdata";
            //string tessdataPath = $"{_globals.FS.FldrAppData}{Path.DirectorySeparatorChar}tessdata";
            using (TesseractEngine engine = new TesseractEngine(tessdataPath, "eng", EngineMode.Default))
            {
                //var pix = new BitmapToPixConverter().Convert(bitmap);
                //var page = engine.Process(pix);
                var page = engine.Process(bitmap);
                
                var text = page.GetText();
                return text;
                
                
                //using (Pix pix = Pix.LoadFromMemory(data))
                //{
                //    using (Tesseract.Page page = engine.Process(pix))
                //    {
                //        return page.GetText();
                //    }
                //}
            }
        }

    }
}
