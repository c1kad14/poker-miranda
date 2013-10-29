using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using AForge;
using AForge.Imaging.Filters;
using AForge.Imaging;
using AForge.Math.Geometry;
using LogLoader;
using Tesseract;
using miranda.ui.Properties;
using Point = System.Drawing.Point;
using System.IO;

//Remove ambiguousness between AForge.Image and System.Drawing.Image

//Remove ambiguousness between AForge.Point and System.Drawing.Point

namespace miranda.ui
{
    /// <summary>
    /// Class that recognizes playing cards from image
    /// 
    /// </summary>
    public class CardRecognizer
    {
        
        //Template Images
        private Bitmap j, k, q; //Face Card Character Templates
        private Bitmap clubs, diamonds, spades, hearts; //Suit Templates

        private FiltersSequence commonSeq; //Commonly filter sequence to be used 
        /// <summary>
        /// Constructor
        /// </summary>
        public CardRecognizer()
        {
            //Initialize common filter sequence , this sequence generally will be applied
            commonSeq = new FiltersSequence();
            commonSeq.Add(Grayscale.CommonAlgorithms.BT709);
            commonSeq.Add(new BradleyLocalThresholding());
            commonSeq.Add(new DifferenceEdgeDetector());


            //Load Templates From Resources , 
            //Templates will be used for template matching
            j = miranda.ui.Properties.Resources.J;
            k = miranda.ui.Properties.Resources.K;
            q = miranda.ui.Properties.Resources.Q;
            clubs = miranda.ui.Properties.Resources.Clubs;
            diamonds = miranda.ui.Properties.Resources.Diamonds;
            spades = miranda.ui.Properties.Resources.Spades;
            hearts = miranda.ui.Properties.Resources.Hearts;


            try
            {
                _engine = new TesseractEngine(@"./tessdata", "rus", EngineMode.Default);
                //_engine.SetVariable("tessedit_char_whitelist", "$.,0123456789");
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                Ex.Report(ex);
            }
            
            
            
        }

        /// <summary>
        /// Scans  and returns suit of face cards.
        /// Uses template matching for recognizing suit of card
        /// </summary>
        /// <param name="bmp">Top right part of card Image</param>
        /// <param name="color">Color of card</param>
        /// <returns>Scanned Suit</returns>
        private Suit ScanSuit(Bitmap bmp, char color)
        {
            //Initialize templateMatching class with 0.8 similarity threshold
            ExhaustiveTemplateMatching templateMatching = new ExhaustiveTemplateMatching(0.95f);
            Suit suit = Suit.NOT_RECOGNIZED;

            if (color == 'R') //If card is red then it can be hearts or diamonds
            {
                if (templateMatching.ProcessImage(bmp, hearts).Length > 0)
                    suit = Suit.Hearts; //Check If template matches for hearts
                if (templateMatching.ProcessImage(bmp, diamonds).Length > 0)
                    suit = Suit.Diamonds; //Check If template matches for diamonds
            }
            else //If its black
            {
                if (templateMatching.ProcessImage(bmp, spades).Length > 0)
                    suit = Suit.Spades; //Check If template matches for spades
                if (templateMatching.ProcessImage(bmp, clubs).Length > 0)
                    suit = Suit.Clubs;//Check If template matches for clubs
            }
            return suit;
        }
        
        /// <summary>
        /// Scans rank of face cards
        /// </summary>
        /// <param name="cardImage"></param>
        /// <returns></returns>
        private Rank ScanRank(Bitmap bmp)
        {
            //Initiliaze template matching class with 0.75 threshold
            ExhaustiveTemplateMatching templateMatchin = new ExhaustiveTemplateMatching(0.99f);
            Rank rank = Rank.NOT_RECOGNIZED;

            if (templateMatchin.ProcessImage(bmp, j).Length > 0) //If Jack template matches
                rank = Rank.Jack;
            if (templateMatchin.ProcessImage(bmp, k).Length > 0)//If King template matches
                rank = Rank.King;
            if (templateMatchin.ProcessImage(bmp, q).Length > 0)//If Queen template matches
                rank = Rank.Queen;

            if (templateMatchin.ProcessImage(bmp, Resources.A).Length > 0)//If Queen template matches
                rank = Rank.Ace;
            if (templateMatchin.ProcessImage(bmp, Resources._2).Length > 0)//If Queen template matches
                rank = Rank.Two;
            if (templateMatchin.ProcessImage(bmp, Resources._3).Length > 0)//If Queen template matches
                rank = Rank.Three;
            if (templateMatchin.ProcessImage(bmp, Resources._4).Length > 0)//If Queen template matches
                rank = Rank.Four;
            if (templateMatchin.ProcessImage(bmp, Resources._5).Length > 0)//If Queen template matches
                rank = Rank.Five;
            if (templateMatchin.ProcessImage(bmp, Resources._6).Length > 0)//If Queen template matches
                rank = Rank.Six;
            if (templateMatchin.ProcessImage(bmp, Resources._7).Length > 0)//If Queen template matches
                rank = Rank.Seven;
            if (templateMatchin.ProcessImage(bmp, Resources._8).Length > 0)//If Queen template matches
                rank = Rank.Eight;
            if (templateMatchin.ProcessImage(bmp, Resources._9).Length > 0)//If Queen template matches
                rank = Rank.Nine;
            if (templateMatchin.ProcessImage(bmp, Resources._10).Length > 0)//If Queen template matches
                rank = Rank.Ten;
            return rank;
        }
        
        
        /// <summary>
        /// Scans dominant color on image and returns it.
        /// Crops rank part on image and analyzes suit part on image
        /// </summary>
        /// <param name="bmp">Bitmap to be scanned</param>
        /// <returns>Returns 'B' for black , 'R' for red</returns>
        private char ScanColor_(Bitmap bmp)
        {
            char color = 'B';
            //Crop rank part
            /*
            Crop crop = new Crop(new Rectangle(0, bmp.Height / 2, bmp.Width, bmp.Height / 2));
            bmp = crop.Apply(bmp);
            */
            //Bitmap temp = commonSeq.Apply(bmp); //Apply filters

            ////Find suit blob on image
            //BlobCounter counter = new BlobCounter();
            //counter.ProcessImage(temp);
            //Blob[] blobs = counter.GetObjectsInformation();

            //if (blobs.Length > 0) //If blobs found
            //{
            //    Blob max = blobs[0];
            //    //Find blob whose size is biggest 
            //    foreach (Blob blob in blobs)
            //    {
            //        if (blob.Rectangle.Height > max.Rectangle.Height)
            //            max = blob;
            //        else if (blob.Rectangle.Height == max.Rectangle.Height)
            //            max = blob.Rectangle.Width > max.Rectangle.Width ? blob : max;
            //    }
            //    QuadrilateralTransformation trans = new QuadrilateralTransformation();
            //    trans.SourceQuadrilateral = PointsCloud.FindQuadrilateralCorners(counter.GetBlobsEdgePoints(max));
            //    bmp = trans.Apply(bmp); //Extract suit
            //}
            //Lock Bits for processing
            BitmapData imageData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
               ImageLockMode.ReadOnly, bmp.PixelFormat);
            int totalRed = 0;
            int totalBlack = 0;

            unsafe
            {
                //Count red and black pixels
                try
                {
                    UnmanagedImage img = new UnmanagedImage(imageData);

                    int height = img.Height;
                    int width = img.Width;
                    int pixelSize = (img.PixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;
                    byte* p = (byte*)img.ImageData.ToPointer();

                    // for each line
                    for (int y = 0; y < height; y++)
                    {
                        // for each pixel
                        for (int x = 0; x < width; x++, p += pixelSize)
                        {
                            int r = (int)p[RGB.R]; //Red pixel value
                            int g = (int)p[RGB.G]; //Green pixel value
                            int b = (int)p[RGB.B]; //Blue pixel value

                            if (r > g + b) //If red component is bigger then total of green and blue component
                                totalRed++;  //then its red

                            if (r <= g + b && r < 50 && g < 50 && b < 50) //If all components less 50
                                totalBlack++; //then its black
                        }
                    }

                }
                finally
                {
                    bmp.UnlockBits(imageData); //Unlock
                }
            }
            if (totalRed > totalBlack) //If red is dominant
                color = 'R'; //Set color as Red

            return color;
        }

        private char ScanColor(Bitmap bmp)
        {
            char color = 'B';
            int totalRed = 0;
            int totalBlack = 0;
            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color pixelColor = bmp.GetPixel(x, y);
                    var r = pixelColor.R;
                    var g = pixelColor.G;
                    var b = pixelColor.B;


                    if (r > g + b) //If red component is bigger then total of green and blue component
                        totalRed++;  //then its red

                    if (r <= g + b && r < 50 && g < 50 && b < 50) //If all components less 50
                        totalBlack++; //then its black

                }
            }

            if (totalRed > totalBlack) //If red is dominant
                color = 'R'; //Set color as Red

            return color;
        }
        /// <summary>
        /// Returns point who has minimum x and y
        /// </summary>
        /// <param name="points">Points to be searched</param>
        /// <returns>Returns point who has minimum x and y</returns>
        public static Point GetStringPoint(Point[] points)
        {
            Point[] tempArr = new Point[points.Length];
            Array.Copy(points, tempArr, points.Length);
            Array.Sort(tempArr, new PointComparer());

            return tempArr[0].X < tempArr[1].X ? tempArr[0] : tempArr[1];
        }
        /// <summary>
        /// Detects and recognizes cards from source image
        /// </summary>
        /// <param name="source">Source image to be scanned</param>
        /// <returns>Recognized Cards</returns>
        public CardCollection Recognize(Bitmap source, string filePath, int id,
            int minSize, Rectangle suitRect, Rectangle rankRect
            )
        {
            CardCollection collection = new CardCollection();  //Collection that will hold cards
            Bitmap temp = source.Clone() as Bitmap; //Clone image to keep original image

            FiltersSequence seq = new FiltersSequence();
            seq.Add(Grayscale.CommonAlgorithms.BT709);  //First add  grayScaling filter
            seq.Add(new OtsuThreshold()); //Then add binarization(thresholding) filter
            temp = seq.Apply(source); // Apply filters on source image
            
            //if (!string.IsNullOrEmpty(fileName))
            //{
            //    temp.Save(fileName, ImageFormat.Bmp);
            //}
            //Extract blobs from image whose size width and height larger than 150
            BlobCounter extractor = new BlobCounter();
            extractor.FilterBlobs = true;
            extractor.MinWidth = extractor.MinHeight = minSize;//TODO card size
            //extractor.MaxWidth = extractor.MaxHeight = 70;//TODO card size
            extractor.ProcessImage(temp);

            //Will be used transform(extract) cards on source image 
            //QuadrilateralTransformation quadTransformer = new QuadrilateralTransformation();

            

            foreach (Blob blob in extractor.GetObjectsInformation())
            {
                var cardImg = source.Clone(blob.Rectangle, PixelFormat.DontCare);

                
                Card card = new Card(cardImg); //Create Card Object

                

                Bitmap suitBmp = card.GetPart(suitRect);
                char color = ScanColor(suitBmp); //Scan color

                
                seq.Clear();

                seq.Add(Grayscale.CommonAlgorithms.BT709);
                seq.Add(new OtsuThreshold());
                suitBmp = seq.Apply(suitBmp);

                card.Suit = ScanSuit(suitBmp, color); //Scan suit of face card


                Bitmap rankBmp = card.GetPart(rankRect);
                seq.Clear();

                seq.Add(Grayscale.CommonAlgorithms.BT709);
                seq.Add(new OtsuThreshold());
                rankBmp = seq.Apply(rankBmp);

                //var ext = new BlobsFiltering(0, 0, 40, 40);
                //ext.ApplyInPlace(rankBmp);
                card.Rank = ScanRank(rankBmp); //Scan Rank of non-face card

                //if (card.Rank == Rank.NOT_RECOGNIZED)
                //{
                //    if (!string.IsNullOrEmpty(filePath))
                //    {
                //        while (File.Exists(filePath + id + ".bmp"))
                //            id++;
                //        top.Save(filePath + id + ".bmp", ImageFormat.Bmp);
                //    }
                //}

                if(card.Rank != Rank.NOT_RECOGNIZED && card.Suit != Suit.NOT_RECOGNIZED)
                    collection.Add(card); //Add card to collection
            }

            collection.SortByRank();
            return collection;
        }

        public Button RecognizeOneButton(Bitmap source, Rectangle rect, string filePath, int id)
        {
            var seq = new FiltersSequence();
            var card = new Button(source); //Create Card Object

            card.Rect = rect;

            seq.Clear();

            seq.Add(Grayscale.CommonAlgorithms.BT709);
            seq.Add(new OtsuThreshold());
            source = seq.Apply(source);

            card.Tip = ScanButtonTip(source); //Scan Rank of non-face card

            //if (card.Tip == ButtonTip.NOT_RECOGNIZED)
            //{
            //    if (!string.IsNullOrEmpty(filePath))
            //    {
            //        while (File.Exists(filePath + id + ".bmp"))
            //            id++;
            //        top.Save(filePath + id + ".bmp", ImageFormat.Bmp);
            //    }
            //}

            return card;
        }


        private TesseractEngine _engine;
        public string RecognizeText(Bitmap source)
        {
            try
            {
                //var res = IsNotNumber(source);
                //if (res)
                //{
                //    return "none";
                //}

                var temp = source.Clone() as Bitmap; //Clone image to keep original image

                var seq = new FiltersSequence();

                //seq.Add(new Grayscale(0.7, 0.7, 0.7));
                seq.Add(Grayscale.CommonAlgorithms.BT709);
                //seq.Add(new OtsuThreshold()); //Then add binarization(thresholding) filter
                seq.Add(new Invert());
                seq.Add(new ResizeBilinear(source.Width * 2 , source.Height * 2));
                temp = seq.Apply(source); // Apply filters on source image

                //using (var engine = new TesseractEngine(@"./tessdata", "rus", EngineMode.Default))
                {
                    
                    using (var page = _engine.Process(temp, PageSegMode.SingleLine))
                    {
                        var text = page.GetText();
                        var conf = page.GetMeanConfidence();

                        //Ex.Report(new Exception(text));
                        //if (conf < 0.8)
                        //    return "none";
                        return text;
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                Ex.Report(e);
                return "";
            }
        }

        public string RecognizeTextSmall(Bitmap source)
        {
            try
            {
                //var res = IsNotNumber(source);
                //if (res)
                //{
                //    return "none";
                //}

                var temp = source.Clone() as Bitmap; //Clone image to keep original image

                var seq = new FiltersSequence();

                //seq.Add(new Grayscale(0.7, 0.7, 0.7));
                seq.Add(Grayscale.CommonAlgorithms.BT709);
                //seq.Add(new OtsuThreshold()); //Then add binarization(thresholding) filter
                seq.Add(new Invert());
                //seq.Add(new ResizeBilinear(source.Width * 2, source.Height * 2));
                temp = seq.Apply(source); // Apply filters on source image

                //using (var engine = new TesseractEngine(@"./tessdata", "rus", EngineMode.Default))
                {

                    using (var page = _engine.Process(temp, PageSegMode.SingleLine))
                    {
                        var text = page.GetText();
                        var conf = page.GetMeanConfidence();

                        //Ex.Report(new Exception(text));
                        //if (conf < 0.5)
                        //    return "none";
                        return text;
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                Ex.Report(e);
                return "";
            }
        }

        public string RecognizeText2(Bitmap source, string filePath, int id)
        {
            try
            {
                //var res = IsNotNumber(source);
                //if (res)
                //{
                //    return "none";
                //}

                var temp = source.Clone() as Bitmap; //Clone image to keep original image

                var seq = new FiltersSequence();

                //seq.Add(new Grayscale(0.7, 0.7, 0.7));
                seq.Add(Grayscale.CommonAlgorithms.BT709);
                
                //seq.Add(new Invert());
                //seq.Add(new ResizeBilinear(source.Width * 2, source.Height * 2));
                //seq.Add(new OtsuThreshold()); //Then add binarization(thresholding) filter
                temp = seq.Apply(source); // Apply filters on source image

                //using (var engine = new TesseractEngine(@"./tessdata", "rus", EngineMode.Default))
                {

                    using (var page = _engine.Process(temp))
                    {
                        var text = page.GetText();
                        var conf = page.GetMeanConfidence();

                        //Ex.Report(new Exception(text));
                        return text;
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                Ex.Report(e);
                return "";
            }
        }

        //TODO not fucking working
        public string RecognizeBet(Bitmap source)
        {
            try
            {
                //var res = IsNotNumber(source);
                //if (res)
                //{
                //    return "none";
                //}

                var temp = source.Clone() as Bitmap; //Clone image to keep original image


                //*
                var seq = new FiltersSequence();

                seq.Add(new Grayscale(0, 1, 0));
                //seq.Add(Grayscale.CommonAlgorithms.BT709);
                //seq.Add(new ResizeBilinear(source.Width * 2, source.Height * 2));
                //seq.Add(new OtsuThreshold()); //Then add binarization(thresholding) filter
                //seq.Add(new Threshold(50));
                //seq.Add(new Invert());
                
                temp = seq.Apply(source); // Apply filters on source image
                //*/

                //var extractor = new BlobCounter();
                //extractor.FilterBlobs = true;
                
                //extractor.MaxHeight = 15;
                //extractor.MinHeight = 0;
                ////extractor.MaxWidth = 10;
                ////extractor.MinWidth = 10;
                ////extractor.BackgroundThreshold = Color.Green;
                //extractor.ProcessImage(temp);

                //////Will be used transform(extract) cards on source image 
                ////QuadrilateralTransformation quadTransformer = new QuadrilateralTransformation();

                //foreach (Blob blob in extractor.GetObjectsInformation())
                //{
                //    ////Get Edge points of card
                //    //List<IntPoint> edgePoints = extractor.GetBlobsEdgePoints(blob);
                //    ////Calculate/Find corners of card on source image from edge points
                //    //List<IntPoint> corners = PointsCloud.FindQuadrilateralCorners(edgePoints);

                //    //var cardImg = source.Clone(blob.Rectangle, PixelFormat.DontCare);
                //    var cardImg = temp.Clone(blob.Rectangle, PixelFormat.DontCare);
                //}
                //using (var engine = new TesseractEngine(@"./tessdata", "rus", EngineMode.Default))
                {
                    //_engine.SetVariable("tessedit_char_whitelist", "$.,0123456789");
                    using (var page = _engine.Process(temp))
                    {
                        var text = page.GetText();
                        var conf = page.GetMeanConfidence();

                        //Ex.Report(new Exception(text));
                        //if (conf < 0.8)
                        //    return "none";
                        return text;
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                Ex.Report(e);
                return "";
            }
        }

        private bool IsNotNumber(Bitmap source)
        {
            var template = Resources.PlayerEmpty;
            var temp = source.Clone() as Bitmap; //Clone image to keep original image

            var seq = new FiltersSequence();
            seq.Add(Grayscale.CommonAlgorithms.BT709);
            temp = seq.Apply(source); // Apply filters on source image

            var templ = seq.Apply(template);

            var templateMatchin = new ExhaustiveTemplateMatching(0.9f);
            TemplateMatch[] templates;
            if (
                temp.Width < template.Width
                ||
                temp.Height < template.Height
                )
                templates = templateMatchin.ProcessImage(templ, temp);
            else
                templates = templateMatchin.ProcessImage(temp, templ);

            var res = templates.Length > 0;


            template = Resources.PlayerMissing;
            templ = seq.Apply(template);

            templateMatchin = new ExhaustiveTemplateMatching(0.9f);
            TemplateMatch[] templates2;
            if (
                temp.Width < template.Width
                ||
                temp.Height < template.Height
                )
                templates2 = templateMatchin.ProcessImage(templ, temp);
            else
                templates2 = templateMatchin.ProcessImage(temp, templ);

            res |= templates2.Length > 0;
            return res;
        }

        private ButtonTip ScanButtonTip(Bitmap bmp)
        {
            //Initiliaze template matching class with 0.75 threshold
            ExhaustiveTemplateMatching templateMatchin = new ExhaustiveTemplateMatching(0.9f);
            var tip = ButtonTip.NOT_RECOGNIZED;
            if (bmp.Size.Height <= Resources.chk.Size.Height || bmp.Size.Width <= Resources.chk.Size.Width)
            {
                return tip;
            }

            if (templateMatchin.ProcessImage(bmp, Resources.chk).Length > 0) //If Jack template matches
                tip = ButtonTip.Check;
            if (templateMatchin.ProcessImage(bmp, Resources.fld).Length > 0)//If King template matches
                tip = ButtonTip.Fold;
            if (templateMatchin.ProcessImage(bmp, Resources.bt).Length > 0)//If Queen template matches
                tip = ButtonTip.Bet;
            if (templateMatchin.ProcessImage(bmp, Resources.cll).Length > 0)//If Queen template matches
                tip = ButtonTip.Call;
            if (templateMatchin.ProcessImage(bmp, Resources.rz).Length > 0)//If Queen template matches
                tip = ButtonTip.Raise;

            
            return tip;
        }

        public bool ScanByTemplate(Bitmap source, Bitmap template)
        {
            var temp = source.Clone() as Bitmap; //Clone image to keep original image
            var tempTempl = template;

            var seq = new FiltersSequence();
            seq.Add(Grayscale.CommonAlgorithms.BT709);  //First add  grayScaling filter
            //seq.Add(new Threshold(200));
            seq.Add(new OtsuThreshold()); //Then add binarization(thresholding) filter
            temp = seq.Apply(source); // Apply filters on source image

            
            //tempTempl = seq.Apply(template); // Apply filters on source image

            var templateMatchin = new ExhaustiveTemplateMatching(0.9f);
            TemplateMatch[] templates;
            templates = templateMatchin.ProcessImage(temp, tempTempl);


            return templates.Length > 0;
        }

        public bool IsButton(Bitmap source, Bitmap template)
        {
            template = Resources.dealer;
            var temp = source.Clone() as Bitmap; //Clone image to keep original image

            //var seq = new FiltersSequence();
            //seq.Add(Grayscale.CommonAlgorithms.BT709);
            ////seq.Add(new OtsuThreshold()); //Then add binarization(thresholding) filter
            //temp = seq.Apply(source); // Apply filters on source image
            //var templ = seq.Apply(template);

            var templ = template;

            //var extractor = new BlobCounter();
            //extractor.ProcessImage(temp);

            ////Will be used transform(extract) cards on source image 
            ////QuadrilateralTransformation quadTransformer = new QuadrilateralTransformation();



            //foreach (Blob blob in extractor.GetObjectsInformation())
            //{
                
            //    var cardImg = source.Clone(blob.Rectangle, PixelFormat.DontCare);
            //}

            //return false;
            var templateMatchin = new ExhaustiveTemplateMatching(0.8f);
            TemplateMatch[] templates;
            if (
                temp.Width < template.Width
                ||
                temp.Height < template.Height
                )
                templates = templateMatchin.ProcessImage(templ, temp);
            else
                templates = templateMatchin.ProcessImage(temp, templ);

            return templates.Length > 0;

        }

        public bool PlayerHasCards(Bitmap bmpFl)
        {
            var template = Resources.playerCard;
            var temp = bmpFl.Clone() as Bitmap; //Clone image to keep original image

            //var seq = new FiltersSequence();
            //seq.Add(Grayscale.CommonAlgorithms.BT709);
            ////seq.Add(new OtsuThreshold()); //Then add binarization(thresholding) filter
            //temp = seq.Apply(source); // Apply filters on source image
            //var templ = seq.Apply(template);

            var templ = template;

            //var extractor = new BlobCounter();
            //extractor.ProcessImage(temp);

            ////Will be used transform(extract) cards on source image 
            ////QuadrilateralTransformation quadTransformer = new QuadrilateralTransformation();



            //foreach (Blob blob in extractor.GetObjectsInformation())
            //{

            //    var cardImg = source.Clone(blob.Rectangle, PixelFormat.DontCare);
            //}

            //return false;
            var templateMatchin = new ExhaustiveTemplateMatching(0.8f);
            TemplateMatch[] templates;
            if (
                temp.Width < template.Width
                ||
                temp.Height < template.Height
                )
                templates = templateMatchin.ProcessImage(templ, temp);
            else
                templates = templateMatchin.ProcessImage(temp, templ);

            return templates.Length > 0;
        }
    }
}
