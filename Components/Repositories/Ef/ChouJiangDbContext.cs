using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components.Domains;
using log4net;

namespace Components.Repositories.Ef
{
    public class ChouJiangDbConfiguration : DbConfiguration
    {
        public ChouJiangDbConfiguration()
        {
            this.SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy(3, TimeSpan.FromSeconds(10)));
        }
    }
    [DbConfigurationType(typeof(ChouJiangDbConfiguration))]
    public class ChouJiangDbContext : DbContext
    {
        protected static ILog logger = LogManager.GetLogger(typeof(ChouJiangDbContext));
        private string guid = Guid.NewGuid().ToString();
        public string InstanceId
        {
            get
            {
                return guid;
            }
        }
        public ChouJiangDbContext()
            : base("ChouJiangDb")
        {
        }
        public ChouJiangDbContext(String connString)
            : base(connString)
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AccountMoneyLog>().ToTable("AccountMoneyLogs");
            modelBuilder.Entity<AccountVouchersLog>().ToTable("AccountVouchersLogs");
            modelBuilder.Entity<AccountChargeLog>().ToTable("AccountChargeLogs");
            modelBuilder.Entity<AccountCashOutOperationLog>().ToTable("AccountCashOutOperationLogs");

            modelBuilder.Configurations.Add(new UserMapping());
            modelBuilder.Configurations.Add(new AccountMapping());
            modelBuilder.Configurations.Add(new AccountCashOutLogMapping());

        }
        public class UserMapping : EntityTypeConfiguration<User>
        {
            public UserMapping()
            {
                ToTable("Users");



            }
        }
        public class AccountMapping : EntityTypeConfiguration<Account>
        {
            public AccountMapping()
            {
                ToTable("Accounts");
                HasKey(a => a.Id)
                    .HasRequired(a => a.User)
                    .WithRequiredDependent(u => u.Account)
                    .WillCascadeOnDelete(true);

                HasMany(a => a.MoneyLogs)
                    .WithRequired(l => l.Account)
                    .HasForeignKey(l => l.AccountId)
                    .WillCascadeOnDelete(true);

                HasMany(a => a.CashOutLogs)
                    .WithRequired(l => l.Account)
                    .HasForeignKey(l => l.AccountId)
                    .WillCascadeOnDelete(true);

                HasMany(a => a.VouchersLogs)
                    .WithRequired(l => l.Account)
                    .HasForeignKey(l => l.AccountId)
                    .WillCascadeOnDelete(true);

                HasMany(a => a.ChargeLogs)
                    .WithRequired(l => l.Account)
                    .HasForeignKey(l => l.AccountId)
                    .WillCascadeOnDelete(true);
            }
        }
        public class AccountCashOutLogMapping : EntityTypeConfiguration<AccountCashOutLog>
        {
            public AccountCashOutLogMapping()
            {
                ToTable("AccountCashOutLogs");

                HasMany(a => a.OpLogs)
                    .WithRequired(l => l.CashOut)
                    .HasForeignKey(l => l.CashOutId)
                    .WillCascadeOnDelete(true);
            }
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Account> Account { get; set; }
        public DbSet<AccountChargeLog> AccountChargeLog { get; set; }
        public DbSet<AccountVouchersLog> AccountVouchersLog { get; set; }
    }


}
