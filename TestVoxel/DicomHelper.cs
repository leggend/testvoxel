using Dicom;
using Dicom.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TestVoxel
{
    public class DicomHelper
    {
        public byte[,,] GetDicomVoxelData(string filePath)
        {
            byte[,,] voxelData = new byte[,,] { };
            using (var zip = ZipFile.OpenRead(filePath))
            {
                var rx = new Regex(@"([0-9]+)(?!.*[0-9]+)");

                var entries = zip.Entries
                    .Where(e => e.Name != null && e.Length > 0)
                    .OrderBy(e => rx.Match(e.Name).Groups[1].Value, new SemiNumericComparer())
                    .ToArray();
                var firstEntry = entries.First();

                var slices = zip.Entries.Count;
                Console.WriteLine($"Zip contains {slices} slices");

                int width, height;

                using (var stream = firstEntry.Open())
                {
                    byte[] buffer = new byte[firstEntry.Length];
                    stream.Read(buffer);

                    var dicom = DicomFile.Open(new MemoryStream(buffer));
                    var dicomImage = new DicomImage(dicom.Dataset);
                    width = dicomImage.Width;
                    height = dicomImage.Height;
                    slices = entries.Length;
                    voxelData = new byte[slices, height, width];
                    Console.WriteLine($"Dimensions: Z: {slices}, Y: {height}, X: {width}");
                }

                for (int slice = 0; slice < slices; slice++)
                {
                    var entry = entries[slice];
                    using (var stream = entry.Open())
                    {
                        Console.WriteLine($"Loading: '{entry.FullName}'...");

                        byte[] buffer = new byte[entry.Length];
                        stream.Read(buffer);

                        var dicom = DicomFile.Open(new MemoryStream(buffer));
                        var dicomImage = new DicomImage(dicom.Dataset);

                        var image = dicomImage.RenderImage();
                        var data = image.Pixels.Data;

                        for (int i = 0; i < height; i++)
                        {
                            for (int j = 0; j < width; j++)
                            {
                                voxelData[slice, i, j] = (byte)data[(i * width) + j];
                            }
                        }
                    }
                }
            }

            return voxelData;
        }

        private class SemiNumericComparer : IComparer<string>
        {
            public static bool IsNumeric(object value)
            {
                try
                {
                    int i = Convert.ToInt32(value.ToString());
                    return true;
                }
                catch (FormatException)
                {
                    return false;
                }
            }

            public int Compare(string s1, string s2)
            {
                if (IsNumeric(s1) && IsNumeric(s2))
                {
                    if (Convert.ToInt32(s1) > Convert.ToInt32(s2))
                    {
                        return 1;
                    }

                    if (Convert.ToInt32(s1) < Convert.ToInt32(s2))
                    {
                        return -1;
                    }

                    if (Convert.ToInt32(s1) == Convert.ToInt32(s2))
                    {
                        return 0;
                    }
                }

                if (IsNumeric(s1) && !IsNumeric(s2))
                {
                    return -1;
                }

                if (!IsNumeric(s1) && IsNumeric(s2))
                {
                    return 1;
                }

                return string.Compare(s1, s2, true);
            }
        }

    }
}
