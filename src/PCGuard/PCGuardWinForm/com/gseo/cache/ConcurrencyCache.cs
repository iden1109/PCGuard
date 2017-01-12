using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace com.gseo.cache
{
    /// <summary>
    /// 一個定期清潔肚子內資料的暫存器
    /// </summary>
    /// <typeparam name="TKey">Key</typeparam>
    public class ConcurrencyCache<TKey>
    {
        private static ConcurrencyCache<TKey> _concurrencyCache; //Singleton

        private ConcurrentDictionary<TKey, DateTime> _dictionary;
        private Task _task;
        private CancellationTokenSource _cancel;
        private int _periodDay; //預設清資料的週期天數

        private ConcurrencyCache()
        {
            _dictionary = new ConcurrentDictionary<TKey, DateTime>();
            _cancel = new CancellationTokenSource();
            _task = Task.Factory.StartNew(RunClearThread, _cancel.Token);
            _periodDay = 60; //Cache保留60天
        }

        /// <summary>
        /// 取得實體
        /// </summary>
        /// <returns></returns>
        public static ConcurrencyCache<TKey> GetInstance()
        {
            if (_concurrencyCache == null)
                _concurrencyCache = new ConcurrencyCache<TKey>();

            return _concurrencyCache;
        }

        /// <summary>
        /// 新增資料到暫存器
        /// </summary>
        /// <param name="item">Key</param>
        /// <param name="value">Value</param>
        public void Add(TKey item, DateTime value)
        {
            if (item == null)
                return;
            if (value == null)
                return;

            if (_dictionary.ContainsKey(item))
            {
                DateTime oldValue = _dictionary[item];
                _dictionary.TryUpdate(item, value, oldValue);
            }
            else
            {
                _dictionary.TryAdd(item, value);
            }
        }

        /// <summary>
        /// 暫存器內是否有這個Key
        /// </summary>
        /// <param name="item">Key</param>
        /// <returns></returns>
        public bool ContainKey(TKey item)
        {
            if (item == null)
                return false;

            return _dictionary.ContainsKey(item);
        }

        /// <summary>
        /// 清空暫存器內容
        /// </summary>
        public void Clear()
        {
            _dictionary.Clear();
        }

        /// <summary>
        /// 停止定時清潔
        /// </summary>
        public void Stop()
        {
            if (_cancel != null)
                _cancel.Cancel();
            Clear();
        }

        /// <summary>
        /// 啟動定時清潔
        /// </summary>
        public void Start()
        {
            Stop();
            _cancel = new CancellationTokenSource();
            _task = Task.Factory.StartNew(RunClearThread, _cancel.Token);
        }

        /// <summary>
        /// 定期整理資料
        /// </summary>
        private void RunClearThread()
        {
            while (true)
            {
                if (_cancel.IsCancellationRequested)
                    return;

                DateTime startDate = DateTime.Now.AddDays(-1 * _periodDay);
                DateTime tempDate;

                foreach (KeyValuePair<TKey, DateTime> item in _dictionary)
                {
                    if (DateTime.Compare(item.Value, startDate) < 0)
                    {
                        _dictionary.TryRemove(item.Key, out tempDate);
                        //Debug.WriteLine("ConcurrencyCache RunClearThread() remove:" + item.Key);
                    }
                }

                Thread.Sleep(3600000);//1小時clean一次
            }
        }

    }
}
