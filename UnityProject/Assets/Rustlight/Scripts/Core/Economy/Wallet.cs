using System;
using Rustlight.Core.Save;

namespace Rustlight.Core.Economy
{
    /// <summary>Spends and earns both currencies on top of the saved <see cref="WalletData"/>.</summary>
    public sealed class Wallet
    {
        private readonly WalletData _data;
        public event Action Changed;

        public Wallet(WalletData data) => _data = data;

        public long Scrap => _data.scrap;
        public long Cores => _data.cores;

        public void AddScrap(long amount) { if (amount > 0) { _data.scrap += amount; Changed?.Invoke(); } }
        public void AddCores(long amount) { if (amount > 0) { _data.cores += amount; Changed?.Invoke(); } }

        public bool TrySpendScrap(long amount)
        {
            if (amount < 0 || _data.scrap < amount) return false;
            _data.scrap -= amount;
            Changed?.Invoke();
            return true;
        }

        public bool TrySpendCores(long amount)
        {
            if (amount < 0 || _data.cores < amount) return false;
            _data.cores -= amount;
            Changed?.Invoke();
            return true;
        }
    }
}
