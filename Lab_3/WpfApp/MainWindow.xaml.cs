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

namespace WpfApp
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
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
                var img = await File.ReadAllBytesAsync(path, ctn.Token);
                //var tmpImage = new ImageInfo(path);
                int hash = Tools.ComputeHash(img);
                //tmpImage.value = new ImageValue() { data = img, image = tmpImage };

                using (var db = new ApplicationContext())
                {
                    var query = db.images.Where(x => x.hash == hash).Include(item => item.value);
                    var item = query.Where(x => Enumerable.SequenceEqual(x.value.data, img))
                                .Include(x => x.emotions)
                                .FirstOrDefault();
                    if ((item != null) && (item.hash == hash))
                        listImages.Add(item);
                    else
                    {
                        var result = await nnModel.InferenceAsync(img, ctn);
                        var tmpImage = new ImageInfo(path);
                        tmpImage.value = new ImageValue() { data = img, image = tmpImage };
                        tmpImage.hash = hash;

                        foreach (var elem in result)
                        {
                            tmpImage.emotions.Add(new Emotion() { value = elem.Item2, name = elem.Item1, image = tmpImage });
                        }
                        listImages.Add(tmpImage);

                        db.images.Add(tmpImage);
                        db.SaveChanges();
                    }
                }
                //var result = await nnModel.InferenceAsync(img, ctn);
                //var tmpImage = new ImageInfo(path);
                //IEnumerable<(string, float)> result 
                //foreach (var item in result)
                //{
                //    tmpImage.emotions.Add(new Emotion() { value = item.Item2, name = item.Item1, image = tmpImage });
                //}
                //listImages.Add(tmpImage);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }

        private async void Upload(object sender, RoutedEventArgs? e = null)
        {
            try
            {   
                if ((pathsImages != null) && (pathsImages.Length > 0)) { 
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
            catch
            {
            }
            finally
            {
                isCalculating = false;
                outputList.Focus();
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
        private void HandlerDelete(object sender)
        {
            var item = outputList.SelectedItem as ImageInfo;
            if (item == null)
                return;
            using (var db = new ApplicationContext())
            {
                var photo = db.images.Where(x => x.hash == item.hash).FirstOrDefault();
                if (photo == null)
                    return;
                db.images.Remove(photo);
                db.SaveChanges();
                listImages.Remove(item);
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
