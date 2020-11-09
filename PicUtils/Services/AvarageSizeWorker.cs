using Core.Models;
using Core.Parallel;
using Core.Services;
using PicUtils.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PicUtils.Services
{
    class AvarageSizeWorker : ImgTaskCreator
    {
        private readonly FrameTaskController<string> TaskController;

        public AvarageSizeWorker()
        {
            IsValid();

            ThreadingInstanceController<AvarageContainer>.SetFactoryMethod(() => new AvarageContainer());

            TaskController = new FrameTaskController<string>(Conf.ThreadCount, TaskCreator, Frame);
        }

        private void IsValid()
        {
            if (Conf.ThreadCount < 1 || Conf.ThreadCount > 16)
            {
                throw new Exception("-ThCout invalid");
            }
        }

        public override void Frame(string Value)
        {
            var av = ThreadingInstanceController<AvarageContainer>.GetInstance();
            
            try
            {
                using var bmp = new Bitmap(Value);
                av.SumW += bmp.Width;
                av.SumH += bmp.Height;
                av.Count++;

                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId}] {Value}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Pic: {Value} Exception: {e.Message}");
                return;
            }
        }

        public override void Run()
        {
            TaskController.Run();
            var av = GetAvarage();
            Console.WriteLine($"Avarage width = {av.Item1}px. Avarage height = {av.Item2}px.");
        }

        public async override Task RunAsync()
        {
            await TaskController.RunAsync();
            var av = GetAvarage();
            Console.WriteLine($"Avarage width = {av.Item1}px. Avarage height = {av.Item2}px.");
        }

        private Tuple<int, int> GetAvarage() 
        {           
            var avRes = ThreadingInstanceController<AvarageContainer>.GetInstances(TaskController.GetFrameThreadIds())
                .Aggregate(new AvarageContainer(), (c, v) => 
                { 
                    c.SumW += v.SumW; 
                    c.SumH += v.SumH; 
                    c.Count += v.Count; 
                    return c; 
                });

            return Tuple.Create((int)(avRes.SumW / avRes.Count), (int)(avRes.SumH / avRes.Count));
        }
    }
}
