using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Baltin.UnitaskFBT
{
    /// <summary>
    /// Async Functional Behavior Tree pattern
    /// 1. Convenient for debugging, since you can set breakpoint both on tree nodes and on delegates passed as parameters
    /// 2. Zero memory allocation, if you do not use closures in delegates
    ///    (to ensure which it is recommended to use the 'static' modifier before declaring the delegate)
    /// 3. Extremely fast, because here inside there are only the simplest conditions, loops and procedure calls
    /// </summary>
    /// <typeparam name="T">Type of 'blackboard' that represents the controlled object.</typeparam>
    public static class UnitaskFbtNodes
    {
        /// <summary>
        /// Classic inverter node
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="func">Delegate receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<bool> Inverter<T>(
            this T board, 
            Func<T, UniTask<bool>> func)
            => !await func.Invoke(board);

        /// <summary>
        /// Execute the given func delegate if the given condition is true 
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="ct"></param>
        /// <param name="condition">Condition given as a delegate returning true</param>
        /// <param name="func">Action to execute if condition is true. Delegates receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<bool> If<T>(
            this T board, 
            Func<T, bool> condition, 
            Func<T, UniTask<bool>> func) 
            => condition.Invoke(board) && await func.Invoke(board);

        /// <summary>
        /// Classic selector node
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="f1">Delegate receiving T and returning Status</param>
        /// <param name="f2">Delegate receiving T and returning Status</param>
        /// <param name="f3">Optional delegate receiving T and returning Status</param>
        /// <param name="f4">Optional delegate receiving T and returning Status</param>
        /// <param name="f5">Optional delegate receiving T and returning Status</param>
        /// <param name="f6">Optional delegate receiving T and returning Status</param>
        /// <param name="f7">Optional delegate receiving T and returning Status</param>
        /// <param name="f8">Optional delegate receiving T and returning Status</param>
        /// <param name="token"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<bool> Selector<T>(this T board, 
            Func<T, UniTask<bool>> f1,
            Func<T, UniTask<bool>> f2,
            Func<T, UniTask<bool>> f3 = null,
            Func<T, UniTask<bool>> f4 = null,
            Func<T, UniTask<bool>> f5 = null,
            Func<T, UniTask<bool>> f6 = null,
            Func<T, UniTask<bool>> f7 = null,
            Func<T, UniTask<bool>> f8 = null)
        {
            var s = f1 is not null && await f1.Invoke(board); if(s) return true;
            s = f2 is not null && await f2.Invoke(board); if(s) return true;
            s = f3 is not null && await f3.Invoke(board); if(s) return true;
            s = f4 is not null && await f4.Invoke(board); if(s) return true;
            s = f5 is not null && await f5.Invoke(board); if(s) return true;
            s = f6 is not null && await f6.Invoke(board); if(s) return true;
            s = f7 is not null && await f7.Invoke(board); if(s) return true;
            s = f8 is not null && await f8.Invoke(board); if(s) return true;

            return false;
        }
        
        /// <summary>
        /// Classic sequencer node
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="f1">Delegate receiving T and returning Status</param>
        /// <param name="f2">Delegate receiving T and returning Status</param>
        /// <param name="f3">Optional delegate receiving T and returning Status</param>
        /// <param name="f4">Optional delegate receiving T and returning Status</param>
        /// <param name="f5">Optional delegate receiving T and returning Status</param>
        /// <param name="f6">Optional delegate receiving T and returning Status</param>
        /// <param name="f7">Optional delegate receiving T and returning Status</param>
        /// <param name="f8">Optional delegate receiving T and returning Status</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<bool> Sequencer<T>(this T board,
            Func<T, UniTask<bool>> f1,
            Func<T, UniTask<bool>> f2,
            Func<T, UniTask<bool>> f3 = null,
            Func<T, UniTask<bool>> f4 = null,
            Func<T, UniTask<bool>> f5 = null,
            Func<T, UniTask<bool>> f6 = null,
            Func<T, UniTask<bool>> f7 = null,
            Func<T, UniTask<bool>> f8 = null)
        {
            var s = f1 is not null && await f1.Invoke(board); if(!s) return false;
            s = f2 is not null && await f2.Invoke(board); if(!s) return false;
            s = f3 is not null && await f3.Invoke(board); if(!s) return false;
            s = f4 is not null && await f4.Invoke(board); if(!s) return false;
            s = f5 is not null && await f5.Invoke(board); if(!s) return false;
            s = f6 is not null && await f6.Invoke(board); if(!s) return false;
            s = f7 is not null && await f7.Invoke(board); if(!s) return false;
            s = f8 is not null && await f8.Invoke(board); if(!s) return false;
            
            return true;
        }

        //Conditional nodes are syntax sugar that check condition before executing the action
        //Every conditional node can be replaced by two nodes the first of them is an Action node containing the condition and wrapping the second mains node 
        //But usually is more convenient to use one conditional node instead

        /// <summary>
        /// Check condition, execute given action and then return its inverted result 
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="condition">Condition given as a delegate returning bool</param>
        /// <param name="func">Action returning Status</param>
        /// <param name="elseFunc"></param>
        /// <returns>If the condition is false return Failure. Else return inverted value of the func</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<bool> ConditionalInverter<T>(
            this T board,
            Func<T, bool> condition,
            Func<T, UniTask<bool>> func,
            Func<T, UniTask<bool>> elseFunc = null)
            => condition.Invoke(board)
                ? await board.Inverter(func)
                : elseFunc != null && await board.Inverter(elseFunc);

        /// <summary>
        /// Check condition before Selector
        /// Returns Failure if the condition is false
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="condition">Condition given as a delegate returning bool</param>
        /// <param name="f1">Delegate receiving T and returning Status</param>
        /// <param name="f2">Delegate receiving T and returning Status</param>
        /// <param name="f3">Optional delegate receiving T and returning Status</param>
        /// <param name="f4">Optional delegate receiving T and returning Status</param>
        /// <param name="f5">Optional delegate receiving T and returning Status</param>
        /// <param name="f6">Optional delegate receiving T and returning Status</param>
        /// <returns>If the condition is false return Failure. Else return the result of Selector</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<bool> ConditionalSelector<T>(
                this T board,
                Func<T, bool> condition,
                Func<T, UniTask<bool>> f1,
                Func<T, UniTask<bool>> f2,
                Func<T, UniTask<bool>> f3 = null,
                Func<T, UniTask<bool>> f4 = null,
                Func<T, UniTask<bool>> f5 = null,
                Func<T, UniTask<bool>> f6 = null)
            => condition.Invoke(board) && await board.Selector(f1, f2, f3, f4, f5, f6);

        /// <summary>
        /// Check condition before Sequencer
        /// </summary>
        /// <param name="board">Blackboard object</param>
        /// <param name="condition">Condition given as a delegate returning bool</param>
        /// <param name="f1">Delegate receiving T and returning Status</param>
        /// <param name="f2">Delegate receiving T and returning Status</param>
        /// <param name="f3">Optional delegate receiving T and returning Status</param>
        /// <param name="f4">Optional delegate receiving T and returning Status</param>
        /// <param name="f5">Optional delegate receiving T and returning Status</param>
        /// <param name="f6">Optional delegate receiving T and returning Status</param>
        /// <returns>If the condition is false return Failure. Else return the result of Sequencer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async UniTask<bool> IfSequencer<T>(this T board,
                Func<T, bool> condition,
                Func<T, UniTask<bool>> f1,
                Func<T, UniTask<bool>> f2,
                Func<T, UniTask<bool>> f3 = null,
                Func<T, UniTask<bool>> f4 = null,
                Func<T, UniTask<bool>> f5 = null,
                Func<T, UniTask<bool>> f6 = null)
            => condition.Invoke(board) && await board.Sequencer(f1, f2, f3, f4, f5, f6);
    }
}