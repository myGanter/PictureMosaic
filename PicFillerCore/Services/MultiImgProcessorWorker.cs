using Core.Services;
using PicFillerCore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicFillerCore.Services
{
    class MultiImgProcessorWorker : SingleImgProcessorWorker
    {
        private const string ResPath = "Result";

        public MultiImgProcessorWorker()
        {
            Config = AppConfigService.GetConfig<Conf>();

            IsValid();
        }

        private void IsValid()
        {
            if (!Directory.Exists(Config.PicsPath))
            {
                throw new Exception("-PicsPath invalid");
            }
        }

        public override void Run()
        {
            var files = Directory.GetFiles(Config.PicsPath);

            foreach (var f in files)
            {
                Config.Pic = f;
                base.Run();
            }
        }

        public async override Task RunAsync()
        {
            var files = Directory.GetFiles(Config.PicsPath);

            foreach (var f in files)
            {
                Config.Pic = f;
                await base.RunAsync();
            }
        }

        protected override void SavePic()
        {
            CreateResDir();
            var path = Path.Combine(Config.PicsPath, ResPath, Config.Pic.Split('\\').Last());
            MosaicService.GetBmp().Save(path);
        }

        private void CreateResDir()
        {
            var path = Path.Combine(Config.PicsPath, ResPath);

            if (Directory.Exists(path))
                return;

            Directory.CreateDirectory(path);
        }
    }
}
