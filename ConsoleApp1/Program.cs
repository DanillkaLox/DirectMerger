using System.Diagnostics;

class ExternalMergeSortOptimized
{
    private const int BufferSize = (100 * 1024 * 1024)/2;
    private static string tempDirectory = "TempFiles";

    static void Main()
    {
        string inputFile = "input_large.txt";
        string outputFile = "output_sorted.txt";
        
        if (!Directory.Exists(tempDirectory))
        {
            Directory.CreateDirectory(tempDirectory);
        }
        
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        
        ExternalSort(inputFile, outputFile);
        
        stopwatch.Stop();
        TimeSpan ts = stopwatch.Elapsed;
        Console.WriteLine($"Сортування завершено. Загальний час: {ts.TotalMinutes:F2} хвилин.");
        
        Directory.Delete(tempDirectory, true);
    }
    
    static void ExternalSort(string inputFile, string outputFile)
    {
        List<string> tempFiles = SplitFile(inputFile);
        MergeFiles(tempFiles, outputFile);
    }
    
    static List<string> SplitFile(string inputFile)
    {
        List<string> tempFiles = new List<string>();
        using (StreamReader reader = new StreamReader(inputFile))
        {
            List<int> buffer = new List<int>();
            string line;
            int fileIndex = 0;
            while ((line = reader.ReadLine()) != null)
            {
                buffer.Add(int.Parse(line));
                if (buffer.Count * sizeof(int) >= BufferSize)
                {
                    tempFiles.Add(SortAndSaveTempFile(buffer, fileIndex++));
                    buffer.Clear();
                }
            }
            
            if (buffer.Count > 0)
            {
                tempFiles.Add(SortAndSaveTempFile(buffer, fileIndex++));
            }
        }
        return tempFiles;
    }
    
    static string SortAndSaveTempFile(List<int> data, int index)
    {
        data.Sort();
        string tempFileName = Path.Combine(tempDirectory, $"tempfile_{index}.txt");
        using (StreamWriter writer = new StreamWriter(tempFileName))
        {
            foreach (var number in data)
            {
                writer.WriteLine(number);
            }
        }
        return tempFileName;
    }
    
    static void MergeFiles(List<string> tempFiles, string outputFile)
    {
        List<StreamReader> readers = tempFiles.Select(f => new StreamReader(f)).ToList();
        using (StreamWriter writer = new StreamWriter(outputFile))
        {
            var pq = new SortedDictionary<int, Queue<int>>();
            
            for (int i = 0; i < readers.Count; i++)
            {
                if (readers[i].Peek() >= 0)
                {
                    int value = int.Parse(readers[i].ReadLine());
                    if (!pq.ContainsKey(value))
                        pq[value] = new Queue<int>();
                    pq[value].Enqueue(i);
                }
            }

            while (pq.Count > 0)
            {
                int minValue = pq.Keys.First();
                int readerIndex = pq[minValue].Dequeue();
                if (pq[minValue].Count == 0)
                    pq.Remove(minValue);

                writer.WriteLine(minValue);
                
                if (readers[readerIndex].Peek() >= 0)
                {
                    int nextValue = int.Parse(readers[readerIndex].ReadLine());
                    if (!pq.ContainsKey(nextValue))
                        pq[nextValue] = new Queue<int>();
                    pq[nextValue].Enqueue(readerIndex);
                }
            }
        }
        
        foreach (var reader in readers)
        {
            reader.Close();
        }
        
        foreach (var tempFile in tempFiles)
        {
            File.Delete(tempFile);
        }
    }
}