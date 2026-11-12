// SPDX-FileCopyrightText: 2026 Mora <trixxedheart@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server._AS.Shuttles;

/// <summary>
/// Raised when a character enters cryosleep.
/// </summary>
public readonly record struct ASCrewEnteredCryoEvent(EntityUid Body);

/// <summary>
/// Raised when a character leaves cryosleep.
/// </summary>
public readonly record struct ASCrewLeftCryoEvent(EntityUid Body);
