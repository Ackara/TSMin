using Acklann.Daterpillar.Attributes;
using Acklann.Daterpillar.Linq;
using System;
using System.Data;
using Tecari.LLC.Gateways;

namespace Tecari.LLC
{

    public partial class Transaction : IEntity
    {
        [Key, Column(id)]
        public string Id { get; set; }

        [Column(date)]
        public DateTime Date { get; set; }

        [Column(category)]
        public string Category { get; set; }

        [Column(description)]
        public string Description { get; set; }

        [Column(amount)]
        public double Amount { get; set; }

        [Column(currency)]
        public string Currency { get; set; }

        [Column(payout)]
        public bool Payout { get; set; }

        [Column(status)]
        public TransactionStatus Status { get; set; }

        [Column(gateway)]
        public PaymentProvider Gateway { get; set; }

        [Column(reference_id)]
        public string ReferenceId { get; set; }

        [Column(invoice_id)]
        public string InvoiceId { get; set; }

        #region IEntity

        [SqlIgnore] public const string TABLE = "transaction";
        [SqlIgnore] public const string id = nameof(id);
        [SqlIgnore] public const string date = nameof(date);
        [SqlIgnore] public const string category = nameof(category);
        [SqlIgnore] public const string description = nameof(description);
        [SqlIgnore] public const string amount = nameof(amount);
        [SqlIgnore] public const string currency = nameof(currency);
        [SqlIgnore] public const string payout = nameof(payout);
        [SqlIgnore] public const string status = nameof(status);
        [SqlIgnore] public const string gateway = nameof(gateway);
        [SqlIgnore] public const string reference_id = nameof(reference_id);
        [SqlIgnore] public const string invoice_id = nameof(invoice_id);

        private readonly string[] _columns = new string[] { id, date, category, description, amount, currency, payout, status, gateway, reference_id, invoice_id };

        public string GetTableName() => TABLE;

        public string[] GetColumnList() => _columns;

        public object[] GetValueList()
        {
            return new object[]
            {
                Id.Serialize(),
                Date.Serialize(),
                Category.Serialize(),
                Description.Serialize(),
                Amount.Serialize(),
                Currency.Serialize(),
                Payout.Serialize(),
                Status.Serialize(),
                Gateway.Serialize(),
                ReferenceId.Serialize(),
                InvoiceId.Serialize()
            };
        }

        public void Load(IDataRecord record)
        {
            Id = (string)record[id];
            Date = (DateTime)record[date];
            Category = (string)record[category];
            Description = (string)record[description];
            Amount = (double)record[amount];
            Currency = (string)record[currency];
            Payout = (bool)record[payout];
            Status = (TransactionStatus)record[status];
            Gateway = (PaymentProvider)record[gateway];
            ReferenceId = (string)record[reference_id];
            InvoiceId = (string)record[invoice_id];
        }

        #endregion IEntity
    }
}