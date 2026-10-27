# Command Info
roll-command-description = Rolls dice privately, or shows the result to nearby players.
roll-command-help = Usage: roll <sides|NdS[+/-modifier]> [show]

# Errors
roll-command-invalid = Invalid roll. Use: roll <sides|NdS[+/-modifier]> [show]
roll-command-missing-expression = Specify what to roll, Example: roll 2d20+5
roll-command-too-many-arguments = Too many arguments. Only "show" may follow the dice expression
roll-command-invalid-option = Invalid option. Use "show" to display the result to nearby players
roll-command-invalid-count = The number of dice must be a whole number before the "d"
roll-command-count-out-of-range = The number of dice must be between 1 and {$maximum}
roll-command-invalid-sides = The number of sides must be a whole number, such as 20 or 2d20
roll-command-sides-out-of-range = Dice must have between 1 and {$maximum} sides
roll-command-invalid-modifier = The modifier must be a whole number prefixed with + or -
roll-command-modifier-out-of-range = The modifier must be between -{$maximum} and +{$maximum}
roll-command-show-disabled = Showing rolls to nearby players is disabled. Rolling privately instead

# Results
roll-command-result-private = Rolled {$total} ({$calculation})
roll-command-result-public = rolls {$total} ({$calculation})
