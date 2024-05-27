using System;
using System.IO.Compression;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PT11
{
    internal class Program
    {
        delegate long CalculationDelegate(int n, int k);

        [STAThread]
        static void Main()
        {
            int N = 5, K = 3;

            Task<long> numeratorTask = Task.Run(() => CalculateNumerator(N, K));
            Task<long> denominatorTask = Task.Run(() => CalculateDenominator(K));

            Task.WaitAll(numeratorTask, denominatorTask);

            long num = numeratorTask.Result;
            long den = denominatorTask.Result;

            long biCo = num / den;

            Console.WriteLine($"Symbol Newtona (N po K) dla N={N} i K={K} wynosi: {biCo}");

            CalculationDelegate numeratorDelegate = new CalculationDelegate(CalculateNumerator);
            CalculationDelegate denominatorDelegate = (n, k) => CalculateDenominator(n);

            IAsyncResult numeratorResult = numeratorDelegate.BeginInvoke(N, K, null, null);
            IAsyncResult denominatorResult = denominatorDelegate.BeginInvoke(K, K, null, null);

            long num2 = numeratorDelegate.EndInvoke(numeratorResult);
            long den2 = denominatorDelegate.EndInvoke(denominatorResult);

            long biCo2 = num2 / den2;

            Console.WriteLine($"Symbol Newtona (N po K) dla N={N} i K={K} wynosi: {biCo}");

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string folderPath = dialog.SelectedPath;

                    CompressFilesInFolder(folderPath);
                    Console.WriteLine("Kompresja zakończona.");

                    DecompressFilesInFolder(folderPath);
                    Console.WriteLine("Dekompresja zakończona.");
                }
            }
        }

        static long CalculateNumerator(int N, int K)
        {
            long result = 1;
            for (int i = 0; i < K; i++)
            {
                result *= (N - i);
            }
            return result;
        }

        static long CalculateDenominator(int K)
        {
            long result = 1;
            for (int i = 1; i <= K; i++)
            {
                result *= i;
            }
            return result;
        }

        static void CompressFilesInFolder(string folderPath)
        {
            var files = Directory.GetFiles(folderPath);
            Parallel.ForEach(files, (file) =>
            {
                CompressFile(file);
            });
        }


        static void CompressFile(string filePath)
        {
            string compressedFilePath = filePath + ".gz";
            using (FileStream originalFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (FileStream compressedFileStream = new FileStream(compressedFilePath, FileMode.Create, FileAccess.Write))
            using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
            {
                originalFileStream.CopyTo(compressionStream);
            }
        }
        static void DecompressFile(string filePath)
        {
            string decompressedFilePath = filePath.Remove(filePath.Length - 3); // Remove ".gz" extension
            using (FileStream compressedFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (FileStream decompressedFileStream = new FileStream(decompressedFilePath, FileMode.Create, FileAccess.Write))
            using (GZipStream decompressionStream = new GZipStream(compressedFileStream, CompressionMode.Decompress))
            {
                decompressionStream.CopyTo(decompressedFileStream);
            }
        }

        static void DecompressFilesInFolder(string folderPath)
        {
            var compressedFiles = Directory.GetFiles(folderPath, "*.gz");
            Parallel.ForEach(compressedFiles, (file) =>
            {
                DecompressFile(file);
            });
        }

    }
}
