using Microsoft.Win32;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NNLibrary;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Polly;
using Polly.Retry;


namespace Client
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string url = "http://localhost:5254";

        private AsyncRetryPolicy _RetryPolicy;
        private int MaxRetries = 3;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] String propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        //Variables
        private NN nnModel = new NN();
        private CancellationTokenSource cts = new CancellationTokenSource();
        private ObservableCollection<ImageInfo> listImages = new ObservableCollection<ImageInfo>();

        public string[]? pathsImages = null;
        public string currentEmotion { get; set; } = "anger";
        private string[] allEmothions = new string[] { "anger", "contempt", "disgust", "fear", "happiness", "neutral", "sadness", "surprise" };
        private double _bar = 0.0;
        public double bar
        {
            get
            {
                return _bar;
            }
            set
            {
                _bar = value;
                RaisePropertyChanged(nameof(bar));
            }
        }
        private bool _isCalculating = false;
        public bool isCalculating
        {
            get
            {
                return _isCalculating;
            }
            set
            {
                _isCalculating = value;
                RaisePropertyChanged(nameof(isCalculating));
            }
        }

        //Commands 
        public ICommand Clear { get; private set; }
        public ICommand Cancel { get; private set; }
        public ICommand ISort { get; private set; }
        public ICommand Delete { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            outputList.ItemsSource = listImages;
            //Commands
            Clear = new RelayCommand(_ => { HandlerClear(this); }, CanClear);
            Cancel = new RelayCommand(_ => { HandlerCancel(this); }, CanCancel);
            ISort = new RelayCommand(_ => { HandlerSort(this); }, CanClear);
            Delete = new RelayCommand(_ => { HandlerDelete(this); }, CanClear);

            //bar = 50;
            for (int i = 0; i < allEmothions.Length; i++)
                listEmothions.Items.Add(allEmothions[i]);

            _RetryPolicy = Policy.Handle<HttpRequestException>().WaitAndRetryAsync(MaxRetries, times =>
                TimeSpan.FromMilliseconds(3000)); //3 sec
        }

        private void ClickedChooseImgs(object sender, RoutedEventArgs? e = null)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.InitialDirectory = System.IO.Path.GetFullPath("../../../../Images");

            if (ofd.ShowDialog() == true)
            {
                pathsImages = new string[ofd.FileNames.Length];
                for (int i = 0; i < pathsImages.Length; i++)
                    pathsImages[i] = ofd.FileNames[i];
            }
        }

        private async Task CalculateOneImg(string path, CancellationTokenSource ctn)
        {
            try
            {
                await _RetryPolicy.ExecuteAsync(async () =>
                {
                    var img = await File.ReadAllBytesAsync(path, ctn.Token);

                    var httpClient = new HttpClient();
                    httpClient.BaseAddress = new Uri($"{url}/images");
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await HttpClientJsonExtensions.PostAsJsonAsync(httpClient, "", img);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void Upload(object sender, RoutedEventArgs? e = null)
        {
            try
            {
                if ((pathsImages != null) && (pathsImages.Length > 0))
                {
                    bar = 0.0;
                    double barStep = 100.0 / pathsImages.Length;

                    isCalculating = true;
                    foreach (var path in pathsImages)
                    {
                        await CalculateOneImg(path, cts);
                        bar += barStep;
                    }
                    sort();
                    pathsImages = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                isCalculating = false;
                outputList.Focus();
            }
        }

        private async void Load(object sender, RoutedEventArgs? e = null)
        {
            try
            {
                await _RetryPolicy.ExecuteAsync(async () => 
                {
                    var httpClient = new HttpClient();
                    var response = await httpClient.GetAsync($"{url}/images");

                    if (response.IsSuccessStatusCode)
                    {
                        List<int> values = await response.Content.ReadFromJsonAsync<List<int>>();
                        foreach (int val in values)
                        {
                            var response_inner = await httpClient.GetAsync($"{url}/images/{val}");
                            ImageInfo item = await response_inner.Content.ReadFromJsonAsync<ImageInfo>();
                            listImages.Add(item);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Not Hello!");
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void sort()
        {
            //int index = 0;
            //for (int i = 0; i < allEmothions.Length; i++)
            //    if (allEmothions[i] == currentEmotion)
            //        index = i;
            listImages = new ObservableCollection<ImageInfo>(
                listImages.OrderByDescending(p => p.emotions.Where(p => p.name == currentEmotion).Max(p => p.value))
                );
            outputList.ItemsSource = listImages;
        }

        private void HandlerClear(object sender)
        {
            listImages.Clear();
        }

        private void HandlerSort(object sender)
        {
            sort();
        }

        private void HandlerCancel(object sender)
        {
            cts.Cancel();
        }
        private async void HandlerDelete(object sender)
        {
            try
            {
                await _RetryPolicy.ExecuteAsync(async () =>
                {
                    var httpClient = new HttpClient();
                    var response = await httpClient.DeleteAsync($"{url}/images");
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("OK!");
                    }
                    else
                    {
                        MessageBox.Show("Not deleted :(");
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private bool CanClear(object sender)
        {
            return !isCalculating;
        }

        private bool CanCancel(object sender)
        {
            return isCalculating;
        }
    }
}
