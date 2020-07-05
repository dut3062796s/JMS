﻿using JMS.Impls;
using JMS.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JMS
{
    class LockKeyManager
    {
        System.Collections.Concurrent.ConcurrentDictionary<string, KeyObject> _cache;
        Gateway _gateway;
        IConfiguration _configuration;
        int _timeout;
        ILogger<LockKeyManager> _logger;
        public LockKeyManager(Gateway gateway,IConfiguration configuration,ILogger<LockKeyManager> logger)
        {
            _timeout = configuration.GetValue<int>("UnLockKeyTimeout");
            _cache = new System.Collections.Concurrent.ConcurrentDictionary<string, KeyObject>();
            _gateway = gateway;
            _configuration = configuration;
            _logger = logger;

            SystemEventCenter.MicroServiceOnffline += SystemEventCenter_MicroServiceOnffline;
            SystemEventCenter.MicroServiceOnline += SystemEventCenter_MicroServiceOnline;

            new Thread(checkTimeout).Start();
        }

        private void SystemEventCenter_MicroServiceOnline(object sender, Dtos.RegisterServiceInfo e)
        {
            string[] keys = null;
            while (true)
            {
                try
                {
                    keys = _cache.Keys.ToArray();
                    break;
                }
                catch
                {
                    Thread.Sleep(0);
                }
            }
            for (int i = 0; i < keys.Length; i++)
            {
                KeyObject obj = null;
                try
                {
                    obj = _cache[keys[i]];
                }
                catch
                {
                    continue;
                }

                if (obj != null)
                {
                    if (obj.Locker == e.ServiceId)
                    {
                        obj.RemoveTime = null;
                    }
                }
            }
        }

        private void SystemEventCenter_MicroServiceOnffline(object sender, Dtos.RegisterServiceInfo e)
        {
            //把这个微服务的lockkey设置下线时间
            string[] keys = null;
            while (true)
            {
                try
                {
                    keys = _cache.Keys.ToArray();
                    break;
                }
                catch
                {
                    Thread.Sleep(0);
                }
            }
            for(int i = 0; i < keys.Length; i ++)
            {
                KeyObject obj = null;
                try
                {
                    obj = _cache[keys[i]];
                }
                catch
                {
                    continue;
                }

                if(obj != null)
                {
                    if(obj.Locker == e.ServiceId)
                    {
                        obj.RemoveTime = DateTime.Now.AddMilliseconds(_timeout);
                    }
                }
            }
        }

        private void checkTimeout()
        {
            while(true)
            {
                try
                {
                    foreach (var pair in _cache)
                    {
                        var obj = pair.Value;
                        if (obj.Locker > 0)
                        {
                            if (obj.RemoveTime != null && DateTime.Now >= obj.RemoveTime.GetValueOrDefault())
                            {
                                _cache.TryRemove(obj.Key, out obj);
                                _logger?.LogInformation($"key:{obj.Key}超时被unlock");
                            }
                        }
                       
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, ex.Message);
                }
                finally
                {
                    Thread.Sleep(2000);
                }
            }
        }

        public bool TryLock(string key, IMicroServiceReception locker)
        {
            KeyObject keyObj = null;
            while (true)
            {
                if (_cache.TryGetValue(key, out keyObj) == false)
                {
                    if (_cache.TryAdd(key, new KeyObject()
                    {
                        Key = key,
                        Locker = locker.Id,
                    }))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if(keyObj.Locker == locker.Id)
                    {
                        keyObj.RemoveTime = null;
                        return true;
                    }
                    if (Interlocked.CompareExchange(ref keyObj.Locker, locker.Id, 0) == 0)
                    {
                        keyObj.RemoveTime = null;
                        return true;
                    }
                    else
                        return false;
                }
            }
        }

        public void UnLock(string key,IMicroServiceReception service)
        {
            KeyObject keyObj = null;
            if (_cache.TryGetValue(key, out keyObj)  )
            {
                if (keyObj.Locker == service.Id)
                {
                    _cache.TryRemove(key, out keyObj);
                }
            }
        }
    }

    class KeyObject
    {
        public string Key;
        /// <summary>
        /// 不为0:locked 0:unlocked
        /// </summary>
        public int Locker;
        /// <summary>
        /// 设定移除时间
        /// </summary>
        public DateTime? RemoveTime;
    }
}
