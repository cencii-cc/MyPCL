using MyPCL.Utils.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MyPCL.Utils.Interface.ILoadingTriggerEvenHandler;

namespace MyPCL.Utils.Interface
{
    public class ILoadingTriggerEvenHandler
    {
        public delegate void LoadingStateChangedHandler(MyLoadingState NewState, MyLoadingState OldState);

        public delegate void ProgressChangedHandler(double NewProgress, double OldProgress);
    }

    public interface ILoadingTrigger
    {
        bool IsLoader { get; }

        MyLoadingState LoadingState { set; get; }

        event LoadingStateChangedHandler LoadingStateChanged;

        event ProgressChangedHandler ProgressChanged;
    }
}
