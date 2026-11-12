using Robust.Shared.Player;

namespace Content.Shared._NF.Bank.Events;
public sealed record BalanceChangedEvent(ICommonSession Session, long Amount); // Aurora's Song - Expand balance to long
