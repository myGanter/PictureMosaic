using Core.Expansions;
using Core.Models;
using Core.Parallel;
using Core.Services;
using PicUtils.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PicUtils.Services
{
    class ScaleCutDataSetWorker : ImgTaskCreator
    {
        private readonly ScaleCutConf CutConf;

        private readonly NumberIterator Number;

        private readonly FrameTaskController<string> TaskController;

        public ScaleCutDataSetWorker()
        {
            CutConf = AppConfigService.GetConfig<ScaleCutConf>();

            IsValid();

            Number = new NumberIterator();

            TaskController = new FrameTaskController<string>(Conf.ThreadCount, TaskCreator, Frame);           
        }

        private void IsValid()
        {
            if (Conf.ThreadCount < 1 || Conf.ThreadCount > 16)
            {
                throw new Exception("-ThCout invalid");
            }

            if (CutConf.Width < 1 || CutConf.Height < 1)
            {
                throw new Exception("-NW or -NH invalid");
            }

            try
            {
                Directory.CreateDirectory(CutConf.ResultPuth);
            }
            catch (Exception e)
            {
                throw new Exception("-ResPuth invalid", e);
            }
        }

        public override void Frame(string Value)
        {
            try
            {
                using var bmp = new Bitmap(Value);
                using var newBmp = bmp.ScaleCutBmpCenter(CutConf.Width, CutConf.Height);

                string fname;
                if (CutConf.SaveOriginName)
                {
                    fname = $"{CutConf.ResultPuth}\\{Value.Split('\\').Last()}";
                }
                else
                {
                    fname = $"{CutConf.ResultPuth}\\{Number.Value}.png";
                }

                newBmp.Save(fname);

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
        }

        public async override Task RunAsync()
        {
            await TaskController.RunAsync();
        }
    }
}
