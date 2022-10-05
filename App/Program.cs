using NNLibrary;
using System;

class Program
{   
    static async Task<int> Main(string[] args)
    {   
        var nn = new NN();
        var cts = new CancellationTokenSource();
        string[] files = new string[] {"../images/happy.png", "../images/anger.jpg", "../images/sadness.jpg", "../images/surprise.jpg"};
        var tasks = new Task<IEnumerable<(string First, float Second)>>[4];
        for (int i = 0; i < 4; i++) {
            var img = File.ReadAllBytes(files[i]);
            if (i == 2)
                tasks[i] = nn.InferenceAsync(img, cts);
            else
                tasks[i] = nn.InferenceAsync(img);
        }
        cts.Cancel();
        
        await Task.WhenAll(tasks);

        foreach(var j in tasks){
            if (j.Result != null)
                foreach(var i in j.Result)
                    Console.WriteLine($"{i.First}: {i.Second}");
            else
                Console.WriteLine($"Null");
            Console.WriteLine($"--------------------");
        }
        return 0;
    }
}
