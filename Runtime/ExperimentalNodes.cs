using System;
using Cysharp.Threading.Tasks;

namespace Baltin.UFBT
{
    public static class ExperimentalNodes
    {
        /// <summary>
        /// Simplest Parallel node
        /// It is not GC free because of using array required as an input for UniTask.WhenAll
        /// </summary>
        /// <param name="board"></param>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="f3"></param>
        /// <param name="f4"></param>
        /// <param name="f5"></param>
        /// <param name="f6"></param>
        /// <param name="f7"></param>
        /// <param name="f8"></param>
        /// <param name="successIfAll"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async UniTask<bool> Parallel<T>(this T board, 
            Func<T, UniTask<bool>> f1,
            Func<T, UniTask<bool>> f2,
            Func<T, UniTask<bool>> f3 = null,
            Func<T, UniTask<bool>> f4 = null,
            Func<T, UniTask<bool>> f5 = null,
            Func<T, UniTask<bool>> f6 = null,
            Func<T, UniTask<bool>> f7 = null,
            Func<T, UniTask<bool>> f8 = null,
            bool successIfAll = true)
        {
            var tasks = new[]
            {
                f1?.Invoke(board) ?? UniTask.FromResult(true),
                f2?.Invoke(board) ?? UniTask.FromResult(true),
                f3?.Invoke(board) ?? UniTask.FromResult(true),
                f4?.Invoke(board) ?? UniTask.FromResult(true),
                f5?.Invoke(board) ?? UniTask.FromResult(true),
                f6?.Invoke(board) ?? UniTask.FromResult(true),
                f7?.Invoke(board) ?? UniTask.FromResult(true),
                f8?.Invoke(board) ?? UniTask.FromResult(true)
            };

            var results = await UniTask.WhenAll(tasks);

            if (successIfAll)
            {
                foreach (var r in results)
                    if (!r) return false;
                return true;
            }
            
            foreach (var r in results)
                if (r) return true;
            return false;
        }
        
        /// <summary>
        /// GC free parallel for node 
        /// </summary>
        /// <param name="board"></param>
        /// <param name="playerLoopTiming"></param>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="f3"></param>
        /// <param name="f4"></param>
        /// <param name="f5"></param>
        /// <param name="f6"></param>
        /// <param name="f7"></param>
        /// <param name="f8"></param>
        /// <param name="successIfAll"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async UniTask<bool> ParallelGCFree<T>(
            this T board,
            PlayerLoopTiming playerLoopTiming,
            Func<T, UniTask<bool>> f1,
            Func<T, UniTask<bool>> f2,
            Func<T, UniTask<bool>> f3 = null,
            Func<T, UniTask<bool>> f4 = null,
            Func<T, UniTask<bool>> f5 = null,
            Func<T, UniTask<bool>> f6 = null,
            Func<T, UniTask<bool>> f7 = null,
            Func<T, UniTask<bool>> f8 = null,
            bool successIfAll = true)
        {
            // Immediate invoke all the task 
            var t1 = f1?.Invoke(board) ?? UniTask.FromResult(true);
            var t2 = f2?.Invoke(board) ?? UniTask.FromResult(true);
            var t3 = f3?.Invoke(board) ?? UniTask.FromResult(true);
            var t4 = f4?.Invoke(board) ?? UniTask.FromResult(true);
            var t5 = f5?.Invoke(board) ?? UniTask.FromResult(true);
            var t6 = f6?.Invoke(board) ?? UniTask.FromResult(true);
            var t7 = f7?.Invoke(board) ?? UniTask.FromResult(true);
            var t8 = f8?.Invoke(board) ?? UniTask.FromResult(true);

            // Tasks completions
            bool c1, c2, c3, c4, c5, c6, c7, c8;
            c1 = c2 = c3 = c4 = c5 = c6 = c7 = c8 = false;

            // Tasks results
            bool r1, r2, r3, r4, r5, r6, r7, r8;
            r1 = r2 = r3 = r4 = r5 = r6 = r7 = r8 = false;
            
            while (true)
            {
                if (!c1 && t1.Status.IsCompleted()) { r1 = t1.GetAwaiter().GetResult(); c1 = true; }
                if (!c2 && t2.Status.IsCompleted()) { r2 = t2.GetAwaiter().GetResult(); c2 = true; }
                if (!c3 && t3.Status.IsCompleted()) { r3 = t3.GetAwaiter().GetResult(); c3 = true; }
                if (!c4 && t4.Status.IsCompleted()) { r4 = t4.GetAwaiter().GetResult(); c4 = true; }
                if (!c5 && t5.Status.IsCompleted()) { r5 = t5.GetAwaiter().GetResult(); c5 = true; }
                if (!c6 && t6.Status.IsCompleted()) { r6 = t6.GetAwaiter().GetResult(); c6 = true; }
                if (!c7 && t7.Status.IsCompleted()) { r7 = t7.GetAwaiter().GetResult(); c7 = true; }
                if (!c8 && t8.Status.IsCompleted()) { r8 = t8.GetAwaiter().GetResult(); c8 = true; }

                // Проверка, можно ли завершить досрочно для successIfAll = false
                if (!successIfAll)
                {
                    if (r1 || r2 || r3 || r4 || r5 || r6 || r7 || r8)
                        return true;
                }

                // Если все задачи завершены, выходим
                if (c1 && c2 && c3 && c4 && c5 && c6 && c7 && c8)
                    break;

                // Yield for waiting the next tick
                await UniTask.Yield(playerLoopTiming);
            }

            if (successIfAll)
                return r1 && r2 && r3 && r4 && r5 && r6 && r7 && r8;

            return false;
        }
    }
}