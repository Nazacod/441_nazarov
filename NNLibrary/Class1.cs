using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;
using SixLabors.ImageSharp.Processing;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using System.Collections.Generic;
using System.IO;

namespace NNLibrary{
    public class NN
    {
        public InferenceSession session;
        static SemaphoreSlim netLock = new SemaphoreSlim(1,1);
        public NN(){
            using var modelStream = typeof(NN).Assembly.GetManifestResourceStream("NNLibrary.emotion-ferplus-7.onnx");
            using var memoryStream = new MemoryStream();
            modelStream.CopyTo(memoryStream);
            this.session = new InferenceSession(memoryStream.ToArray());
        }
        public async Task<IEnumerable<(string First, float Second)>> InferenceAsync(byte[] img, CancellationTokenSource? cts = null){
            return await Task<IEnumerable<(string First, float Second)>>.Factory.StartNew(() => {
                var myStream = new MemoryStream(img);
                using Image<Rgb24> image = Image.Load<Rgb24>(myStream);
                image.Mutate(ctx => {
                    ctx.Resize(new Size(64,64));
                });

                var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("Input3", GrayscaleImageToTensor(image)) };
                
                if (CancelTaskRequested(cts))
                    return null;

                netLock.Wait();
                using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = this.session.Run(inputs);
                netLock.Release();
                var emotions = Softmax(results.First(v => v.Name == "Plus692_Output_0").AsEnumerable<float>().ToArray());

                string[] keys = { "neutral", "happiness", "surprise", "sadness", "anger", "disgust", "fear", "contempt" };
                
                return keys.Zip(emotions);
            });
        }
        public IEnumerable<(string First, float Second)> Inference(){
            using Image<Rgb24> image = Image.Load<Rgb24>("face1.png");
            image.Mutate(ctx => {
                ctx.Resize(new Size(64,64));
            });

            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("Input3", GrayscaleImageToTensor(image)) };
            using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = this.session.Run(inputs);
            var emotions = Softmax(results.First(v => v.Name == "Plus692_Output_0").AsEnumerable<float>().ToArray());

            string[] keys = { "neutral", "happiness", "surprise", "sadness", "anger", "disgust", "fear", "contempt" };
            
            return keys.Zip(emotions);
        }
        private static DenseTensor<float> GrayscaleImageToTensor(Image<Rgb24> img)
        {
            var w = img.Width;
            var h = img.Height;
            var t = new DenseTensor<float>(new[] { 1, 1, h, w });

            img.ProcessPixelRows(pa => 
            {
                for (int y = 0; y < h; y++)
                {           
                    Span<Rgb24> pixelSpan = pa.GetRowSpan(y);
                    for (int x = 0; x < w; x++)
                    {
                        t[0, 0, y, x] = pixelSpan[x].R; // B and G are the same
                    }
                }
            });
            
            return t;
        }
        private static float[] Softmax(float[] z)
        {
            var exps = z.Select(x => Math.Exp(x)).ToArray();
            var sum = exps.Sum();
            return exps.Select(x => (float)(x / sum)).ToArray();
        }
        private bool CancelTaskRequested(CancellationTokenSource? cts) {
            if (cts == null)
                return false;
            if (cts.IsCancellationRequested)
                return true;
            return false;
        }
    }
}