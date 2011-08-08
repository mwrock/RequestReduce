using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace RequestReduce.Utilities.Quantizer
{
    public interface IWuQuantizer
    {
        Image QuantizeImage(Bitmap image);
    }

    public class WuQuantizer : IWuQuantizer
    {
        private readonly Color BackgroundColor = SystemColors.Control;
        private readonly Double[] Factors = PrecalculateFactors();

        private const int TransparencyThreshold = 100;
        private const Int32 MaxColor = 512;
        private const Int32 Red = 2;
        private const Int32 Green = 1;
        private const Int32 Blue = 0;
        private const Int32 SideSize = 33;
        private const Int32 MaxSideIndex = 32;
        private const Int32 MaxVolume = SideSize * SideSize * SideSize;

        private Int64[, ,] weights;
        private Int64[, ,] momentsRed;
        private Int64[, ,] momentsGreen;
        private Int64[, ,] momentsBlue;
        private Single[, ,] moments;

        private Int32[] tag;
        private Int32[] quantizedPixels;
        private Int32[] table;
        private Color[] pixels;

        private Int32 imageSize;
        private Int32 pixelIndex;

        private Int32[] reds;
        private Int32[] greens;
        private Int32[] blues;
        private Int32[] sums;
        private Int32[] indices;

        private WuColorCube[] cubes;


        public Image QuantizeImage(Bitmap image)
        {
            Prepare(image);
            ProcessImagePixels(image);

            // creates a target bitmap in 8-bit format
            var result = new Bitmap(image.Width, image.Height, PixelFormat.Format8bppIndexed);

            List<Color> palette = GetQuantizedPalette(256);
            SetPalette(palette, result);

            // processes the quantization
            ProcessImagePixels(image, result);

            return result;
        }

        private void ProcessImagePixels(Bitmap sourceImage, Bitmap targetImage)
        {
            // locks the image data
            Rectangle sourceBounds = Rectangle.FromLTRB(0, 0, sourceImage.Width, sourceImage.Height);
            Rectangle targetBounds = Rectangle.FromLTRB(0, 0, targetImage.Width, targetImage.Height);
            BitmapData sourceData = null;
            BitmapData targetData = null;
            try
            {
                sourceData = sourceImage.LockBits(sourceBounds, ImageLockMode.ReadOnly, sourceImage.PixelFormat);
                targetData = targetImage.LockBits(targetBounds, ImageLockMode.WriteOnly, targetImage.PixelFormat);

                // source image - calculates all the values necessary for enumeration
                const byte sourceBitDepth = 32;
                Int32 sourceByteLength = sourceData.Stride < 0 ? -sourceData.Stride : sourceData.Stride;
                Int32 sourceByteCount = Math.Max(1, sourceBitDepth >> 3);
                Int32 sourceImageWidth = sourceImage.Width;
                Int32 sourceImageHeight = sourceImage.Height;
                Int32 sourceSize = sourceByteLength * sourceImageHeight;
                Int32 sourceOffset = 0;

                // target image - calculates all the values necessary for enumeration
                Byte targetBitDepth = 8;
                Int32 targetByteLength = targetData.Stride < 0 ? -targetData.Stride : targetData.Stride;
                Int32 targetByteCount = Math.Max(1, targetBitDepth >> 3);
                Int32 targetImageHeight = targetImage.Height;
                Int32 targetSize = targetByteLength * targetImageHeight;
                Int32 targetOffset = 0;

                // initializes the transfer buffers, and current pixel offset
                Byte[] sourceBuffer = new Byte[sourceSize];
                Byte[] targetBuffer = new Byte[targetSize];
                Byte[] sourceValue = new Byte[sourceByteCount];
                Byte[] targetValue = new Byte[targetByteCount];

                // transfers whole image to a memory
                Marshal.Copy(sourceData.Scan0, sourceBuffer, 0, sourceSize);

                // enumerates the pixels row by row
                for (Int32 y = 0; y < sourceImageHeight; y++)
                {
                    // aquires the pointer to the first row pixel
                    Int32 sourceIndex = 0, targetIndex = 0;

                    // enumerates the buffer per pixel
                    for (Int32 x = 0; x < sourceImageWidth; x++)
                    {
                        Int32 sourceIndexOffset = sourceIndex >> 3;
                        Int32 targetIndexOffset = targetIndex >> 3;

                        // when read is allowed, retrieves current value (in bytes)
                        for (Int32 valueIndex = 0; valueIndex < sourceByteCount; valueIndex++)
                        {
                            sourceValue[valueIndex] = sourceBuffer[sourceOffset + valueIndex + sourceIndexOffset];
                        }

                        // moves to next pixel for both images
                        int alpha = sourceValue[3];
                        int red = sourceValue[2];
                        int green = sourceValue[1];
                        int blue = sourceValue[0];
                        Int32 argb = alpha << 24 | red << 16 | green << 8 | blue;
                        Color color = Color.FromArgb(argb);

                        //color = color.A < TransparencyThreshold ? color : ConvertAlpha(color);
                        Int32 paletteIndex = GetPaletteIndex(color);
                        if (color.A < TransparencyThreshold)
                            paletteIndex = targetImage.Palette.Entries.Length - 1;

                        targetValue[0] = (Byte) paletteIndex;

                        // when write is allowed, copies the value back to the row buffer
                        for (Int32 valueIndex = 0; valueIndex < targetByteCount; valueIndex++)
                        {
                            targetBuffer[targetOffset + valueIndex + targetIndexOffset] = targetValue[valueIndex];
                        }

                        sourceIndex += sourceBitDepth;
                        targetIndex += targetBitDepth;
                    }

                    // increases offset by a row
                    sourceOffset += sourceByteLength;
                    targetOffset += targetByteLength;
                }

                Marshal.Copy(targetBuffer, 0, targetData.Scan0, targetSize);
            }
            finally
            {
                // releases the lock on the image data
                if(sourceData != null)
                    sourceImage.UnlockBits(sourceData);
                if(targetData != null)
                    targetImage.UnlockBits(targetData);
            }
        }

        private void AddColor(Color color)
        {
            //color = ConvertAlpha(color);

            Int32 indexRed = (color.R >> 3) + 1;
            Int32 indexGreen = (color.G >> 3) + 1;
            Int32 indexBlue = (color.B >> 3) + 1;

            weights[indexRed, indexGreen, indexBlue]++;
            momentsRed[indexRed, indexGreen, indexBlue] += color.R;
            momentsGreen[indexRed, indexGreen, indexBlue] += color.G;
            momentsBlue[indexRed, indexGreen, indexBlue] += color.B;
            moments[indexRed, indexGreen, indexBlue] += table[color.R] + table[color.G] + table[color.B];

            quantizedPixels[pixelIndex] = (indexRed << 10) + (indexRed << 6) + indexRed + (indexGreen << 5) + indexGreen + indexBlue;
            pixels[pixelIndex] = color;
            pixelIndex++;
        }

        private void ProcessImagePixels(Bitmap sourceImage)
        {
            // locks the image data
            Rectangle bounds = Rectangle.FromLTRB(0, 0, sourceImage.Width, sourceImage.Height);
            BitmapData data = sourceImage.LockBits(bounds, ImageLockMode.ReadOnly, sourceImage.PixelFormat);

            try
            {
                // source image - calculates all the values necessary for enumeration
                const byte bitDepth = 32;
                Int32 byteLength = data.Stride < 0 ? -data.Stride : data.Stride;
                Int32 byteCount = Math.Max(1, bitDepth >> 3);
                Int32 imageWidth = sourceImage.Width;
                Int32 imageHeight = sourceImage.Height;
                Int32 size = byteLength * imageHeight;
                Int32 offset = 0;

                // initializes the transfer buffers, and current pixel offset
                Byte[] buffer = new Byte[size];
                Byte[] value = new Byte[byteCount];

                // transfers whole image to a memory
                Marshal.Copy(data.Scan0, buffer, 0, size);

                // enumerates the pixels row by row
                for (Int32 y = 0; y < imageHeight; y++)
                {
                    // aquires the pointer to the first row pixel
                    Int32 index = 0;

                    // enumerates the buffer per pixel
                    for (Int32 x = 0; x < imageWidth; x++)
                    {
                        Int32 indexOffset = index >> 3;

                        for (Int32 valueIndex = 0; valueIndex < byteCount; valueIndex++)
                        {
                            value[valueIndex] = buffer[offset + valueIndex + indexOffset];
                        }

                        // enumerates the pixel, and returns the control to the outside
                        int alpha = value[3];
                        int red = value[2];
                        int green = value[1];
                        int blue = value[0];
                        Int32 argb = alpha << 24 | red << 16 | green << 8 | blue;
                        AddColor(Color.FromArgb(argb));

                        index += bitDepth;
                    }

                    // increases offset by a row
                    offset += byteLength;
                }

                Marshal.Copy(buffer, 0, data.Scan0, size);
            }
            finally
            {
                // releases the lock on the image data
                sourceImage.UnlockBits(data);
            }
        }


        private Int32 GetPaletteIndex(Color color)
        {
			if(color.A < TransparencyThreshold)
			{
				pixelIndex++;
				return 255;
			}
			return indices[pixelIndex++];
        }

        private static Double[] PrecalculateFactors()
        {
            Double[] result = new Double[256];

            for (Int32 value = 0; value < 256; value++)
            {
                result[value] = value / 255.0;
            }

            return result;
        }

        private Color ConvertAlpha(Color color)
        {
            Color result = color;

            if (color.A < 255)
            {
                // performs a alpha blending (second color is BackgroundColor, by default a Control color)
                Double colorFactor = Factors[color.A];
                Double backgroundFactor = Factors[255 - color.A];
                Int32 red = (Int32)(color.R * colorFactor + BackgroundColor.R * backgroundFactor);
                Int32 green = (Int32)(color.G * colorFactor + BackgroundColor.G * backgroundFactor);
                Int32 blue = (Int32)(color.B * colorFactor + BackgroundColor.B * backgroundFactor);
                Int32 argb = 255 << 24 | red << 16 | green << 8 | blue;
                result = Color.FromArgb(argb);
            }

            return result;
        }

        private void SetPalette(List<Color> palette, Bitmap result)
        {

            // retrieves a target image palette
            ColorPalette imagePalette = result.Palette;

            // copies all color entries
            for (Int32 index = 0; index < palette.Count; index++)
            {
                imagePalette.Entries[index] = palette[index];
            }

            // assigns the palette to the target image
            result.Palette = imagePalette;
        }

        private void CalculateMoments()
        {
            Int64[] area = new Int64[SideSize];
            Int64[] areaRed = new Int64[SideSize];
            Int64[] areaGreen = new Int64[SideSize];
            Int64[] areaBlue = new Int64[SideSize];
            Single[] area2 = new Single[SideSize];

            for (Int32 redIndex = 1; redIndex <= MaxSideIndex; ++redIndex)
            {
                for (Int32 index = 0; index <= MaxSideIndex; ++index)
                {
                    area[index] = 0;
                    areaRed[index] = 0;
                    areaGreen[index] = 0;
                    areaBlue[index] = 0;
                    area2[index] = 0;
                }

                for (Int32 greenIndex = 1; greenIndex <= MaxSideIndex; ++greenIndex)
                {
                    Int64 line = 0;
                    Int64 lineRed = 0;
                    Int64 lineGreen = 0;
                    Int64 lineBlue = 0;
                    Single line2 = 0.0f;

                    for (Int32 blueIndex = 1; blueIndex <= MaxSideIndex; ++blueIndex)
                    {
                        line += weights[redIndex, greenIndex, blueIndex];
                        lineRed += momentsRed[redIndex, greenIndex, blueIndex];
                        lineGreen += momentsGreen[redIndex, greenIndex, blueIndex];
                        lineBlue += momentsBlue[redIndex, greenIndex, blueIndex];
                        line2 += moments[redIndex, greenIndex, blueIndex];

                        area[blueIndex] += line;
                        areaRed[blueIndex] += lineRed;
                        areaGreen[blueIndex] += lineGreen;
                        areaBlue[blueIndex] += lineBlue;
                        area2[blueIndex] += line2;

                        weights[redIndex, greenIndex, blueIndex] = weights[redIndex - 1, greenIndex, blueIndex] + area[blueIndex];
                        momentsRed[redIndex, greenIndex, blueIndex] = momentsRed[redIndex - 1, greenIndex, blueIndex] + areaRed[blueIndex];
                        momentsGreen[redIndex, greenIndex, blueIndex] = momentsGreen[redIndex - 1, greenIndex, blueIndex] + areaGreen[blueIndex];
                        momentsBlue[redIndex, greenIndex, blueIndex] = momentsBlue[redIndex - 1, greenIndex, blueIndex] + areaBlue[blueIndex];
                        moments[redIndex, greenIndex, blueIndex] = moments[redIndex - 1, greenIndex, blueIndex] + area2[blueIndex];
                    }
                }
            }
        }

        private Int64 Volume(WuColorCube cube, Int64[, ,] moment)
        {
            return moment[cube.RedMaximum, cube.GreenMaximum, cube.BlueMaximum] -
                   moment[cube.RedMaximum, cube.GreenMaximum, cube.BlueMinimum] -
                   moment[cube.RedMaximum, cube.GreenMinimum, cube.BlueMaximum] +
                   moment[cube.RedMaximum, cube.GreenMinimum, cube.BlueMinimum] -
                   moment[cube.RedMinimum, cube.GreenMaximum, cube.BlueMaximum] +
                   moment[cube.RedMinimum, cube.GreenMaximum, cube.BlueMinimum] +
                   moment[cube.RedMinimum, cube.GreenMinimum, cube.BlueMaximum] -
                   moment[cube.RedMinimum, cube.GreenMinimum, cube.BlueMinimum];
        }

        private Int64 Top(WuColorCube cube, Int32 direction, Int32 position, Int64[, ,] moment)
        {
            switch (direction)
            {
                case Red:
                    return (moment[position, cube.GreenMaximum, cube.BlueMaximum] -
                            moment[position, cube.GreenMaximum, cube.BlueMinimum] -
                            moment[position, cube.GreenMinimum, cube.BlueMaximum] +
                            moment[position, cube.GreenMinimum, cube.BlueMinimum]);

                case Green:
                    return (moment[cube.RedMaximum, position, cube.BlueMaximum] -
                            moment[cube.RedMaximum, position, cube.BlueMinimum] -
                            moment[cube.RedMinimum, position, cube.BlueMaximum] +
                            moment[cube.RedMinimum, position, cube.BlueMinimum]);

                case Blue:
                    return (moment[cube.RedMaximum, cube.GreenMaximum, position] -
                            moment[cube.RedMaximum, cube.GreenMinimum, position] -
                            moment[cube.RedMinimum, cube.GreenMaximum, position] +
                            moment[cube.RedMinimum, cube.GreenMinimum, position]);

                default:
                    return 0;
            }
        }

        private Int64 Bottom(WuColorCube cube, Int32 direction, Int64[, ,] moment)
        {
            switch (direction)
            {
                case Red:
                    return (-moment[cube.RedMinimum, cube.GreenMaximum, cube.BlueMaximum] +
                             moment[cube.RedMinimum, cube.GreenMaximum, cube.BlueMinimum] +
                             moment[cube.RedMinimum, cube.GreenMinimum, cube.BlueMaximum] -
                             moment[cube.RedMinimum, cube.GreenMinimum, cube.BlueMinimum]);

                case Green:
                    return (-moment[cube.RedMaximum, cube.GreenMinimum, cube.BlueMaximum] +
                             moment[cube.RedMaximum, cube.GreenMinimum, cube.BlueMinimum] +
                             moment[cube.RedMinimum, cube.GreenMinimum, cube.BlueMaximum] -
                             moment[cube.RedMinimum, cube.GreenMinimum, cube.BlueMinimum]);

                case Blue:
                    return (-moment[cube.RedMaximum, cube.GreenMaximum, cube.BlueMinimum] +
                             moment[cube.RedMaximum, cube.GreenMinimum, cube.BlueMinimum] +
                             moment[cube.RedMinimum, cube.GreenMaximum, cube.BlueMinimum] -
                             moment[cube.RedMinimum, cube.GreenMinimum, cube.BlueMinimum]);
                default:
                    return 0;
            }
        }

        private Single Maximize(WuColorCube cube, Int32 direction, Int32 first, Int32 last, Int32[] cut, Int64 wholeRed, Int64 wholeGreen, Int64 wholeBlue, Int64 wholeWeight)
        {
            Int64 bottomRed = Bottom(cube, direction, momentsRed);
            Int64 bottomGreen = Bottom(cube, direction, momentsGreen);
            Int64 bottomBlue = Bottom(cube, direction, momentsBlue);
            Int64 bottomWeight = Bottom(cube, direction, weights);

            Single result = 0.0f;
            cut[0] = -1;

            for (Int32 position = first; position < last; ++position)
            {
                // determines the cube cut at a certain position
                Int64 halfRed = bottomRed + Top(cube, direction, position, momentsRed);
                Int64 halfGreen = bottomGreen + Top(cube, direction, position, momentsGreen);
                Int64 halfBlue = bottomBlue + Top(cube, direction, position, momentsBlue);
                Int64 halfWeight = bottomWeight + Top(cube, direction, position, weights);

                // the cube cannot be cut at bottom (this would lead to empty cube)
                if (halfWeight != 0)
                {
                    Single halfDistance = halfRed * halfRed + halfGreen * halfGreen + halfBlue * halfBlue;
                    Single temp = halfDistance / halfWeight;

                    halfRed = wholeRed - halfRed;
                    halfGreen = wholeGreen - halfGreen;
                    halfBlue = wholeBlue - halfBlue;
                    halfWeight = wholeWeight - halfWeight;

                    if (halfWeight != 0)
                    {
                        halfDistance = halfRed * halfRed + halfGreen * halfGreen + halfBlue * halfBlue;
                        temp += halfDistance / halfWeight;

                        if (temp > result)
                        {
                            result = temp;
                            cut[0] = position;
                        }
                    }
                }
            }

            return result;
        }

        private Boolean Cut(WuColorCube first, WuColorCube second)
        {
            Int32 direction;

            Int32[] cutRed = { 0 };
            Int32[] cutGreen = { 0 };
            Int32[] cutBlue = { 0 };

            Int64 wholeRed = Volume(first, momentsRed);
            Int64 wholeGreen = Volume(first, momentsGreen);
            Int64 wholeBlue = Volume(first, momentsBlue);
            Int64 wholeWeight = Volume(first, weights);

            Single maxRed = Maximize(first, Red, first.RedMinimum + 1, first.RedMaximum, cutRed, wholeRed, wholeGreen, wholeBlue, wholeWeight);
            Single maxGreen = Maximize(first, Green, first.GreenMinimum + 1, first.GreenMaximum, cutGreen, wholeRed, wholeGreen, wholeBlue, wholeWeight);
            Single maxBlue = Maximize(first, Blue, first.BlueMinimum + 1, first.BlueMaximum, cutBlue, wholeRed, wholeGreen, wholeBlue, wholeWeight);

            if ((maxRed >= maxGreen) && (maxRed >= maxBlue))
            {
                direction = Red;

                // cannot split empty cube
                if (cutRed[0] < 0) return false;
            }
            else
            {
                if ((maxGreen >= maxRed) && (maxGreen >= maxBlue))
                {
                    direction = Green;
                }
                else
                {
                    direction = Blue;
                }
            }

            second.RedMaximum = first.RedMaximum;
            second.GreenMaximum = first.GreenMaximum;
            second.BlueMaximum = first.BlueMaximum;

            // cuts in a certain direction
            switch (direction)
            {
                case Red:
                    second.RedMinimum = first.RedMaximum = cutRed[0];
                    second.GreenMinimum = first.GreenMinimum;
                    second.BlueMinimum = first.BlueMinimum;
                    break;

                case Green:
                    second.GreenMinimum = first.GreenMaximum = cutGreen[0];
                    second.RedMinimum = first.RedMinimum;
                    second.BlueMinimum = first.BlueMinimum;
                    break;

                case Blue:
                    second.BlueMinimum = first.BlueMaximum = cutBlue[0];
                    second.RedMinimum = first.RedMinimum;
                    second.GreenMinimum = first.GreenMinimum;
                    break;
            }

            // determines the volumes after cut
            first.Volume = (first.RedMaximum - first.RedMinimum) * (first.GreenMaximum - first.GreenMinimum) * (first.BlueMaximum - first.BlueMinimum);
            second.Volume = (second.RedMaximum - second.RedMinimum) * (second.GreenMaximum - second.GreenMinimum) * (second.BlueMaximum - second.BlueMinimum);

            // the cut was successfull
            return true;
        }

        private Single CalculateVariance(WuColorCube cube)
        {
            Single volumeRed = Volume(cube, momentsRed);
            Single volumeGreen = Volume(cube, momentsGreen);
            Single volumeBlue = Volume(cube, momentsBlue);
            Single volumeMoment = VolumeFloat(cube, moments);
            Single volumeWeight = Volume(cube, weights);

            Single distance = volumeRed * volumeRed + volumeGreen * volumeGreen + volumeBlue * volumeBlue;

            return volumeMoment - (distance / volumeWeight);
        }

        private Single VolumeFloat(WuColorCube cube, Single[, ,] moment)
        {
            return moment[cube.RedMaximum, cube.GreenMaximum, cube.BlueMaximum] -
                   moment[cube.RedMaximum, cube.GreenMaximum, cube.BlueMinimum] -
                   moment[cube.RedMaximum, cube.GreenMinimum, cube.BlueMaximum] +
                   moment[cube.RedMaximum, cube.GreenMinimum, cube.BlueMinimum] -
                   moment[cube.RedMinimum, cube.GreenMaximum, cube.BlueMaximum] +
                   moment[cube.RedMinimum, cube.GreenMaximum, cube.BlueMinimum] +
                   moment[cube.RedMinimum, cube.GreenMinimum, cube.BlueMaximum] -
                   moment[cube.RedMinimum, cube.GreenMinimum, cube.BlueMinimum];
        }

        private void Mark(WuColorCube cube, Int32 label, Int32[] tagToMark)
        {
            for (Int32 redIndex = cube.RedMinimum + 1; redIndex <= cube.RedMaximum; ++redIndex)
            {
                for (Int32 greenIndex = cube.GreenMinimum + 1; greenIndex <= cube.GreenMaximum; ++greenIndex)
                {
                    for (Int32 blueIndex = cube.BlueMinimum + 1; blueIndex <= cube.BlueMaximum; ++blueIndex)
                    {
                        tagToMark[(redIndex << 10) + (redIndex << 6) + redIndex + (greenIndex << 5) + greenIndex + blueIndex] = label;
                    }
                }
            }
        }

        private List<Color> GetQuantizedPalette(int colorCount)
        {
            --colorCount;

            // preprocess the colors
            CalculateMoments();

            Int32 next = 0;
            Single[] volumeVariance = new Single[MaxColor];

            // processes the cubes
            for (Int32 cubeIndex = 1; cubeIndex < colorCount; ++cubeIndex)
            {
                // if cut is possible; make it
                if (Cut(cubes[next], cubes[cubeIndex]))
                {
                    volumeVariance[next] = cubes[next].Volume > 1 ? CalculateVariance(cubes[next]) : 0.0f;
                    volumeVariance[cubeIndex] = cubes[cubeIndex].Volume > 1 ? CalculateVariance(cubes[cubeIndex]) : 0.0f;
                }
                else // the cut was not possible, revert the index
                {
                    volumeVariance[next] = 0.0f;
                    cubeIndex--;
                }

                next = 0;
                Single temp = volumeVariance[0];

                for (Int32 index = 1; index <= cubeIndex; ++index)
                {
                    if (volumeVariance[index] > temp)
                    {
                        temp = volumeVariance[index];
                        next = index;
                    }
                }

                if (temp <= 0.0)
                {
                    colorCount = cubeIndex + 1;
                    break;
                }
            }

            Int32[] lookupRed = new Int32[MaxColor];
            Int32[] lookupGreen = new Int32[MaxColor];
            Int32[] lookupBlue = new Int32[MaxColor];

            tag = new Int32[MaxVolume];

            // precalculates lookup tables
            for (int k = 0; k < colorCount; ++k)
            {
                Mark(cubes[k], k, tag);

                long weight = Volume(cubes[k], weights);

                if (weight > 0)
                {
                    lookupRed[k] = (int)(Volume(cubes[k], momentsRed) / weight);
                    lookupGreen[k] = (int)(Volume(cubes[k], momentsGreen) / weight);
                    lookupBlue[k] = (int)(Volume(cubes[k], momentsBlue) / weight);
                }
                else
                {
                    lookupRed[k] = 0;
                    lookupGreen[k] = 0;
                    lookupBlue[k] = 0;
                }
            }

            // copies the per pixel tags 
            for (Int32 index = 0; index < imageSize; ++index)
            {
                quantizedPixels[index] = tag[quantizedPixels[index]];
            }

            reds = new Int32[colorCount + 1];
            greens = new Int32[colorCount + 1];
            blues = new Int32[colorCount + 1];
            sums = new Int32[colorCount + 1];
            indices = new Int32[imageSize];

            // scans and adds colors
            for (Int32 index = 0; index < imageSize; index++)
            {
                Color color = pixels[index];
                var r = color.R;
                var g = color.G;
                var b = color.B;

                Int32 match = quantizedPixels[index];
                Int32 bestMatch = match;
                Int32 bestDistance = 100000000;

                for (Int32 lookup = 0; lookup < colorCount; lookup++)
                {
                    Int32 foundRed = lookupRed[lookup];
                    Int32 foundGreen = lookupGreen[lookup];
                    Int32 foundBlue = lookupBlue[lookup];
                    Int32 deltaRed = r - foundRed;
                    Int32 deltaGreen = g - foundGreen;
                    Int32 deltaBlue = b - foundBlue;

                    Int32 distance = deltaRed * deltaRed + deltaGreen * deltaGreen + deltaBlue * deltaBlue;

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestMatch = lookup;
                    }
                }

                reds[bestMatch] += r;
                greens[bestMatch] += g;
                blues[bestMatch] += b;
                sums[bestMatch]++;

                indices[index] = bestMatch;
            }

            List<Color> result = new List<Color>();

            // generates palette
            for (Int32 paletteIndex = 0; paletteIndex < colorCount; paletteIndex++)
            {
                if (sums[paletteIndex] > 0)
                {
                    reds[paletteIndex] /= sums[paletteIndex];
                    greens[paletteIndex] /= sums[paletteIndex];
                    blues[paletteIndex] /= sums[paletteIndex];
                }

                Color color = Color.FromArgb(255, reds[paletteIndex], greens[paletteIndex], blues[paletteIndex]);
                result.Add(color);
            }
            result.Add(Color.FromArgb(0, 0, 0, 0));
            pixelIndex = 0;
            return result;
        }

        private void Prepare(Image image)
        {
            // creates all the cubes
            cubes = new WuColorCube[MaxColor];

            // initializes all the cubes
            for (var cubeIndex = 0; cubeIndex < MaxColor; cubeIndex++)
            {
                cubes[cubeIndex] = new WuColorCube();
            }

            // resets the reference minimums
            cubes[0].RedMinimum = 0;
            cubes[0].GreenMinimum = 0;
            cubes[0].BlueMinimum = 0;

            // resets the reference maximums
            cubes[0].RedMaximum = MaxSideIndex;
            cubes[0].GreenMaximum = MaxSideIndex;
            cubes[0].BlueMaximum = MaxSideIndex;

            weights = new Int64[SideSize, SideSize, SideSize];
            momentsRed = new Int64[SideSize, SideSize, SideSize];
            momentsGreen = new Int64[SideSize, SideSize, SideSize];
            momentsBlue = new Int64[SideSize, SideSize, SideSize];
            moments = new Single[SideSize, SideSize, SideSize];

            table = new Int32[256];

            for (Int32 tableIndex = 0; tableIndex < 256; ++tableIndex)
            {
                table[tableIndex] = tableIndex * tableIndex;
            }

            pixelIndex = 0;
            imageSize = image.Width * image.Height;

            quantizedPixels = new Int32[imageSize];
            pixels = new Color[imageSize];
        }

    }
}
