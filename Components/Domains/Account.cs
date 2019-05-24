using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Components.Domains
{
    /// <summary>
    /// 用户账户
    /// </summary>
    public class Account
    {
        public int Id { get; set; }
        /// <summary>
        /// 总余额，真实的钱，可提现，来源是收入，比如课程收益、打赏等
        /// </summary>
        public decimal Money { get; set; }
        /// <summary>
        /// 累计收益
        /// </summary>
        public decimal MoneyTotal { get; set; }
        /// <summary>
        /// 锁定的金额，不能使用和提现
        /// </summary>
        public decimal MoneyLocked { get; set; }
        /// <summary>
        /// 代金券（点数）余额，用户充值的钱
        /// </summary>
        public decimal Vouchers { get; set; }
        /// <summary>
        /// 当前可用金额，可用于提现的金额
        /// </summary>
        public decimal MoneyCanUse { get { return Money - MoneyLocked; } }
        public decimal MoneyDistribution { get { return Money - MoneyLocked; } }

        public virtual User User { get; set; }

        public virtual List<AccountMoneyLog> MoneyLogs { get; set; }
        public virtual List<AccountVouchersLog> VouchersLogs { get; set; }
        public virtual List<AccountCashOutLog> CashOutLogs { get; set; }
        public virtual List<AccountChargeLog> ChargeLogs { get; set; }
    }


    /// <summary>
    /// 用户现金日志
    /// </summary>
    public class AccountMoneyLog
    {
        public int Id { get; set; }
        [StringLength(100)]

        /// <summary>
        /// 具体日志信息的编号
        /// </summary>
        public string Number { get; set; }
        public int AccountId { get; set; }
        public decimal Money { get; set; }
        public decimal Before { get; set; }
        public decimal After { get; set; }
        public DateTime CreateTime { get; set; }
        public AccountMoneyLogType Type { get; set; }
        [StringLength(200)]
        public string Description { get; set; }

        public virtual Account Account { get; set; }
    }

    /// <summary>
    /// 用户现金日志类型
    /// </summary>
    public enum AccountMoneyLogType
    {
        /// <summary>
        /// 报名收入
        /// </summary>
        ClassIncome,
        /// <summary>
        /// 打赏收入
        /// </summary>
        BonusIncome,
        /// <summary>
        /// 充值支出
        /// </summary>
        ChargeOut,
        /// <summary>
        /// 提现支出
        /// </summary>
        CashOut,
        /// <summary>
        /// 分销收入
        /// </summary>
        DistributionIncome
    }

    public enum CashOutType
    {
        /// <summary>
        /// 管理中心，充值与提现中提现
        /// </summary>
        ManageCashOut,
        /// <summary>
        /// 个人中心，钱包中提现
        /// </summary>
        UserCashOut,
        /// <summary>
        /// 合伙人提现
        /// </summary>
        PartnerCashOut
    }
    /// <summary>
    /// 提现记录
    /// </summary>
    public class AccountCashOutLog
    {
        public int Id { get; set; }
        [StringLength(50)]
        public string Number { get; set; }
        public decimal Money { get; set; }
        public int AccountId { get; set; }
        public DateTime CreateTime { get; set; }
        public CashOutType CashOutType { get; set; }
        public AccountCashOutStatus Status { get; set; }
        [StringLength(50)]
        public string PayNumber { get; set; }
        public decimal ActualPayMoney { get; set; }
        public virtual Account Account { get; set; }
        public virtual List<AccountCashOutOperationLog> OpLogs { get; set; }
    }

    /// <summary>
    /// 提现记录操作日志
    /// </summary>
    public class AccountCashOutOperationLog
    {
        public int Id { get; set; }
        public int ManagerId { get; set; }
        public int CashOutId { get; set; }
        public DateTime CreateTime { get; set; }
        [StringLength(200)]
        public string Message { get; set; }
        public AccountCashOutStatus StatusOld { get; set; }
        public AccountCashOutStatus StatusNew { get; set; }

        [JsonIgnore]
        public virtual AccountCashOutLog CashOut { get; set; }
    }
    /// <summary>
    /// 提现记录状态
    /// </summary>
    public enum AccountCashOutStatus
    {
        Auditing,
        Paying,
        Payed,
        Denny
    }
    /// <summary>
    /// 用户代金券日志
    /// </summary>
    public class AccountVouchersLog
    {
        public int Id { get; set; }
        [StringLength(50)]
        public string Number { get; set; }
        public int AccountId { get; set; }
        public decimal Vouchers { get; set; }
        public decimal Before { get; set; }
        public decimal After { get; set; }
        public DateTime CreateTime { get; set; }
        public AccountVouchersLogType Type { get; set; }
        [StringLength(200)]
        public string Description { get; set; }
        /// <summary>
        /// 具体日志信息的ID
        /// </summary>
        public int DetailId { get; set; }

        public virtual Account Account { get; set; }
    }
    /// <summary>
    /// 用户代金券账户日志类型
    /// </summary>
    public enum AccountVouchersLogType
    {
        Charge,
        Billing
    }

    /// <summary>
    /// 代金券充值日志
    /// </summary>
    public class AccountChargeLog
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        [StringLength(100)]
        [Index("INDEX_CHARGE_NUMBER", IsUnique = true)]
        public string Number { get; set; }
        /// <summary>
        /// 充值使用了多少现金
        /// </summary>
        public decimal Money { get; set; }
        /// <summary>
        /// 获得了多少代金券
        /// </summary>
        public decimal Vouchers { get; set; }
        public DateTime CreateTime { get; set; }
        [StringLength(100)]
        [Index("INDEX_PAY", IsUnique = true, Order = 0)]
        public string PayMethod { get; set; }
        [StringLength(100)]
        [Index("INDEX_PAY", IsUnique = true, Order = 1)]
        public string PayNumber { get; set; }
        [StringLength(200)]
        public string Description { get; set; }

        public virtual Account Account { get; set; }
    }
}