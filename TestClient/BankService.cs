﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TestClient
{
    using JMS;
    using System;
    using System.Collections.Generic;
    using System.Linq;


    [InvokerInfo("Bank")]
    public class BankService : IImplInvoker
    {

        protected JMS.IMicroService _microService;

        public BankService(JMS.IMicroService microService)
        {
            this._microService = microService;
        }

        public virtual long CreateBankAccount(long userid, int seconds, double sucessRate)
        {
            return this._microService.Invoke<long>("CreateBankAccount", userid, seconds, sucessRate);
        }

        public virtual System.Threading.Tasks.Task<long> CreateBankAccountAsync(long userid, int seconds, double sucessRate)
        {
            return this._microService.InvokeAsync<long>("CreateBankAccount", userid, seconds, sucessRate);
        }

        public virtual void LockKey(string key)
        {
            this._microService.Invoke("LockKey", key);
        }

        public virtual System.Threading.Tasks.Task LockKeyAsync(string key)
        {
            return this._microService.InvokeAsync("LockKey", key);
        }

        public virtual void UnLockKey(string key)
        {
            this._microService.Invoke("UnLockKey", key);
        }

        public virtual System.Threading.Tasks.Task UnLockKeyAsync(string key)
        {
            return this._microService.InvokeAsync("UnLockKey", key);
        }
    }
}
