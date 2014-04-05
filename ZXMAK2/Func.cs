using System;
using System.Collections.Generic;
using System.Text;

namespace ZXMAK2
{
    public delegate T Func<T>();
    public delegate TResult Func<TResult,T>(T arg);
    public delegate TResult Func<TResult, T1,T2>(T1 arg1, T2 arg2);
    public delegate TResult Func<TResult, T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
    public delegate TResult Func<TResult, T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    public delegate TResult Func<TResult, T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    public delegate TResult Func<TResult, T1, T2, T3, T4, T5, T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    public delegate TResult Func<TResult, T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    public delegate TResult Func<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
    public delegate TResult Func<TResult, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);
    public delegate void Action();
}
