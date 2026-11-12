# THE() is not used here because the entity and its name can technically be disconnected if a nameOverride is passed...
# Aurora's Song - Changed to bolditalic from italic
chat-manager-entity-subtle-wrap-message = [bolditalic]{ PROPER($entity) ->
    *[false] The {$entityName} {$message}[/italic]
    [true] [Name]{CAPITALIZE($entityName)}[/Name] {$message}[/italic]
}

chat-manager-entity-subtle-ooc-wrap-message = [bolditalic](OOC) [Name]{$entityName}[/Name]: {$message}[/italic]
