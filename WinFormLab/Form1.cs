using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormLab
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            ProgressForm progressForm = new ProgressForm();
            progressForm.StartPosition = FormStartPosition.Manual;
            progressForm.Location = new Point(this.Location.X - progressForm.Width - 5, this.Location.Y);
            progressForm.Show();

            Worker worker = new Worker(progressForm.UpdateProgress);
            label1.Text = (await worker.Run2()).ToString();
            label1.Text = await Task.Run(() => worker.Run5().ToString());
        }
    }

    class Worker
    {
        private Action<ProgressInfo> _reportProgress;
        private object _lockObj = new object();

        public Worker(Action<ProgressInfo> reportProgress)
        {
            _reportProgress = reportProgress;
        }

        private async Task FooAsync()
        {
            var rnd = new Random();
            await Task.Delay(TimeSpan.FromMilliseconds(200 * rnd.Next(1, 3)));
            await Task.Delay(TimeSpan.FromMilliseconds(200 * rnd.Next(1, 3)));
            await Task.Delay(TimeSpan.FromMilliseconds(200 * rnd.Next(1, 3)));
        }

        public async Task<double> Run()
        {
            Stopwatch sw = Stopwatch.StartNew();
            _reportProgress(new ProgressInfo(100, 0, $"Run!"));

            int processed = 0;
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 10000; i++)
            {
                Task task = FooAsync();
                tasks.Add(task);
                _ = task.ContinueWith((x) =>
                {
                    lock (_lockObj)
                    {
                        processed++;
                    }
                    _reportProgress(new ProgressInfo(10000, processed, $"Task {i}"));
                });
            }
            await Task.WhenAll(tasks);
            sw.Stop();
            return sw.Elapsed.TotalMilliseconds;
        }

        private static SemaphoreSlim _sphs;

        private async Task FooAsync2()
        {
            await _sphs.WaitAsync();
            var rnd = new Random();
            await Task.Delay(TimeSpan.FromMilliseconds(200 * rnd.Next(1, 3)));
            await Task.Delay(TimeSpan.FromMilliseconds(200 * rnd.Next(1, 3)));
            await Task.Delay(TimeSpan.FromMilliseconds(200 * rnd.Next(1, 3)));
            _sphs.Release();
        }

        public async Task<double> Run2()
        {
            int max, xx;
            ThreadPool.GetMaxThreads(out max, out xx);
            _sphs = new SemaphoreSlim(max / 4);

            Stopwatch sw = Stopwatch.StartNew();
            _reportProgress(new ProgressInfo(100, 0, $"Run!"));


            int processed = 0;
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 10000; i++)
            {
                Task task = FooAsync2();
                tasks.Add(task);
                _ = task.ContinueWith((x) =>
                {
                    lock (_lockObj)
                    {
                        processed++;
                    }
                    _reportProgress(new ProgressInfo(10000, processed, $"Task {i}"));
                });
            }
            await Task.WhenAll(tasks);
            sw.Stop();
            return sw.Elapsed.TotalMilliseconds;
        }

        public double Run3()
        {
            Stopwatch sw = Stopwatch.StartNew();
            _reportProgress(new ProgressInfo(100, 0, $"Run!"));

            int processed = 0;
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 10000; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var rnd = new Random();
                    SpinWait.SpinUntil(() => false, TimeSpan.FromMilliseconds(200 * rnd.Next(1, 3)));
                    SpinWait.SpinUntil(() => false, TimeSpan.FromMilliseconds(200 * rnd.Next(1, 3)));
                    SpinWait.SpinUntil(() => false, TimeSpan.FromMilliseconds(200 * rnd.Next(1, 3)));

                    lock (_lockObj)
                    {
                        processed++;
                    }
                    _reportProgress(new ProgressInfo(10000, processed, $"Task {i}"));
                }));
            }
            Task.WaitAll(tasks.ToArray());
            sw.Stop();
            return sw.Elapsed.TotalMilliseconds;
        }

        public double Run4()
        {
            Stopwatch sw = Stopwatch.StartNew();
            _reportProgress(new ProgressInfo(100, 0, $"Run!"));

            int processed = 0;
            //List<Task> tasks = new List<Task>();
            Parallel.For(0, 10000, new ParallelOptions() { MaxDegreeOfParallelism = 512 }, (i) =>
               {
                   var rnd = new Random();
                   SpinWait.SpinUntil(() => false, TimeSpan.FromMilliseconds(200 * rnd.Next(1, 3)));
                   SpinWait.SpinUntil(() => false, TimeSpan.FromMilliseconds(200 * rnd.Next(1, 3)));
                   SpinWait.SpinUntil(() => false, TimeSpan.FromMilliseconds(200 * rnd.Next(1, 3)));

                   lock (_lockObj)
                   {
                       processed++;
                   }
                   _reportProgress(new ProgressInfo(10000, processed, $"Task {i}"));

               });

            //Task.WaitAll(tasks.ToArray());
            sw.Stop();
            return sw.Elapsed.TotalMilliseconds;
        }

        public double Run5()
        {
            int max, xx;
            ThreadPool.GetMaxThreads(out max, out xx);

            Stopwatch sw = Stopwatch.StartNew();
            _reportProgress(new ProgressInfo(100, 0, $"Run!"));

            int processed = 0;
            Parallel.For(0, 10000, new ParallelOptions() { MaxDegreeOfParallelism = max / 4 }, async (i) =>
            {
                var rnd = new Random();
                await Task.Delay(TimeSpan.FromMilliseconds(200 * rnd.Next(1, 3)));
                await Task.Delay(TimeSpan.FromMilliseconds(200 * rnd.Next(1, 3)));
                await Task.Delay(TimeSpan.FromMilliseconds(200 * rnd.Next(1, 3)));

                lock (_lockObj)
                {
                    processed++;
                }
                _reportProgress(new ProgressInfo(10000, processed, $"Task {i}"));

            });

            sw.Stop();
            return sw.Elapsed.TotalMilliseconds;
        }
    }
}
