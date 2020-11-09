using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface IAppWorker<T>
    {
        void TaskCreator(Action<T> Adder);

        void Frame(T Value);

        Task RunAsync();

        void Run();
    }

    //public interface IAppWorker<T, G> : IAppWorker<T>
    //{
    //    new Task<G> RunAsync();

    //    abstract Task IAppWorker<T>.RunAsync();

    //    new G Run();

    //    abstract void IAppWorker<T>.Run();
    //}
}
