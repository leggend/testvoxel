using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace TestVoxel
{
    class Program
    {
        static void Main(string[] args)
        {
            VoxelToStl();
            Console.WriteLine("Hello World!");
        }

        private static void VoxelToStl()
        {
            string pathFileDicom = @"C:\ProgramData\Infomed\SuiteOne\Gesdoc\storage\1bed46d1-1578-4237-9e3d-6fab7ab46d44\3DSlice.zip";
            string pathFileStl = @"C:\Users\jordilas\Desktop\3DSlices.stl";
            var dicomHelper = new DicomHelper();
            var data = dicomHelper.GetDicomVoxelData(pathFileDicom);
            var strlHelper = new Stl3dHelper(data);
            // string pathFileStl_Bone = @"C:\Users\jordilas\Desktop\3DSlices_bone.stl";
            // strlHelper.Generate(pathFileStl_Bone, 120, 255);
            string pathFileStl_Muscle = @"C:\Users\jordilas\Desktop\3DSlices_muscle.stl";
            strlHelper.Generate(pathFileStl_Muscle, 30, 119);
        }
    }
}
