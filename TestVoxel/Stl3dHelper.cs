using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace TestVoxel
{
    public class Stl3dHelper
    {
        public byte MinVisibleValue { get; set; } = 120;

        public byte MaxVisibleValue { get; set; } = 255;

        public byte[,,] Voxel { get; set; } = new byte[,,] { };
        public float Scale { get; set; } = 1f;

        public Stl3dHelper() { }

        public Stl3dHelper(byte[,,] voxel)
        {
            Voxel = voxel;
        }

        public void Generate(string pathFile, byte min, byte max)
        {
            MinVisibleValue = min;
            MaxVisibleValue = max;
            if (MaxVisibleValue < MinVisibleValue)
            {
                throw new Exception("MaxVisibleValue no puede ser mas pequeño que MinVisibleValue");
            }

            FileInfo fInf = new FileInfo(pathFile);
            if (fInf.Exists)
            {
                fInf.Delete();
            }
            // WriteStlAsciiFile(pathFile);
            WriteStlBinaryFile(pathFile);
        }

        private void WriteStlBinaryFile(string pathFile)
        {
            using (StreamWriter file = new StreamWriter(pathFile))
            {
                var vectors = GetVectorsList();
                var totalPoligons = vectors.Count / 3;

                var writer = new BinaryWriter(file.BaseStream);

                // write header
                var header = new byte[80]; // can be a garbage value
                writer.Write(header);

                writer.Write((uint)totalPoligons);

                for (var i = 0; i < totalPoligons; i++)
                {
                    var firstVector = i * 3;

                    writer.Write(0);
                    writer.Write(0);
                    writer.Write(0);

                    writer.Write(vectors[firstVector].X);
                    writer.Write(vectors[firstVector].Y);
                    writer.Write(vectors[firstVector].Z);

                    writer.Write(vectors[firstVector + 1].X);
                    writer.Write(vectors[firstVector + 1].Y);
                    writer.Write(vectors[firstVector + 1].Z);

                    writer.Write(vectors[firstVector + 2].X);
                    writer.Write(vectors[firstVector + 2].Y);
                    writer.Write(vectors[firstVector + 2].Z);

                    writer.Write((ushort)0); // garbage value
                }
                writer.Flush();
            }
        }

        private void WriteStlAsciiFile(string pathFile)
        {
            using (StreamWriter file = new StreamWriter(pathFile))
            {
                file.WriteLine("solid");

                var vectors = GetVectorsList();
                var totalPoligons = vectors.Count / 3;

                for (var i = 0; i < totalPoligons; i++)
                {
                    var firstVector = i * 3;
                    var normal = "0 0 0";
                    file.WriteLine($"\tfacet normal {normal}");
                    file.WriteLine("\t\touter loop");
                    file.WriteLine($"\t\t\tvertex {vectors[firstVector].X} {vectors[firstVector].Y} {vectors[firstVector].Z}");
                    file.WriteLine($"\t\t\tvertex {vectors[firstVector + 1].X} {vectors[firstVector + 1].Y} {vectors[firstVector + 1].Z}");
                    file.WriteLine($"\t\t\tvertex {vectors[firstVector + 2].X} {vectors[firstVector + 2].Y} {vectors[firstVector + 2].Z}");
                    file.WriteLine("\t\tendloop");
                    file.WriteLine("\tendfacet");
                }
                file.WriteLine("endsolid");

            }
        }

        private List<Vector3> GetVectorsList()
        {
            List<Vector3> vectors = new List<Vector3>();

            var totalZ = Voxel.GetLength(0);
            var totalY = Voxel.GetLength(1);
            var totalX = Voxel.GetLength(2);

            for (int z = 0; z < totalZ; z++)
            {
                for (int y = 0; y < totalY; y++)
                {
                    for (int x = 0; x < totalX; x++)
                    {
                        if (IsCellVisible(Voxel[z, y, x]))
                        {
                            List<CubeFace> visibleFaces = new List<CubeFace>();

                            //FACE WEST
                            if (x == 0 || !IsCellVisible(Voxel[z, y, x - 1]))
                            {
                                visibleFaces.Add(CubeFace.WEST);
                            }
                            //FACE NORTH
                            if (y == totalY - 1 || !IsCellVisible(Voxel[z, y + 1, x]))
                            {
                                visibleFaces.Add(CubeFace.NORTH);
                            }
                            //FACE EAST
                            if (x == totalX - 1 || !IsCellVisible(Voxel[z, y, x + 1]))
                            {
                                visibleFaces.Add(CubeFace.EAST);
                            }
                            //FACE SOUTH
                            if (y == 0 || !IsCellVisible(Voxel[z, y - 1, x]))
                            {
                                visibleFaces.Add(CubeFace.SOUTH);
                            }
                            //FACE BOTTOM
                            if (z == 0 || !IsCellVisible(Voxel[z - 1, y, x]))
                            {
                                visibleFaces.Add(CubeFace.BOTTOM);
                            }
                            //FACE TOP
                            if (z == totalZ - 1 || !IsCellVisible(Voxel[z + 1, y, x]))
                            {
                                visibleFaces.Add(CubeFace.TOP);
                            }

                            // Exclude etire cube for optimization
                            if (visibleFaces.Count < 6)
                            {
                                visibleFaces.ForEach((face) => vectors.AddRange(GetCubeFaceVectors(face, x, y, z)));
                            }
                        }
                    }
                }
            }
            return vectors;
        }

        private bool IsCellVisible(byte value)
        {
            return value >= MinVisibleValue && value <= MaxVisibleValue;
        }

        private string GetFacetNormal(CubeFace face)
        {
            string normal = "0 0 0";
            switch (face)
            {
                case CubeFace.WEST:
                    normal = "0 0 0";
                    break;
                case CubeFace.NORTH:
                    normal = "0 0 0";
                    break;
                case CubeFace.EAST:
                    normal = "0 0 0";
                    break;
                case CubeFace.SOUTH:
                    normal = "0 0 0";
                    break;
                case CubeFace.TOP:
                    normal = "0 0 0";
                    break;
                case CubeFace.BOTTOM:
                    normal = "0 0 0";
                    break;
                default:
                    normal = "0 0 0";
                    break;
            }
            return normal;
        }

        private List<Vector3> GetCubeFaceVectors(CubeFace face, int x, int y, int z)
        {
            List<Vector3> vectors = new List<Vector3>();

            switch(face)
            {
                case CubeFace.WEST:
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.SouthWestBottom, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.NorthWestBottom, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.NorthWestTop, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.SouthWestBottom, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.NorthWestTop, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.SouthWestTop, x, y, z));
                    break;
                case CubeFace.NORTH:
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.NorthEastBottom, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.NorthWestBottom, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.NorthWestTop, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.NorthEastBottom, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.NorthWestTop, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.NorthEastTop, x, y, z));
                    break;
                case CubeFace.EAST:
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.SouthEastBottom, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.NorthEastBottom, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.NorthEastTop, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.SouthEastBottom, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.NorthEastTop, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.SouthEastTop, x, y, z));
                    break;
                case CubeFace.SOUTH:
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.SouthEastBottom, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.SouthWestBottom, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.SouthWestTop, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.SouthEastBottom, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.SouthWestTop, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.SouthEastTop, x, y, z));
                    break;
                case CubeFace.TOP:
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.SouthEastTop, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.SouthWestTop, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.NorthWestTop, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.SouthEastTop, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.NorthWestTop, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.NorthEastTop, x, y, z));
                    break;
                case CubeFace.BOTTOM:
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.SouthEastBottom, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.SouthWestBottom, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.NorthWestBottom, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.SouthEastBottom, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.NorthWestBottom, x, y, z));
                    vectors.Add(GetVectorFaceCubePoint(CubeVectorPoint.NorthEastBottom, x, y, z));
                    break;
            }

            return vectors;
        }

        private Vector3 GetVectorFaceCubePoint(CubeVectorPoint point, int x, int y, int z)
        {
            Vector3 result = new Vector3();
            switch(point)
            {
                case CubeVectorPoint.SouthWestBottom:
                    result = new Vector3(Scale * x, Scale * y, Scale * z);
                    break;
                case CubeVectorPoint.NorthWestBottom:
                    result = new Vector3(Scale * x, (Scale * y) + Scale, Scale * z);
                    break;
                case CubeVectorPoint.NorthWestTop:
                    result = new Vector3(Scale * x, (Scale * y) + Scale, (Scale * z) + Scale);
                    break;
                case CubeVectorPoint.SouthWestTop:
                    result = new Vector3(Scale * x, Scale * y, (Scale * z) + Scale);
                    break;


                case CubeVectorPoint.SouthEastBottom:
                    result = new Vector3((Scale * x) + Scale, Scale * y, Scale * z);
                    break;
                case CubeVectorPoint.NorthEastBottom:
                    result = new Vector3((Scale * x) + Scale, (Scale * y) + Scale, Scale * z);
                    break;
                case CubeVectorPoint.NorthEastTop:
                    result = new Vector3((Scale * x) + Scale, (Scale * y) + Scale, (Scale * z) + Scale);
                    break;
                case CubeVectorPoint.SouthEastTop:
                    result = new Vector3((Scale * x) + Scale, Scale * y, (Scale * z) + Scale);
                    break;

            }
            return result;
        }

        public enum CubeFace
        {
            WEST,
            NORTH,
            EAST,
            SOUTH,
            TOP,
            BOTTOM
        }

        public enum CubeVectorPoint
        {
            SouthWestBottom,
            NorthWestBottom,
            NorthWestTop,
            SouthWestTop,
            SouthEastBottom,
            NorthEastBottom,
            NorthEastTop,
            SouthEastTop
        }
    }
}
