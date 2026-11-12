using Robust.Shared.Serialization;

namespace Content.Shared._NF.Cargo.BUI;

[Serializable, NetSerializable]
public sealed class NFCargoConsoleInterfaceState(
    string name,
    int count,
    int capacity,
    long balance, // Aurora's Song - Expand balance to long
    List<NFCargoOrderData> orders) : BoundUserInterfaceState
{
    public string Name = name;
    public int Count = count;
    public int Capacity = capacity;
    public long Balance = balance; // Aurora's Song - Expand balance to long
    public List<NFCargoOrderData> Orders = orders;
}
