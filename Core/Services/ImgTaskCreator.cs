using Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public abstract class ImgTaskCreator : IAppWorker<string>
    {
        protected readonly CoreAppConf Conf;

        public ImgTaskCreator()
        {
            Conf = AppConfigService.GetConfig<CoreAppConf>();

            IsValid();
        }

        private void IsValid()
        {
            if (Conf.Paths == null || Conf.Paths.Any(x => !Directory.Exists(x)))
            {
                throw new Exception("-P invalid");                
            }
        }

        public abstract void Frame(string Value);

        public abstract void Run();

        public abstract Task RunAsync();

        public virtual void TaskCreator(Action<string> Adder)
        {            
            var allowedExt = new string[2] { ".jpg", ".png" };

            var stack = new Stack<string>();
            foreach (var p in Conf.Paths)
                stack.Push(p);

            while (stack.Count > 0)
            {
                var dir = stack.Pop();

                var files = Directory
                    .GetFiles(dir)
                    .Where(x => allowedExt.Any(y => x.ToLower().EndsWith(y)));

                foreach (var i in files)
                    Adder(i);

                foreach (var i in Directory.GetDirectories(dir))
                    stack.Push(i);
            }

            Console.WriteLine(new string('_', Console.WindowWidth));
        }
    }
}
