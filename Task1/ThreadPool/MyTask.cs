using System;

namespace ThreadPool {
    public class MyTask<TResult> : IMyTask<TResult> {
        private volatile bool _isCompleted;
        private volatile AggregateException _exception;
        private TResult _result;
        private Func<TResult> _func;
        private MyThreadPool _threadPool;

        public bool IsCompeted => _isCompleted;
        public Action ActionToPerform => Run;

        public TResult Result {
            get {
                while (!_isCompleted) {
                    if (_exception != null) {
                        throw _exception;
                    }
                }

                return _result;
            }
        }

        public MyTask(Func<TResult> func, MyThreadPool threadPool) {
            _isCompleted = false;
            _func = func;
            _threadPool = threadPool;
        }

        private void Run() {
            try {
                _result = _func();
                _isCompleted = true;
            }
            catch (Exception e) {
                _exception = new AggregateException(e);
            }
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> func) {
            return _threadPool.Enqueue(() => func(Result));
        }
    }
}