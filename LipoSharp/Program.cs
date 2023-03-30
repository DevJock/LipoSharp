using System.Diagnostics;
using System.Text;

namespace LipoSharp;

public class MainProgram
{
    private static string[] _supportedExtensions = new[] { ".dylib", ".so", ".a" };
    private static string _outputFolder = "./output";

    public static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Invalid number of arguments. Needs 2 inputs");
            return;
        }

        var src1 = args[0];
        var src2 = args[1];
        var src1IsDir = Directory.Exists(src1);
        var src2IsDir = Directory.Exists(src2);

        if (src1IsDir != src2IsDir)
        {
            Console.WriteLine("Both sources need to be the same");
            return;
        }

        if (!Directory.Exists(_outputFolder))
        {
            Directory.CreateDirectory(_outputFolder);
        }

        if (src1IsDir)
        {
            Console.WriteLine("Sources are Directories");
            Console.WriteLine("Using source 1 as a guide");
            var dir1 = new DirectoryInfo(src1);
            var dir2 = new DirectoryInfo(src2);
            var filesInDir1 = dir1.GetFiles("*.*", SearchOption.AllDirectories);
            var filesInDir2 = dir2.GetFiles("*.*", SearchOption.AllDirectories);
            bool res;
            foreach (var file in filesInDir1)
            {
                var otherFile = filesInDir2.FirstOrDefault(x => x.Name.Equals(file.Name));
                if (otherFile is null)
                {
                    Console.WriteLine($"Could not find matching file. Skipping. {file.Name}");
                    continue;
                }

                res = LipoSuck(file, otherFile);
                if (res)
                {
                    Console.WriteLine($"Finished liposuction of {file.Name}");
                }
            }
        }
        else
        {
            var file1 = new FileInfo(src1);
            var file2 = new FileInfo(src2);
            var res = LipoSuck(file1, file2);
            if (res)
            {
                Console.WriteLine($"Finished liposuction of {file1.Name}");
            }
        }
    }

    private static bool LipoSuck(FileInfo src1, FileInfo src2)
    {
        if (!_supportedExtensions.Contains(src1.Extension) || !_supportedExtensions.Contains(src2.Extension))
        {
            Console.WriteLine($"Skipping unsupported file: {src1.Name}");
            return false;
        }
        if (!src1.Name.Equals(src2.Name))
        {
            Console.WriteLine("Two files don't appear to be the same. Proceed at your own risk");
        }

        if (!src1.Extension.Equals(src2.Extension))
        {
            Console.WriteLine("Two files don't appear to be of the same type. Proceed at your own risk");
        }

        var rootDirectory = Directory.GetCurrentDirectory();
        var currentDirectory = src1.Directory;
        var pathBuilder = new StringBuilder();
        Debug.Assert(currentDirectory != null, nameof(currentDirectory) + " != null");
        while (currentDirectory.FullName != rootDirectory)
        {
            pathBuilder.Insert(0,currentDirectory.Name);
            pathBuilder.Insert(0, '/');
            currentDirectory = currentDirectory.Parent;
        }

        var relativePath = pathBuilder.ToString();
        var saveFileName = src1.Name;
        var savePath = $"{_outputFolder}{relativePath}";
        Directory.CreateDirectory(savePath);
        var saveFilePath = Path.Combine(savePath, saveFileName);
        var lipo = Process.Start("lipo", $"{src1} {src2} -create -output {saveFilePath}");
        lipo.OutputDataReceived += (sender, args) =>
        {
            Console.WriteLine(args.Data);
        };

        return true;
    }
}