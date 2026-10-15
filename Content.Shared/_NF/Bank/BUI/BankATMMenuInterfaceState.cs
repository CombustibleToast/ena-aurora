using Robust.Shared.Serialization;

namespace Content.Shared._NF.Bank.BUI;

[NetSerializable, Serializable]
public sealed class BankATMMenuInterfaceState : BoundUserInterfaceState
{
    /// <summary>
    /// bank balance of the character using the atm
    /// </summary>
    public long Balance; // Aurora's Song - Expand balance to long

    /// <summary>
    /// are the buttons enabled
    /// </summary>
    public bool Enabled;

    /// <summary>
    /// how much cash is inserted
    /// </summary>
    public int Deposit;

    public BankATMMenuInterfaceState(long balance, bool enabled, int deposit) // Aurora's Song - Expand balance to long
    {
        Balance = balance;
        Enabled = enabled;
        Deposit = deposit;
    }
}
